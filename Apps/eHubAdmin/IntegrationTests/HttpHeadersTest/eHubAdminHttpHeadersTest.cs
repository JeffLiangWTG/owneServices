using eServices.eHubAdmin.IntegrationTests.Attributes;
using NUnit.Framework;
using System.Net;

namespace eServices.eHubAdmin.IntegrationTests
{
	[TestFixture]
	[WithEHubAdminService]
	public class eHubAdminHttpHeadersTest
	{
		[TestCase("X-AspNet-Version")]
		[TestCase("X-Powered-By")]
		[TestCase("X-AspNetMvc-Version")]
		public void TestHttpHeadersNoVersionInfo(string versionInfo)
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("");
			var request = WebRequest.Create(endPoint);

			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse)request.GetResponse())
			{
				Assert.IsNull(response.Headers[versionInfo]);
			}
		}

		[TestCase("GET")]
		[TestCase("HEAD")]
		public void TestWtgStatusHealthCheckIsSuccessful(string httpRequestType)
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("wtg/status");
			var request = WebRequest.Create(endPoint);

			request.Method = httpRequestType;
			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse)request.GetResponse())
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
