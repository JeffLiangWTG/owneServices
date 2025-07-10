using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class VersionReportContactImportHelperTest : TestCaseWithFactory
	{
		public void TestImportContacts()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User Existing One";
			userAccount1.EUA_Email = "user.existing1@test.com.au";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				WorkPhone = "+61238473332",
				WorkPhoneExtension = "284",
				JobTitle = "Sales Rep",
				LanguageCode = "EN",
				IsActive = true
			};

			var staffReport2 = new StaffReport()
			{
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two@test.org",
				BranchCode = "BBB",
				WorkPhone = "+862084471234",
				JobTitle = "Export Agent",
				IsActive = true
			};

			var staffReports = new List<StaffReport>() { staffReport1, staffReport2 };

			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: false);
			AssertUserAccount(staffReport1);
			AssertUserAccount(staffReport2);
			database.Reload();
			AssertEquals(true, database.LD_StaffFirstReportUtc.IsEmpty);

			staffReport1.EmailAddress = "user.one@test.com";
			staffReport2.EmailAddress = "user.two@test.com";

			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			AssertUserAccount(staffReport1);
			AssertUserAccount(staffReport2);
			database.Reload();
			AssertEquals(false, database.LD_StaffFirstReportUtc.IsEmpty);
		}

		public void TestImportContacts_WithExistingContacts()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one.old@test.org";

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "User One (1)";
			contact1b.OC_Email = "user.one@test.org";

			var contact2a = org.Contacts.AddNew();
			contact2a.OC_ContactName = "User Two";
			contact2a.OC_Email = "user.two.very.old@test.org";

			var contact2b = org.Contacts.AddNew();
			contact2b.OC_ContactName = "User Two (1)";
			contact2b.OC_Email = "user.two.old@test.org";

			var contact2c = org.Contacts.AddNew();
			contact2c.OC_ContactName = "User Two (2)";
			contact2c.OC_Email = "user.two@test.org";

			var contact3a = org.Contacts.AddNew();
			contact3a.OC_ContactName = "User Three";
			contact3a.OC_Email = "user.three@test.org";
			contact3a.OC_IsActive = false;
			contact3a.OC_WebAccessEnabled = true;

			var contact3b = org.Contacts.AddNew();
			contact3b.OC_ContactName = "User Three (1)";
			contact3b.OC_Email = "user.three@test.org";
			contact3b.OC_IsActive = true;
			contact3b.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				WorkPhone = "+61238473332",
				WorkPhoneExtension = "284",
				JobTitle = "Sales Rep",
				LanguageCode = "EN",
				IsActive = true
			};

			var staffReport2 = new StaffReport()
			{
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two@test.org",
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReport3 = new StaffReport()
			{
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three@test.org",
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReports = new List<StaffReport>() { staffReport1, staffReport2, staffReport3 };
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			AssertUserAccount(staffReport1, "User One (1)");
			AssertUserAccount(staffReport2, "User Two (2)");
			AssertUserAccount(staffReport3, "User Three (1)");
			contact3a.Reload();
			AssertEquals(false, contact3a.OC_IsActive);
		}

		public void TestImportContacts_DuplicateNames()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				Code = "US1",
				Name = "USER ONE",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReport2 = new StaffReport()
			{
				Code = "US2",
				Name = "User One",
				EmailAddress = "user.two@test.org",
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReports = new List<StaffReport>() { staffReport1, staffReport2 };
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			AssertUserAccount(staffReport1, "USER ONE");
			AssertUserAccount(staffReport2, "User One (1)");
		}

		public void TestImportContacts_StaffWithSameEmailAndName()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var commonEmail = "user.one@test.org";

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = commonEmail;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				Code = "US1",
				Name = "USER ONE",
				EmailAddress = commonEmail,
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReport2 = new StaffReport()
			{
				Code = "US2",
				Name = "User One",
				EmailAddress = commonEmail,
				BranchCode = "AAA",
				IsActive = true
			};

			var staffReports = new List<StaffReport>() { staffReport1, staffReport2 };
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			AssertUserAccount(staffReport1, "USER ONE");
			AssertUserAccount(staffReport2, "User One (1)");

			var secondImportedUserAccount = new BusinessObjectFactory().LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "US2"));
			var contact = secondImportedUserAccount.WebAccessContact;
			AssertEquals(commonEmail, contact.OC_Email);
			AssertEquals(ContactRelationshipStatusList.Codes.DistinctEmailRequired, secondImportedUserAccount.EUA_ContactRelationshipStatus);
		}

		public void TestImportContacts_InactiveStaffReport()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				Code = "US1",
				Name = "USER ONE",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				IsActive = false
			};

			var staffReports = new List<StaffReport>() { staffReport1 };
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);

			contact1a.Reload();
			AssertEquals("Linked contact should be deactived", false, contact1a.OC_IsActive);
		}

		public void TestImportContacts_PersonMergeQueue()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";
			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};
			var staffReports = new List<StaffReport>() { staffReport1 };
			var syncFactory = new BusinessObjectFactory();
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			syncFactory.Save();

			org1.Contacts.Reload(true);
			var contact1 = org1.Contacts[0];
			contact2a.Reload();
			contact2b.Reload();
			var queueItems = Factory.Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, contact1.OC_PER));
			AssertEquals(2, queueItems.Length);
			AssertEquals(1, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact2a.OC_PER));
			AssertEquals(1, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact2b.OC_PER));

			staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One NEW",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};
			staffReports = new List<StaffReport>() { staffReport1 };
			syncFactory = new BusinessObjectFactory();
			VersionReportContactImportHelper.ImportContacts(database.PK, staffReports, isFullStaffList: true);
			syncFactory.Save();

			queueItems = Factory.Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, contact1.OC_PER));
			AssertEquals("No new queue items", 2, queueItems.Length);
		}

		void AssertUserAccount(StaffReport staffReport, string expectedContactName = "", bool emptyEmail = false)
		{
			var factory = new BusinessObjectFactory();
			var userAccount = factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, staffReport.Code));
			var contact = userAccount.WebAccessContact;

			AssertNotNull("Contact", contact);

			if (!string.IsNullOrEmpty(expectedContactName))
			{
				AssertEquals("Contact Name", expectedContactName, contact.OC_ContactName);
			}
			else
			{
				AssertEquals("Contact Name", staffReport.Name, contact.OC_ContactName);
			}

			if (emptyEmail)
			{
				AssertEquals("Contact Email", "", contact.OC_Email);
			}
			else
			{
				AssertEquals("Contact Email", staffReport.EmailAddress, contact.OC_Email);
			}

			AssertEquals("Contact Work Phone", staffReport.WorkPhone ?? string.Empty, contact.OC_Phone);
			AssertEquals("Contact Work Phone Extension", staffReport.WorkPhoneExtension ?? string.Empty, contact.OC_PhoneExtension);
			AssertEquals("Contact Job Title", staffReport.JobTitle ?? string.Empty, contact.OC_Title);
			AssertEquals("Contact Language", staffReport.LanguageCode ?? "EN", contact.OC_Language);
			AssertNotEquals("Contact Address", ZGuid.Empty, contact.OC_OA_OrgAddress);
		}
	}
}
