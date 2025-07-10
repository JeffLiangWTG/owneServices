using System;
using System.Net;
using CargoWise.Application;
using WTG.Foundation.Http;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	class WebClient
	{
		public static WebResponseWithDetails WebGetServerPath(string webAddress)
		{
			return WebGet(webAddress, "ServerPath.aspx", 120);
		}

		public static WebResponseWithDetails WebGet(string webAddress, string webPage, int timeoutInSeconds = 120)
		{
			try
			{
				var url = $"http://{webAddress}/{webPage}";
				using var httpClient = ObjectFactory.Get<IHttpClientFactory>().Create();
				httpClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

				var content = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
				return new WebResponseWithDetails(webAddress, webPage, content);
			}
			catch (WebException ex)
			{
				return new WebResponseWithDetails(webAddress, webPage, ex);
			}
			catch (Exception ex)
			{
				return new WebResponseWithDetails(webAddress, webPage, ex);
			}
		}
	}
}
