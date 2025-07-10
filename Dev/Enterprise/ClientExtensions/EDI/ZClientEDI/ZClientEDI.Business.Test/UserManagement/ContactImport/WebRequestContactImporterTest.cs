using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.TrustedMessaging.MyAccount.Models;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class WebRequestContactImporterTest : TestCaseWithFactory
	{
		public void TestImportFromContactXsd()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var mainAddress = org.MainAddress;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";
			address1.OA_Code = "Branch A (AAA)";

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 Test Rd";
			address2.OA_Address2 = "Building B";
			address2.OA_City = "Sydney";
			address2.OA_State = "NSW";
			address2.OA_PostCode = "2000";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.OA_CompanyNameOverride = "Company BBB";
			address2.OA_Code = "Branch B (BBB)";
			address2.OA_IsActive = false;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = "user.a@123.com";

			var contact2a = org.Contacts.AddNew();
			contact2a.OC_ContactName = "User B";
			contact2a.OC_Email = "user.b.old@123.com";
			contact2a.OC_IsActive = true;
			contact2a.OC_WebAccessEnabled = true;

			var contact2b = org.Contacts.AddNew();
			contact2b.OC_ContactName = "User B (1)";
			contact2b.OC_Email = "user.b@123.com";
			contact2b.OC_IsActive = true;
			contact2b.OC_WebAccessEnabled = false;

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User C (1)";
			contact3.OC_Email = "user.c.wrong@123.com";

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "USC";
			userAccount.EUA_FullName = "User C";
			userAccount.EUA_Email = "user.c@123.com";
			userAccount.EUA_OC_WebAccessContact = contact3.PK;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "CCC";
			clientBranch1.LCB_OA = address1.PK;

			var clientBranch2 = Factory.New<ClientBranch>();
			clientBranch2.LCB_LD = database.PK;
			clientBranch2.LCB_Code = "BBB";
			clientBranch2.LCB_OA = address2.PK;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User A",
				EmailAddress = "user.a@123.com",
				WebAccessEnable = true
			};

			var contactXsd2 = new Xsd.OrgContact
			{
				Name = "User B",
				EmailAddress = "user.b@123.com",
				WebAccessEnable = true
			};

			var contactXsd3 = new Xsd.OrgContact
			{
				Name = "User C",
				EmailAddress = "user.c@123.com",
				WebAccessEnable = true
			};

			var contactXsd4 = new Xsd.OrgContact
			{
				Name = "User D",
				EmailAddress = "user.d@123.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromContactXsd(contactXsd1, null, "AAA").Contact;
			importer.SaveIfNeeded();
			var importedContact2 = importer.ImportFromContactXsd(contactXsd2, null, "BBB").Contact;
			importer.SaveIfNeeded();
			var importedContact3 = importer.ImportFromContactXsd(contactXsd3, userAccount.EUA_UserID, "CCC").Contact;
			importer.SaveIfNeeded();
			var importedContact4 = importer.ImportFromContactXsd(contactXsd4, null, "DDD").Contact;
			importer.SaveIfNeeded();

			AssertImportedContact(importedContact1, contactXsd1);
			AssertImportedContact(importedContact2, contactXsd2, "User B (1)");
			AssertImportedContact(importedContact3, contactXsd3, "User C");
			AssertImportedContact(importedContact4, contactXsd4);

			AssertEquals(importedContact1.OC_OA_OrgAddress, mainAddress.PK);
			AssertEquals(importedContact2.OC_OA_OrgAddress, address2.PK);
			AssertEquals(importedContact3.OC_OA_OrgAddress, address1.PK);
			AssertEquals(importedContact4.OC_OA_OrgAddress, mainAddress.PK);
		}

		public void TestImportFromContactXsd_DuplicateEmails()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var mainAddress = org.MainAddress;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";
			address1.OA_Code = "Branch A (AAA)";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.OC_WebAccessEnabled = false;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 2";
			contact2.OC_Email = "u2@cw1.com";
			contact2.OC_WebAccessEnabled = false;

			var contact0 = org.Contacts.AddNew();
			contact0.OC_ContactName = "User ZZZ";
			contact0.OC_Email = "u1@cw1.com";
			contact0.OC_WebAccessEnabled = true;

			var userAccount0 = Factory.New<EdiCustomerUserAccount>();
			userAccount0.EUA_LD = database.PK;
			userAccount0.EUA_UserID = "USZ";
			userAccount0.EUA_FullName = "User ZZZ";
			userAccount0.EUA_Email = "u1@cw1.com";
			userAccount0.EUA_OC_WebAccessContact = contact0.PK;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User 1",
				EmailAddress = "u1@cw1.com",
				WebAccessEnable = true
			};

			var contactXsd2 = new Xsd.OrgContact
			{
				Name = "User 2",
				EmailAddress = "u2@cw1.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromContactXsd(contactXsd1, userAccount1.EUA_UserID, "AAA").Contact;
			importer.SaveIfNeeded();
			var importedContact2 = importer.ImportFromContactXsd(contactXsd2, null, "AAA").Contact;
			importer.SaveIfNeeded();

			AssertEquals(true, contact0.OC_WebAccessEnabled);
			AssertImportedContact(importedContact1, contactXsd1, webAccessEnabled: false);
			AssertEquals(false, importedContact1.OC_WebAccessEnabled);
			AssertImportedContact(importedContact2, contactXsd2);
			AssertEquals(true, importedContact2.OC_WebAccessEnabled);
		}

		public void TestImportFromContactXsd_NoEmail()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var mainAddress = org.MainAddress;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";
			address1.OA_Code = "Branch A (AAA)";

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User 1",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var imported = importer.ImportFromContactXsd(contactXsd1, "US1", "AAA");
			importer.SaveIfNeeded();

			AssertNull(imported.UserAccount);
			AssertNull(imported.Contact);
			AssertEquals(ContactImportResult.NoEmailAddressErrorMessage, imported.ErrorMessage);
		}

		public void TestImportFromContactXsd_NoOrganization()
		{
			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User 1",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, null);
			var imported = importer.ImportFromContactXsd(contactXsd1, "US1", "AAA");
			importer.SaveIfNeeded();

			AssertNull(imported.UserAccount);
			AssertNull(imported.Contact);
			AssertEquals(ContactImportResult.NoOrganizationErrorMessage, imported.ErrorMessage);
		}

		public void TestImportFromContactXsd_ContactLinkedToSameDatabaseAnotherUser()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "user@cw1.com";
			contact1.OC_WebAccessEnabled = true;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 (1)";
			contact2.OC_Email = "user@cw1.com";
			contact2.OC_WebAccessEnabled = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "user@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User 2";
			userAccount2.EUA_Email = "user@cw1.com";

			Factory.Save();

			var contactXsd = new Xsd.OrgContact
			{
				Name = "User 2",
				EmailAddress = "user@cw1.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var importedContact = importer.ImportFromContactXsd(contactXsd, userAccount2.EUA_UserID, "").Contact;
			importer.SaveIfNeeded();

			AssertEquals("Should use existing contact with same email", contact2.PK, importedContact.PK);
			AssertEquals("Should not enable web access", false, importedContact.OC_WebAccessEnabled);
		}

		public void TestImportFromContactXsd_EmailVerificationRequired_ShouldLinkContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = "user.a@123.com";

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User A",
				EmailAddress = "user.a@123.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var importResult = importer.ImportFromContactXsd(contactXsd1, "USA", "AAA");
			importer.SaveIfNeeded();
			var importedUserAccount = importResult.UserAccount;

			AssertEquals("Email verification should be required", true, importedUserAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("User Account should link to contact", contact1.PK, importedUserAccount.EUA_OC_WebAccessContact);
		}

		public void TestImportFromContactXsd_ShouldUpdateContactEmail()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var originalEmailAddress = "user.a@123.com";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = originalEmailAddress;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = originalEmailAddress;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsActive = false;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User A",
				EmailAddress = "user@cw1.com",
				WebAccessEnable = true
			};

			AssertNotEquals("Precondition: EUA_PreviousEmail should not hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new WebRequestContactImporter(Factory, database);
			var importResult = importer.ImportFromContactXsd(contactXsd1, "US1", "AAA");
			importer.SaveIfNeeded();
			var importedUserAccount = importResult.UserAccount;

			AssertEquals("Precondition: Should be the same user account", userAccount1.PK, importedUserAccount.PK);
			AssertEquals("Precondition: Email verification should not be required", false, importedUserAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Contact relationship should be inactive", false, importedUserAccount.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Contact relationship status should be EMC", ContactRelationshipStatusList.Codes.EmailChanged, importedUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals("User Account email should be updated", contactXsd1.EmailAddress, importedUserAccount.EUA_Email);
			AssertEquals("EUA_PreviousEmail should now hold the previous email address", originalEmailAddress, importedUserAccount.EUA_PreviousEmail);
			AssertEquals("Contact email should be updated", contactXsd1.EmailAddress, contact1.OC_Email);
		}

		public void TestImportFromContactXsd_DuplicateEmail_ShouldUpdateContactEmail()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var originalEmailAddress = "user.a@123.com";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = originalEmailAddress;
			contact1.OC_WebAccessEnabled = false;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User B";
			contact2.OC_Email = "contact2@123.com";
			contact2.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = originalEmailAddress;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsActive = false;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User A",
				EmailAddress = contact2.OC_Email,
				WebAccessEnable = true
			};

			AssertNotEquals("Precondition: EUA_PreviousEmail should not hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new WebRequestContactImporter(Factory, database);
			var importResult = importer.ImportFromContactXsd(contactXsd1, "US1", "AAA");
			importer.SaveIfNeeded();
			var importedUserAccount = importResult.UserAccount;

			AssertEquals("Precondition: Should be the same user account", userAccount1.PK, importedUserAccount.PK);
			AssertEquals("Precondition: Email verification should not be required", false, importedUserAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Contact relationship should be inactive", false, importedUserAccount.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Contact relationship status should be EMC", ContactRelationshipStatusList.Codes.DistinctEmailRequired, importedUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals("User Account email should be updated", contactXsd1.EmailAddress, importedUserAccount.EUA_Email);
			AssertEquals("EUA_PreviousEmail should now hold the previous email address", originalEmailAddress, importedUserAccount.EUA_PreviousEmail);
			AssertEquals("Contact email should be updated", contactXsd1.EmailAddress, contact1.OC_Email);
		}

		public void TestImportFromContactXsd_DuplicateEmail_ShouldUpdateContactEmail_ActiveUserAccount()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var originalEmailAddress = "user.a@123.com";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = originalEmailAddress;
			contact1.OC_WebAccessEnabled = false;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User B";
			contact2.OC_Email = "contact2@123.com";
			contact2.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = originalEmailAddress;
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User A",
				EmailAddress = contact2.OC_Email,
				WebAccessEnable = true
			};

			AssertNotEquals("Precondition: EUA_PreviousEmail should not hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new WebRequestContactImporter(Factory, database);
			var importResult = importer.ImportFromContactXsd(contactXsd1, "US1", "AAA");
			importer.SaveIfNeeded();
			var importedUserAccount = importResult.UserAccount;

			AssertEquals("Precondition: Should be the same user account", userAccount1.PK, importedUserAccount.PK);
			AssertEquals("Precondition: Email verification should not be required", false, importedUserAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Contact relationship should be active", true, importedUserAccount.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Contact relationship status should be EMC", ContactRelationshipStatusList.Codes.DistinctEmailRequired, importedUserAccount.EUA_ContactRelationshipStatus);
			AssertEquals("User Account email should be updated", contactXsd1.EmailAddress, importedUserAccount.EUA_Email);
			AssertEquals("EUA_PreviousEmail should now hold the previous email address", originalEmailAddress, importedUserAccount.EUA_PreviousEmail);
			AssertEquals("Contact email should be updated", contactXsd1.EmailAddress, contact1.OC_Email);
		}

		public void TestImportFromUserAccount_ContactLinkedToDifferentDatabaseUser()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence1.Company.Header;
			var licence2 = BillingTestHelper.CreateAnotherDatabase(licence1.Company, "PRD");
			var database1 = licence1.Database;
			database1.LD_OH_WebAccessOrg = org.PK;
			var database2 = licence2.Database;
			database2.LD_OH_WebAccessOrg = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "user@cw1.com";
			contact.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "user@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_UserID = "US1";
			userAccount2.EUA_FullName = "User 1";
			userAccount2.EUA_Email = "user@cw1.com";

			Factory.Save();

			var importer = new WebRequestContactImporter(Factory, database2);
			importer.ImportFromUserAccount(userAccount2);
			importer.SaveIfNeeded();

			userAccount2.Reload();
			AssertEquals("Contact is linked", contact.PK, userAccount2.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated to stop auto login", false, userAccount2.EUA_IsContactRelationshipActive);
		}

		public void TestImportFromUserAccount_BranchCode()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence1.Company.Header;
			var database1 = licence1.Database;
			database1.LD_OH_WebAccessOrg = org.PK;
			var address = org.Addresses.AddNew();
			address.Address1 = "72 O'Riordan Street";

			var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch.LCB_LD = database1.PK;
			clientBranch.LCB_Code = "SYD";
			clientBranch.LCB_OA = address.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "user@cw1.com";
			contact.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "user@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			AssertNotEquals("Precondition", address.PK, contact.OC_OA_OrgAddress);
			var importer = new WebRequestContactImporter(Factory, database1);
			importer.ImportFromUserAccount(userAccount1, clientBranch.LCB_Code);
			importer.SaveIfNeeded();

			userAccount1.Reload();
			AssertEquals("Contact is linked", contact.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Branch is linked", address.PK, contact.OC_OA_OrgAddress);
		}

		public void TestImportFromUserAccount_NoBranchCode()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Company.Header;
			var database = licence.Database;
			database.LD_OH_WebAccessOrg = org.PK;
			var address = org.Addresses.AddNew();
			address.Address1 = "72 O'Riordan Street";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "user@cw1.com";
			contact.OC_WebAccessEnabled = true;
			contact.OC_OA_OrgAddress = address.PK;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = database.PK;
			user.EUA_UserID = "US1";
			user.EUA_FullName = "User 1";
			user.EUA_Email = "user@cw1.com";
			user.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromUserAccount(user);
			importer.SaveIfNeeded();
			AssertEquals("Contact address is not overridden", address.PK, contact.OC_OA_OrgAddress);

			var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch.LCB_LD = database.PK;
			clientBranch.LCB_Code = "SYD";
			clientBranch.LCB_OA = address.PK;
			contact.OC_OA_OrgAddress = org.MainAddress.PK;
			Factory.Save();

			AssertNotEquals("Precondition", address.PK, contact.OC_OA_OrgAddress);
			importer.ImportFromUserAccount(user, clientBranch.LCB_Code);
			importer.SaveIfNeeded();
			AssertEquals("Contact address is updated", address.PK, contact.OC_OA_OrgAddress);
		}

		public void TestImportFromContactXsdShouldSetWebAccessWithEmptyPassword()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var mainAddress = org.MainAddress;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";
			address1.OA_Code = "Branch A (AAA)";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.OC_WebAccessEnabled = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User 1",
				EmailAddress = "u1@cw1.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromContactXsd(contactXsd1, userAccount1.EUA_UserID, "AAA").Contact;
			importer.SaveIfNeeded();

			AssertEquals(true, importedContact1.OC_WebAccessEnabled);
			Assert("Password is empty", importedContact1.OC_PasswordHash.IsEmpty);
		}

		public void TestImportFromUserAccountShouldSetWebAccessWithEmptyPassword()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence1.Company.Header;
			var database1 = licence1.Database;
			database1.LD_OH_WebAccessOrg = org.PK;
			var address = org.Addresses.AddNew();
			address.Address1 = "72 O'Riordan Street";

			var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch.LCB_LD = database1.PK;
			clientBranch.LCB_Code = "SYD";
			clientBranch.LCB_OA = address.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "user@cw1.com";
			contact.OC_WebAccessEnabled = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "user@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			AssertNotEquals("Precondition", address.PK, contact.OC_OA_OrgAddress);
			var importer = new WebRequestContactImporter(Factory, database1);
			importer.ImportFromUserAccount(userAccount1, clientBranch.LCB_Code);
			importer.SaveIfNeeded();

			userAccount1.Reload();
			AssertEquals("Contact is linked", contact.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Web Access is granted", true, contact.OC_WebAccessEnabled);
			Assert("Password is empty", contact.OC_PasswordHash.IsEmpty);
		}

		public void TestImportFromContactXsd_ContactInfoUpdateType()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = "TST";
			var org = database.LicEnterprise.Organisation;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User A";
			contact.OC_Email = "user.a@123.com";
			contact.OC_Title = "Title 1";

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "USC";
			userAccount.EUA_FullName = "User A";
			userAccount.EUA_Email = "user.a@123.com";
			userAccount.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var contactXsd = new Xsd.OrgContact
			{
				Name = "New User A",
				EmailAddress = "new.user.a@123.com",
				JobTitle = "New Title 1",
				WebAccessEnable = true,
			};

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromContactXsd(contactXsd, userAccount.EUA_UserID, "");
			importer.SaveIfNeeded();

			AssertEquals(contact.OC_ContactName, "New User A");
			AssertEquals(contact.OC_Email, "new.user.a@123.com");
			AssertEquals(contact.OC_Title, "New Title 1");

			//1. If the user account is not PRD and there's a PRD system linked to the same contact, only update email
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "SR2", true);
			var database2 = licence2.Database;
			database2.LD_LicenceType = "PRD";
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_UserID = "USC";
			userAccount2.EUA_FullName = "User A";
			userAccount2.EUA_Email = "user.a@123.com";
			userAccount2.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			var contactXsd2 = new Xsd.OrgContact
			{
				Name = "New User A [PRD]",
				EmailAddress = "new.user.a.prd@prd.com",
				JobTitle = "New Title 1 [PRD]",
				WebAccessEnable = true,
			};

			importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromContactXsd(contactXsd2, userAccount.EUA_UserID, "");
			importer.SaveIfNeeded();

			AssertEquals(contact.OC_ContactName, "New User A");
			AssertEquals(contact.OC_Email, "new.user.a.prd@prd.com");
			AssertEquals(contact.OC_Title, "New Title 1");

			// 2. If the user account is not PRD and there's a PRD system linked to the same person 
			// (but not same contact), update everything except OC_ContactName and OC_Mobile
			var org2 = database2.LicEnterprise.Organisation;
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "User A";
			contact2.OC_Email = "user.a@123.com";
			contact2.OC_Title = "Title 1";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgContact SET OC_PER = '{contact.OC_PER}' WHERE OC_PK = '{contact2.PK}';");

			var contactXsd3 = new Xsd.OrgContact
			{
				Name = "New User A [CW1]",
				EmailAddress = "new.user.a.cw1@cw1.com",
				JobTitle = "New Title 1 [CW1]",
				WebAccessEnable = true,
			};

			importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromContactXsd(contactXsd3, userAccount.EUA_UserID, "");
			importer.SaveIfNeeded();

			AssertEquals(contact.OC_ContactName, "New User A");
			AssertEquals(contact.OC_Email, "new.user.a.cw1@cw1.com");
			AssertEquals(contact.OC_Title, "New Title 1 [CW1]");
		}

		public void TestImportFromContactXsdNewUserAccountShouldRequireEmailVerification()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			Factory.Save();

			var contactXsd = new Xsd.OrgContact
			{
				Name = "User 1",
				EmailAddress = "user@cw1.com",
				WebAccessEnable = true
			};

			var importer = new WebRequestContactImporter(Factory, database);
			var newUserAccount = importer.ImportFromContactXsd(contactXsd, "US1", "").UserAccount;
			AssertEquals(true, newUserAccount.EUA_IsEmailVerificationRequired);
			newUserAccount.Delete();

			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			importer = new WebRequestContactImporter(Factory, database);
			newUserAccount = importer.ImportFromContactXsd(contactXsd, "US1", "").UserAccount;
			AssertEquals(true, newUserAccount.EUA_IsEmailVerificationRequired);
		}

		public void TestMergeWebSecurityRights()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var clientCompany = licence1.ClientCompany;
			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var licence3 = BillingTestHelper.CreateAnotherLicence(database, "OOO");

			var org1 = database.LicEnterprise.Organisation;
			var org2 = licence2.Company.Header;
			var org3 = licence3.Company.Header;

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";
			contact1b.OC_Email = "user.one@test.org";

			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "User One";
			contact2.OC_Email = "user.one@test.org";
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("654321");

			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "User One";
			contact3.OC_Email = "user.one23@test.org";
			contact3.OC_WebAccessEnabled = true;

			var webRight1 = EDIWebSecurityRightsList.Downloads;
			var webRight2 = EDIWebSecurityRightsList.WTGInternalExams;
			var webRight3 = EDIWebSecurityRightsList.CargoWiseTechnicalGuides;

			AssertEquals("Pre-condition: not granted by default", false, webRight1.IsGrantedByDefault);
			AssertEquals("Pre-condition: not granted by default", false, webRight2.IsGrantedByDefault);
			AssertEquals("Pre-condition: granted by default", true, webRight3.IsGrantedByDefault);

			var org2Security1 = org2.SecurityRights.Cast<OrgSecurity>().Single(x => x.OX_SecurityItemName == webRight1.SecurityItemName);
			org2Security1.OX_Granted = true;
			var contact2Security1 = contact2.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().Single(x => x.OZ_OX == org2Security1.PK);
			contact2Security1.OZ_Granted = false;

			var org2Security2 = org2.SecurityRights.Cast<OrgSecurity>().Single(x => x.OX_SecurityItemName == webRight2.SecurityItemName);
			org2Security2.OX_Granted = false;
			var contact2Security2 = contact2.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().Single(x => x.OZ_OX == org2Security2.PK);
			contact2Security2.OZ_Granted = true;

			var org2Security3 = org2.SecurityRights.Cast<OrgSecurity>().Single(x => x.OX_SecurityItemName == webRight3.SecurityItemName);
			org2Security3.OX_Granted = false;
			var contact2Security3 = contact2.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().Single(x => x.OZ_OX == org2Security3.PK);
			contact2Security3.OZ_Granted = false;

			var org3Security2 = org3.SecurityRights.Cast<OrgSecurity>().Single(x => x.OX_SecurityItemName == webRight2.SecurityItemName);
			org3Security2.OX_Granted = false;

			Factory.Save();

			var importer = new WebRequestContactImporter(Factory, database);

			var contactXsd1 = new Xsd.OrgContact
			{
				Name = "User One",
				EmailAddress = "user.one@test.org",
				WebAccessEnable = true
			};

			var importedContact = importer.ImportFromContactXsd(contactXsd1, null, "").Contact;
			importer.SaveIfNeeded();
			importer.MergeContactWebSecurity(importedContact);

			AssertEquals("No matched contact is granted right", false, IsWebSecurityRightGranted(importedContact.PK, org1.PK, webRight1));
			AssertEquals("At least one matched contact is granted right", true, IsWebSecurityRightGranted(importedContact.PK, org1.PK, webRight2));
			AssertEquals("No matched contact is granted right but right is granted by default", true, IsWebSecurityRightGranted(importedContact.PK, org1.PK, webRight3));
		}

		bool IsWebSecurityRightGranted(ZGuid contactPK, ZGuid contactOrgPK, WebSecurityRight right)
		{
			var orgRightQuery = new ZQuery(OrgSecuritySchema.OX_OH, contactOrgPK);
			if (right.SecurityGuid.IsEmpty)
			{
				orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, right.SecurityItemName);
			}
			else
			{
				orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SU, right.SecurityGuid);
			}

			var orgRight = Factory.LoadTop1<OrgSecurity>(orgRightQuery);
			if (orgRight == null)
			{
				return right.IsGrantedByDefault;
			}

			var contactRightQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OC, contactPK)
				.AddToFilter(OrgSecurityContactsSchema.OZ_OX, orgRight.PK);

			var contactRight = Factory.LoadTop1<OrgSecurityContacts>(contactRightQuery);

			return contactRight?.OZ_Granted ?? orgRight.OX_Granted;
		}

		void AssertImportedContact(OrgContact contact, Xsd.OrgContact contactXsd, string expectedContactName = "", bool webAccessEnabled = true, bool emptyEmail = false)
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Contact", contact);

				if (!string.IsNullOrEmpty(expectedContactName))
				{
					AssertEquals("Contact Name", expectedContactName, contact.OC_ContactName);
				}
				else
				{
					AssertEquals("Contact Name", contactXsd.Name, contact.OC_ContactName);
				}

				if (emptyEmail)
				{
					AssertEquals("Contact Email", "", contact.OC_Email);
					AssertEquals("Contact Web Access", false, contact.OC_WebAccessEnabled);
				}
				else
				{
					AssertEquals("Contact Email", contactXsd.EmailAddress, contact.OC_Email);
					AssertEquals("Contact Web Access", webAccessEnabled, contact.OC_WebAccessEnabled);
				}

				AssertEquals("Contact Active", true, contact.OC_IsActive);
				AssertNotEquals("Contact Verified Date", ZDateTime.Empty, contact.OC_DetailsVerified);
			});
		}

		public void TestImportFromTrustedUserInfo_DuplicateEmail()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = "TST";
			var org = database.LicEnterprise.Organisation;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User A";
			contact.OC_Email = "user.a@123.com";
			contact.OC_Title = "Title 1";
			contact.OC_WebAccessEnabled = true;

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "USC";
			userAccount.EUA_FullName = "User A";
			userAccount.EUA_Email = "user.a@123.com";
			userAccount.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var userInfo = new TrustedUserInfo()
			{
				UserId = "AAA",
				FullName = "User A1",
				Email = "user.a@123.com"
			};

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			var user2 = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, userInfo.UserId);
			AssertNotNull(user2);
			AssertNotNull(user2.WebAccessContact);
			AssertEquals("DER", user2.EUA_ContactRelationshipStatus);
			AssertNotEquals("Should have created a new contact", contact.PK, user2.EUA_OC_WebAccessContact);
			AssertEquals("web access shouldn't be enabled because of the DER", false, user2.WebAccessContact.OC_WebAccessEnabled);
			AssertEquals("Email should be set", userInfo.Email, user2.WebAccessContact.OC_Email);
		}

		public void TestImportFromTrustedUserInfo_DuplicateEmail_Existing()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = "TST";
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = "user.a@123.com";
			contact1.OC_Title = "Title 1";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User B";
			contact2.OC_Email = "user.a@123.com";
			contact2.OC_Title = "Title 2";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User C";
			contact3.OC_Email = "user.c@123.com";
			contact3.OC_Title = "Title 3";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "USC";
			userAccount1.EUA_FullName = "User A";
			userAccount1.EUA_Email = "user.a@123.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "AAA";
			userAccount2.EUA_FullName = "User A";
			userAccount2.EUA_Email = "user.a@123.com";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			Factory.Save();

			var userInfo = new TrustedUserInfo()
			{
				UserId = "AAA",
				FullName = "User A1",
				Email = "user.a@123.com"
			};

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			var user2 = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, userInfo.UserId);
			AssertNotNull(user2);
			AssertEquals(contact2.PK, user2.EUA_OC_WebAccessContact);
			AssertEquals("DER", user2.EUA_ContactRelationshipStatus);

			var userInfo2 = new TrustedUserInfo()
			{
				UserId = "CCC",
				FullName = "User C1",
				Email = "user.c@123.com"
			};

			importer.ImportFromTrustedUserInfo(userInfo2);
			importer.SaveIfNeeded();

			var user3 = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, userInfo2.UserId);
			AssertNotNull(user3);
			AssertEquals(string.Empty, user3.EUA_ContactRelationshipStatus);
		}

		public void TestImportFromTrustedUserInfo_BlankEmail()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Company.Header;
			var db = licence.Database;
			db.LD_OH_WebAccessOrg = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = string.Empty;
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			var userInfo = new TrustedUserInfo()
			{
				UserId = "AAA",
				FullName = "User A1",
				Email = string.Empty,
			};

			var importer = new WebRequestContactImporter(Factory, db);
			var (user, _) = importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			user.EUA_IsEmailVerificationRequired = false;
			user.EUA_IsContactRelationshipActive = true;
			user.EUA_ContactRelationshipStatus = string.Empty;
			user.Factory.Save();

			userInfo.Email = "aaa@test.com";
			var (_, importedContact) = importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			AssertEquals("aaa@test.com", importedContact.OC_Email);
			AssertEquals(true, importedContact.OC_WebAccessEnabled);

			userInfo.Email = string.Empty;
			(_, importedContact) = importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			AssertEquals(string.Empty, importedContact.OC_Email);
			AssertEquals(false, importedContact.OC_WebAccessEnabled);
		}

		public void TestImportFromTrustedUserInfo_RemoveInapplicableDerStatus()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = "TST";
			var org = database.LicEnterprise.Organisation;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User A";
			contact.OC_Email = "user.a@123.com";
			contact.OC_Title = "Title 1";

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "USC";
			userAccount.EUA_FullName = "User A";
			userAccount.EUA_Email = "user.a@123.com";
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			userAccount.EUA_ContactRelationshipStatus = "DER";

			Factory.Save();

			var userInfo = new TrustedUserInfo()
			{
				UserId = "USC",
				FullName = "User A1",
				Email = "user.a@123.com"
			};

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			var user2 = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, userInfo.UserId);
			AssertEquals("", user2.EUA_ContactRelationshipStatus);
		}

		public void TestImportFromTrustedUserInfo_RelinkToAlternativeContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var database = licence.Database;
			database.LD_LicenceType = "TST";
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User A";
			contact1.OC_Email = "user.a@123.com";
			contact1.OC_Title = "Title 1";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User A [1]";
			contact2.OC_Email = "user.a@123.com";
			contact2.OC_Title = "Title 1";
			contact2.OC_IsActive = false;
			contact2.OC_WebAccessEnabled = false;

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "AAA";
			userAccount.EUA_FullName = "User A";
			userAccount.EUA_Email = "user.a@123.com";
			userAccount.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();

			var userInfo = new TrustedUserInfo()
			{
				UserId = "AAA",
				FullName = "User A",
				Email = "user.a@123.com"
			};

			var importer = new WebRequestContactImporter(Factory, database);
			importer.ImportFromTrustedUserInfo(userInfo);
			importer.SaveIfNeeded();

			AssertEquals(true, contact1.OC_IsActive);
			AssertEquals(true, contact1.OC_WebAccessEnabled);
			AssertEquals(false, contact2.OC_IsActive);
			AssertEquals(false, contact2.OC_WebAccessEnabled);

			AssertEquals(contact1, userAccount.WebAccessContact);
			AssertEquals(true, userAccount.EUA_IsEmailVerificationRequired);
		}

		public void TestImportFromTrustedUserInfo_MergeContactToStaffPerson()
		{
			// Simulate staff creation as GlbStaff instead of EDIGlbStaff
			var staffPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_EmailAddress, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES (@staffPK, 'AAA', 'Staff A', 'aaa@test.com', GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
			p =>
			{
				p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
			});

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			var org = database.WebAccessOrg;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var userInfo = new TrustedUserInfo()
				{
					UserId = "AAA",
					FullName = "Staff A",
					Email = "aaa@test.com"
				};

				var importer = new WebRequestContactImporter(Factory, database);
				importer.ImportFromTrustedUserInfo(userInfo);
				importer.SaveIfNeeded();
			}

			var staff = Factory.Load<GlbStaff>(staffPK);
			org.Contacts.Load();
			var contact = org.Contacts[0];

			AssertNotNull(staff);
			AssertNotNull(contact);

			AssertEquals("Imported contact should link to staff person", staff.GS_PER, contact.OC_PER);
		}
	}
}
