using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LoginSupersededPageTest : TestCaseWithFactory
	{
		LoginSupersededForTest GetNewControl()
		{
			var testPage = new LoginSupersededForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		LoginSupersededForTest TestPage
		{
			get
			{
				if (testPage == null)
				{
					testPage = GetNewControl();
				}

				return testPage;
			}
		}

		LoginSupersededForTest testPage;
		public void TestValidContacts()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Precondition", TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should be visible", TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should be visible", TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should not be visible", !TestPage.ErrorMessageForTest.Visible);
		}

		public void TestInvalidTokenQueryString()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();
			var secureQueryString = new SecureQueryString { [LoginRouter.IdentityTokenQueryStringKey] = "dud" };
			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should not be visible", !TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should not be visible", !TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestInvalidQueryString()
		{
			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Email Verification Instructions should not be visible", !TestPage.SupersededInstructionsLabelForTest.Visible);
			Assert("Button should not be visible", !TestPage.SetMasterPasswordButtonForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestSetMasterPasswordButton()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://webtracker.com");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "shame@game.com";
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_Email = "blame@game.com";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_PER = contact.OC_PER;
			contact3.OC_Email = "tame@game.com";
			contact3.OC_WebAccessEnabled = true;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var returnUrl = "https://google.com/";
			var secureQueryString = new SecureQueryString { { LoginRouter.OriginalUrlQueryStringKey, returnUrl }, { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			TestPage.SetMasterPasswordButton_ClickForTest();
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertEquals("Parent should be the superseded contact", contact.PK, accessToken.SAT_ParentId);
			AssertEquals("Should be OrgContact", OrgContactSchema.Constants.Prefix, accessToken.SAT_ParentTableCode);
			AssertEquals("Scope should contain original url from query string", returnUrl, accessToken.SAT_Scope);
			AssertContains("Should be redirected to set master password", "SetMasterPassword", TestPage.Response.RedirectLocation);
		}

		#region Class for Test
		public class LoginSupersededForTest : LoginSuperseded
		{
			public LoginSupersededForTest()
			{
				SupersededInstructionsLabel = new ZTextLabel();
				SetMasterPasswordButton = new Button();
				ErrorMessage = new ZTextLabel();
			}

			public void OnLoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}

			public OrgContactSupersededHelper HelperForTest => Helper;
			public NameValueCollection RequestQueryString_Exposed => base.RequestQueryString;
			public void SetMasterPasswordButton_ClickForTest()
			{
				SetMasterPasswordButton_Click(null, EventArgs.Empty);
			}

			public ZTextLabel SupersededInstructionsLabelForTest => SupersededInstructionsLabel;
			public Button SetMasterPasswordButtonForTest => SetMasterPasswordButton;
			public ZTextLabel ErrorMessageForTest => ErrorMessage;
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
