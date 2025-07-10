using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions.Testing
{
	internal class WebExceptionDetailsTest : ExceptionDetailsTest
	{
		public void TestCleanXmlTag()
		{
			AssertEquals("_4", WebExceptionDetails.CleanXmlTag("4"));
			AssertEquals("___", WebExceptionDetails.CleanXmlTag("$.#x7F"));
		}

		public void TestWriteWebInfo()
		{
			AssertEquals("Precondition", HttpContext.Current, null);
			var testDetails = new WebExceptionDetailsForTest(new Exception());
			var strWriter = new StringWriter();
			testDetails.WriteWebInfoForTest(new XmlTextWriter(strWriter));
			AssertEquals("Should not report anything", strWriter.GetStringBuilder().ToString(), "");

			var testContext = new HttpContext(new HttpRequest(string.Empty, "http://localhost", string.Empty), new HttpResponse(new StringWriter()));
			try
			{
				HttpContext.Current = testContext;
				strWriter = new StringWriter();
				testDetails.WriteWebInfoForTest(new XmlTextWriter(strWriter));
				var info = strWriter.GetStringBuilder().ToString();
				Assert("Elements should be reported", info.IndexOf("<WebInfo>") >= 0 &&
													  info.IndexOf("</WebInfo>") >= 0 &&
													  info.IndexOf("RequestedURL") >= 0 &&
													  info.IndexOf("ReferrerURL") >= 0 &&
													  info.IndexOf("UserAgent") >= 0 &&
													  info.IndexOf("UserHostAddress") >= 0 &&
													  info.IndexOf("WebApplicationPath") >= 0 &&
													  info.IndexOf("ScriptTimeout") >= 0);
				Assert("Session should be reported to be null", info.IndexOf(@"<SessionIsNull>true</SessionIsNull>") >= 0);

				var mock = new Mock<IHttpSessionState>(MockBehavior.Strict);
				mock.Setup(m => m.SessionID)
					.Returns("DummySessionID");
				mock.Setup(m => m.IsNewSession)
					.Returns(true);
				mock.Setup(m => m.Timeout)
					.Returns(42);

				var session = (HttpSessionState)typeof(HttpSessionState).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(new object[] { mock.Object });

				testContext.Items["AspSession"] = session;

				strWriter = new StringWriter();
				testDetails.WriteWebInfoForTest(new XmlTextWriter(strWriter));
				info = strWriter.GetStringBuilder().ToString();

				mock.VerifyAll();

				Assert("Session ID", info.IndexOf(@"<SessionID>DummySessionID</SessionID>") >= 0);
				Assert("Timeout", info.IndexOf(@"<SessionTimeout>42</SessionTimeout>") >= 0);
				Assert("Is new?", info.IndexOf(@"<SessionIsNew>True</SessionIsNew>") >= 0);
				Assert("Should not say session is null", info.IndexOf("Session is null") < 0);
			}
			finally
			{
				HttpContext.Current = null;
			}
		}
	}
}
