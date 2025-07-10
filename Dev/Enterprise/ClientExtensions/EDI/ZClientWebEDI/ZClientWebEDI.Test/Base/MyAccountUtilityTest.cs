using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class MyAccountUtilityTest : TestCaseWithFactory
	{
		public void TestGetRequestHostUrlUntilCalling()
		{
			var internalSafeUrlMethod = typeof(MyAccountUtility).GetMethod("IsSafeHost", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull(internalSafeUrlMethod);
			var isCalled = false;
			var predicate1 = () =>
			{
				isCalled = true;
				return "https://123.com";
			};

			internalSafeUrlMethod.Invoke(null, new object[] { predicate1, "http://wwww.cargowise.com/file/a.html", null });
			Assert("The predicate should be called because requestUrlHost is null or empty", isCalled);

			isCalled = false;
			internalSafeUrlMethod.Invoke(null, new object[] { predicate1, "http://wwww.cargowise.com/file/a.html", string.Empty });
			Assert("The predicate should be called because requestUrlHost is null or empty", isCalled);

			isCalled = false;
			internalSafeUrlMethod.Invoke(null, new object[] { predicate1, "http://wwww.cargowise.com/file/a.html", "http://dummy.com" });
			Assert("The predicate should not be called because requestUrlHost is not null or empty", !isCalled);

			AssertNull("No errors should occur", ErrorReporter.LastExceptionReported);

			isCalled = false;
			Func<string> predicate2 = () =>
			{
				isCalled = true;
				throw new Exception("DummyError");
			};

			var result = (bool)internalSafeUrlMethod.Invoke(null, new object[] { predicate2, "http://wwww.cargowise.com/file/a.html", null });
			Assert("http://wwww.cargowise.com/file/a.html is a valid URL", result);
			Assert("Predicate should be called", isCalled);
			AssertEquals("DummyError", ErrorReporter.LastExceptionReported.Message);
			AssertEquals("Could not get host URL from delegate parameter", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestIsSafeUrl()
		{
			AssertEquals(true, Page.IsSafeUrl("~/file/a.html"));
			AssertEquals(true, Page.IsSafeUrl("/file/a.html"));
			AssertEquals(false, Page.IsSafeUrl("http://wwww.abc.com/file/a.html"));
			AssertEquals(true, Page.IsSafeUrl("http://wwww.cargowise.com/file/a.html"));
			AssertEquals(true, Page.IsSafeUrl("http://wisetechacademy.com/file/a.html"));
		}

		public void TestGetLiteViewModeReturnUrl()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://127.0.0.1/"))
			{
				var url = Page.GetLiteViewModeReturnUrl("/myaccount/abc.htm?a=1");
				AssertEquals(EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.Value.Trim('/') + "/myaccount/abc.htm?a=1", url);
			}
		}

		public void TestGetLiteViewModeReturnUrl_RelativeURI_NoException()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				var address1 = new Uri("http://www.contoso.com/");
				var address2 = new Uri("http://www.contoso.com/index.htm?date=today");
				var relativeUri = address1.MakeRelativeUri(address2);
				var prop = Page.Request.GetType().GetField("_referrer", BindingFlags.NonPublic | BindingFlags.Instance);
				prop.SetValue(Page.Request, relativeUri);
				AssertEquals(false, Page.Request.UrlReferrer.IsAbsoluteUri);
				var url = Page.GetLiteViewModeReturnUrl("/myaccount/abc.htm?a=1&LoginRedirect=1");
				AssertEquals("/myaccount/abc.htm?a=1&LoginRedirect=1", url);
			}
		}

		public void TestGetLiteViewModeReturnUrl_ContainsResizeInlineFramePage()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				var url = Page.GetLiteViewModeReturnUrl("/myaccount/resize-iframe.html?id=abc");
				AssertContains("https://myaccount.com/Home.aspx", url);
			}
		}

		public void TestGetLiteViewModeReturnUrl_OpenRedirectAttack()
		{
			using (EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com/"))
			{
				var url = Page.GetLiteViewModeReturnUrl("www.supersafe.com");
				AssertEquals("https://myaccount.com/Home.aspx", url);
			}
		}

		BasePageForTest Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new BasePageForTest();
					var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
					method.Invoke(fPage, new object[] { HttpContext.Current });
				}

				return fPage;
			}
		}

		BasePageForTest fPage;
	}
}
