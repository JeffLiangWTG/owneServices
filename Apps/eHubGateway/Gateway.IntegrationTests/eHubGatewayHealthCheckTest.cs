using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using System.Configuration;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[TestFixture]
	public class eHubGatewayHealthCheckTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestGatewayHealthCheck()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			var endPoint = ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.HealthCheck.Http.Endpoint"];
			var request = HttpWebRequest.Create(endPoint);

			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse) request.GetResponse())
			{
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(response.IsFromCache);
				Assert.That(response.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						Assert.That(result, Is.EqualTo("INFO(GatewayWebService): Service is alive."));
					}
				}
			}
		}
	}
}