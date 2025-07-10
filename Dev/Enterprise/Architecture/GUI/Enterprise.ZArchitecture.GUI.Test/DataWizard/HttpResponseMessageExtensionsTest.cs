using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class HttpResponseMessageExtensionsTest : TestCase
	{
		public void TestIsProblemDetails_WhenTrue()
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				using (sampleDataResponse.Content = new StringContent(string.Empty, Encoding.UTF8, "application/problem+json"))
				{
					AssertEquals(sampleDataResponse.IsProblemDetails(), true);
				}
			}
		}

		public void TestIsProblemDetails_WhenFalse()
		{
			using (var sampleDataResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest })
			{
				using (sampleDataResponse.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"))
				{
					AssertEquals(sampleDataResponse.IsProblemDetails(), false);
				}
			}
		}
	}
}
