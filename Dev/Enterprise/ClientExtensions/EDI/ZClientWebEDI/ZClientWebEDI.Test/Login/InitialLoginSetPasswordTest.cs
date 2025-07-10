using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class InitialLoginSetPasswordTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return InitialLoginSetPasswordPage;
		}

		public void TestPageLoadAddressOverride()
		{
			var address = TestOrg.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Blah blah ooh la la";
			address.OA_Address1 = "Blah blah ooh la la";
			TestOrgContact.OC_OA_OrgAddress = address.PK;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(TestOrgContact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			AssertPageLoad(AssertOnLoadComplete_WithOrgAddressCompanyNameOverride);
		}

		void AssertOnLoadComplete_WithOrgAddressCompanyNameOverride(object sender, EventArgs e)
		{
			var testPage = (InitialLoginSetPasswordForTest)TestPage;
			AssertPasswordControlsVisible(testPage, true);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Exposed.Text);
			AssertNotEquals("You have been redirected here to set the initial password for your account; however, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.", testPage.SetPasswordInstructionsLabel_Exposed.Text);
			AssertNotEquals("You have been redirected here to set the initial password for your account; however, the contact's password has already been set. Please attempt login again and if this issue is recurring, contact your system administrator.", testPage.SetPasswordInstructionsLabel_Exposed.Text);
			AssertEquals(TestOrg.OH_Code, testPage.OrgCodeText_Exposed.Text);
			AssertEquals("Blah blah ooh la la", testPage.OrgNameText_Exposed.Text);
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler)
		{
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

		public void TestSetPassword()
		{
			using (var page = InitialLoginSetPasswordPage)
			{
				var token = LoginRouterIdentityManager.GenerateToken(TestOrgContact);
				var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
				var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
				page.OnLoad();
				page.NewPassword_Exposed.Text = "Th1sIsMyP4ssword!";
				page.NewPasswordConfirm_Exposed.Text = "2";
				page.OnUpdate();
				AssertEquals("Should not have updated password since confirmation did not match", true, TestOrgContact.Person.PER_PasswordHash.IsEmpty);
				page.NewPasswordConfirm_Exposed.Text = "Th1sIsMyP4ssword!";
				page.OnUpdate();
				TestOrgContact.Reload();
				Assert("Should set person password", TestOrgContact.VerifyPassword("Th1sIsMyP4ssword!"));
				Assert("Should set person password", TestOrgContact.Person.VerifyPassword("Th1sIsMyP4ssword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Exposed.Text);
			}
		}

		public void TestSetPassword_PasswordAlreadySet()
		{
			TestOrgContact.SetHashedPassword("44442");
			Factory.Save();
			using (var page = InitialLoginSetPasswordPage)
			{
				var token = LoginRouterIdentityManager.GenerateToken(TestOrgContact);
				var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
				var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
				page.OnLoad();
				AssertEquals("You have been redirected here to set the initial password for your account; however, your password has already been set. Please attempt login again and if this issue is recurring, contact your system administrator.", page.PasswordChangeMessage_Exposed.Text);
			}
		}

		public void TestSetPassword_PersonPasswordAlreadySet()
		{
			TestOrgContact.Person.SetHashedPassword("42412");
			Factory.Save();
			using (var page = InitialLoginSetPasswordPage)
			{
				var token = LoginRouterIdentityManager.GenerateToken(TestOrgContact);
				var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
				var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
				page.OnLoad();
				AssertEquals("You have been redirected here to set the initial password for your account; however, your password has already been set. Please attempt login again and if this issue is recurring, contact your system administrator.", page.PasswordChangeMessage_Exposed.Text);
			}
		}

		public void TestSetPassword_InvalidQueryString()
		{
			using (var page = InitialLoginSetPasswordPage)
			{
				var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, "dud" } };
				var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
				page.OnLoad();
				AssertEquals("You have been redirected here to set the initial password for your account; however, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.", page.PasswordChangeMessage_Exposed.Text);
			}
		}

		void AssertPasswordControlsVisible(InitialLoginSetPasswordForTest testPage, bool visible)
		{
			CombineAssertions(() =>
			{
				AssertEquals(visible, testPage.OrgCodeLabel_Exposed.Visible);
				AssertEquals(visible, testPage.OrgNameText_Exposed.Visible);
				AssertEquals(visible, testPage.OrgCodeText_Exposed.Visible);
				AssertEquals(visible, testPage.NewPassword_Exposed.Visible);
				AssertEquals(visible, testPage.NewPasswordConfirm_Exposed.Visible);
				AssertEquals(visible, testPage.UpdateButton_Exposed.Visible);
				AssertEquals(visible, testPage.PasswordChangeMessage_Exposed.Visible);
			});
		}

		class InitialLoginSetPasswordForTest : InitialLoginSetPassword
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			protected override Uri RequestUrl => new Uri("http://www.test.com/MyAccount/Login/InitialLoginSetPassword.aspx");
			public InitialLoginSetPasswordForTest()
			{
				PasswordChangeMessage_Exposed = new ZTextLabel();
				CopyrightYear = new ZTextLabel();
				OrgCodeLabel_Exposed = new ZTextLabel();
				OrgNameText_Exposed = new ZTextLabel();
				OrgCodeText_Exposed = new ZTextLabel();
				NewPassword_Exposed = new TextBox();
				NewPasswordConfirm_Exposed = new TextBox();
				UpdateButton_Exposed = new Button();
				SetPasswordHeadingLabel_Exposed = new ZTextLabel();
				SetPasswordInstructionsLabel_Exposed = new ZTextLabel();
			}

			public ZTextLabel OrgCodeLabel_Exposed { get => OrgCodeLabel; set => OrgCodeLabel = value; }

			public ZTextLabel OrgNameText_Exposed { get => OrgNameText; private set => OrgNameText = value; }

			public ZTextLabel OrgCodeText_Exposed { get => OrgCodeText; private set => OrgCodeText = value; }

			public ZTextLabel PasswordChangeMessage_Exposed { get => PasswordChangeMessage; private set => PasswordChangeMessage = value; }

			public Button UpdateButton_Exposed { get => Update; private set => Update = value; }

			public TextBox NewPassword_Exposed { get => NewPassword; private set => NewPassword = value; }

			public TextBox NewPasswordConfirm_Exposed { get => NewPasswordConfirm; private set => NewPasswordConfirm = value; }

			public ZTextLabel SetPasswordHeadingLabel_Exposed { get => SetPasswordHeadingLabel; private set => SetPasswordHeadingLabel = value; }

			public ZTextLabel SetPasswordInstructionsLabel_Exposed { get => SetPasswordInstructionsLabel; private set => SetPasswordInstructionsLabel = value; }

			public void OnUpdate()
			{
				base.Update_Click(this, EventArgs.Empty);
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		InitialLoginSetPasswordForTest InitialLoginSetPasswordPage
		{
			get
			{
				var testPage = new InitialLoginSetPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				testPage.AppInstance.SiteUser.Login("MEHMEH", "newuser@cargowise.com", CWSupportLoginToken.TokenForTest);
				return testPage;
			}
		}

		OrgHeader TestOrg;
		OrgContact TestOrgContact;
		protected override void SetUp()
		{
			base.SetUp();
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = "TESTORG";
			TestOrg.OH_FullName = "TEST ORGANIZATION";
			TestOrgContact = TestOrg.Contacts.AddNew();
			TestOrgContact.OC_Email = "testuser@cargowise.com";
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_IsActive = true;
			Factory.Save();
		}
	}
}
