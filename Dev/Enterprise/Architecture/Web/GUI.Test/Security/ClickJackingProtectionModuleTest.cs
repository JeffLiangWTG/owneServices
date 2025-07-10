using System.Collections.Specialized;
using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	[HttpContextEnabledTest]
	public class ClickJackingProtectionModuleTest : TestCase
	{
		public void TestOnPostRequestHandler_NothingIsDone_IfClickJackingProtectionIsDisabled()
		{
			protectionDisabled = true;
			Assert((bool)context.Session[ClickJackingProtectionModule.SessionKeyClickJackingProtectionDisabled]);

			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
			module.OnPostRequestHandlerExecute(context);
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
		}

		public void TestOnPostRequestHandler_NothingIsDone_IfBufferIsOff()
		{
			bufferOutput = false;
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);

			module.OnPostRequestHandlerExecute(context);
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
		}

		public void TestOnPostRequestHandlerWithEmptyHeader()
		{
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);

			module.OnPostRequestHandlerExecute(context);
			AssertEquals("X-Frame-Options:SAMEORIGIN added", "SAMEORIGIN", context.Response.Headers["x-frame-options"]);
		}

		public void TesOnPostRequestHandlerWithNoneXFrameOptionAdded()
		{
			context.Response.Headers.Add("Customize-XFrameOption", "NONE");
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
			AssertEquals("X-Frame-Options Header", "NONE", context.Response.Headers["Customize-XFrameOption"]);

			module.OnPostRequestHandlerExecute(context);
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
			AssertEquals("Customized response header removed", null, context.Response.Headers["Customize-XFrameOption"]);
		}

		public void TestOnPostRequestHandlerWithNoneXFrameOptionAddedWithExistingHeader()
		{
			context.Response.Headers.Add("Customize-XFrameOption", "DENY");
			AssertEquals("No X-Frame-Options Header", null, context.Response.Headers["x-frame-options"]);
			AssertEquals("X-Frame-Options Header:DENY", "DENY", context.Response.Headers["Customize-XFrameOption"]);

			module.OnPostRequestHandlerExecute(context);
			AssertEquals("X-Frame-Options:DENY added", "DENY", context.Response.Headers["x-frame-options"]);
			AssertEquals("Customized response header removed", null, context.Response.Headers["Customize-XFrameOption"]);
		}

		HttpContextBase GetContext()
		{
			var headers = new NameValueCollection();
			var mockResponse = new Mock<HttpResponseBase>();
			var mockContext = new Mock<HttpContextBase>();
			var mockSession = new Mock<HttpSessionStateBase>();

			mockResponse.SetupAllProperties();
			mockContext.SetupAllProperties();
			mockSession.SetupAllProperties();

			mockResponse.SetupGet(r => r.BufferOutput).Returns(() => bufferOutput);
			mockResponse.SetupGet(r => r.Headers).Returns(headers);
			mockResponse.Setup(r => r.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback((string name, string value) => headers.Add(name, value));
			mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);
			mockSession.SetupGet(s => s[ClickJackingProtectionModule.SessionKeyClickJackingProtectionDisabled]).Returns(() => protectionDisabled);
			mockContext.SetupGet(c => c.Session).Returns(mockSession.Object);

			return mockContext.Object;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			bufferOutput = true;
			context = GetContext();
			module = new ClickJackingProtectionModule();
		}

		HttpContextBase context;
		ClickJackingProtectionModule module;
		bool bufferOutput;
		bool protectionDisabled;

		#endregion
	}
}
