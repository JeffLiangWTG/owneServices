using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class SetPasswordTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return SetPasswordPage;
		}

		public void TestPageLoad()
		{
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
			}
		}

		public void TestPageLoad_NoQueryString()
		{
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			AssertOnLoadComplete += AssertOnLoadComplete_WithInvalidToken;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				AssertOnLoadComplete -= AssertOnLoadComplete_WithInvalidToken;
			}
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler, string token)
		{
			var queryStringKey = "SetKey";
			HttpContext.Current.Request.QueryString.Remove(queryStringKey);
			HttpContext.Current.Request.QueryString.Add(queryStringKey, token);
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			this.AssertOnLoadComplete += onLoadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				this.AssertOnLoadComplete -= onLoadCompleteHandler;
			}
		}

		public void TestSetPassword()
		{
			using (var page = SetPasswordPage)
			{
				var queryStringKey = "SetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyP4ssword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out var accessToken));
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyP4ssword!";
				page.OnUpdate();
				Assert(TestOrgContact.VerifyPassword("Th1sIsMyP4ssword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("SuccessMessage", page.PasswordChangeMessage_Expose.CssClass);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.SetPassword, out accessToken));
				page.OnLoad();
				AssertEquals("The set link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (SetPasswordForTest)this.TestPage;
			AssertPasswordControlsVisible(testPage, false);
			AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
			AssertEquals("", testPage.SetPasswordHeadingLabel_Expose.Text);
			AssertEquals("", testPage.OrgCodeText_Expose.Text);
			AssertEquals("", testPage.OrgNameText_Expose.Text);
			AssertEquals("User context should not be set", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("User context should not be set", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("User context should not be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (SetPasswordForTest)this.TestPage;
			AssertPasswordControlsVisible(testPage, true);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
			AssertEquals(TestOrg.OH_Code, testPage.OrgCodeText_Expose.Text);
			AssertEquals(TestOrg.OH_FullName, testPage.OrgNameText_Expose.Text);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
		}

		void AssertPasswordControlsVisible(SetPasswordForTest testPage, bool visible)
		{
			CombineAssertions(() =>
			{
				AssertEquals(visible, testPage.NewPasswordConfirm_Expose.Visible);
				AssertEquals(visible, testPage.NewPassword_Expose.Visible);
				AssertEquals(visible, testPage.UpdateButton_Expose.Visible);
				AssertEquals(visible, testPage.PasswordChangeRequirementsControl_Expose.Visible);
			});
		}

		public void TestWebUser()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			CreateOrgContact(org, "Test User A", false, false);
			CreateOrgContact(org, "Test User B", false, false);
			AssertWebUser(ZString.Empty);
			CreateOrgContact(org, "Test User C", false, true);
			CreateOrgContact(org, "Test User D", false, true);
			AssertWebUser(ZString.Empty);
			CreateOrgContact(org, "Test User E", true, false);
			CreateOrgContact(org, "Test User F", true, false);
			AssertWebUser(ZString.Empty);
			var contact = CreateOrgContact(org, "Test User G", true, true);
			Factory.Save();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo("", contact.PK.ToGuid(), "OC"), maxUses: 1);
			AssertWebUser("Test User G");
		}

		public void TestSetPassword_CompanySpecific()
		{
			// we can send non-master password mail only when it is not the only contact in the person relationship
			var contact2 = TestOrg.Contacts.AddNew();
			contact2.OC_ContactName = "Test User 2";
			contact2.OC_Email = "user2@cargowise.com";
			contact2.OC_PER = TestOrgContact.Person.PK;
			Factory.Save();

			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordSetSuccessfullyEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var resetInfo = new PasswordResetInfo()
			{ ContactEmail = TestOrgContact.Email, OrgCode = TestOrgContact.OrgCode, EmailTemplateCompanyPk = companyPk.ToString() };
			var jsonScope = JsonConvert.SerializeObject(resetInfo);
			var companyToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo(jsonScope, TestOrgContact.PK.ToGuid(), "INV"), maxUses: 1);
			using (var page = SetPasswordPage)
			{
				var queryStringKey = "SetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, companyToken);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyP4ssword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyP4ssword!";
				page.OnUpdate();
				var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(email.Subject, "Subject Company");
				Assert(email.Body.Contains("Test Email Body Template Company"));
			}
		}

		void AssertWebUser(string expectedContactName)
		{
			using (var page = SetPasswordPage)
			{
				var queryStringKey = "SetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.OnLoad();
				AssertEquals(expectedContactName, page.WebContact?.OC_ContactName);
			}
		}

		OrgContact CreateOrgContact(OrgHeader org, string contactName, bool isActive, bool isWebAccessEnabled)
		{
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			contact.OC_IsActive = isActive;
			contact.OC_WebAccessEnabled = isWebAccessEnabled;
			contact.OC_Email = "testuser@cargowise.com";
			contact.OC_ContactName = contactName;
			Factory.Save();
			return contact;
		}

		class SetPasswordForTest : SetPassword
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new Global();
			}

			protected override Uri RequestUrl => new Uri("http://www.test.com/MyAccount/Admin/SetPassword.aspx");

			public SetPasswordForTest()
			{
				PasswordChangeMessage_Expose = new ZTextLabel();
				CopyRightYear_Expose = new ZTextLabel();
				SetPasswordInstructionsLabel_Expose = new ZTextLabel();
				OrgCodeLabel_Expose = new ZTextLabel();
				OrgCodeText_Expose = new ZTextLabel();
				OrgNameText_Expose = new ZTextLabel();
				OrgNameText_Expose = new ZTextLabel();
				NewPassword_Expose = new TextBox();
				NewPasswordConfirm_Expose = new TextBox();
				UpdateButton_Expose = new Button();
				GoBackMessage_Expose = new HtmlGenericControl();
				SetPasswordHeadingLabel_Expose = new ZTextLabel();
				passwordChangeRequirements = new PasswordChangeRequirementsControl();
			}

			public NameValueCollection RequestQueryString_Expose => base.RequestQueryString;

			ZTextLabel SetPasswordInstructionsLabel_Expose { set => SetPasswordInstructionsLabel = value; }

			ZTextLabel OrgCodeLabel_Expose { set => OrgCodeLabel = value; }

			public ZTextLabel OrgCodeText_Expose { get => OrgCodeText; private set => OrgCodeText = value; }

			public ZTextLabel OrgNameText_Expose { get => OrgNameText; private set => OrgNameText = value; }

			public ZTextLabel PasswordChangeMessage_Expose { get => PasswordChangeMessage; private set => PasswordChangeMessage = value; }

			ZTextLabel CopyRightYear_Expose { set => CopyrightYear = value; }

			public Button UpdateButton_Expose { get => Update; private set => Update = value; }

			public TextBox NewPassword_Expose { get => NewPassword; private set => NewPassword = value; }

			public TextBox NewPasswordConfirm_Expose { get => NewPasswordConfirm; private set => NewPasswordConfirm = value; }

			public HtmlGenericControl GoBackMessage_Expose { get => GoBackMessage; private set => GoBackMessage = value; }

			public ZTextLabel SetPasswordHeadingLabel_Expose { get => SetPasswordHeadingLabel; private set => SetPasswordHeadingLabel = value; }

			public PasswordChangeRequirementsControl PasswordChangeRequirementsControl_Expose => passwordChangeRequirements;

			public void OnUpdate()
			{
				base.Update_Click(this, EventArgs.Empty);
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		SetPasswordForTest SetPasswordPage
		{
			get
			{
				var testPage = new SetPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		OrgHeader TestOrg;
		OrgContact TestOrgContact;
		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			TestToken = "testToken";
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = "TESTORG";

			TestOrgContact = TestOrg.Contacts.AddNew();
			TestOrgContact.OC_Email = "testuser@cargowise.com";
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_IsActive = true;

			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo("", TestOrgContact.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.SetPassword, new AccessTokenInfo("", TestOrgContact.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}
	}
}
