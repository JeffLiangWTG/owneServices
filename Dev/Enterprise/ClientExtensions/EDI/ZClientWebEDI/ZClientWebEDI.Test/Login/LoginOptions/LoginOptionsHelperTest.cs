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
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(LoginOptionsHelper))]
	[HttpContextEnabledTest]
	internal class LoginOptionsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendEmailVerification()
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
			var helper = new LoginOptionsHelper(Factory, contact, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("sam20200109@wisetechglobal.com", helper.EmailAddress);
			AssertEquals("sa*****@w*****", helper.MaskedEmailAddress);
			helper.SendEmailVerification(null);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(contact.Person.PER_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.VerifyEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(userAccount.PK, accessToken.SAT_ParentId);
		}

		[HttpContextEnabledTest]
		public void TestProperties()
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
			var helper = new LoginOptionsHelper(Factory, contact, userAccount);
			AssertEquals(true, helper.IsValid);
			AssertEquals("sam20200109@wisetechglobal.com", helper.EmailAddress);
			AssertEquals("sa*****@w*****", helper.MaskedEmailAddress);
			var passwordVerificationResult = helper.VerifyPassword("xyz");
			AssertEquals(false, passwordVerificationResult);
			passwordVerificationResult = helper.VerifyPassword("1234");
			AssertEquals(true, passwordVerificationResult);
			userAccount.Reload();
			AssertEquals(true, userAccount.EUA_IsContactRelationshipActive);
			Assert(!userAccount.EUA_UserVerifiedDateUtc.IsEmpty);
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
			var helper = new LoginOptionsHelper(Factory, null, null);
			var passwordVerificationResult = helper.VerifyPassword("1234");
			Assert(!passwordVerificationResult);
			helper = new LoginOptionsHelper(Factory, contact, null);
			passwordVerificationResult = helper.VerifyPassword("1234");
			Assert(passwordVerificationResult);
			user1.Reload();
			user2.Reload();
			user3.Reload();
			Assert("User awaiting activation should be activated", user1.EUA_IsContactRelationshipActive);
			Assert("User awaiting activation should be activated", user2.EUA_IsContactRelationshipActive);
			Assert("User changed email should not be activated", !user3.EUA_IsContactRelationshipActive);
			AssertEquals("User awaiting activation should be activated", string.Empty, user1.EUA_ContactRelationshipStatus);
			AssertEquals("User awaiting activation should be activated", string.Empty, user2.EUA_ContactRelationshipStatus);
			AssertEquals("User changed email should not be activated", ContactRelationshipStatusList.Codes.EmailChanged, user3.EUA_ContactRelationshipStatus);
		}

		public void TestMaskEmailAddressShouldNotMaskInvalidEmail()
		{
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed(""));
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("  "));
			AssertEquals("Invalid email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("test"));
			AssertEquals("Valid email address", "te*****@t*****", LoginOptionsHelperForTest.MaskEmailAddressExposed("test@test.com"));
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("."));
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("@"));
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("test@c"));
			AssertEquals("Empty email address", string.Empty, LoginOptionsHelperForTest.MaskEmailAddressExposed("test@"));
		}

		class LoginOptionsHelperForTest : LoginOptionsHelper
		{
			public LoginOptionsHelperForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public static string MaskEmailAddressExposed(string email)
			{
				return MaskEmailAddress(email);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LoginOptionsHelper(Factory, null, null);
		}
	}
}
