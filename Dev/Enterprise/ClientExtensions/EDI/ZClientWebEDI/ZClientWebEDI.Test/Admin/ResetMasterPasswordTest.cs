using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class ResetMasterPasswordTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return ResetMasterPasswordPage;
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

		public void TestReportException()
		{
			int GetNumErrorReports()
			{
				using (var cmd = Db.Connection.Command("SELECT COUNT(*) FROM [StmErrorReport]"))
				{
					return (int)cmd.ExecuteScalar();
				}
			}
			var reportCountBefore = GetNumErrorReports();
			(TestPage as ResetMasterPasswordForTest).ReportException(new Exception("test"));
			(TestPage as ResetMasterPasswordForTest).ReportException(new Exception("test"));
			AssertEquals(reportCountBefore + 2, GetNumErrorReports());
		}

		void AssertPageLoad(EventHandler onLoadCompleteHandler, string token)
		{
			var resetPasswordKey = new Global().ResetPasswordKey;
			HttpContext.Current.Request.QueryString.Remove(resetPasswordKey);
			HttpContext.Current.Request.QueryString.Add(resetPasswordKey, token);
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

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (ResetMasterPasswordForTest)this.TestPage;
			Assert(testPage.ContactsBox_Expose.Visible);
			Assert(testPage.NewPassword_Expose.Visible);
			Assert(testPage.NewPasswordConfirm_Expose.Visible);
			Assert(testPage.Update_Expose.Visible);
			Assert(testPage.PasswordChangeRequirementsControl_Expose.Visible);
			AssertNullOrEmpty(testPage.PasswordChangeMessage_Expose.Text);
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (ResetMasterPasswordForTest)TestPage;
			CombineAssertions(() =>
			{
				Assert("Should not see set password controls", !testPage.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Should not see set password controls", !testPage.ContactsBox_Expose.Visible);
				Assert("Should not see set password controls", !testPage.NewPassword_Expose.Visible);
				Assert("Should not see set password controls", !testPage.NewPasswordConfirm_Expose.Visible);
				Assert("Should not see set password controls", !testPage.Update_Expose.Visible);
				Assert("Should not see set password controls", !testPage.PasswordChangeRequirementsControl_Expose.Visible);
				AssertEquals("The set link you have followed is invalid or expired.", testPage.PasswordChangeMessage_Expose.Text);
				AssertEquals(string.Empty, testPage.ResetMasterPasswordHeadingLabel_Expose.Text);
				AssertEquals(false, testPage.HeadingMessageDiv_Expose.Visible);
			});
		}

		public void TestResetMasterPassword()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestToken);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.PasswordChangeRequirementsControl_Expose.Visible = true;
				page.OnLoad();
				AssertEquals(page.AppInstance.HostingSiteRoot, page.BackLink_Expose.NavigateUrl);
				AssertEquals("../Login/LoginLite.aspx", page.GoBackLoginLink_Expose.NavigateUrl);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OnUpdate();
				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.ContactsBox_Expose.Visible);
				Assert("Password controls should be visible", page.NewPassword_Expose.Visible);
				Assert("Password controls should be visible", page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be visible", page.Update_Expose.Visible);
				Assert("Password controls should be visible", page.PasswordChangeRequirementsControl_Expose.Visible);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("SuccessMessage", page.PasswordChangeMessage_Expose.CssClass);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
			}
		}

		public void TestResetMasterPassword_WithResetInfo()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_Email = TestOrgContact.OC_Email;
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertNotEquals("Precondition: Should be in separate organisations", otherContact.OC_OH, TestOrgContact.OC_OH);
			AssertNotEquals("Precondition: Should be on separate persons", otherContact.OC_PER, TestOrgContact.OC_PER);
			using (var page = ResetMasterPasswordPage)
			{
				var companySpecificSubject = "Hello";
				var companySpecificBody = "Anyone home?";
				WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestTokenWithResetInfo);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.PasswordChangeRequirementsControl_Expose.Visible = true;
				page.OnLoad();
				AssertEquals(page.AppInstance.HostingSiteRoot, page.BackLink_Expose.NavigateUrl);
				AssertEquals(false, page.BackLink_Expose.Visible);
				AssertEquals("Should be populated from token scope", RedirectUrl, page.GoBackLoginLink_Expose.NavigateUrl);
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Should only show persons with contacts in the specified org from the ResetInfo", 1, personList.Count);
				AssertEquals("Should only show persons with contacts in the specified org from the ResetInfo", TestOrgContact.OC_PER, personList[0].Person.PK);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OnUpdate();
				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.ContactsBox_Expose.Visible);
				Assert("Password controls should be visible", page.NewPassword_Expose.Visible);
				Assert("Password controls should be visible", page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be visible", page.Update_Expose.Visible);
				Assert("Password controls should be visible", page.PasswordChangeRequirementsControl_Expose.Visible);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestTokenWithResetInfo, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(TestTokenWithResetInfo, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
				var createdEmails = Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("A password set confirmation should have been sent to the active web access contact", 1, createdEmails.Count);
				var email = createdEmails[0];
				AssertEquals("Contact should be the only recipient", 1, email.Recipients.Count);
				Assert("Email should match contact email", TestOrgContact.OC_Email.EqualsIgnoringCase(email.Recipients[0].Email));
				AssertContains("Email should use template of the company specified in the token scope", companySpecificSubject, email.Subject);
				AssertContains("Email should use template of the company specified in the token scope", companySpecificBody, email.Body);
			}
		}

		public void TestResetMasterPassword_WithoutExistingPassword()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			TestOrgContact.OC_PasswordHash = ZBlob.Empty;
			TestOrgContact.Person.RemovePasswordHash();
			Factory.Save();
			AssertEquals(false, TestOrgContact.HasPassword);
			AssertEquals(false, TestOrgContact.Person.HasPassword);
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestToken);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.PasswordChangeRequirementsControl_Expose.Visible = true;
				page.OnLoad();
				AssertEquals(page.AppInstance.HostingSiteRoot, page.BackLink_Expose.NavigateUrl);
				AssertEquals("../Login/LoginLite.aspx", page.GoBackLoginLink_Expose.NavigateUrl);
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.OnUpdate();
				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				Assert("Password controls should be visible", page.ContactsBox_Expose.Visible);
				Assert("Password controls should be visible", page.NewPassword_Expose.Visible);
				Assert("Password controls should be visible", page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be visible", page.Update_Expose.Visible);
				Assert("Password controls should be visible", page.PasswordChangeRequirementsControl_Expose.Visible);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnUpdate();
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not be logged in", false, page.SiteUser.IsLoggedIn);
				page.NewPasswordConfirm_Expose.Text = "Th1sIsMyPassword!";
				page.OnUpdate();
				AssertEquals("Personal password should be updated", true, TestOrgContact.Person.VerifyPassword("Th1sIsMyPassword!"));
				AssertEquals("Your password has been changed.", page.PasswordChangeMessage_Expose.Text);
				Assert(!accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals("Should not have been logged in", false, page.SiteUser.IsLoggedIn);
				Assert("Password controls should be hidden", !page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert("Password controls should be hidden", !page.ContactsBox_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPassword_Expose.Visible);
				Assert("Password controls should be hidden", !page.NewPasswordConfirm_Expose.Visible);
				Assert("Password controls should be hidden", !page.Update_Expose.Visible);
				Assert("Password controls should be hidden", !page.PasswordChangeRequirementsControl_Expose.Visible);
			}
		}

		public void TestResetMasterPassword_UnlockContact()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherOrg = "OTHERORG";
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			CreateLockoutUserRecord(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(string.Empty, TestOrgContact.OC_Email);
			CreateLockoutUserRecord(otherOrg, TestOrgContact.OC_Email);
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestToken);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.OnLoad();
				var loginOptionsHelper = ((EDIResetMasterPasswordManager)page.DataSource).LoginOptionsHelper;
				var hash = WebApplicationLoginHelper.RetrieveLoginHashFromCookie(TestOrgContact.OrgCode, TestOrgContact.OC_Email);
				AssertEquals("Precondition", null, hash);
				Assert("Precondition", ContactIsLockedOut(TestOrgContact.OrgCode, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(string.Empty, TestOrgContact.OC_Email, hash));
				Assert("Precondition", ContactIsLockedOut(otherOrg, TestOrgContact.OC_Email, hash));
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.PersonForPasswordChange = TestOrgContact.Person;
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

		public void TestShouldHideHeadingLabelIfOnlyOnePerson()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Precondition: Should only have 1 contact/person", 1, personList.Count);
				Assert("ResetMasterPasswordHeadingLabel should not be visible if only one person", !page.ResetMasterPasswordHeadingLabel_Expose.Visible);
			}
		}

		public void TestShouldShowHeadingLabelIfMultiplePersons()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = TestOrgContact.OC_Email;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				AssertEquals("Precondition: Should have 2 contacts/persons", 2, personList.Count);
				Assert("ResetMasterPasswordHeadingLabel should be visible if 2 persons", page.ResetMasterPasswordHeadingLabel_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount()
		{
			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_WebAccessEnabled = true;
			unrelatedContact.OC_Email = TestOrgContact.OC_Email;
			var unrelatedUserAccount = Factory.New<EdiCustomerUserAccount>();
			unrelatedUserAccount.EUA_LD = Database.PK;
			unrelatedUserAccount.EUA_UserID = "U0U";
			unrelatedUserAccount.EUA_FullName = "User U";
			unrelatedUserAccount.EUA_Email = "uU@cw1.com";
			unrelatedUserAccount.EUA_IsContactRelationshipActive = true;
			unrelatedUserAccount.EUA_ContactRelationshipStatus = string.Empty;
			unrelatedUserAccount.EUA_OC_WebAccessContact = unrelatedContact.PK;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				AssertNull(TestOrgContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("No contacts in the person relationship are verified", true, page.PasswordDiv_Expose.Visible);
				AssertEquals("No contacts in the person relationship are verified", false, page.LoginOptionsDiv_Expose.Visible);
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
			using (var page = ResetMasterPasswordPage)
			{
				AssertEquals(userAccount, TestOrgContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(false, page.PasswordDiv_Expose.Visible);
				AssertEquals(true, page.LoginOptionsDiv_Expose.Visible);
				var helper = ((EDIResetMasterPasswordManager)page.DataSource).LoginOptionsHelper;
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
			var testToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(existingContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			using (var page = ResetMasterPasswordPage)
			{
				AssertEquals(testUserWithInactiveRelationship, existingContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, testToken);
				page.PersonForPasswordChange = existingContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(true, page.PasswordDiv_Expose.Visible);
				AssertEquals(false, page.LoginOptionsDiv_Expose.Visible);
			}

			existingContact.SetHashedPassword("123");
			Factory.Save();
			AssertEquals(false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(existingUserAccount));
			AssertEquals(false, LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(testUserWithInactiveRelationship));
			using (var page = ResetMasterPasswordPage)
			{
				AssertEquals(testUserWithInactiveRelationship, existingContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, testToken);
				page.PersonForPasswordChange = existingContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals(false, page.PasswordDiv_Expose.Visible);
				AssertEquals(true, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_SelfDeactivationShouldShowLoginOptions()
		{
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, "DDD"));
			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";
			var productionDatabase = Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;
			var existingUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			existingUserAccount.EUA_LD = productionDatabase.PK;
			existingUserAccount.EUA_UserID = "US2";
			existingUserAccount.EUA_FullName = "Existing One";
			existingUserAccount.EUA_Email = "existing1@cargowise.com";
			existingUserAccount.EUA_OC_WebAccessContact = TestOrgContact.PK;
			existingUserAccount.EUA_IsContactRelationshipActive = false;
			existingUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("Should show login options since there's an SDA with no active user accounts on the contact", false, page.PasswordDiv_Expose.Visible);
				AssertEquals("Should show login options since there's an SDA with no active user accounts on the contact", true, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_SelfDeactivationShouldNotShowLoginOptionsIfOnDifferentEmail()
		{
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, "DDD"));
			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";
			var productionDatabase = Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;
			var testDatabase1 = licenceEnterprise.Databases.AddNew();
			testDatabase1.LD_ServerCode = "TD1";
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			testDatabase1.LD_OH_WebAccessOrg = otherOrg.PK;
			var otherContact = otherOrg.Contacts.AddNew();
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = "different@email.com";
			otherContact.OC_PER = TestOrgContact.OC_PER;
			var existingUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			existingUserAccount.EUA_LD = testDatabase1.PK;
			existingUserAccount.EUA_UserID = "US2";
			existingUserAccount.EUA_FullName = "Existing One";
			existingUserAccount.EUA_Email = "different@email.com";
			existingUserAccount.EUA_OC_WebAccessContact = otherContact.PK;
			existingUserAccount.EUA_IsContactRelationshipActive = false;
			existingUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("Should not show login options since the SDA is on a different contact email", true, page.PasswordDiv_Expose.Visible);
				AssertEquals("Should not show login options since the SDA is on a different contact email", false, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_SelfDeactivationShouldNotShowLoginOptionsIfActiveAccountExists()
		{
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, "DDD"));
			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";
			var productionDatabase = Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;
			var testDatabase1 = licenceEnterprise.Databases.AddNew();
			testDatabase1.LD_ServerCode = "TD1";
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var existingUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			existingUserAccount.EUA_LD = productionDatabase.PK;
			existingUserAccount.EUA_UserID = "US2";
			existingUserAccount.EUA_FullName = "Existing One";
			existingUserAccount.EUA_Email = "existing1@cargowise.com";
			existingUserAccount.EUA_OC_WebAccessContact = TestOrgContact.PK;
			var testUserWithInactiveRelationship = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			testUserWithInactiveRelationship.EUA_LD = testDatabase1.PK;
			testUserWithInactiveRelationship.EUA_UserID = "US1";
			testUserWithInactiveRelationship.EUA_Email = TestOrgContact.OC_Email;
			testUserWithInactiveRelationship.EUA_OC_WebAccessContact = TestOrgContact.PK;
			testUserWithInactiveRelationship.EUA_IsContactRelationshipActive = false;
			testUserWithInactiveRelationship.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("Should not show login options since there's an active user account on the contact", true, page.PasswordDiv_Expose.Visible);
				AssertEquals("Should not show login options since there's an active user account on the contact", false, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_ShouldSkipLoginOptionsIfMatchingEmailIsVerified()
		{
			var contactWithoutMatchingEmail = (EDIOrgContact)Database.WebAccessOrg.Contacts.AddNew();
			contactWithoutMatchingEmail.OC_ContactName = "idk";
			contactWithoutMatchingEmail.OC_WebAccessEnabled = true;
			contactWithoutMatchingEmail.OC_Email = "other@email.com";
			contactWithoutMatchingEmail.OC_PER = TestOrgContact.OC_PER;
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = Database.PK;
			userAccount.EUA_UserID = "U02";
			userAccount.EUA_FullName = "User 2";
			userAccount.EUA_Email = "u2@cw1.com";
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			userAccount.EUA_OC_WebAccessContact = contactWithoutMatchingEmail.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = TestOrgContact.OC_Email;
			contact2.OC_PER = TestOrgContact.OC_PER;
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "U03";
			userAccount2.EUA_FullName = "User 3";
			userAccount2.EUA_Email = "u3@cw1.com";
			userAccount2.EUA_IsContactRelationshipActive = true;
			userAccount2.EUA_ContactRelationshipStatus = string.Empty;
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				AssertEquals(userAccount, contactWithoutMatchingEmail.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("Since contact2 shares the same email in the person relationship and is verified, we should not show verification", true, page.PasswordDiv_Expose.Visible);
				AssertEquals("Since contact2 shares the same email in the person relationship and is verified, we should not show verification", false, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestUnlinkedUserAccount_ShouldNotSkipLoginOptionsIfAUserAccountWithMatchingEmailIsNotVerified()
		{
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = Database.PK;
			userAccount.EUA_UserID = "U02";
			userAccount.EUA_FullName = "User 2";
			userAccount.EUA_Email = "u2@cw1.com";
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			userAccount.EUA_OC_WebAccessContact = TestOrgContact.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_Email = TestOrgContact.OC_Email;
			contact2.OC_PER = TestOrgContact.OC_PER;
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "U03";
			userAccount2.EUA_FullName = "User 3";
			userAccount2.EUA_Email = "u3@cw1.com";
			userAccount2.EUA_IsContactRelationshipActive = true;
			userAccount2.EUA_ContactRelationshipStatus = string.Empty;
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				AssertEquals(userAccount, TestOrgContact.GetMostRecentUnlinkedUserAccount());
				var queryStringKey = "ResetKey";
				page.RequestQueryString_Expose.Remove(queryStringKey);
				page.RequestQueryString_Expose.Add(queryStringKey, TestToken);
				page.PersonForPasswordChange = TestOrgContact.Person;
				page.OnLoad();
				page.CheckBox_OnCheckedChanged_Expose();
				AssertEquals(false, page.AccountVerification_Expose.HeaderVisible);
				AssertEquals(false, page.AccountVerification_Expose.ShouldRedirectAfterVerification);
				AssertEquals("Should show login options since one of the user accounts attached to a contact with matching email was not verified", false, page.PasswordDiv_Expose.Visible);
				AssertEquals("Should show login options since one of the user accounts attached to a contact with matching email was not verified", true, page.LoginOptionsDiv_Expose.Visible);
			}
		}

		public void TestShouldSetupSessionOnLoadIfQueryIsValid()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";

				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(new Global().ResetPasswordKey, TestToken);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
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

		public void TestResetMasterPassword_ErrorMessage()
		{
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				page.OnUpdate();
				AssertEquals("The page has expired. Please refresh and try again.", page.PasswordChangeMessage_Expose.Text);
			}

			using (var page = ResetMasterPasswordPage)
			{
				var companySpecificSubject = "Hello";
				var companySpecificBody = "Anyone home?";
				WebDataRegistry.Instance.MasterPasswordResetSuccessfullyEmailTemplate.SetValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = companySpecificSubject, EmailBody = companySpecificBody });
				var resetPasswordKey = new Global().ResetPasswordKey;
				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(new Global().ResetPasswordKey, TestTokenWithResetInfo);
				page.ResetMasterPasswordHeadingLabel_Expose.Visible = true;
				page.ContactsBox_Expose.Visible = true;
				page.NewPassword_Expose.Visible = true;
				page.NewPasswordConfirm_Expose.Visible = true;
				page.Update_Expose.Visible = true;
				page.OnLoad();
				page.NewPassword_Expose.Text = "Th1sIsMyPassword!";
				page.NewPasswordConfirm_Expose.Text = "2";
				page.PersonForPasswordChange = TestOrgContact.Person;
				TestOrgContact.Person.ContactCollection[0].OC_IsActive = false;
				page.OnUpdate();
				AssertEquals("Please select a contact for login", page.PasswordChangeMessage_Expose.Text);
				AssertEquals("ResetMasterPassword.aspx.cs|Update_Click|defaultContact==null", ErrorReporter.LastKeyReported);
				AssertEquals($@"selectedPersonPk:{TestOrgContact.Person.PK}
selectedPerson.ContactCollection:1
    OC_PK:{TestOrgContact.Person.ContactCollection[0].PK}|OC_IsActive:N|OC_WebAccessEnabled:Y
", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestHeadingLabelWithExpiredPassword()
		{
			WebDataRegistry.Instance.WebPasswordRotationDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30);
			WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-35).ToDateTime());
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://yahoo.com.au/");
			TestOrgContact.Person.PER_EmailAddress = "tester@abc.com";
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_IsActive = true;
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = TestOrgContact.OC_Email;
			Factory.Save();
			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove("ResetKey");
				HttpContext.Current.Request.QueryString.Add("ResetKey", TestToken);
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				Assert(page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(!page.PasswordExpiredMessageLabel_Expose.Visible);
			}

			using (var page = ResetMasterPasswordPage)
			{
				page.RequestQueryString_Expose.Remove("ResetKey");
				HttpContext.Current.Request.QueryString.Add("ResetKey", TestToken);
				page.RequestQueryString_Expose.Remove("ref");
				HttpContext.Current.Request.QueryString.Add("ref", "exp");
				page.OnLoad();
				var personList = ((ResetMasterPasswordManager)page.DataSource).PersonsForBinding;
				Assert(page.ResetMasterPasswordHeadingLabel_Expose.Visible);
				Assert(page.PasswordExpiredMessageLabel_Expose.Visible);
			}
		}

		public void TestEmptyTokenInPasswordResetURLShowsInvalidTokenMessage()
		{
			using (var page = ResetMasterPasswordPage)
			{
				var resetPasswordKey = new Global().ResetPasswordKey;
				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				Assert(accessControl.TryPeek(TestToken, AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals(page.PasswordChangeMessage_Expose.Text, "");

				page.RequestQueryString_Expose.Remove(resetPasswordKey);
				HttpContext.Current.Request.QueryString.Add(resetPasswordKey, null);
				page.OnLoad();
				Assert(!accessControl.TryPeek("", AccessTokenTypes.ResetMasterPassword, out _));
				AssertEquals(page.PasswordChangeMessage_Expose.Text, "The set link you have followed is invalid or expired.");
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
		class ResetMasterPasswordForTest : ResetMasterPassword
		{
			protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/ResetMasterPassword.aspx");

			public ResetMasterPasswordForTest()
			{
				LoginContactsRepeater = new ZRepeater();
				PasswordChangeMessage = new ZTextLabel();
				ContactsBox = new HtmlGenericControl();
				NewPassword = new TextBox();
				NewPasswordConfirm = new TextBox();
				ResetMasterPasswordHeadingLabel = new ZTextLabel { Visible = false };
				PasswordExpiredMessageLabel = new ZTextLabel { Visible = false };
				HeadingMessageDiv = new HtmlGenericControl();
				GoBackLoginLink = new HyperLink();
				BackLink = new HyperLink();
				CopyrightYear = new ZTextLabel();
				Update = new Button();
				AccountVerification = new AccountVerificationControlForTest();
				AccountVerification.Page = this;
				PasswordDiv = new HtmlGenericControl();
				LoginOptionsDiv = new HtmlGenericControl();
				passwordChangeRequirements = new PasswordChangeRequirementsControl();
			}
			
			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public new void ReportException(Exception ex)
			{
				base.ReportException(ex);
			}

			public NameValueCollection RequestQueryString_Expose => base.RequestQueryString;
			public Label PasswordChangeMessage_Expose => PasswordChangeMessage;
			public HtmlGenericControl ContactsBox_Expose => ContactsBox;
			public TextBox NewPassword_Expose => NewPassword;
			public TextBox NewPasswordConfirm_Expose => NewPasswordConfirm;
			public HyperLink BackLink_Expose => BackLink;
			public HyperLink GoBackLoginLink_Expose => GoBackLoginLink;
			public Button Update_Expose => Update;
			public ZTextLabel ResetMasterPasswordHeadingLabel_Expose => ResetMasterPasswordHeadingLabel;
			public HtmlGenericControl HeadingMessageDiv_Expose => HeadingMessageDiv;
			public ZTextLabel PasswordExpiredMessageLabel_Expose => PasswordExpiredMessageLabel;
			public AccountVerificationControl AccountVerification_Expose => AccountVerification;
			public HtmlGenericControl PasswordDiv_Expose => PasswordDiv;
			public HtmlGenericControl LoginOptionsDiv_Expose => LoginOptionsDiv;
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

			protected override GlbPerson GetSelectedPerson()
			{
				return PersonForPasswordChange;
			}

			public GlbPerson PersonForPasswordChange { get; set; }

			public void CheckBox_OnCheckedChanged_Expose() => CheckBox_OnCheckedChanged(null, EventArgs.Empty);
		}

		ResetMasterPasswordForTest ResetMasterPasswordPage
		{
			get
			{
				var testPage = new ResetMasterPasswordForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		EDIOrgContact TestOrgContact;
		GlbBranch Branch;
		LicenceDatabase Database;
		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		string TestTokenWithResetInfo;
		const string RedirectUrl = "http://app.borderwise.com/";
		protected override void SetUp()
		{
			base.SetUp();
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			TestToken = "testToken";
			Database = licence.Database;
			var orgHeader = Database.WebAccessOrg;
			orgHeader.OH_Code = "WiseTech";
			orgHeader.OH_FullName = "WiseTech Global";
			TestOrgContact = (EDIOrgContact)orgHeader.Contacts.AddNew();
			TestOrgContact.OC_Email = "TestUser@wisetechglobal.com";
			TestOrgContact.OC_IsActive = true;
			TestOrgContact.OC_WebAccessEnabled = true;
			Branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			TestOrgContact.Person.PER_FullName = "Franky";
			TestOrgContact.Person.SetHashedPassword("5678");
			Factory.Save();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo(TestOrgContact.OC_Email, Guid.Empty, "INV"), maxUses: 1);
			var resetInfo = new PasswordResetInfo()
			{ Product = ProductTypes.Codes.BorderWise, ContactEmail = TestOrgContact.Email, OrgCode = TestOrgContact.OrgCode, NavigateUrl = new Uri(RedirectUrl), EmailTemplateCompanyPk = Branch.Company.PK.ToString() };
			var jsonScope = JsonConvert.SerializeObject(resetInfo);
			TestTokenWithResetInfo = accessControl.CreateLimitedToken(AccessTokenTypes.ResetMasterPassword, new AccessTokenInfo(jsonScope, Guid.Empty, "INV"), maxUses: 1);
			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}
	}
}
