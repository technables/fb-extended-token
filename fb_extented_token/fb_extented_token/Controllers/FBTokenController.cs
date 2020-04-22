using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace fb_extented_token.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FBTokenController : ControllerBase
    {
        private const string FB_AppId = "your app id";
        private const string FB_AppSecret = "your app secret";
        private string RequestHost()
        {
            HttpContext context = HttpContext.Request.HttpContext;

            var req = HttpContext.Request;

            return $"{req.Scheme}://{req.Host}";
        }

        [HttpGet]
        [Route("")]
        public RedirectResult ConnectFacebook()
        {
            string requestHost = RequestHost();
            string redirectUrl = $"{requestHost}/api/fbtoken/facebook/token";
            string oauthUrl = $"https://www.facebook.com/v6.0/dialog/oauth?client_id={FB_AppId}&redirect_uri={redirectUrl}&scope=manage_pages,publish_pages";


            return RedirectPermanent(oauthUrl);
            // return response;
        }

        [HttpGet]
        [Route("facebook/token")]
        public IActionResult GetExtendedAccessToken([FromQuery]string code)
        {

            string requestHost = RequestHost();
            string redirectUrl = $"{requestHost}/api/fbtoken/facebook/token";

            HttpClient client = new HttpClient();
            //getting short lived token 
            string tokenUrl = $"https://graph.facebook.com/v6.0/oauth/access_token?client_id={FB_AppId}&client_secret={FB_AppSecret}&redirect_uri={redirectUrl}&code={code}";
            HttpResponseMessage response = client.GetAsync(tokenUrl).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            JObject acessObj = JObject.Parse(responseBody);
            string short_lived_token = acessObj["access_token"].ToString();


            //getting profile detail for user
            string profileUrl = $"https://graph.facebook.com/v6.0/me?access_token={short_lived_token}";
            response = client.GetAsync(profileUrl).Result;
            response.EnsureSuccessStatusCode();
            responseBody = response.Content.ReadAsStringAsync().Result;
            JObject profileObj = JObject.Parse(responseBody);
            string profileId = profileObj["id"].ToString();

            //getting extended_token for user
            string extendedTokenUrl = $"https://graph.facebook.com/v6.0/{profileId}/accounts?access_token={short_lived_token}";
            response = client.GetAsync(extendedTokenUrl).Result;
            response.EnsureSuccessStatusCode();
            responseBody = response.Content.ReadAsStringAsync().Result;

            string extended_token = (JObject.Parse(responseBody) as JObject)["data"][0]["access_token"].ToString();

            return Ok(extended_token);
        }

    }
}
