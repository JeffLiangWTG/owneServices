using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	[TestedType(typeof(LoginOptionsHelperForDistinctEmail))]
	internal class LoginOptionsHelperForDistinctEmailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRegisterNewEmail_NoContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = org.PK;
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "sam test";
			userAccount.EUA_Email = "sam@test.com";
			userAccount.EUA_IsEmailVerificationRequired = false;
			Factory.Save();
			var uri = new Uri("http://wwww.cw1.com/autologin.aspx?u=1&pwd=2");
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, null, userAccount);
			AssertEquals(false, userAccount.EUA_IsEmailOverridden);
			helper.RegisterNewEmail("anton@test.com", uri);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("anton@test.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals(true, userAccount.EUA_IsEmailOverridden);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.DistinctEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(userAccount.PK, accessToken.SAT_ParentId);
		}

		public void TestRegisterNewEmail_NoDuplicates()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = Guid.Empty;
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "sam test";
			userAccount.EUA_Email = "sam@test.com";
			userAccount.EUA_IsEmailVerificationRequired = false;
			Factory.Save();
			contact.Person.PER_EmailAddress = "personal@test.com";
			Factory.Save();
			var uri = new Uri("http://wwww.cw1.com/autologin.aspx?u=1&pwd=2");
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, contact, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("personal@test.com", helper.EmailAddress);
			AssertEquals(false, userAccount.EUA_IsEmailOverridden);
			helper.RegisterNewEmail("anton@test.com", uri);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("anton@test.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals(true, userAccount.EUA_IsEmailOverridden);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.DistinctEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(userAccount.PK, accessToken.SAT_ParentId);
		}

		public void TestRegisterNewEmail_NoDuplicates_OtherOrg()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			contact.OC_ContactName = "name 1";
			var otherOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			otherOrg.OH_Code = "ANTAAASYD";
			var otherContact = otherOrg.Contacts.AddNew();
			otherContact.OC_Email = "anton@test.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.SetHashedPassword("1234");
			otherContact.OC_ContactName = "name 2";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			database.LD_OH_WebAccessOrg = Guid.Empty;
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "sam test";
			userAccount.EUA_Email = "sam@test.com";
			userAccount.EUA_IsEmailVerificationRequired = false;
			Factory.Save();
			contact.Person.PER_EmailAddress = "personal@test.com";
			Factory.Save();
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, contact, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("personal@test.com", helper.EmailAddress);
			helper.RegisterNewEmail("anton@test.com", new Uri("http://wwww.cw1.com/autologin.aspx?u=1&pwd=2"));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("anton@test.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.DistinctEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(userAccount.PK, accessToken.SAT_ParentId);
		}

		public void TestRegisterNewEmail_Duplicates_SameOrg()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "SCWAAASYD";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "sam@test.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword("1234");
			contact1.OC_ContactName = "name 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "anton@test.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("5678");
			contact2.OC_ContactName = "name 2";
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact1.PK;
			userAccount.EUA_IsContactRelationshipActive = false;
			Factory.Save();
			contact1.Person.PER_EmailAddress = "personal@test.com";
			Factory.Save();
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, contact1, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("personal@test.com", helper.EmailAddress);
			helper.RegisterNewEmail("anton@test.com", new Uri("http://wwww.cw1.com/autologin.aspx?u=1&pwd=2"));
			AssertEquals("Duplicate found, not sending emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailVerificationForDistinctEmailPage()
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
			Factory.Save();
			contact.Person.PER_EmailAddress = "sam20200109@wisetechglobal.com";
			Factory.Save();
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, contact, userAccount);
			helper.SetContactAndUserAccountValues(contact, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("sam20200109@wisetechglobal.com", helper.EmailAddress);
			AssertEquals("sa*****@w*****", helper.MaskedEmailAddress);
			var originalUrlString = "https://google.com.au/";
			var originalUrl = new Uri(originalUrlString);
			helper.SendEmailVerification(originalUrl);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(contact.Person.PER_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.VerifyEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(userAccount.PK, accessToken.SAT_ParentId);
			AssertEquals(LoginOptionsHelperForDistinctEmail.MergeAccountsVerificationKey + "::" + contact.PK + "::" + originalUrlString, accessToken.SAT_Scope);
		}

		[HttpContextEnabledTest]
		public void TestVerifyPassword()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var db = licence.Database;
			var user1 = Factory.New<EdiCustomerUserAccount>();
			user1.EUA_LD = db.PK;
			user1.EUA_UserID = "TST";
			user1.EUA_FullName = "Test User";
			user1.EUA_OC_WebAccessContact = contact.PK;
			user1.EUA_IsContactRelationshipActive = false;
			user1.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			var user2 = Factory.New<EdiCustomerUserAccount>();
			user2.EUA_LD = db.PK;
			user2.EUA_UserID = "TS2";
			user2.EUA_FullName = "Test User 2";
			user2.EUA_OC_WebAccessContact = contact.PK;
			user2.EUA_IsContactRelationshipActive = false;
			user2.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.ProductDeactivation;
			var user3 = Factory.New<EdiCustomerUserAccount>();
			user3.EUA_LD = db.PK;
			user3.EUA_UserID = "TS3";
			user3.EUA_FullName = "Test User 3";
			user3.EUA_OC_WebAccessContact = contact.PK;
			user3.EUA_IsContactRelationshipActive = false;
			user3.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			Factory.Save();
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var helper = new LoginOptionsHelperForDistinctEmail(Factory, null, null);
			helper.SetContactAndUserAccountValues(null, null);
			var passwordVerificationResult = helper.VerifyPassword("1234");
			Assert(!passwordVerificationResult);
			helper = new LoginOptionsHelperForDistinctEmail(Factory, contact, null);
			helper.SetContactAndUserAccountValues(contact, null);
			passwordVerificationResult = helper.VerifyPassword("1234");
			Assert(passwordVerificationResult);
			user1.Reload();
			user2.Reload();
			user3.Reload();
			Assert("User awaiting activation should not be activated", !user1.EUA_IsContactRelationshipActive);
			Assert("User awaiting activation should not be activated", !user2.EUA_IsContactRelationshipActive);
			Assert("User changed email should not be activated", !user3.EUA_IsContactRelationshipActive);
			AssertEquals("User awaiting activation should not be activated", ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked, user1.EUA_ContactRelationshipStatus);
			AssertEquals("User awaiting activation should not be activated", ContactRelationshipStatusList.Codes.ProductDeactivation, user2.EUA_ContactRelationshipStatus);
			AssertEquals("User changed email should not be activated", ContactRelationshipStatusList.Codes.EmailChanged, user3.EUA_ContactRelationshipStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LoginOptionsHelperForDistinctEmail(Factory, null, null);
		}
	}
}
