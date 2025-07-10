#if NETFRAMEWORK
using System.IO;
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	public class HttpRequestManagerTest : TransactionedTestCase
	{
#if NETFRAMEWORK
		public void TestGetHttpContextBase_NullContext()
		{
			var requestManager = new HttpRequestManager();

			AssertEquals("Precondition: HttpContext.Current should be null in unit tests.", null, HttpContext.Current);

			AssertNull(requestManager.GetHttpContextBase());
			AssertEquals("Http Context was null during login request.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetHttpContextBase()
		{
			var requestManager = new HttpRequestManager();

			var httpContext = new HttpContext(new HttpRequest(string.Empty, "https://www.google.com", string.Empty), new HttpResponse(new StringWriter()));
			HttpContext.Current = httpContext;
			AssertEquals("Precondition", httpContext, HttpContext.Current);

			var result = requestManager.GetHttpContextBase();
			AssertEquals("Equivalent Request should be returned.", httpContext.Request.Url, result.Request.Url);
			AssertEquals("No errors should be reported.", 0, ErrorReporter.TotalErrorCount);
		}

		protected override void TearDown()
		{
			base.TearDown();

			HttpContext.Current = null;
		}
#else
		public void TestGetHttpContextBase_NullContext()
		{
			var requestManager = new HttpRequestManager(null);

			AssertNull(requestManager.GetHttpContext());
			AssertEquals("HttpContext was null during request.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetHttpContextBase()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Scheme = "https";
			httpContext.Request.Host = new HostString("www.google.com");
			httpContext.Request.Path = "/";

			var requestManager = new HttpRequestManager(httpContext);

			var result = requestManager.GetHttpContext();
			var expectedUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";
			var resultUrl = $"{result.Request.Scheme}://{result.Request.Host}{result.Request.Path}";

			AssertEquals("Equivalent Request should be returned.", expectedUrl, resultUrl);
			AssertEquals("No errors should be reported.", 0, ErrorReporter.TotalErrorCount);
		}
#endif
	}
}
