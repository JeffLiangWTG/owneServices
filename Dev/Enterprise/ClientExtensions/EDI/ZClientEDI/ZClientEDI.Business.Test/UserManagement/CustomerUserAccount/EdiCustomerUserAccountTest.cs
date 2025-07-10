namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using System;
	using System.Data;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Client.EDI.Billing.Business.Test;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.Client.EDI.Licencing.Business;
	using Enterprise.Client.EDI.MasterFiles.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(EdiCustomerUserAccount))]
	public class EdiCustomerUserAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEUA_IsEmailVerificationRequiredShouldDefaultToFalse()
		{
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			AssertEquals("Default should be false", false, userAccount.EUA_IsEmailVerificationRequired);
		}

		public void TestHasAcknowledgedUserAgreement()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			AssertEquals(false, userAccount.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(false, userAccount.HasAcknowledgedUserAgreement(agreement2));

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_EUA = userAccount.PK;
			agreementLog.EUL_ERA = agreement1.PK;

			AssertEquals(true, userAccount.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(false, userAccount.HasAcknowledgedUserAgreement(agreement2));
		}

		public void TestGetAndUpdateUserAccountForWebAccess()
		{
			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			Factory.Save();

			var userAccount = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, db, "U001");
			AssertNull(userAccount);

			var newUser = Factory.New<EdiCustomerUserAccount>();
			newUser.EUA_LD = db.PK;
			newUser.EUA_UserID = "U001";
			newUser.EUA_FullName = "User One";
			newUser.EUA_Email = "user.one@test.com";
			Factory.Save();

			userAccount = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, db, "U001");
			AssertNotNull(userAccount);
			AssertEquals(newUser.PK, userAccount.PK);
		}

		public void TestDeactivationOfProductionUserShouldAlsoDeactivateTestUserAccountContactRelationshipsOnSaving()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var productionDatabase = licence.Database;
			productionDatabase.LD_DatabaseNumber = 3001;
			var testDatabaseSameProduct = licence.Company.LicDatabases.AddNew();
			testDatabaseSameProduct.LD_ServerCode = "ALP";
			testDatabaseSameProduct.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabaseSameProduct.LD_Product = ProductTypes.Codes.Enterprise;
			var testDatabaseDiffProduct = licence.Company.LicDatabases.AddNew();
			testDatabaseDiffProduct.LD_ServerCode = "BNE";
			testDatabaseDiffProduct.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabaseDiffProduct.LD_Product = ProductTypes.Codes.CargoWiseOne;

			var trainingDatabaseSameProduct = licence.Company.LicDatabases.AddNew();
			trainingDatabaseSameProduct.LD_ServerCode = "MLB";
			trainingDatabaseSameProduct.LD_LicenceType = DatabaseTypes.Codes.Training;
			trainingDatabaseSameProduct.LD_Product = ProductTypes.Codes.Enterprise;

			var org = productionDatabase.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var productionUser = Factory.New<EdiCustomerUserAccount>();
			productionUser.EUA_LD = productionDatabase.PK;
			productionUser.EUA_UserID = "PU1";
			productionUser.EUA_Email = "aaa@aaa.com";
			productionUser.EUA_FullName = "ooo";
			productionUser.EUA_IsActive = true;
			productionUser.EUA_OC_WebAccessContact = contact.PK;

			var testSystemUserAccountSameProduct = Factory.New<EdiCustomerUserAccount>();
			testSystemUserAccountSameProduct.EUA_LD = testDatabaseSameProduct.PK;
			testSystemUserAccountSameProduct.EUA_UserID = "AE1";
			testSystemUserAccountSameProduct.EUA_Email = "aaa@aaa.com";
			testSystemUserAccountSameProduct.EUA_FullName = "ooo";
			testSystemUserAccountSameProduct.EUA_IsActive = true;
			testSystemUserAccountSameProduct.EUA_OC_WebAccessContact = contact.PK;

			var testSystemUserAccountDifferentProduct = Factory.New<EdiCustomerUserAccount>();
			testSystemUserAccountDifferentProduct.EUA_LD = testDatabaseDiffProduct.PK;
			testSystemUserAccountDifferentProduct.EUA_UserID = "AE2";
			testSystemUserAccountDifferentProduct.EUA_Email = "aaa@aaa.com";
			testSystemUserAccountDifferentProduct.EUA_FullName = "ooo";
			testSystemUserAccountDifferentProduct.EUA_IsActive = true;
			testSystemUserAccountDifferentProduct.EUA_OC_WebAccessContact = contact.PK;

			var trainingSystemUserAccountSameProduct = Factory.New<EdiCustomerUserAccount>();
			trainingSystemUserAccountSameProduct.EUA_LD = trainingDatabaseSameProduct.PK;
			trainingSystemUserAccountSameProduct.EUA_UserID = "AE3";
			trainingSystemUserAccountSameProduct.EUA_Email = "aaa@aaa.com";
			trainingSystemUserAccountSameProduct.EUA_FullName = "ooo";
			trainingSystemUserAccountSameProduct.EUA_IsActive = true;
			trainingSystemUserAccountSameProduct.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			productionUser.EUA_IsActive = false;
			AssertEquals("Should not be deactivated until save", true, testSystemUserAccountSameProduct.EUA_IsActive);
			AssertEquals("Should not be deactivated until save", true, trainingSystemUserAccountSameProduct.EUA_IsActive);
			AssertEquals("Should not be deactivated until save", true, contact.OC_IsActive);

			Factory.Save();

			AssertEquals("Should now be deactivated", false, testSystemUserAccountSameProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should be set to Product Deactivation", ContactRelationshipStatusList.Codes.ProductDeactivation, testSystemUserAccountSameProduct.EUA_ContactRelationshipStatus);
			AssertEquals("Should now be deactivated", false, trainingSystemUserAccountSameProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should be set to Product Deactivation", ContactRelationshipStatusList.Codes.ProductDeactivation, trainingSystemUserAccountSameProduct.EUA_ContactRelationshipStatus);

			AssertEquals("Should not be affected since it has a different product", true, testSystemUserAccountDifferentProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should not be deactivated since a test user is still active", true, contact.OC_IsActive);
		}

		public void TestDeactivationOfTestUserShouldNotDeactivateOtherTestUserAccountContactRelationshipsOnSaving()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var testDatabase1 = licence.Database;
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var testDatabase2 = licence.Company.LicDatabases.AddNew();
			testDatabase2.LD_ServerCode = "ALP";
			testDatabase2.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase2.LD_Product = ProductTypes.Codes.Enterprise;

			var org = testDatabase1.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var testSystem1User = Factory.New<EdiCustomerUserAccount>();
			testSystem1User.EUA_LD = testDatabase1.PK;
			testSystem1User.EUA_UserID = "AE1";
			testSystem1User.EUA_Email = "aaa@aaa.com";
			testSystem1User.EUA_FullName = "aaa";
			testSystem1User.EUA_IsActive = true;
			testSystem1User.EUA_OC_WebAccessContact = contact.PK;

			var testSystem2User = Factory.New<EdiCustomerUserAccount>();
			testSystem2User.EUA_LD = testDatabase2.PK;
			testSystem2User.EUA_UserID = "AE2";
			testSystem2User.EUA_Email = "aaa@aaa.com";
			testSystem2User.EUA_FullName = "aaa";
			testSystem2User.EUA_IsActive = true;
			testSystem2User.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			testSystem1User.EUA_IsActive = false;
			Factory.Save();

			AssertEquals("Deactivation of another test user should not deactivate this one", true, testSystem2User.EUA_IsActive);
			AssertEquals("Should not be deactivated", true, contact.OC_IsActive);
		}

		public void TestDeactivatingLastTestUserShouldDeactivateContactOnSaving()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var testDatabase1 = licence.Database;
			testDatabase1.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase1.LD_Product = ProductTypes.Codes.Enterprise;
			var testDatabase2 = licence.Company.LicDatabases.AddNew();
			testDatabase2.LD_ServerCode = "ALP";
			testDatabase2.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase2.LD_Product = ProductTypes.Codes.Enterprise;

			var org = testDatabase1.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var testSystem1User = Factory.New<EdiCustomerUserAccount>();
			testSystem1User.EUA_LD = testDatabase1.PK;
			testSystem1User.EUA_UserID = "AE1";
			testSystem1User.EUA_Email = "aaa@aaa.com";
			testSystem1User.EUA_FullName = "aaa";
			testSystem1User.EUA_IsActive = true;
			testSystem1User.EUA_OC_WebAccessContact = contact.PK;

			var testSystem2User = Factory.New<EdiCustomerUserAccount>();
			testSystem2User.EUA_LD = testDatabase2.PK;
			testSystem2User.EUA_UserID = "AE2";
			testSystem2User.EUA_Email = "aaa@aaa.com";
			testSystem2User.EUA_FullName = "aaa";
			testSystem2User.EUA_IsActive = true;
			testSystem2User.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			testSystem1User.EUA_IsActive = false;
			Factory.Save();

			AssertEquals("Precondition: Deactivation of another test user should not deactivate this one", true, testSystem2User.EUA_IsActive);
			AssertEquals("Should not be deactivated", true, contact.OC_IsActive);

			testSystem2User.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should be deactivated", false, contact.OC_IsActive);
		}

		public void TestDeactivationOfProductionUserShouldNotDeactivateTestUserAccountContactRelationshipsOnSavingIfOtherActiveProductionUsersExist()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var productionDatabase = licence.Database;
			productionDatabase.LD_DatabaseNumber = 3001;
			var productionDatabase2 = licence.Company.LicDatabases.AddNew();
			productionDatabase2.LD_ServerCode = "BNE";
			productionDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase2.LD_Product = ProductTypes.Codes.Enterprise;
			var productionDatabase3 = licence.Company.LicDatabases.AddNew();
			productionDatabase3.LD_ServerCode = "ACT";
			productionDatabase3.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase3.LD_Product = ProductTypes.Codes.Enterprise;
			var testDatabaseSameProduct = licence.Company.LicDatabases.AddNew();
			testDatabaseSameProduct.LD_ServerCode = "ALP";
			testDatabaseSameProduct.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabaseSameProduct.LD_Product = ProductTypes.Codes.Enterprise;

			var org = productionDatabase.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var productionUser = Factory.New<EdiCustomerUserAccount>();
			productionUser.EUA_LD = productionDatabase.PK;
			productionUser.EUA_UserID = "PU1";
			productionUser.EUA_Email = "ooo@aaa.com";
			productionUser.EUA_FullName = "ooo";
			productionUser.EUA_IsActive = true;
			productionUser.EUA_OC_WebAccessContact = contact.PK;

			var productionUser2 = Factory.New<EdiCustomerUserAccount>();
			productionUser2.EUA_LD = productionDatabase2.PK;
			productionUser2.EUA_UserID = "PU2";
			productionUser2.EUA_Email = "eee@aaa.com";
			productionUser2.EUA_FullName = "eee";
			productionUser2.EUA_IsActive = true;
			productionUser2.EUA_OC_WebAccessContact = contact.PK;

			var productionUser3 = Factory.New<EdiCustomerUserAccount>();
			productionUser3.EUA_LD = productionDatabase3.PK;
			productionUser3.EUA_UserID = "PU3";
			productionUser3.EUA_Email = "ddd@aaa.com";
			productionUser3.EUA_FullName = "ddd";
			productionUser3.EUA_IsActive = true;
			productionUser3.EUA_OC_WebAccessContact = contact.PK;

			var testSystemUserAccountSameProduct = Factory.New<EdiCustomerUserAccount>();
			testSystemUserAccountSameProduct.EUA_LD = testDatabaseSameProduct.PK;
			testSystemUserAccountSameProduct.EUA_UserID = "TU1";
			testSystemUserAccountSameProduct.EUA_Email = "aaa@aaa.com";
			testSystemUserAccountSameProduct.EUA_FullName = "aaa";
			testSystemUserAccountSameProduct.EUA_IsActive = true;
			testSystemUserAccountSameProduct.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should not be deactivated since another active production user exists", true, testSystemUserAccountSameProduct.EUA_IsActive);
			AssertEquals("Should not be deactivated", true, contact.OC_IsActive);

			productionUser.EUA_IsActive = true;
			productionDatabase2.LD_Product = ProductTypes.Codes.CargoWiseOne;
			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should be unchanged since production user 3 is still active", true, testSystemUserAccountSameProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should not be deactivated", true, contact.OC_IsActive);

			productionUser.EUA_IsActive = true;
			productionUser3.EUA_IsContactRelationshipActive = false;
			productionUser3.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should now be deactivated since the other production users either have inactive relationships or have a different product", false, testSystemUserAccountSameProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should be set to Product Deactivation", ContactRelationshipStatusList.Codes.ProductDeactivation, testSystemUserAccountSameProduct.EUA_ContactRelationshipStatus);
			AssertEquals("Should not be deactivated since productionUser2 is still active", true, contact.OC_IsActive);
		}

		public void TestDeactivationOfProductionUserShouldDeactivateTestUserAccountContactRelationshipsOnSavingIfOtherProductionUserHasInactiveRelationship()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var productionDatabase = licence.Database;
			productionDatabase.LD_DatabaseNumber = 3001;
			var productionDatabase2 = licence.Company.LicDatabases.AddNew();
			productionDatabase2.LD_ServerCode = "ACT";
			productionDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase2.LD_Product = ProductTypes.Codes.Enterprise;
			var testDatabaseSameProduct = licence.Company.LicDatabases.AddNew();
			testDatabaseSameProduct.LD_ServerCode = "ALP";
			testDatabaseSameProduct.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabaseSameProduct.LD_Product = ProductTypes.Codes.Enterprise;

			var org = productionDatabase.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var productionUser = Factory.New<EdiCustomerUserAccount>();
			productionUser.EUA_LD = productionDatabase.PK;
			productionUser.EUA_UserID = "PU1";
			productionUser.EUA_Email = "ooo@aaa.com";
			productionUser.EUA_FullName = "ooo";
			productionUser.EUA_IsActive = true;
			productionUser.EUA_OC_WebAccessContact = contact.PK;

			var productionUser2 = Factory.New<EdiCustomerUserAccount>();
			productionUser2.EUA_LD = productionDatabase2.PK;
			productionUser2.EUA_UserID = "PU3";
			productionUser2.EUA_Email = "ddd@aaa.com";
			productionUser2.EUA_FullName = "ddd";
			productionUser2.EUA_IsActive = true;
			productionUser2.EUA_OC_WebAccessContact = contact.PK;

			var testSystemUserAccountSameProduct = Factory.New<EdiCustomerUserAccount>();
			testSystemUserAccountSameProduct.EUA_LD = testDatabaseSameProduct.PK;
			testSystemUserAccountSameProduct.EUA_UserID = "TU1";
			testSystemUserAccountSameProduct.EUA_Email = "aaa@aaa.com";
			testSystemUserAccountSameProduct.EUA_FullName = "aaa";
			testSystemUserAccountSameProduct.EUA_IsActive = true;
			testSystemUserAccountSameProduct.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should not be deactivated since another active production user exists", true, testSystemUserAccountSameProduct.EUA_IsActive);

			productionUser.EUA_IsActive = true;
			productionUser2.EUA_IsContactRelationshipActive = false;
			productionUser2.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should now be deactivated since the other production user has inactive contact relationship", false, testSystemUserAccountSameProduct.EUA_IsContactRelationshipActive);
			AssertEquals("Should be set to Product Deactivation", ContactRelationshipStatusList.Codes.ProductDeactivation, testSystemUserAccountSameProduct.EUA_ContactRelationshipStatus);
			AssertEquals("Should be deactivated since no user accounts have an active contact relationship to this contact", false, contact.OC_IsActive);
		}

		public void TestActivateContactRelationshipAndSave()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.WebAccessOrg;
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "new.u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "EMC";
			Factory.Save();

			userAccount1.ActivateContactRelationshipAndSave();
			AssertEquals(false, contact1.HasChanges);
			AssertEquals(false, userAccount1.HasChanges);
			AssertEquals(true, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("", userAccount1.EUA_ContactRelationshipStatus);
			AssertEquals("new.u1@cw1.com", contact1.OC_Email);
		}

		public void TestRelatedProperties()
		{
			var ediCustomerUserAccount = Factory.New<EdiCustomerUserAccount>();

			AssertNull(ediCustomerUserAccount.DatabaseLicenceEnterprise);
			AssertNull(ediCustomerUserAccount.EDIWebAccessContact);
			AssertNull(ediCustomerUserAccount.ContactOrganisation);
			AssertNull(ediCustomerUserAccount.ContactPerson);

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var licenceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "ABC";
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			ediCustomerUserAccount.EUA_LD = licenceDatabase.PK;

			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Test Name";
			var person = Factory.NewWithValidTestData<EDIGlbPerson>();
			person.PER_FullName = "Test Person Name";
			contact.OC_PER = person.PK;
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "Test Organisation Name";
			contact.OC_OH = org.PK;
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;

			AssertNotNull(ediCustomerUserAccount.DatabaseLicenceEnterprise);
			AssertEquals("ABC", ediCustomerUserAccount.DatabaseLicenceEnterprise.LE_EnterpriseCode);
			AssertNotNull(ediCustomerUserAccount.EDIWebAccessContact);
			AssertEquals("Test Name", ediCustomerUserAccount.EDIWebAccessContact.OC_ContactName);
			AssertNotNull(ediCustomerUserAccount.ContactOrganisation);
			AssertEquals("Test Organisation Name", ediCustomerUserAccount.ContactOrganisation.OH_FullName);
			AssertNotNull(ediCustomerUserAccount.ContactPerson);
			AssertEquals("Test Person Name", ediCustomerUserAccount.ContactPerson.PER_FullName);
		}

		public void TestAccountVerificationStatus()
		{
			var ediCustomerUserAccount = Factory.New<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_IsContactRelationshipActive = true;

			AssertEquals("Verified", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
			AssertEquals("Verification Required (Distinct Email Required)", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			AssertEquals("Verification Required (User ID Reassigned)", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
			AssertEquals("Verification Required (New System Pairing)", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			AssertEquals("Verification Required (User ID Reactivated)", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_ContactRelationshipStatus = string.Empty;
			ediCustomerUserAccount.EUA_IsContactRelationshipActive = false;
			AssertEquals("Account Requires Verification", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_IsEmailVerificationRequired = true;
			AssertEquals("Pending Email Verification", ediCustomerUserAccount.AccountVerificationStatus);

			ediCustomerUserAccount.EUA_IsActive = false;
			AssertEquals("User ID Deactivated", ediCustomerUserAccount.AccountVerificationStatus);
		}

		public void TestIsAwaitingActivation()
		{
			var user = Factory.New<EdiCustomerUserAccount>();
			Assert(!user.IsAwaitingActivation);

			user.EUA_IsContactRelationshipActive = false;
			Assert(!user.IsAwaitingActivation);

			var statusNotActivation = new string[]
			{
				ContactRelationshipStatusList.Codes.EmailChanged,
				ContactRelationshipStatusList.Codes.DistinctEmailRequired,
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword,
				ContactRelationshipStatusList.Codes.SelfDeactivation
			};

			foreach (CodeDescriptionPair statusPair in new ContactRelationshipStatusList())
			{
				user.EUA_ContactRelationshipStatus = statusPair.Code;
				if (statusNotActivation.Contains(statusPair.Code))
				{
					Assert(!user.IsAwaitingActivation);
				}
				else
				{
					Assert(user.IsAwaitingActivation);
				}
			}

			var contact = Factory.New<OrgContact>();
			user.EUA_OC_WebAccessContact = contact.PK;
			user.EUA_ContactRelationshipStatus = string.Empty;
			Assert(user.IsAwaitingActivation);
		}

		public void TestActivateUserAccountAndSave()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "new.u1@cw1.com";
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "MUL";

			userAccount1.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Should activate since contact is not in db", true, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Should remove status", string.Empty, userAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestActivateUserAccountAndSave_RecalculateDistinctEmailRequiredStatus()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "new.u1@cw1.com";
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "DER";

			userAccount1.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Should activate", true, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Should remove 'DER'", "", userAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestActivateUserAccountAndSave_WithContactInDb()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
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

			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			Factory.Save();

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = testDatabase1.PK;
			userAccount2.EUA_UserID = "U01";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "u1@cw1.com";
			userAccount2.EUA_IsContactRelationshipActive = false;

			userAccount2.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount2.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Should associate to existing contact", contact1.PK, userAccount2.EUA_OC_WebAccessContact);
			AssertEquals("Should be active relationship", true, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("Should be blank", string.Empty, userAccount2.EUA_ContactRelationshipStatus);
		}

		public void TestActivateUserAccountAndSave_ContactInDbWithExistingActiveUserAccount()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
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
			testDatabase1.LD_OH_WebAccessOrg = org.PK;

			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = productionDatabase.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsContactRelationshipActive = true;
			Factory.Save();

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = testDatabase1.PK;
			userAccount2.EUA_UserID = "U01";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "u1@cw1.com";

			userAccount2.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount2.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Should associate to existing contact", contact1.PK, userAccount2.EUA_OC_WebAccessContact);
			AssertEquals("Should not activate relationship since there is an active account", false, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("Status should get set from Importer", ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked, userAccount2.EUA_ContactRelationshipStatus);
		}

		public void TestActivateUserAccountAndSave_ContactInDbWithExistingInactiveUserAccountNoPassword()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
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
			testDatabase1.LD_OH_WebAccessOrg = org.PK;

			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.OC_IsActive = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = productionDatabase.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "EMC";
			Factory.Save();

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = testDatabase1.PK;
			userAccount2.EUA_UserID = "U01";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "u1@cw1.com";
			userAccount2.EUA_IsContactRelationshipActive = false;
			userAccount2.EUA_ContactRelationshipStatus = "EMC";

			AssertEquals("Precondition", false, contact1.OC_IsActive);
			AssertEquals("Precondition", false, contact1.HasPassword);

			userAccount2.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount2.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Should associate to existing contact", contact1.PK, userAccount2.EUA_OC_WebAccessContact);
			AssertEquals("Should activate since there are no other active accounts", true, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("Should remove status", string.Empty, userAccount2.EUA_ContactRelationshipStatus);
			AssertEquals("Should activate contact", true, contact1.OC_IsActive);
		}

		public void TestActivateUserAccountAndSave_ContactInDbWithExistingInactiveUserAccountExistingPassword()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
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
			testDatabase1.LD_OH_WebAccessOrg = org.PK;

			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.SetHashedPassword("1234");

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = productionDatabase.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "EMC";
			Factory.Save();

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = testDatabase1.PK;
			userAccount2.EUA_UserID = "U01";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "u1@cw1.com";

			AssertEquals("Precondition", true, contact1.HasPassword);

			userAccount2.ActivateUserAccountAndSave();
			AssertEquals("Should activate", false, userAccount2.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Should associate to existing contact", contact1.PK, userAccount2.EUA_OC_WebAccessContact);
			AssertEquals("Should not activate since there is already a password", false, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("Status should get set from Importer", ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked, userAccount2.EUA_ContactRelationshipStatus);
		}

		public void TestCantActivateUserWhenLicenceDatabaseInactive()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));

			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";

			var licenceDatabase = licence.Database;
			licenceDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			licenceDatabase.LD_OH_WebAccessOrg = org.PK;

			var licenceDatabaseInactive = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabaseInactive.LD_LicenceType = DatabaseTypes.Codes.Production;
			licenceDatabaseInactive.LD_OH_WebAccessOrg = org.PK;
			licenceDatabaseInactive.LD_IsActive = false;

			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			licenceDatabase2.LD_OH_WebAccessOrg = org.PK;

			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";

			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			contact2.OC_ContactName = "User 2";
			contact2.OC_Email = "u2@cw1.com";

			var contact3 = (EDIOrgContact)org.Contacts.AddNew();
			contact3.OC_ContactName = "User 3";
			contact3.OC_Email = "u3@cw1.com";

			var userAccountActiveLD = Factory.New<EdiCustomerUserAccount>();
			userAccountActiveLD.EUA_LD = licenceDatabase.PK;
			userAccountActiveLD.EUA_OC_WebAccessContact = contact1.PK;
			userAccountActiveLD.EUA_UserID = "User 1";

			var userAccountInctiveLD = Factory.New<EdiCustomerUserAccount>();
			userAccountInctiveLD.EUA_LD = licenceDatabaseInactive.PK;
			userAccountInctiveLD.EUA_OC_WebAccessContact = contact3.PK;
			userAccountInctiveLD.EUA_UserID = "User 3";

			Factory.Save();

			var userAccountNotInDatabase = Factory.New<EdiCustomerUserAccount>();
			userAccountNotInDatabase.EUA_LD = licenceDatabaseInactive.PK;
			userAccountNotInDatabase.EUA_OC_WebAccessContact = ((EDIOrgContact)org.Contacts.AddNew()).PK;

			AssertEquals("UserAccount with active LD should be active", true, userAccountActiveLD.EUA_IsActive);
			AssertEquals("UserAccount with inactive LD should be inactive", false, userAccountInctiveLD.EUA_IsActive);
			AssertEquals("UserAccount not in database should be active", true, userAccountNotInDatabase.EUA_IsActive);

			userAccountActiveLD.EUA_IsActive = false;
			userAccountInctiveLD.EUA_IsActive = false;
			userAccountNotInDatabase.EUA_IsActive = false;

			AssertEquals("UserAccount with active LD should be inactive", false, userAccountActiveLD.EUA_IsActive);
			AssertEquals("UserAccount with inactive LD should be inactive", false, userAccountInctiveLD.EUA_IsActive);
			AssertEquals("UserAccount not in database should be inactive", false, userAccountNotInDatabase.EUA_IsActive);

			userAccountActiveLD.EUA_IsActive = true;
			userAccountInctiveLD.EUA_IsActive = true;
			userAccountNotInDatabase.EUA_IsActive = true;

			AssertEquals("UserAccount with active LD should be active", true, userAccountActiveLD.EUA_IsActive);
			AssertEquals("UserAccount with inactive LD should be inactive", false, userAccountInctiveLD.EUA_IsActive);
			AssertEquals("UserAccount not in database should be inactive", false, userAccountNotInDatabase.EUA_IsActive);
		}

		public void TestDeactivatingLastTestUserShouldDeactivateWiseTechAcademyUser()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var productionDatabase = licence.Database;
			productionDatabase.LD_DatabaseNumber = 3001;
			var productionDatabase2 = licence.Company.LicDatabases.AddNew();
			productionDatabase2.LD_ServerCode = "ACT";
			productionDatabase2.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase2.LD_Product = ProductTypes.Codes.WiseTechAcademy;

			var org = productionDatabase.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var contact = org.Contacts.AddNew();

			var productionUser = Factory.New<EdiCustomerUserAccount>();
			productionUser.EUA_LD = productionDatabase.PK;
			productionUser.EUA_UserID = "PU1";
			productionUser.EUA_Email = "ooo@aaa.com";
			productionUser.EUA_FullName = "ooo";
			productionUser.EUA_IsActive = true;
			productionUser.EUA_OC_WebAccessContact = contact.PK;

			var productionUser2 = Factory.New<EdiCustomerUserAccount>();
			productionUser2.EUA_LD = productionDatabase2.PK;
			productionUser2.EUA_UserID = "PU3";
			productionUser2.EUA_Email = "ddd@aaa.com";
			productionUser2.EUA_FullName = "ddd";
			productionUser2.EUA_IsActive = true;
			productionUser2.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			productionUser.EUA_IsActive = false;
			Factory.Save();
			AssertEquals("Should be deactivated when the product code is WiseTechAcademy", false, contact.OC_IsActive);
		}

		public void TestGetSystemText()
		{
			var enterpriseCode = "DDD";
			var serverCode = "SYD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));

			var org = licenceEnterprise.Organisation;
			org.OH_Code = "SCWAAASYD";

			var productionDatabase = licence.Database;
			productionDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;
			productionDatabase.LD_OH_WebAccessOrg = org.PK;

			var glowDatabase = licenceEnterprise.Databases.AddNew();
			glowDatabase.LD_ServerCode = "TD1";
			glowDatabase.LD_LicenceType = DatabaseTypes.Codes.Test;
			glowDatabase.LD_Product = ProductTypes.Codes.GLOW;
			glowDatabase.LD_OH_WebAccessOrg = org.PK;
			glowDatabase.LD_TenantID = "IDK";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.SetHashedPassword("1234");

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = productionDatabase.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsContactRelationshipActive = false;
			userAccount1.EUA_ContactRelationshipStatus = "EMC";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = glowDatabase.PK;
			userAccount2.EUA_UserID = "U01";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "u1@cw1.com";
			Factory.Save();

			AssertEquals(FormattableString.Invariant($"{new ProductTypes(true).GetDescriptionFromCode(productionDatabase.LD_Product)} ({enterpriseCode} {serverCode})"), userAccount1.GetSystemText());
			AssertEquals(FormattableString.Invariant($"{new ProductTypes(true).GetDescriptionFromCode(glowDatabase.LD_Product)} ({enterpriseCode} {glowDatabase.LD_TenantID})"), userAccount2.GetSystemText());
		}

		public void TestCreateNewUserAccount()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var userID = "AXZ";
			var userName = "Frankie";
			var email = "frankie@work.com";
			var country = "AU";
			var userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, licenceDatabase, userID, userName, email, country);
			var userAccount2 = EdiCustomerUserAccount.CreateNewUserAccount(Factory, licenceDatabase, "ABC", "Abattoir", "abattoir@work.com", "INV");
			var userAccount3 = EdiCustomerUserAccount.CreateNewUserAccount(Factory, licenceDatabase, "BBC", "Baboon", "baboon@work.com", "ZZ");

			AssertEquals(userID, userAccount.EUA_UserID);
			AssertEquals(licenceDatabase.PK, userAccount.EUA_LD);
			AssertEquals(email, userAccount.EUA_Email);
			AssertEquals(userName, userAccount.EUA_FullName);
			AssertEquals(country, userAccount.EUA_RN_NKCountry);
			AssertEquals("New accounts should require email verification", true, userAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Country should not get assigned invalid codes", string.Empty, userAccount2.EUA_RN_NKCountry);
			AssertEquals("Country should not get assigned invalid codes", string.Empty, userAccount3.EUA_RN_NKCountry);
		}

		public void TestCreateNewUserAccount_NullCountry()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var userID = "AXZ";
			var userName = "Frankie";
			var email = "frankie@work.com";
			var userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, licenceDatabase, userID, userName, email, null);

			AssertEquals(userID, userAccount.EUA_UserID);
			AssertEquals(licenceDatabase.PK, userAccount.EUA_LD);
			AssertEquals(email, userAccount.EUA_Email);
			AssertEquals(userName, userAccount.EUA_FullName);
			AssertEquals(true, userAccount.EUA_RN_NKCountry.IsEmpty);
		}

		public void TestIsAutoLogged()
		{
			var userAccount = Factory.New<EdiCustomerUserAccountForTest>();
			Assert("Job doesn't implement workflow", !userAccount.IsAutoLogged_Exposed);
		}

		class EdiCustomerUserAccountForTest : EdiCustomerUserAccount
		{
			public EdiCustomerUserAccountForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsAutoLogged_Exposed => IsAutoLogged;
		}
	}
}
