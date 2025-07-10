#if NETFRAMEWORK
using System.IO;
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	sealed class HttpContextExtensionsTest : TestCaseWithFactory
	{
		public void TestGetImageMIMEType()
		{
			AssertImageMIMEType("test.gif", "image/gif");
			AssertImageMIMEType("test.bmp", "image/bmp");
			AssertImageMIMEType("test.jpg", "image/jpg");
			AssertImageMIMEType("test.jpeg", "image/jpg");
			AssertImageMIMEType("test.png", "image/png");
			AssertImageMIMEType("", "image/png");
			AssertImageMIMEType("test.unknown", "image/png");
			AssertEquals("image/png", ((HttpContext)null).GetImageMIMEType());
		}

		void AssertImageMIMEType(string filePath, string expectedMIMEType)
		{
#if NETFRAMEWORK
			var request = new HttpRequest(string.Empty, $"http://wisetechglobal.com/{filePath}", string.Empty);
			var response = new HttpResponse(new StringWriter());
			var context = new HttpContext(request, response);
#else
			var context = new DefaultHttpContext();
			context.Request.Scheme = "https";
			context.Request.Host = new HostString("wisetechglobal.com");
			context.Request.Path = $"/{filePath}";
#endif

			AssertEquals(expectedMIMEType, context.GetImageMIMEType());
		}
	}
}
