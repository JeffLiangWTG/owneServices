using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[HttpContextEnabledTest]
	sealed class ChooseCompanyManagerPageTest : ZPageLifeCycleTest
	{
		public void TestSetRememberMe()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AARDVARK";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "email@work.com";
			contact1.OC_OH = org.PK;
			var password = "ChangeMe123!";
			contact1.SetHashedPassword(password);
			Factory.Save();

			Factory.Save();

			var page = GetPageForTest();
			page.AppInstance.ApplicationCookie.WriteUser(string.Empty, contact1.OC_Email, password);
			var manager = new ChooseCompanyManager(page.AppInstance, new[] { contact1.PK.ToGuid() });

			var responseCookie = page.Response.Cookies[page.AppInstance.ApplicationCookie.CookieName];
			var initialCookieCount = page.Response.Cookies.Count;

			var encoder = new TwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
			AssertEquals("Should not be set since manager.RememberMe is false", string.Empty, encoder.Decrypt(responseCookie.Values["0"]));

			manager.SetRememberMe(contact1);
			page.Response.Redirect("http://google.com.au");

			responseCookie = page.Response.Cookies.Get(initialCookieCount + 1);
			AssertEquals("Should be set since manager.RememberMe is true", org.OH_Code, encoder.Decrypt(responseCookie.Values["0"]));
			AssertEquals("Should be set since manager.RememberMe is true", contact1.OC_Email, encoder.Decrypt(responseCookie.Values["1"]));
			AssertEquals("Should be set since manager.RememberMe is true", password, encoder.Decrypt(responseCookie.Values["2"]));
		}

		#region Implementation

		PageForTest GetPageForTest()
		{
			var page = new PageForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics",
						BindingFlags.NonPublic | BindingFlags.Instance,
						null,
						new Type[] { typeof(HttpContext) },
						null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class PageForTest : ZPage
		{
			public PageForTest()
			{
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : ZGlobal
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}

				public override string DefaultPage { get; }
			}
		}

		protected override ZPage GetNewPage()
		{
			return GetPageForTest();
		}

		#endregion
	}
}
