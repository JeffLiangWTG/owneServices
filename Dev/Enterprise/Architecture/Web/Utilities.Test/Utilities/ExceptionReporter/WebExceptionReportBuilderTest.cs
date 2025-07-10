using System;
using System.IO;
using System.Reflection;
using System.Web;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	public class WebExceptionReportBuilderTest : ExceptionReportBuilderTest
	{
		public void TestExceptionDetails()
		{
			AssertNotNull("WebReportBuilder should not be null", WebReportBuilder);
			AssertEquals("WebReportBuilder should return Exception Details of type WebExceptionDetails", typeof(WebExceptionDetails), WebReportBuilder.GetExceptionDetailsForTesting(new Exception("Test Exception")).GetType());
		}

		public void TestReportContainsWebElements()
		{
			string attachment = WebReportBuilder.GenerateReport();
			Assert("Elements should be reported", attachment.IndexOf("RequestedURL") >= 0 &&
													attachment.IndexOf("ReferrerURL") >= 0 &&
													attachment.IndexOf("UserAgent") >= 0 &&
													attachment.IndexOf("UserHostAddress") >= 0 &&
													attachment.IndexOf("WebApplicationPath") >= 0 &&
													attachment.IndexOf("ScriptTimeout") >= 0 &&
													attachment.IndexOf("Session") >= 0);
		}

		#region Implementation

		HttpContext testContext;

		protected WebExceptionReportBuilder WebReportBuilder
		{
			get
			{
				if (webTestAttachmentBuilder == null)
				{
					Exception ex = new Exception("Exception");
					ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error ID", "Test Key", "Test Error");
					webTestAttachmentBuilder = new WebExceptionReportBuilder(reportArgs);
				}
				return webTestAttachmentBuilder;
			}
		}
		protected WebExceptionReportBuilder webTestAttachmentBuilder;

		protected override ExceptionBuilder TestBuilder
		{
			get { return WebReportBuilder; }
		}

		protected override void SetUp()
		{
			new HttpRuntime(); // Ensure static constructor has been run
			object staticRuntime = (typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
			typeof(HttpRuntime).GetField("_appDomainAppPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(staticRuntime, Env.TempPath);
			testContext = new HttpContext(new HttpRequest("", "http://localhost", ""), new HttpResponse(new StringWriter()));
			HttpContext.Current = testContext;
		}

		protected override void TearDown()
		{
			HttpContext.Current = null;
		}

		#endregion Implementation
	}
}
