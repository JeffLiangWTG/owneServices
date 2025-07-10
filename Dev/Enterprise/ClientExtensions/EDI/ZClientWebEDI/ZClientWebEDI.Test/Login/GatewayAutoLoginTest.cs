using System;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class GatewayAutoLoginTest : TestCaseWithFactory
	{
		public void TestPageLoad_SiteUserNotLoggedIn()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "howdy@partner.com";
			contact1.OC_ContactName = "cowboy";
			contact1.SetHashedPassword("1234");
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = "alien@1.com";
			contact2.OC_ContactName = "alien";
			contact2.SetHashedPassword("1234");
			Factory.Save();
			var callback = "ab://callback/here";
			var page = GetPageForTest();
			page.Request.QueryString.Add("redirect_uri", callback);
			page.Request.QueryString.Add("state", "anyState");
			page.DoPageLoad();
			AssertStartsWith("Site user was not logged in", page.AppInstance.ErrorPage, page.Response.RedirectLocation);
		}

		public void TestPageLoad_InvalidRedirectUri()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "howdy@partner.com";
			contact.OC_ContactName = "cowboy";
			contact.SetHashedPassword("1234");
			Factory.Save();
			var callback = "Now Where Did I Put That Absolute Uri?";
			var page = GetPageForTest();
			page.Request.QueryString.Add("redirect_uri", callback);
			page.Request.QueryString.Add("state", "anyState");
			page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
			page.DoPageLoad();
			AssertStartsWith("redirect_uri is invalid", page.AppInstance.ErrorPage, page.Response.RedirectLocation);
		}

		public void TestPageLoad_InvalidState()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "howdy@partner.com";
			contact.OC_ContactName = "cowboy";
			contact.SetHashedPassword("1234");
			Factory.Save();
			var callback = "ab://callback/here";
			var page = GetPageForTest();
			page.Request.QueryString.Add("redirect_uri", callback);
			page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
			page.DoPageLoad();
			AssertStartsWith("state is invalid", page.AppInstance.ErrorPage, page.Response.RedirectLocation);
		}

		public void TestPageLoad_ShouldRedirect()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "howdy@partner.com";
			contact.OC_ContactName = "cowboy";
			contact.SetHashedPassword("1234");
			Factory.Save();
			string token;
			var callback = "ab://callback/here";
			var page = GetPageForTest();
			page.Request.QueryString.Add("redirect_uri", callback);
			page.Request.QueryString.Add("state", "anyState");
			page.SiteUser.LoginForTest(contact.OrgCode, contact.OC_Email, "1234");
			page.DoPageLoad();
			var queryParameters = new Uri(page.Response.RedirectLocation).ParseQueryString();
			token = queryParameters["code"];
			AssertEquals("ab://callback/here?code=" + Uri.EscapeDataString(token) + "&state=anyState", page.Response.RedirectLocation);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var result = accessControl.TryConsume(token, AccessTokenTypes.MyAccountGlowIdentity, out var info);
			AssertEquals(true, result);
			AssertEquals(string.Empty, info.Scope);
			AssertEquals(contact.PK, info.ParentId);
			AssertEquals(OrgContactSchema.Constants.Prefix, info.ParentTableCode);
			result = accessControl.TryConsume(token, AccessTokenTypes.MyAccountGlowIdentity, out _);
			AssertEquals("Token should only be valid for one use.", false, result);
		}

		#region Implementation
		GatewayLoginForTest GetPageForTest()
		{
			var page = new GatewayLoginForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class GatewayLoginForTest : GatewayLogin
		{
			public void DoPageLoad()
			{
				try
				{
					base.Page_Load(null, EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException || ex is NullReferenceException)
					{
						throw;
					}
				}
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}
		#endregion
	}
}
