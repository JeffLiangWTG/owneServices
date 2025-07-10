using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class EmailVerificationTest : TestCaseWithFactory
	{
		#region Verify Email and set EUA_IsContactRelationshipActive
		public void TestVerifyDistinctEmailToken_ActivateContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_OC_WebAccessContact = contact.PK;
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
				userAccount.EUA_Email = "sam@test.com";
				userAccount.Database.LD_OH_WebAccessOrg = org.PK;
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"abc@gmail.com::{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.DistinctEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				page.DoVerifyEmail();
				AssertEquals("Status should be back to empty", "", userAccount.EUA_ContactRelationshipStatus);
				AssertEquals("sam@test.com", userAccount.EUA_Email);
				AssertEquals("abc@gmail.com", userAccount.WebAccessContact.OC_Email);
				AssertStartsWith("Should redirect to UpdateContactInformation", "/webapp/Login/UpdateContactInformation.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestVerifyDistinctEmailToken_NoContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				database.LD_OH_WebAccessOrg = org.PK;
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
				userAccount.EUA_Email = "sam@test.com";
				userAccount.EUA_LD = database.PK;
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"abc@gmail.com::{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.DistinctEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				page.DoVerifyEmail();
				AssertEquals("Status should be back to empty", "", userAccount.EUA_ContactRelationshipStatus);
				AssertEquals("sam@test.com", userAccount.EUA_Email);
				AssertEquals("abc@gmail.com", userAccount.WebAccessContact.OC_Email);
				AssertStartsWith("Should redirect to UpdateContactInformation page", "/webapp/Login/UpdateContactInformation.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestVerifyDistinctEmailToken_ShouldSetupSession()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				database.LD_OH_WebAccessOrg = org.PK;
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
				userAccount.EUA_Email = "sam@test.com";
				userAccount.EUA_LD = database.PK;
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"abc@gmail.com::{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.DistinctEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForEnvironmentTest();
				page.Request.QueryString.Add("token", tokenGood);
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
				AssertNoExceptionThrown("Should not throw exception since environment should be setup first", () => {
					page.DoPageLoad();
					page.DoVerifyEmail();
				});
				AssertEquals("Status should be back to empty", "", userAccount.EUA_ContactRelationshipStatus);
				AssertEquals("sam@test.com", userAccount.EUA_Email);
				AssertEquals("abc@gmail.com", userAccount.WebAccessContact.OC_Email);
				AssertNotEquals("Site user should not be null", null, page.SiteUser);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
				AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
				AssertStartsWith("Should redirect to UpdateContactInformation page", "/webapp/Login/UpdateContactInformation.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestVerifyEmailToken_ActivateContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_OC_WebAccessContact = contact.PK;
				userAccount.EUA_IsContactRelationshipActive = false;
				userAccount.Database.LD_OH_WebAccessOrg = org.PK;
				Factory.Save();
				contact.Person.PER_EmailAddress = "sam@test.com";
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				page.DoVerifyEmail();
				var routingDescriptor = new UserEmailVerificationRoutingDescriptor(new Uri(url), userAccount.Database, userAccount);
				AssertEquals("Precondition", false, routingDescriptor.IsRoutingRequired);
				AssertEquals("Contact relationship should have been activated", true, userAccount.EUA_IsContactRelationshipActive);
				AssertStartsWith("Should redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestVerifyEmailToken_ActivateContactRelationship()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.Database.LD_OH_WebAccessOrg = org.PK;
			Factory.Save();
			string url = "google.com";
			var infoGood = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoGood, TimeSpan.FromDays(1), 1);
			var infoNoUser = new AccessTokenInfo(FormattableString.Invariant($"{url}"), Guid.Empty, EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenNoUser = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoNoUser, TimeSpan.FromDays(1), 1);
			var infoWrongType = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenWrongType = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, infoWrongType, TimeSpan.FromDays(1), 1);
			var infoExpired = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenExpired = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoExpired, TimeSpan.FromDays(-1), 1);
			var infoBadContact = new AccessTokenInfo(FormattableString.Invariant($"{url}"), Guid.NewGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenBadContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoBadContact, TimeSpan.FromDays(1), 1);
			var infoBadUser = new AccessTokenInfo(FormattableString.Invariant($"{url}"), Guid.NewGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenBadUser = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoBadUser, TimeSpan.FromDays(1), 1);
			var infoUsed = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenUsed = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoUsed, TimeSpan.FromDays(1), 0);
			var page = GetPageForTest();
			page.Request.QueryString.Add("token", tokenNoUser);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenWrongType);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenExpired);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenBadContact);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenBadUser);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenUsed);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(false, userAccount.EUA_IsContactRelationshipActive);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenGood);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			AssertEquals(true, userAccount.EUA_IsContactRelationshipActive);
		}

		public void TestVerifyEmailToken_ActivateUserAccountExistingContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				var org = licence.Company.Header;
				org.Contacts.RemoveAndDeleteAll();
				var address = org.Addresses.AddNew();
				address.Address1 = "72 O'Riordan Street";
				var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
				clientBranch.LCB_LD = database.PK;
				clientBranch.LCB_Code = "SYD";
				clientBranch.LCB_OA = address.PK;
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = "US1";
				userAccount.EUA_FullName = "sam test";
				userAccount.EUA_Email = "sam@test.com";
				Factory.Save();
				const string url = "http://google.com";
				var infoExistingContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}:{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenExistingContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoExistingContact, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenExistingContact);
				page.DoPageLoad();
				page.DoVerifyEmail();
				AssertEquals("Email verification should be complete", false, userAccount.EUA_IsEmailVerificationRequired);
				AssertEquals("Should be linked to the contact", contact.PK, userAccount.EUA_OC_WebAccessContact);
				var response = page.Response.RedirectLocation;
				var orignalUrl = ExtractOriginalUrl(response);
				AssertEquals("Should set redirect url from token scope", "http://google.com/", orignalUrl);
			}
		}

		public void TestVerifyEmailToken_ActivateUserAccountNewContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				var org = licence.Company.Header;
				org.Contacts.RemoveAndDeleteAll();
				var address = org.Addresses.AddNew();
				address.Address1 = "72 O'Riordan Street";
				var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
				clientBranch.LCB_LD = database.PK;
				clientBranch.LCB_Code = "SYD";
				clientBranch.LCB_OA = address.PK;
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = "US2";
				userAccount.EUA_FullName = "barny dino";
				userAccount.EUA_Email = "barny@dino.com";
				Factory.Save();
				const string url = "http://google.com";
				var infoNewContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}:{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenNewContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoNewContact, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenNewContact);
				page.DoPageLoad();
				page.DoVerifyEmail();
				AssertEquals("Email verification should be complete", false, userAccount.EUA_IsEmailVerificationRequired);
				AssertNotEquals("Should be linked to a new contact", ZGuid.Empty, userAccount.EUA_OC_WebAccessContact);
				AssertNotNull(userAccount.WebAccessContact);
				Assert("New contact should have empty password", userAccount.WebAccessContact.OC_PasswordHash.IsEmpty);
				AssertEquals("Contact relationship should have been activated", true, userAccount.EUA_IsContactRelationshipActive);
				AssertStartsWith("Should redirect to UpdateContactInformation page", "/webapp/Login/UpdateContactInformation.aspx", page.Response.RedirectLocation);
				var originalUrl = ExtractOriginalUrl(page.Response.RedirectLocation);
				AssertEquals("Should set redirect url from token scope", "http://google.com/", originalUrl);
			}
		}

		public void TestVerifyEmailToken_ShouldSetupSession()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				var org = licence.Company.Header;
				org.Contacts.RemoveAndDeleteAll();
				var address = org.Addresses.AddNew();
				address.Address1 = "72 O'Riordan Street";
				var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
				clientBranch.LCB_LD = database.PK;
				clientBranch.LCB_Code = "SYD";
				clientBranch.LCB_OA = address.PK;
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = "US1";
				userAccount.EUA_FullName = "sam test";
				userAccount.EUA_Email = "sam@test.com";
				Factory.Save();
				const string url = "http://google.com";
				var infoExistingContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}:{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenExistingContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoExistingContact, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForEnvironmentTest();
				page.Request.QueryString.Add("token", tokenExistingContact);
				Env.ClearUserContext();
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentUserPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentBranchPK);
				AssertEquals("Precondition: User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
				AssertNoExceptionThrown("Should not throw exception since environment should be setup first", () => {
					page.DoPageLoad();
					page.DoVerifyEmail();
				});
				AssertEquals("Email verification should be complete", false, userAccount.EUA_IsEmailVerificationRequired);
				AssertEquals("Should be linked to the contact", contact.PK, userAccount.EUA_OC_WebAccessContact);
				var response = page.Response.RedirectLocation;
				var orignalUrl = ExtractOriginalUrl(response);
				AssertEquals("Should set redirect url from token scope", "http://google.com/", orignalUrl);
			}
		}

		public void TestVerifyEmailToken_LoginContactWithNoPasswordShouldTriggerAccountReconfiguration()
		{
			//This test needs to change given we no longer have a page for account reconfig.
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "sam@test.com";
				contact.OC_WebAccessEnabled = true;
				var masterOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
				masterOrg.OH_Code = "SCWSYDBNE";
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_OC_WebAccessContact = contact.PK;
				userAccount.EUA_IsContactRelationshipActive = false;
				userAccount.Database.LD_OH_WebAccessOrg = masterOrg.PK;
				Factory.Save();
				const string url = "http://google.com";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				Assert("Precondition", contact.OC_PasswordHash.IsEmpty);
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				page.DoVerifyEmail();
				userAccount.Reload();
				AssertEquals("Contact relationship should have been activated", true, userAccount.EUA_IsContactRelationshipActive);
				AssertNotEquals("Contact should have been cloned by AccountReconfigurationRoutingDescriptor", contact.PK, userAccount.EUA_OC_WebAccessContact);
				AssertEquals("New contact should be in master org", masterOrg.PK, userAccount.WebAccessContact.OC_OH);
				AssertStartsWith("Should redirect to UpdateContactInformation page", "/webapp/Login/UpdateContactInformation.aspx", page.Response.RedirectLocation);
				var originalUrl = ExtractOriginalUrl(page.Response.RedirectLocation);
				AssertEquals("Should set redirect url from token scope", "http://google.com/", originalUrl);
			}
		}

		public void TestVerifyEmailToken_MergeContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "~f~";
				var oldContact = org.Contacts.AddNew();
				oldContact.OC_ContactName = "name 1";
				oldContact.OC_Email = "some@email.address";
				oldContact.OC_WebAccessEnabled = true;
				var contact1 = org.Contacts.AddNew();
				contact1.OC_ContactName = "name 1 (1)";
				contact1.OC_Email = "some@email.address";
				contact1.OC_WebAccessEnabled = false;
				var contact2 = org.Contacts.AddNew();
				contact2.OC_ContactName = "name 2";
				contact2.OC_Email = "nonunique@email.address";
				contact2.OC_WebAccessEnabled = true;
				contact2.SetHashedPassword("1234");
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				database.LD_OH_WebAccessOrg = org.PK;
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_OC_WebAccessContact = contact1.PK;
				userAccount.EUA_IsContactRelationshipActive = false;
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = "US1";
				userAccount.EUA_FullName = "name";
				userAccount.EUA_Email = "some@email.address";
				Factory.Save();
				contact2.Person.PER_EmailAddress = "person2@home.com";
				contact2.Person.SetHashedPassword("5678");
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"{LoginOptionsHelperForDistinctEmail.MergeAccountsVerificationKey}::{contact2.PK}::{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				page.DoVerifyEmail();
				userAccount.Reload();
				contact1.Reload();
				contact2.Reload();
				var routingDescriptor = new UserEmailVerificationRoutingDescriptor(new Uri(url), userAccount.Database, userAccount);
				AssertEquals("Precondition", false, routingDescriptor.IsRoutingRequired);
				AssertEquals("Contact relationship should have been activated", true, userAccount.EUA_IsContactRelationshipActive);
				AssertEquals("Contact relationship should have been activated", string.Empty, userAccount.EUA_ContactRelationshipStatus);
				AssertEquals("Web Access Contact should be updated to contact2", contact2.PK, userAccount.EUA_OC_WebAccessContact);
				AssertEquals("Email should be updated", true, userAccount.EUA_IsEmailOverridden);
				AssertNotEquals("Email should not match contact", contact2.OC_Email, userAccount.EUA_Email);
				AssertEquals("Should be deactivated", false, contact1.OC_IsActive);
				AssertEquals("Should be deactivated", false, contact1.OC_WebAccessEnabled);
				AssertEquals("Email should be removed", string.Empty, contact1.OC_Email);
				AssertStartsWith("Should redirect to LoginComplete", "/webapp/Login/LoginComplete.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestPageLoadShouldNotVerifyEmail()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (EDIDataRegistry.Instance.MyAccountIndexPage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://indexwise.com"))
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var org = Factory.NewWithValidTestData<EDIOrgHeader>();
				org.OH_Code = "SCWAAASYD";
				var contact = org.Contacts.AddNew();
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("1234");
				var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
				userAccount.EUA_OC_WebAccessContact = contact.PK;
				userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
				userAccount.EUA_Email = "sam@test.com";
				userAccount.Database.LD_OH_WebAccessOrg = org.PK;
				Factory.Save();
				const string url = "http://google.com/";
				var infoGood = new AccessTokenInfo(FormattableString.Invariant($"abc@gmail.com::{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
				var tokenGood = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.DistinctEmailToken, infoGood, TimeSpan.FromDays(1), 1);
				Factory.Save();
				var page = GetPageForTest();
				page.Request.QueryString.Add("token", tokenGood);
				page.DoPageLoad();
				AssertEquals("Status should not change", ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount.EUA_ContactRelationshipStatus);
				AssertEquals("sam@test.com", userAccount.EUA_Email);
				AssertEquals("Should not be redirected", null, page.Response.RedirectLocation);
			}
		}

		#endregion
		#region Verify Email and set EUA_IsEmailVerificationRequired
		public void TestVerifyEmailToken_ActivateTestUserAccount()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			var address = org.Addresses.AddNew();
			address.Address1 = "72 O'Riordan Street";
			var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch.LCB_LD = database.PK;
			clientBranch.LCB_Code = "SYD";
			clientBranch.LCB_OA = address.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "sam test";
			userAccount.EUA_Email = "sam@test.com";
			userAccount.EUA_IsEmailVerificationRequired = true;
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "barny dino";
			userAccount2.EUA_Email = "barny@dino.com";
			userAccount2.EUA_IsEmailVerificationRequired = true;
			var userAccount3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "quiney dino";
			userAccount3.EUA_Email = "quiney@dino.com";
			userAccount3.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			const string url = "http://google.com";
			var infoExistingContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}:{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenExistingContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoExistingContact, TimeSpan.FromDays(1), 1);
			var infoNewContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}"), userAccount2.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenNewContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoNewContact, TimeSpan.FromDays(1), 1);
			var page = GetPageForTest();
			page.Request.QueryString.Add("token", tokenExistingContact);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount.Reload();
			contact.Reload();
			AssertEquals("Should be connected to the existing contact", contact.PK, userAccount.EUA_OC_WebAccessContact);
			AssertEquals("Email verification should be complete", false, userAccount.EUA_IsEmailVerificationRequired);
			AssertEquals(true, userAccount.EUA_IsActive);
			AssertEquals(true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals(string.Empty, userAccount.EUA_ContactRelationshipStatus);
			page.Request.QueryString.Remove("token");
			page.Request.QueryString.Add("token", tokenNewContact);
			page.DoPageLoad();
			page.DoVerifyEmail();
			userAccount2.Reload();
			var newContact = userAccount2.WebAccessContact;
			AssertNotNull(newContact);
			AssertEquals("Email verification should be complete", false, userAccount2.EUA_IsEmailVerificationRequired);
			AssertEquals("Since the contact is newly created, the relationship is active", true, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("Should be populated from the user account", userAccount2.EUA_Email, newContact.OC_Email);
			AssertEquals("Should be linked to the licence's organisation", org.PK, newContact.OC_OH);
			AssertEquals(true, userAccount2.EUA_IsActive);
			AssertEquals(string.Empty, userAccount2.EUA_ContactRelationshipStatus);
		}

		[ExpectNoExceptions]
		public void TestVerifyEmailToken_ActivateTestUserAccount_NoOrg()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = Guid.Empty;
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "sam test";
			userAccount.EUA_Email = "sam@test.com";
			userAccount.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			const string url = "http://google.com";
			var infoExistingContact = new AccessTokenInfo(FormattableString.Invariant($"{UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey}:{url}"), userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			var tokenExistingContact = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.VerifyEmailToken, infoExistingContact, TimeSpan.FromDays(1), 1);
			var page = GetPageForTest();
			page.Request.QueryString.Add("token", tokenExistingContact);
			page.DoPageLoad();
			page.DoVerifyEmail();
		}

		#endregion
		#region Implementation
		static string ExtractOriginalUrl(string redirectUrl)
		{
			var redirectUri = new Uri(redirectUrl, UriKind.RelativeOrAbsolute);
			var uriDeconstructor = new UriDeconstructor(redirectUri);
			var query = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			var data = query[LoginRouter.QueryStringKey];
			var secureQueryString = new SecureQueryString(WebUtility.UrlDecode(data));
			var originalUrl = secureQueryString[LoginRouter.OriginalUrlQueryStringKey];
			return originalUrl;
		}

		protected override void SetUp()
		{
			base.SetUp();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
		}

		EmailVerificationForTest GetPageForTest()
		{
			var page = new EmailVerificationForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		EmailVerificationForTest GetPageForEnvironmentTest()
		{
			var page = new EmailVerificationForTest(true);
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class EmailVerificationForTest : EmailVerification
		{
			public EmailVerificationForTest(bool isEnvironmentTest = false)
			{
				IsEnvironmentTest = isEnvironmentTest;
			}

			bool IsEnvironmentTest { get; }

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

			public void DoVerifyEmail()
			{
				try
				{
					base.VerifyEmail(null, EventArgs.Empty);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException || ex is NullReferenceException)
					{
						throw;
					}
				}
			}

			protected override bool VerifyEmailTokenCore(AccessTokenInfo token)
			{
				if (IsEnvironmentTest)
				{
					var x = EnvProxy.Instance.CurrentUser.Initials;
				}

				return base.VerifyEmailTokenCore(token);
			}

			protected override bool VerifyDistinctEmailToken(AccessTokenInfo token)
			{
				if (IsEnvironmentTest)
				{
					var x = EnvProxy.Instance.CurrentUser.Initials;
				}

				return base.VerifyDistinctEmailToken(token);
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
