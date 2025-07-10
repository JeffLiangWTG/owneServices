using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class SetMasterPasswordTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return SetMasterPasswordPage;
		}

		public void TestPageLoad()
		{
			AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
		}

		public void TestPageLoadTokenExpired()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
		}

		public void TestPageLoadTokenInvalidType()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
		}

		public void TestPageLoadTokenInvalidToken()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
		}

		public void TestPageLoadTokenEmptyToken()
		{
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler, string token)
		{
			HttpContext.Current.Request.QueryString.Remove(SecureQueryString.QueryStringKey);
			var passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, token } }.ToString());
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			AssertOnLoadComplete += onLoadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				AssertOnLoadComplete -= onLoadCompleteHandler;
			}
		}

		public void TestPageLoadNonSecureToken()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();

			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(WebUserAdminManager.SetMasterPasswordKey);
				HttpContext.Current.Request.QueryString.Add(WebUserAdminManager.SetMasterPasswordKey, TestToken_Expired);
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}

			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(WebUserAdminManager.SetMasterPasswordKey);
				HttpContext.Current.Request.QueryString.Add(WebUserAdminManager.SetMasterPasswordKey, TestToken);
				page.OnLoad();
				Assert(page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(page.ContactsBox_Expose.Visible);
				Assert(page.NewPassword_Expose.Visible);
				Assert(page.NewPasswordConfirm_Expose.Visible);
				Assert(page.Update_Expose.Visible);
				Assert(page.PasswordChangeRequirementsControl_Expose.Visible);
				AssertNullOrEmpty(page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = SetMasterPasswordPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";

				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, "invalidToken" } }.ToString());
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
				page.OnUpdate();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } }.ToString());
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("SuccessMessage", page.PasswordChangeMessage_Expose.CssClass);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertStartsWith("Should have redirected", "/webapp/Login/", page.Response.RedirectLocation);
				var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
				AssertEquals("Should have included original redirect url", RedirectUrl, originalUrl);
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_NoRedirectUrl()
		{
			using (var page = SetMasterPasswordPage)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";
				TestOrgContact = orgHeader.Contacts.AddNew();
				TestOrgContact.OC_Email = "TestUser@tekdekz.com";
				TestOrgContact.OC_IsActive = true;
				TestOrgContact.OC_WebAccessEnabled = true;
				Factory.Save();
				TestOrgContact.Person.PER_FullName = "Pranky";
				Factory.Save();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(string.Empty, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, testToken } }.ToString());
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				AssertEquals("Precondition: Should not be logged in", false, page.SiteUser.IsLoggedIn);
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(testToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertStartsWith("Should have redirected", "/webapp/Login/", page.Response.RedirectLocation);
				var uriDeconstructor = new UriDeconstructor(new Uri(page.Response.RedirectLocation, UriKind.Relative));
				var queryDictionary = HttpUtility.ParseQueryString(uriDeconstructor.Query);
				var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(queryDictionary[LoginRouter.QueryStringKey]));
				var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
				AssertEquals("Should not have an original redirect url", null, originalUrl);
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_AlreadyLoggedIn()
		{
			using (var page = SetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, TestToken } }.ToString());
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertStartsWith("Should have redirected", RedirectUrl, page.Response.RedirectLocation);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestSetMasterPassword_AlreadyLoggedIn_NoRedirectUrl()
		{
			using (var page = SetMasterPasswordPage)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "TechDeckz";
				orgHeader.OH_FullName = "Tech Decks";
				TestOrgContact = orgHeader.Contacts.AddNew();
				TestOrgContact.OC_Email = "TestUser@tekdekz.com";
				TestOrgContact.OC_IsActive = true;
				TestOrgContact.OC_WebAccessEnabled = true;
				TestOrgContact.SetHashedPassword("1234");
				Factory.Save();
				TestOrgContact.Person.PER_FullName = "Pranky";
				Factory.Save();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(string.Empty, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
				page.RequestQueryString_Expose.Remove(SecureQueryString.QueryStringKey);
				var passwordSecureQueryString = WebUtility.UrlEncode(new SecureQueryString { { WebUserAdminManager.SetMasterPasswordKey, testToken } }.ToString());
				HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, passwordSecureQueryString);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.SiteUser.Login(TestOrgContact.OrgCode, TestOrgContact.OC_Email, "1234");
				AssertEquals("Precondition: Should be logged in", true, page.SiteUser.IsLoggedIn);
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(testToken, AccessTokenTypes.SetMasterPassword, out _));
				AssertEquals("Should not have redirected", null, page.Response.RedirectLocation);
				AssertEquals("Should remain logged in", true, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (SetMasterPasswordForTest)this.TestPage;
			Assert(testPage.SetMasterPasswordHeadingLabel_Expose.Visible);
			Assert(testPage.ContactsBox_Expose.Visible);
			Assert(testPage.NewPassword_Expose.Visible);
			Assert(testPage.NewPasswordConfirm_Expose.Visible);
			Assert(testPage.Update_Expose.Visible);
			Assert(testPage.PasswordChangeRequirementsControl_Expose.Visible);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (SetMasterPasswordForTest)TestPage;
			CombineAssertions(() =>
			{
				Assert("Should not see set password controls", !testPage.SetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Should not see set password controls", !testPage.ContactsBox_Expose.Visible);
				Assert("Should not see set password controls", !testPage.NewPassword_Expose.Visible);
				Assert("Should not see set password controls", !testPage.NewPasswordConfirm_Expose.Visible);
				Assert("Should not see set password controls", !testPage.Update_Expose.Visible);
				Assert("Should not see set password controls", !testPage.PasswordChangeRequirementsControl_Expose.Visible);
				AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
				AssertEquals(string.Empty, testPage.SetMasterPasswordHeadingLabel_Expose.Text);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("User context should not be set", Guid.Empty, Env.CurrentDepartmentPK);
			});
		}

		class SetMasterPasswordForTest : SetMasterPassword
		{
			protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/SetMasterPassword.aspx");

			public SetMasterPasswordForTest()
			{
				PasswordChangeMessage = new ZTextLabel();
				ContactsBox = new HtmlGenericControl();
				NewPassword = new TextBox();
				NewPasswordConfirm = new TextBox();
				SetMasterPasswordHeadingLabel = new ZTextLabel();
				Update = new Button();
				passwordChangeRequirements = new PasswordChangeRequirementsControl();
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public NameValueCollection RequestQueryString_Expose => base.RequestQueryString;
			public Label PasswordChangeMessage_Expose => PasswordChangeMessage;
			public HtmlGenericControl ContactsBox_Expose => ContactsBox;
			public TextBox NewPassword_Expose => NewPassword;
			public TextBox NewPasswordConfirm_Expose => NewPasswordConfirm;
			public Button Update_Expose => Update;
			public ZTextLabel SetMasterPasswordHeadingLabel_Expose => SetMasterPasswordHeadingLabel;
			public PasswordChangeRequirementsControl PasswordChangeRequirementsControl_Expose => passwordChangeRequirements;

			public void OnUpdate()
			{
				Update_Click(this, EventArgs.Empty);
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

		SetMasterPasswordForTest SetMasterPasswordPage
		{
			get
			{
				var testPage = new SetMasterPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		OrgContact TestOrgContact;
		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		const string RedirectUrl = "http://google.com.au/";
		protected override void SetUp()
		{
			base.SetUp();
			TestToken = "testToken";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";
			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.SetHashedPassword("1234");
			Factory.Save();
			TestOrgContact.Person.PER_FullName = "Franky";
			Factory.Save();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(RedirectUrl, TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.SetMasterPassword, new AccessTokenInfo(RedirectUrl, TestOrgContact.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}
	}
}
