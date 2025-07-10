using System.Net;
using System.Net.Http;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class HttpExtensionsTest : TestCase
	{
		public void TestResponseGetAsString()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Sample Response") };
			response.Content.Headers.Add("content-id", "demo content");
			response.Headers.Add("Receipt-ID", "e0606fe6233348119cf18023c0fb5271");
			var responseDetails = response.GetAsString();
			AssertEquals(@"Received response details:
Request Uri: <Empty> 
Header: Receipt-ID : e0606fe6233348119cf18023c0fb5271
Body: Sample Response
StatusCode: OK
Reason: OK
IsSuccessful: True", responseDetails);
		}

		public void TestHeadersGetAsString()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Sample Response") };
			response.Content.Headers.Add("content-id", "demo content");
			response.Content.Headers.Add("ext", new string[] { "txt", "xml", "pdf" });
			var headerAsString = response.Content.Headers.GetAsString();
			AssertEquals(@"Content-Type : text/plain; charset=utf-8
content-id : demo content
ext : txt, xml, pdf", headerAsString);
		}
	}
}
