using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class ResetPasswordTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return ResetPasswordPage;
		}

		public void TestPageLoad()
		{
			AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
			AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
		}

		public void TestPageLoad_NoQueryString()
		{
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

		void AssertPageLoad(EventHandler onLOadCompleteHandler, string token)
		{
			var queryStringKey = "ResetKey";
			HttpContext.Current.Request.QueryString.Remove(queryStringKey);
			HttpContext.Current.Request.QueryString.Add(queryStringKey, token);
			this.AssertOnLoadComplete += onLOadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				this.AssertOnLoadComplete -= onLOadCompleteHandler;
			}
		}

		public void TestBorderWiseResetPassword_PageLoad()
		{
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken_BorderWise);
				page.OnInit();
				page.OnLoad();
				AssertEquals("http://app.borderwise.com/", page.GoBackLoginLink_Expose.NavigateUrl);
				AssertEquals(false, page.BackLink_Expose.Visible);
			}
		}

		public void TestBorderWiseResetPassword_Success()
		{
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken_BorderWise);
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				EDIDataRegistry.Instance.BorderWiseUmpApiAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://ump.borderwise.com/api/v1/ump/");
				page.HttpClientHandlerMock = GetHttpClientHandlerForTesting(HttpStatusCode.OK, "Ok");
				page.OnUpdate();
				Assert(TestOrgContact.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("SuccessMessage", page.PasswordChangeMessage_Expose.CssClass);
				AssertEquals(false, page.OrgCodeDownList_Expose.Visible);
				AssertEquals(false, page.NewPassword_Expose.Visible);
				AssertEquals(false, page.NewPasswordConfirm_Expose.Visible);
				AssertEquals(false, page.UpdateButton_Expose.Visible);
				AssertEquals(true, page.GoBackMessage_Expose.Visible);
				AssertEquals(false, page.PasswordChangeRequirementsControl_Expose.Visible);
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(!accessControl.TryPeek(TestToken_BorderWise, AccessTokenTypes.ResetPassword, out _));
			}
		}

		HttpClientHandler GetHttpClientHandlerForTesting(HttpStatusCode httpStatusCode, string content)
		{
			return new HttpClientHandlerMock(new HttpResponseMessage(httpStatusCode)
			{
				Content = new StringContent(content)
				{ Headers = { ContentType = new MediaTypeHeaderValue("application/json") } }
			});
		}

		class HttpClientHandlerMock : HttpClientHandler
		{
			readonly HttpResponseMessage response;
			public HttpClientHandlerMock(HttpResponseMessage response)
			{
				this.response = response;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(response);
			}
		}

		public void TestResetPassword()
		{
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				page.OnLoad();
				EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				page.HttpClientHandlerMock = GetHttpClientHandlerForTesting(HttpStatusCode.OK, "Ok");
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetPassword, out var accessToken));
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				Assert(TestOrgContact.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals(false, page.OrgCodeDownList_Expose.Visible);
				AssertEquals(false, page.NewPassword_Expose.Visible);
				AssertEquals(false, page.NewPasswordConfirm_Expose.Visible);
				AssertEquals(false, page.UpdateButton_Expose.Visible);
				AssertEquals(false, page.PasswordChangeRequirementsControl_Expose.Visible);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.ResetPassword, out accessToken));
			}

			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.NewPassword_Expose.Text = "1";
				page.NewPasswordConfirm_Expose.Text = "1";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				AssertNoExceptionThrown(() => page.OnUpdate());
				AssertEquals("The reset link you have followed is invalid or expired.", page.PasswordChangeMessage_Expose.Text);
			}
		}

		public void TestResetPassword_CompanySpecific()
		{
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "MLAH", EmailBody = "WAH" });
			WebDataRegistry.Instance.PasswordResetSuccessfullyEmailTemplate.SetValue(companyPk, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "Subject Company", EmailBody = "Test Email Body Template Company" });
			var accessControl = new TokenizedAccessControl();
			var resetInfo = new PasswordResetInfo()
			{ ContactEmail = TestOrgContact.Email, OrgCode = TestOrgContact.OrgCode, EmailTemplateCompanyPk = companyPk.ToString() };
			var jsonScope = JsonConvert.SerializeObject(resetInfo);
			var companyToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(jsonScope, Guid.Empty, "INV"), maxUses: 1);
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, companyToken);
				page.HttpClientHandlerMock = GetHttpClientHandlerForTesting(HttpStatusCode.OK, "Ok");
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				page.OnLoad();
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(email.Subject, "Subject Company");
				Assert(email.Body.Contains("Test Email Body Template Company"));
			}
		}

		public void TestResetPassword_UnlockContact()
		{
			var otherOrg = "OTHERORG";
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(string.Empty, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(otherOrg, TestOrgContact.OC_Email);
			Factory.Save();
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem(TestOrgContact.OrgCode, TestOrgContact.PK.ToString()));
				EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				page.HttpClientHandlerMock = GetHttpClientHandlerForTesting(HttpStatusCode.OK, "Ok");
				page.OnLoad();
				var loginOptionsHelper = (LoginOptionsHelper)page.DataSource;
				var hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
				AssertEquals("Precondition", null, hash);
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, hash));
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
				AssertEquals("Should load hash from cookie", 32, hash.Length);
				Assert("Should unlock account after changing password", !ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Should not unlock account without hash", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, null));
				Assert("Should not unlock account with same email but no org code", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, null));
				Assert("Should not unlock account with different org code", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, null));
			}
		}

		#region Unlock Contact Helper
		void CreateLockoutUserRecord(string companyCode, string loginName)
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = string.IsNullOrEmpty(companyCode) ? loginName : (loginName + " " + companyCode);
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		bool ContactIsLockedOut(string companyCode, string loginName, byte[] hash)
		{
			return LoginAttemptRecorder.IsLockedOut(companyCode, loginName, hash);
		}

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;
		#endregion
		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (ResetPasswordForTest)this.TestPage;
			AssertPasswordControlsVisible(testPage, false);
			AssertEquals("The reset link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
		}

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (ResetPasswordForTest)this.TestPage;
			AssertPasswordControlsVisible(testPage, true);
			AssertEquals(1, testPage.OrgCodeDownList_Expose.Items.Count);
			AssertEquals(testPage.OrgCodeDownList_Expose.SelectedItem.Text, "TESTORSYD - TESTORG LTD.");
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
		}

		void AssertPasswordControlsVisible(ResetPasswordForTest testPage, bool visible)
		{
			CombineAssertions(() =>
			{
				AssertEquals(visible, testPage.OrgCodeDownList_Expose.Visible);
				AssertEquals(visible, testPage.NewPasswordConfirm_Expose.Visible);
				AssertEquals(visible, testPage.NewPassword_Expose.Visible);
				AssertEquals(visible, testPage.UpdateButton_Expose.Visible);
				AssertEquals(visible, testPage.BackHyperLink_Expose.Visible);
				AssertEquals(visible, testPage.PasswordChangeRequirementsControl_Expose.Visible);
			});
		}

		public void TestOrgCodeDropDownList()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH ORG";
			CreateOrgContact(org, "Test User A", false, false);
			CreateOrgContact(org, "Test User C", false, true);
			CreateOrgContact(org, "Test User E", true, false);
			var contact1 = CreateOrgContact(org, "Test User G", true, true);
			Factory.Save();
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.OnInit();
				page.OnLoad();
				var items = page.OrgCodeDownList_Expose.Items.OfType<ListItem>().OrderBy(x => x.Text).ToArray();
				AssertEquals(2, items.Length);
				AssertEquals("MEHMEH - MEHMEH ORG", items[0].Text);
				AssertEquals(contact1.PK.ToString(), items[0].Value);
				AssertEquals("TESTORSYD - TESTORG LTD.", items[1].Text);
				AssertEquals(TestOrgContact.PK.ToString(), items[1].Value);
				page.OrgCodeDownList_Expose.SelectedIndex = 1;
				AssertEquals(TestOrgContact.OC_ContactName, page.GetSelectedWebUser()?.OC_ContactName);
				page.OrgCodeDownList_Expose.SelectedIndex = 0;
				AssertEquals(contact1.OC_ContactName, page.GetSelectedWebUser()?.OC_ContactName);
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
			return contact;
		}

		public void TestUnlinkedUserAccount()
		{
			using (var page = ResetPasswordPage)
			{
				AssertNull(TestOrgContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				page.OrgCodeDownList_Expose.Text = "TESTORG";
				page.OnLoad();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(true, page.PasswordDiv_Expose.Visible);
				AssertEquals(false, page.LoginOptionsDiv_Expose.Visible);
			}

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = Database.PK;
			userAccount.EUA_UserID = "U02";
			userAccount.EUA_FullName = "User 2";
			userAccount.EUA_Email = "u2@cw1.com";
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			userAccount.EUA_OC_WebAccessContact = TestOrgContact.PK;
			Factory.Save();
			using (var page = ResetPasswordPage)
			{
				AssertEquals(userAccount, TestOrgContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				page.OrgCodeDownList_Expose.Text = "TESTORG";
				page.OnLoad();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(false, page.PasswordDiv_Expose.Visible);
				AssertEquals(true, page.LoginOptionsDiv_Expose.Visible);
				var helper = (LoginOptionsHelper)page.DataSource;
				AssertEquals(TestOrgContact.PK, helper.Contact.PK);
				AssertEquals(userAccount.PK, helper.UserAccount.PK);
			}
		}

		public void TestUnlinkedUserAccount_SkipLoginOptions()
		{
			var enterpriseCode = "ENT";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "COM", "SRV", true);
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";
			var productionDatabase = licence.Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;
			var testDatabase1 = licenceEnterprise.Databases.AddNew();
			testDatabase1.LD_ServerCode = "TD1";
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var newOrg = licence.Company.Header;
			newOrg.Contacts.RemoveAndDeleteAll();
			var existingContact = newOrg.Contacts.AddNew() as EDIOrgContact;
			existingContact.OC_ContactName = "Existing One";
			existingContact.OC_Email = "existing1@cargowise.com";
			existingContact.OC_IsActive = existingContact.OC_WebAccessEnabled = true;
			var existingUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			existingUserAccount.EUA_LD = productionDatabase.PK;
			existingUserAccount.EUA_UserID = "US2";
			existingUserAccount.EUA_FullName = "Existing One";
			existingUserAccount.EUA_Email = "existing1@cargowise.com";
			existingUserAccount.EUA_OC_WebAccessContact = existingContact.PK;
			var testUserWithInactiveRelationship = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			testUserWithInactiveRelationship.EUA_LD = testDatabase1.PK;
			testUserWithInactiveRelationship.EUA_UserID = "US1";
			testUserWithInactiveRelationship.EUA_Email = existingContact.OC_Email;
			testUserWithInactiveRelationship.EUA_OC_WebAccessContact = existingContact.PK;
			testUserWithInactiveRelationship.EUA_IsEmailVerificationRequired = false;
			testUserWithInactiveRelationship.EUA_IsContactRelationshipActive = false;
			testUserWithInactiveRelationship.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			Factory.Save();
			AssertEquals(false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(existingUserAccount));
			AssertEquals(true, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(testUserWithInactiveRelationship));
			var accessControl = new TokenizedAccessControl();
			var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(existingContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			using (var page = ResetPasswordPage)
			{
				AssertEquals(testUserWithInactiveRelationship, existingContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, testToken);
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("User 1", existingContact.PK.ToString()));
				page.OrgCodeDownList_Expose.Text = "User 1";
				page.OnLoad();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(true, page.PasswordDiv_Expose.Visible);
				AssertEquals(false, page.LoginOptionsDiv_Expose.Visible);
			}

			existingContact.SetHashedPassword("123");
			Factory.Save();
			AssertEquals(false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(existingUserAccount));
			AssertEquals(false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(testUserWithInactiveRelationship));
			using (var page = ResetPasswordPage)
			{
				AssertEquals(testUserWithInactiveRelationship, existingContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, testToken);
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("User 1", existingContact.PK.ToString()));
				page.OrgCodeDownList_Expose.Text = "User 1";
				page.OnLoad();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(false, page.PasswordDiv_Expose.Visible);
				AssertEquals(true, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_SelfDeactivation()
		{
			var enterpriseCode = "ENT";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "COM", "SRV", true);
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";
			var productionDatabase = licence.Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;
			var testDatabase1 = licenceEnterprise.Databases.AddNew();
			testDatabase1.LD_ServerCode = "TD1";
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var newOrg = licence.Company.Header;
			newOrg.Contacts.RemoveAndDeleteAll();
			var existingContact = newOrg.Contacts.AddNew() as EDIOrgContact;
			existingContact.OC_ContactName = "Existing One";
			existingContact.OC_Email = "existing1@cargowise.com";
			existingContact.OC_IsActive = existingContact.OC_WebAccessEnabled = true;
			existingContact.SetHashedPassword("1234");
			var existingUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			existingUserAccount.EUA_LD = productionDatabase.PK;
			existingUserAccount.EUA_UserID = "US2";
			existingUserAccount.EUA_FullName = "Existing One";
			existingUserAccount.EUA_Email = "existing1@cargowise.com";
			existingUserAccount.EUA_OC_WebAccessContact = existingContact.PK;
			existingUserAccount.EUA_IsContactRelationshipActive = false;
			existingUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			var testUserWithInactiveRelationship = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			testUserWithInactiveRelationship.EUA_LD = testDatabase1.PK;
			testUserWithInactiveRelationship.EUA_UserID = "US1";
			testUserWithInactiveRelationship.EUA_Email = existingContact.OC_Email;
			testUserWithInactiveRelationship.EUA_OC_WebAccessContact = existingContact.PK;
			testUserWithInactiveRelationship.EUA_IsEmailVerificationRequired = false;
			testUserWithInactiveRelationship.EUA_IsContactRelationshipActive = false;
			testUserWithInactiveRelationship.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			Factory.Save();
			AssertEquals("Precondition", false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(existingUserAccount));
			AssertEquals("Precondition", false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(testUserWithInactiveRelationship));
			var accessControl = new TokenizedAccessControl();
			var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(existingContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			using (var page = ResetPasswordPage)
			{
				AssertEquals(null, existingContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, testToken);
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("User 1", existingContact.PK.ToString()));
				page.OrgCodeDownList_Expose.Text = "User 1";
				page.OnLoad();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(true, page.PasswordDiv_Expose.Visible);
				AssertEquals(false, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestOrgCodeDropDownList_With_OrgCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH ORG";
			var contact1 = CreateOrgContact(org, "Test User A", true, true);
			Factory.Save();
			using (var page = ResetPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken_BorderWise);
				page.OnInit();
				page.OnLoad();
				var items = page.OrgCodeDownList_Expose.Items.OfType<ListItem>().OrderBy(x => x.Text).ToArray();
				AssertEquals(1, items.Length);
				AssertEquals("TESTORSYD - TESTORG LTD.", items[0].Text);
				AssertEquals(TestOrgContact.PK.ToString(), items[0].Value);
				page.OrgCodeDownList_Expose.SelectedIndex = 0;
				AssertEquals(TestOrgContact.OC_ContactName, page.GetSelectedWebUser()?.OC_ContactName);
			}
		}

		public void TestShouldSetupSessionOnLoadIfQueryIsValid()
		{
			using (var page = ResetPasswordPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.NewPassword_Expose.Text = "1";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OrgCodeDownList_Expose.Items.Add(new ListItem("TESTORG", TestOrgContact.PK.ToString()));
				EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				page.HttpClientHandlerMock = GetHttpClientHandlerForTesting(HttpStatusCode.OK, "Ok");
				AssertEquals("Precondition", false, page.Request.IsAuthenticated);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
				page.OnLoad();
				AssertNotEquals("Site user should not be null", null, page.SiteUser);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			}
		}

		public void TestHeadingLabelWithExpiredPassword()
		{
			WebDataRegistry.Instance.WebPasswordRotationDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-35).ToDateTime());
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");
			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove("ResetKey");
				HttpContext.Current.Request.QueryString.Add("ResetKey", TestToken);
				page.OnLoad();
				Assert(!page.PasswordExpiredMessageLabel_Expose.Visible);
			}

			using (var page = ResetPasswordPage)
			{
				page.RequestQueryString_Expose.Remove("ResetKey");
				HttpContext.Current.Request.QueryString.Add("ResetKey", TestToken);
				page.RequestQueryString_Expose.Remove("ref");
				HttpContext.Current.Request.QueryString.Add("ref", "exp");
				page.OnLoad();
				Assert(page.PasswordExpiredMessageLabel_Expose.Visible);
			}
		}

		class ResetPasswordForTest : ResetPassword
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new Global();
			}

			protected override Uri RequestUrl => new Uri("http://www.test.com/MyAccount/Admin/ResetPassword.aspx");

			public ResetPasswordForTest()
			{
				PasswordChangeMessage_Expose = new ZTextLabel();
				CopyRightYear_Expose = new ZTextLabel();
				OrgCodeLabel_Expose = new ZTextLabel();
				NewPassword_Expose = new TextBox();
				NewPasswordConfirm_Expose = new TextBox();
				UpdateButton_Expose = new Button();
				BackHyperLink_Expose = new HyperLink();
				OrgCodeDownList_Expose = new ZDropDownList();
				GoBackMessage_Expose = new HtmlGenericControl();
				AccountVerification_Expose = new AccountVerificationControlForTest();
				AccountVerification_Expose.Page = this;
				PasswordDiv_Expose = new HtmlGenericControl();
				LoginOptionsDiv_Expose = new HtmlGenericControl();
				GoBackLoginLink_Expose = new HyperLink();
				BackLink_Expose = new HyperLink();
				PasswordExpiredMessageLabel_Expose = new ZTextLabel { Visible = false };
				HeadingMessageDiv_Expose = new HtmlGenericControl();
				passwordChangeRequirements = new PasswordChangeRequirementsControl();
			}

			public NameValueCollection RequestQueryString_Expose => base.RequestQueryString;

			ZTextLabel OrgCodeLabel_Expose { set => OrgCodeLabel = value; }

			public ZTextLabel PasswordChangeMessage_Expose { get => PasswordChangeMessage; private set => PasswordChangeMessage = value; }

			ZTextLabel CopyRightYear_Expose { set => CopyrightYear = value; }

			public Button UpdateButton_Expose { get => Update; private set => Update = value; }

			public HyperLink BackHyperLink_Expose { get => BackLink; private set => BackLink = value; }

			public HyperLink GoBackLoginLink_Expose { get => GoBackLoginLink; private set => GoBackLoginLink = value; }

			public HyperLink BackLink_Expose { get => BackLink; private set => BackLink = value; }

			public TextBox NewPassword_Expose { get => NewPassword; private set => NewPassword = value; }

			public ZDropDownList OrgCodeDownList_Expose { get => OrgCodeDropDownList; private set => OrgCodeDropDownList = value; }

			public TextBox NewPasswordConfirm_Expose { get => NewPasswordConfirm; private set => NewPasswordConfirm = value; }

			public HtmlGenericControl GoBackMessage_Expose { get => GoBackMessage; private set => GoBackMessage = value; }

			public HtmlGenericControl PasswordDiv_Expose { get => PasswordDiv; private set => PasswordDiv = value; }

			public HtmlGenericControl LoginOptionsDiv_Expose { get => LoginOptionsDiv; private set => LoginOptionsDiv = value; }

			public AccountVerificationControlForTest AccountVerification_Expose { get => (AccountVerificationControlForTest)AccountVerification; private set => AccountVerification = value; }

			public ZTextLabel PasswordExpiredMessageLabel_Expose { get => PasswordExpiredMessageLabel; private set => PasswordExpiredMessageLabel = value; }

			public HtmlGenericControl HeadingMessageDiv_Expose { get => HeadingMessageDiv; private set => HeadingMessageDiv = value; }

			public PasswordChangeRequirementsControl PasswordChangeRequirementsControl_Expose => passwordChangeRequirements;

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
				base.OnLoadComplete(EventArgs.Empty);
			}

			public void OnInit()
			{
				base.OnInit(EventArgs.Empty);
			}

			public void OnUpdate()
			{
				base.Update_Click(this, EventArgs.Empty);
			}

			public HttpClientHandler HttpClientHandlerMock { get; set; }

			protected override HttpClientHandler GetHttpClientHandler()
			{
				return HttpClientHandlerMock ?? Mock.Of<HttpClientHandler>();
			}

			public override void Dispose()
			{
				passwordChangeRequirements?.Dispose();
				base.Dispose();
			}
		}

		ResetPasswordForTest ResetPasswordPage
		{
			get
			{
				var testPage = new ResetPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		OrgHeader TestOrg;
		EDIOrgContact TestOrgContact;
		LicenceDatabase Database;
		string TestToken;
		string TestToken_BorderWise;
		string TestToken_Expired;
		string TestToken_InvalidType;
		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			TestToken = "testToken";
			Database = licence.Database;
			TestOrg = Database.WebAccessOrg;
			TestOrg.OH_Code = "TESTORG";
			TestOrg.OH_FullName = "TESTORG LTD.";

			var person = Factory.NewWithValidTestData<GlbPerson>();

			TestOrgContact = (EDIOrgContact)TestOrg.Contacts.AddNew();
			TestOrgContact.OC_ContactName = "Test User";
			TestOrgContact.OC_Email = "testuser@cargowise.com";
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_PER = person.PK;

			// we can reset contact password only when it is not the only contact in the person relationship
			var contact2 = TestOrg.Contacts.AddNew();
			contact2.OC_ContactName = "Test User 2";
			contact2.OC_Email = "user@cargowise.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;
			contact2.OC_PER = person.PK;

			Factory.Save();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
			var resetInfo = new PasswordResetInfo()
			{ Product = ProductTypes.Codes.BorderWise, ContactEmail = TestOrgContact.Email, OrgCode = TestOrgContact.OrgCode, NavigateUrl = new Uri("http://app.borderwise.com") };
			var jsonScope = JsonConvert.SerializeObject(resetInfo);
			TestToken_BorderWise = accessControl.CreateLimitedToken(AccessTokenTypes.ResetPassword, new AccessTokenInfo(jsonScope, Guid.Empty, "INV"), maxUses: 1);
			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}
	}
}
