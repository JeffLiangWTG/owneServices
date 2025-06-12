using System;
using System.Configuration;
using System.Net;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	[WithGatewayService]
	public class eHubGatewayWsdlTest
	{
		[TestCase("location", "wsdl")]
		[TestCase("location", "singleWsdl")]
		[TestCase("schemaLocation", "wsdl=wsdl0")]
		[TestCase("schemaLocation", "xsd=xsd0")]
		[TestCase("ref", "disco")]
		[TestCase("docRef", "disco")]
		public void TestWsdlPage_ChangeLocationToHttps(string attribute, string query)
		{
			var endpoint = ConfigurationManager.AppSettings["CargoWise.eHub.Gateway.Wsdl.Http.Endpoint"];
			var builder = new UriBuilder(endpoint) { Query = query};

			var request = CreateRequestWithTimeout(builder.Uri.AbsoluteUri);

			using (var response = (HttpWebResponse)request.GetResponse())
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(response.GetResponseStream());
				var nodes = xmlDoc.SelectNodes($"//*[@{attribute}]");

				Assert.NotZero(nodes?.Count ?? 0);
				for (var index = 0; index < nodes?.Count; index++)
				{
					var uri = new Uri(nodes[index].Attributes[attribute].Value);
					Assert.IsTrue("https".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase));
				}
			}
		}
		protected static  WebRequest CreateRequestWithTimeout(string endPoint)
		{
			var request = (HttpWebRequest)WebRequest.Create(endPoint);
			request.Credentials = CredentialCache.DefaultCredentials;
			request.ReadWriteTimeout = 30000;
			return request;
		}
	}
}