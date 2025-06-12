using NUnit.Framework;
using System.Configuration;
using System.Net;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[TestFixture]
	[WithGatewayService]
	public class eHubGatewayHttpHeadersTest
	{
		[TestCase("X-AspNet-Version")]
		[TestCase("Server")]
		[TestCase("X-Powered-By")]
		[TestCase("X-AspNetMvc-Version")]
		public void TestHttpHeadersNoVersionInfo(string versionInfo)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			var endPoint = ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.HealthCheck.Http.Endpoint"];
			var request = WebRequest.Create(endPoint);

			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse) request.GetResponse())
			{
				Assert.IsNull(response.Headers[versionInfo]);
			}
		}
	}
}