using canes_news_api.Modal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace canes_news_api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        
        [HttpGet("get")]
        [ResponseCache(Duration = 20000, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get()
        {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("disable-background-networking", true);
            options.AddUserProfilePreference("enable-fast-unload", true);
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--headless");

            var driver = new ChromeDriver(@"C:\chromedriver\chromedriver.exe", options);

            driver.Navigate().GoToUrl("https://pagev.org/haberler");

            var postBox = driver.FindElement(By.ClassName("information-posts"));
            var posts = postBox.FindElements(By.TagName("li"));
            var postList = new List<PostVm>();
            if (posts.Count > 0)
            {
                foreach (var post in posts)
                {
                    var image = GetImageUrlFromHtml(post.GetAttribute("innerHTML")).Trim();
                    var date = post.FindElement(By.ClassName("date")).Text.Replace(" ","").Replace(":","").Trim();
                    var title = post.FindElement(By.ClassName("item-info")).FindElement(By.TagName("a")).Text.Trim();
                    var link = post.FindElement(By.ClassName("item-info")).FindElement(By.TagName("a")).GetAttribute("href").Trim();
                    var description = post.FindElement(By.ClassName("item-text")).Text.Trim();

                    postList.Add(new PostVm
                    {
                        Date = date,
                        Description=description,
                        Image = image,
                        Link= link,
                        Title = title
                    });
                }
            }


            return Ok(new
            {
                status=200,
                postList
            });
        }

        [HttpGet("get-post")]
        public async Task<IActionResult> GetPost([FromQuery] string link)
        {

            if (string.IsNullOrEmpty(link))
            {
                return Ok(new
                {
                    status = 400,
                });
            }

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("disable-background-networking", true);
            options.AddUserProfilePreference("enable-fast-unload", true);
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");


            var html = "";
            try
            {
                var driver = new ChromeDriver(@"C:\chromedriver\chromedriver.exe", options);
                driver.Navigate().GoToUrl(link);

                html = driver.FindElement(By.ClassName("page-text")).GetAttribute("innerHTML");
            }
            catch
            {
                var driver = new ChromeDriver(@"C:\chromedriver\chromedriver.exe", options);
                driver.Navigate().GoToUrl(link);

                html = driver.FindElement(By.ClassName("page-text")).GetAttribute("innerHTML");
            }


           

            return Ok(new
            {
                status=200,
                html
            });
        }

        static string GetImageUrlFromHtml(string htmlContent)
        {
            // Düzenli ifade ile background-image URL'sini çıkarma
            string pattern = @"background-image:url\((.*?)\)";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
