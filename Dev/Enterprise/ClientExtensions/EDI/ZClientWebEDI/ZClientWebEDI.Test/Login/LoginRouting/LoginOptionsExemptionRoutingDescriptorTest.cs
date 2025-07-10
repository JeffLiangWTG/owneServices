using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class LoginOptionsExemptionRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var user = Factory.New<EdiCustomerUserAccount>();
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(user);
			AssertNull("No routing page url", descriptor.RoutingUrl);
		}

		public void TestRoutingAction()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "COM", "SRV");
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = licence.Database.PK;
			user.EUA_UserID = "AAA";
			user.EUA_IsEmailVerificationRequired = false;
			user.EUA_IsContactRelationshipActive = false;
			Factory.Save();
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(user);
			descriptor.RoutingAction();
			user.Reload();
			AssertEquals("Contact relationship is activated", true, user.EUA_IsContactRelationshipActive);
		}

		#region IsRoutingRequired
		public void TestIsRoutingRequired_TestSystemWithLinkedProductionSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			Factory.Save();
			NewTestUser.ActivateUserAccountAndSave();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("User is allowed to activate contact relationship", true, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TrainingSystemWithLinkedProductionSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			NewTestUser.Database.LD_LicenceType = DatabaseTypes.Codes.Training;
			NewTestUser.Database.Factory.Save();
			Factory.Save();
			NewTestUser.ActivateUserAccountAndSave();
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("User is allowed to activate contact relationship", true, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithNoProductionSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.Delete();
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: No Production User Account", ProductionUser.IsDeleted);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("There is no production user account", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithInactiveRelationshipProductionSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_IsContactRelationshipActive = false;
			ProductionUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be inactive", !ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The production user account relationship is inactive", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithInactiveProductionSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be inactive", !ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The production user account is inactive", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithAnotherUnverifiedSystemAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ExistingTestUser.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Account should require verification", ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("Another test user account is pending email verification", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithProductionSystemWithDifferentEmailAndNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_Email = "blah@different.com";
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The linked production user has a different email", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithProductionSystemAndPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			TestContact.SetHashedPassword("1234");
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The linked contact has a password", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithProductionSystemAndPersonPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			TestContact.Person.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			TestContact.Person.PER_PasswordHashIterations = 9239;
			TestContact.Person.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should not be empty", !TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The linked contact has a personal password", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_ProductionSystemWithNoPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			NewTestUser.Database.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();
			AssertEquals("Precondition", false, NewTestUser.EUA_IsContactRelationshipActive);
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("The new user account is production", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_TestSystemWithLinkedProductionSystemAndPersonalPassword()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			TestContact.Person.PER_EmailAddress = "ala@carte.com";
			TestContact.Person.Factory.Save();
			Factory.Save();
			Assert("Precondition", TestContact.OC_PasswordHash.IsEmpty);
			Assert("Precondition: Personal Password should be empty", TestContact.Person.PER_PasswordHash.IsEmpty);
			Assert("Precondition: Production User Account should be active", ProductionUser.EUA_IsActive);
			Assert("Precondition: Production User Account Contact relationship should be active", ProductionUser.EUA_IsContactRelationshipActive);
			Assert("Precondition: Other Test User Accounts should be verified", !ExistingTestUser.EUA_IsEmailVerificationRequired);
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(NewTestUser);
			AssertEquals("They can verify their account via personal email", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_UserReactivatedWithNoPasswordAndStatus()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_IsActive = true;
			ProductionUser.EUA_IsContactRelationshipActive = false;
			ProductionUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			ProductionUser.WebAccessContact.OC_IsActive = true;
			ProductionUser.WebAccessContact.OC_WebAccessEnabled = true;
			ProductionUser.WebAccessContact.OC_PasswordHash = ZBlob.Empty;
			Factory.Save();
			TestConnection.ExecuteNonQuery($"update dbo.EdiCustomerUserAccount set EUA_ContactRelationshipStatus='' where EUA_PK='{ProductionUser.PK}'");
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(ProductionUser);
			AssertEquals("The product user is in fact reactivated with no password", true, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_MultipleUserAccountsLinked()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_IsActive = true;
			ProductionUser.EUA_IsContactRelationshipActive = false;
			ProductionUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			ProductionUser.WebAccessContact.OC_IsActive = true;
			ProductionUser.WebAccessContact.OC_WebAccessEnabled = true;
			ProductionUser.WebAccessContact.RemovePasswordAndHash();
			Factory.Save();
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(ProductionUser);
			AssertEquals("The product user should be eligible for reactivation", true, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_UserReactivatedMultipleContacts()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.EUA_IsActive = true;
			ProductionUser.EUA_IsContactRelationshipActive = false;
			ProductionUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			ProductionUser.WebAccessContact.OC_IsActive = true;
			ProductionUser.WebAccessContact.OC_WebAccessEnabled = true;
			ProductionUser.WebAccessContact.RemovePasswordAndHash();
			var otherCompanyCode = "XYZ";
			var otherOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			otherOrg.OH_RL_NKClosestPort = "AUSYD";
			otherOrg.OH_FullName = "DDD" + otherCompanyCode + " Company";
			otherOrg.OH_Code = "DDD" + otherCompanyCode;
			var otherContact = otherOrg.Contacts.AddNew();
			otherContact.OC_PER = TestContact.OC_PER;
			Factory.Save();
			var descriptor = new LoginOptionsExemptionRoutingDescriptor(ProductionUser);
			AssertEquals("User with multiple contacts on their person should not be eligible for reactivation", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_InactiveContactRelationshipWithNoAvailableLoginOptions()
		{
			SetupForVerifyEmailTokenSkipLoginOptions();
			ProductionUser.WebAccessContact.OC_IsActive = true;
			ProductionUser.WebAccessContact.OC_WebAccessEnabled = true;
			ProductionUser.WebAccessContact.RemovePasswordAndHash();
			ProductionUser.EUA_IsActive = true;
			Factory.Save();
			var statusCannotBypass = new string[] { ContactRelationshipStatusList.Codes.EmailChanged, ContactRelationshipStatusList.Codes.DistinctEmailRequired, ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, ContactRelationshipStatusList.Codes.SelfDeactivation };
			var statusCanBypass = new ContactRelationshipStatusList().Cast<CodeDescriptionPair>().Where(x => !statusCannotBypass.Contains(x.Code)).Select(x => x.Code);
			foreach (var status in statusCanBypass)
			{
				ProductionUser.EUA_IsContactRelationshipActive = false;
				ProductionUser.EUA_ContactRelationshipStatus = status;
				Factory.Save();
				var descriptor = new LoginOptionsExemptionRoutingDescriptor(ProductionUser);
				AssertEquals($"User with status {status} should be eligible for reactivation", true, descriptor.IsRoutingRequired);
			}
		}

		void SetupForVerifyEmailTokenSkipLoginOptions()
		{
			var enterpriseCode = "ENT";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "COM", "SRV");
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
			var testDatabase2 = licenceEnterprise.Databases.AddNew();
			testDatabase2.LD_ServerCode = "TD2";
			testDatabase2.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase2.LD_Product = ProductTypes.Codes.Enterprise;
			TestContact = org.Contacts.AddNew();
			TestContact.OC_Email = "sam@test.com";
			TestContact.OC_WebAccessEnabled = true;
			ProductionUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ProductionUser.EUA_LD = productionDatabase.PK;
			ProductionUser.EUA_UserID = "US1";
			ProductionUser.EUA_Email = TestContact.OC_Email;
			ProductionUser.EUA_OC_WebAccessContact = TestContact.PK;
			ProductionUser.EUA_IsContactRelationshipActive = true;
			ExistingTestUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ExistingTestUser.EUA_LD = testDatabase1.PK;
			ExistingTestUser.EUA_UserID = "US2";
			ExistingTestUser.EUA_Email = TestContact.OC_Email;
			ExistingTestUser.EUA_OC_WebAccessContact = TestContact.PK;
			ExistingTestUser.EUA_IsEmailVerificationRequired = false;
			ExistingTestUser.EUA_IsContactRelationshipActive = true;
			NewTestUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			NewTestUser.EUA_LD = testDatabase2.PK;
			NewTestUser.EUA_UserID = "US1";
			NewTestUser.EUA_Email = TestContact.OC_Email;
			NewTestUser.EUA_IsEmailVerificationRequired = true;
			NewTestUser.EUA_IsContactRelationshipActive = false;
			Factory.Save();
		}

		OrgContact TestContact;
		EdiCustomerUserAccount ProductionUser;
		EdiCustomerUserAccount ExistingTestUser;
		EdiCustomerUserAccount NewTestUser;
		#endregion
	}
}