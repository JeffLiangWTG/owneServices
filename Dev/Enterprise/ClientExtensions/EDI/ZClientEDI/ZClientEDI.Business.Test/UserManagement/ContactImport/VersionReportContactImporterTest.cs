using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class VersionReportContactImporterTest : TestCaseWithFactory
	{
		public void TestImportFromStaffReport()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one.old@test.org";

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
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one.old@test.com.au";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.one.old@test.com.au";

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";

			var userAccount4 = Factory.New<EdiCustomerUserAccount>();
			userAccount4.EUA_LD = database.PK;
			userAccount4.EUA_UserID = "US4";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				WorkPhone = "+61238473332",
				WorkPhoneExtension = "284",
				JobTitle = "Sales Rep",
				LanguageCode = "EN"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two@test.org",
				BranchCode = "AAA",
				LanguageCode = "ZH-CN"
			};

			var staffReport3 = new StaffReport()
			{
				IsActive = true,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three@test.org",
				BranchCode = "AAA"
			};

			var staffReport4 = new StaffReport()
			{
				IsActive = true,
				Code = "US4",
				Name = "User Four",
				EmailAddress = "user.four@test.org"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			var importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			var importedContact3 = importer.ImportFromSingleStaffReport(staffReport3);
			var importedContact4 = importer.ImportFromSingleStaffReport(staffReport4);
			importer.SaveIfNeeded();

			AssertImportedContact(importedContact1, staffReport1, "User One");
			AssertImportedContact(importedContact2, staffReport2, "User Two (2)");
			AssertImportedContact(importedContact3, staffReport3, "User Three (1)");
			AssertImportedContact(importedContact4, staffReport4, "User Four");
		}

		public void TestImportFromSingleStaffReport_ShouldUpdateContactEmail()
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

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = false,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org"
			};

			AssertNotEquals("Precondition: EUA_PreviousEmail should not hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			importer.SaveIfNeeded();

			AssertEquals("Precondition: Email verification should not be required", false, userAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Contact relationship should be inactive", false, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Contact relationship status should be EMC", ContactRelationshipStatusList.Codes.EmailChanged, userAccount1.EUA_ContactRelationshipStatus);
			AssertEquals("User Account email should be updated", staffReport1.EmailAddress, userAccount1.EUA_Email);
			AssertEquals("EUA_PreviousEmail should now hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			AssertEquals("Precondition: Imported contact should match existing one", contact1.PK, importedContact1.PK);
			AssertEquals("Contact email should be updated", staffReport1.EmailAddress, contact1.OC_Email);
		}

		public void TestImportFromSingleStaffReport_DuplicateEmail_ShouldUpdateContactEmail()
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

			var staffReport1 = new StaffReport()
			{
				IsActive = false,
				Code = "US1",
				Name = "User One",
				EmailAddress = contact2.OC_Email
			};

			AssertNotEquals("Precondition: EUA_PreviousEmail should not hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			importer.SaveIfNeeded();

			AssertEquals("Precondition: Email verification should not be required", false, userAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Precondition: Contact relationship should be inactive", false, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition: Contact relationship status should be DER", ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount1.EUA_ContactRelationshipStatus);
			AssertEquals("User Account email should be updated", staffReport1.EmailAddress, userAccount1.EUA_Email);
			AssertEquals("EUA_PreviousEmail should now hold the previous email address", originalEmailAddress, userAccount1.EUA_PreviousEmail);
			AssertEquals("Precondition: Imported contact should match existing one", contact1.PK, importedContact1.PK);
			AssertEquals("Contact email should be updated", staffReport1.EmailAddress, contact1.OC_Email);

			((IErrorReporter)ExceptionReporter.Instance).Clear();
		}

		public void TestImportFromStaffReport_RecoverFromSaveException()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.three@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one.new@test.org"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two.new@test.org"
			};

			var staffReport3 = new StaffReport()
			{
				IsActive = true,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three.new@test.org"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var staffReports = new List<StaffReport>() { staffReport1, staffReport2, staffReport3 };
			importer.ImportFactory_Exposed.Saving += f => { f.ServiceContainer.AddAfterOnSavingService(new DuplicateContactNameService(org, "User Two")); };
			importer.ImportFromStaffReports(staffReports);

			contact1.Reload();
			contact2.Reload();
			contact3.Reload();

			AssertEquals("Email should be updated", "user.one.new@test.org", contact1.OC_Email);
			AssertEquals("Email should be updated", "user.two.new@test.org", contact2.OC_Email);
			AssertEquals("Email should be updated", "user.three.new@test.org", contact3.OC_Email);
		}

		public void TestImportFromStaffReport_MergeContactToStaffPerson()
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
				var staffReport1 = new StaffReport()
				{
					IsActive = true,
					Code = "AAA",
					Name = "Staff A",
					EmailAddress = "aaa@test.com"
				};

				var importer = new VersionReportContactImporter(Factory, database);
				var staffReports = new List<StaffReport>() { staffReport1 };
				importer.ImportFromStaffReports(staffReports);
			}

			var staff = Factory.Load<GlbStaff>(staffPK);
			org.Contacts.Load();
			var contact = org.Contacts[0];

			AssertNotNull(staff);
			AssertNotNull(contact);

			AssertEquals("Imported contact should link to staff person", staff.GS_PER, contact.OC_PER);
		}

		public void TestImportFromStaffReport_FetchHintsUserAccounts()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EEE", "DEF", "SYD");
			var database2 = licence2.Database;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.three@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database2.PK;
			userAccount3.EUA_UserID = "US1";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one.new@test.org"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two.new@test.org"
			};

			var staffReport3 = new StaffReport()
			{
				IsActive = true,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three.new@test.org"
			};

			var importer = new VersionReportContactImporterForTest(Factory, database);
			var staffReports = new List<StaffReport>() { staffReport1, staffReport2, staffReport3 };
			importer.ImportFactory_Exposed.Saving += f => { f.ServiceContainer.AddAfterOnSavingService(new DuplicateContactNameService(org, "User Two")); };
			importer.ImportFromStaffReports(staffReports);

			AssertEquals(2, importer.LastFetchHintsUserAccounts.Length);
			Assert(importer.LastFetchHintsUserAccounts.Any(i => i.PK == userAccount1.PK));
			Assert(importer.LastFetchHintsUserAccounts.Any(i => i.PK == userAccount2.PK));
		}

		public void TestRetryWithExceptions()
		{
			ErrorReporter.Clear();
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one.new@test.org"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two.new@test.org"
			};

			var staffReport3 = new StaffReport()
			{
				IsActive = true,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three.new@test.org"
			};

			var idx = 0;
			var importer = new VersionReportContactImporterForRetryTest(Factory, database);
			importer.ShouldThrow = () =>
			{
				idx++;

				if (idx == 2 || idx == 7)
				{
					return false;
				}
				else
				{
					return true;
				}
			};

			var staffReports = new List<StaffReport>() { staffReport1, staffReport2, staffReport3 };
			importer.ImportFromStaffReports(staffReports);

			var newFactory = new BusinessObjectFactory();
			AssertEquals(true, newFactory.Exists(typeof(OrgContact), new ZQuery(OrgContactSchema.OC_Email, "user.one.new@test.org")));
			AssertEquals(true, newFactory.Exists(typeof(OrgContact), new ZQuery(OrgContactSchema.OC_Email, "user.two.new@test.org")));
			AssertEquals(false, newFactory.Exists(typeof(OrgContact), new ZQuery(OrgContactSchema.OC_Email, "user.three.new@test.org")));

			AssertEquals("VersionReportContactImporter.ImportFromStaffReport", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestImportFromStaffReport_UserAccountReactivationHasCorrectRelationshipValues_NewContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAA";
			org.Contacts.AddNew();

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_OC_ContractInstallerOrInternalTechContact = org.Contacts.FirstOrDefault().PK;
			database.LD_OH_WebAccessOrg = org.PK;

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_FullName = "John Test";
			userAccount.EUA_UserID = "ABC";
			userAccount.EUA_Email = "a@a.com";
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_OC_WebAccessContact = Guid.Empty;
			userAccount.EUA_IsActive = false;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = string.Empty;

			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
			AssertNoErrors("Should have no errors after initial save", userAccount);

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Test";
			staff.GS_Code = "ABC";
			staff.GS_EmailAddress = "b@b.com";
			staff.GS_IsActive = true;
			Factory.Save();

			var staffReport = new StaffReport(staff);
			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			AssertEquals("EUA_IsActive should be true after reactivation", true, userAccount.EUA_IsActive);
			AssertEquals("EUA_IsContactRelationshipActive should be true after reactivation", true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals("EUA_ContactRelationshipStatus should be empty after reactivation", string.Empty, userAccount.EUA_ContactRelationshipStatus);
			AssertNoErrors("Should have no errors after reactivation", userAccount);
		}

		public void TestImportFromStaffReport_UserAccountReactivationHasCorrectRelationshipValues_ExistingContactConnectedToOtherAccount()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAA";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Test";
			contact.OC_Email = "a@a.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = false;

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			database.LD_OH_WebAccessOrg = org.PK;

			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			database2.LD_OH_WebAccessOrg = org.PK;

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_FullName = "John Test";
			userAccount.EUA_UserID = "ABC";
			userAccount.EUA_Email = "a@a.com";
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_OC_WebAccessContact = Guid.Empty;
			userAccount.EUA_IsActive = false;
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = string.Empty;

			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_FullName = "John Test";
			userAccount2.EUA_UserID = "ABD";
			userAccount2.EUA_Email = "a@a.com";
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_OC_WebAccessContact = contact.PK;
			userAccount2.EUA_IsActive = true;
			userAccount2.EUA_IsContactRelationshipActive = true;
			userAccount2.EUA_ContactRelationshipStatus = string.Empty;

			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
			AssertNoErrors("Should have no errors after initial save", userAccount);

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Test";
			staff.GS_Code = "ABC";
			staff.GS_EmailAddress = "b@b.com";
			staff.GS_IsActive = true;
			Factory.Save();

			var staffReport = new StaffReport(staff);
			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			AssertEquals("EUA_IsActive should be true after reactivation", true, userAccount.EUA_IsActive);
			AssertEquals("EUA_IsContactRelationshipActive should be true after reactivation", true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals("EUA_ContactRelationshipStatus should be empty after reactivation", string.Empty, userAccount.EUA_ContactRelationshipStatus);
			AssertNotEquals("userAccount should have a new contact after import", contact.PK, userAccount.EUA_OC_WebAccessContact);
			AssertNoErrors("Should have no errors after reactivation", userAccount);
		}

		public void TestContactActiveStatusWhenLastUserAccountIsWTA()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAA";
			var dbWTA = Factory.NewWithValidTestData<LicenceDatabase>();
			dbWTA.LD_Product = ProductTypes.Codes.WiseTechAcademy;
			dbWTA.LD_OH_WebAccessOrg = org.PK;

			var dbNonWTA = Factory.NewWithValidTestData<LicenceDatabase>();
			dbNonWTA.LD_Product = ProductTypes.Codes.Enterprise;
			dbNonWTA.LD_OH_WebAccessOrg = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Test";
			contact.OC_Email = "a@a.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = false;

			var userAccountWTA = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccountWTA.EUA_FullName = "John Test";
			userAccountWTA.EUA_UserID = "ABD";
			userAccountWTA.EUA_Email = "a@a.com";
			userAccountWTA.EUA_LD = dbWTA.PK;
			userAccountWTA.EUA_OC_WebAccessContact = contact.PK;
			userAccountWTA.EUA_IsActive = true;
			userAccountWTA.EUA_IsContactRelationshipActive = true;
			userAccountWTA.EUA_ContactRelationshipStatus = string.Empty;

			var userAccountNonWTA = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccountNonWTA.EUA_FullName = "John Test 2";
			userAccountNonWTA.EUA_UserID = "AAA";
			userAccountNonWTA.EUA_Email = "b@b.com";
			userAccountNonWTA.EUA_LD = dbNonWTA.PK;
			userAccountNonWTA.EUA_OC_WebAccessContact = contact.PK;
			userAccountNonWTA.EUA_IsActive = true;
			userAccountNonWTA.EUA_IsContactRelationshipActive = true;
			userAccountNonWTA.EUA_ContactRelationshipStatus = string.Empty;

			var staffNonWTA = Factory.NewWithValidTestData<GlbStaff>();
			staffNonWTA.GS_FullName = "John Test 2";
			staffNonWTA.GS_IsActive = false;
			staffNonWTA.GS_Code = "AAA";
			staffNonWTA.GS_EmailAddress = "b@b.com";
			Factory.Save();

			var staffReport = new StaffReport(staffNonWTA);
			var importer = new VersionReportContactImporter(Factory, dbNonWTA);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			org.Contacts.Load();

			AssertEquals("Active status should be set to inactive because last account is on WTA Database", false, org.Contacts[0].OC_IsActive);
		}

		public void TestContactActiveStatusWhenLastUserAccountIsNotWTA()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAA";

			var dbNonWTA = Factory.NewWithValidTestData<LicenceDatabase>();
			dbNonWTA.LD_Product = ProductTypes.Codes.Enterprise;
			dbNonWTA.LD_OH_WebAccessOrg = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Test";
			contact.OC_Email = "a@a.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = false;

			var userAccountWTA = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccountWTA.EUA_FullName = "John Test";
			userAccountWTA.EUA_UserID = "ABD";
			userAccountWTA.EUA_Email = "a@a.com";
			userAccountWTA.EUA_LD = dbNonWTA.PK;
			userAccountWTA.EUA_OC_WebAccessContact = contact.PK;
			userAccountWTA.EUA_IsActive = true;
			userAccountWTA.EUA_IsContactRelationshipActive = true;
			userAccountWTA.EUA_ContactRelationshipStatus = string.Empty;

			var userAccountNonWTA = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccountNonWTA.EUA_FullName = "John Test 2";
			userAccountNonWTA.EUA_UserID = "AAA";
			userAccountNonWTA.EUA_Email = "b@b.com";
			userAccountNonWTA.EUA_LD = dbNonWTA.PK;
			userAccountNonWTA.EUA_OC_WebAccessContact = contact.PK;
			userAccountNonWTA.EUA_IsActive = true;
			userAccountNonWTA.EUA_IsContactRelationshipActive = true;
			userAccountNonWTA.EUA_ContactRelationshipStatus = string.Empty;

			var staffNonWTA = Factory.NewWithValidTestData<GlbStaff>();
			staffNonWTA.GS_FullName = "John Test 2";
			staffNonWTA.GS_IsActive = false;
			staffNonWTA.GS_Code = "AAA";
			staffNonWTA.GS_EmailAddress = "b@b.com";
			Factory.Save();

			var staffReport = new StaffReport(staffNonWTA);
			var importer = new VersionReportContactImporter(Factory, dbNonWTA);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			org.Contacts.Load();

			AssertEquals("Active status should remain active because last account is on a non-WTA Database", true, org.Contacts[0].OC_IsActive);
		}

		class DuplicateContactNameService : IAfterOnSavingBOProcessingService
		{
			public DuplicateContactNameService(OrgHeader org, ZString duplicateName)
			{
				Org = org;
				DuplicateName = duplicateName;
			}
			readonly OrgHeader Org;
			readonly ZString DuplicateName;

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var org = anotherFactory.Load<OrgHeader>(Org.PK);
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = DuplicateName;
				anotherFactory.Save();
			}
		}

		#region Import from varies scenarios

		public void TestImportFromStaffReport_SameStaffNames()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one.old@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.com.au";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "user.aaa.one@test.com.au";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User One",
				EmailAddress = "user.aaa.one@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			var importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();
			AssertImportedContact(importedContact1, staffReport1, "User One (1)");
			AssertImportedContact(importedContact2, staffReport2, "User One (2)");

			importer = new VersionReportContactImporter(Factory, database);
			org.Contacts.Reload(true);
			importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();
			AssertImportedContact(importedContact1, staffReport1, "User One (1)");
			AssertImportedContact(importedContact2, staffReport2, "User One (2)");
		}

		public void TestImportFromStaffReport_SameStaffEmails()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one.old@test.org";

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "Master Account";
			contact1b.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one.old@test.org";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "UXD";
			userAccount2.EUA_FullName = "Master Account";
			userAccount2.EUA_Email = "user.one@test.org";

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US2";
			userAccount3.EUA_FullName = "User One";
			userAccount3.EUA_Email = "user.one.222@test.org";

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "UXD",
				Name = "Master Account",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport3 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User One",
				EmailAddress = "user.one.222@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			importer.SaveIfNeeded();
			var importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			var importedContact3 = importer.ImportFromSingleStaffReport(staffReport3);
			importer.SaveIfNeeded();

			AssertImportedContact(importedContact1, staffReport1, "User One (1)");
			AssertImportedContact(importedContact2, staffReport2, "Master Account (1)", false);
			AssertImportedContact(importedContact3, staffReport3, "User One (2)");

			AssertEquals("", userAccount1.EUA_ContactRelationshipStatus);
			AssertEquals(ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount2.EUA_ContactRelationshipStatus);
			AssertEquals("user.one@test.org", importedContact2.OC_Email);
			AssertEquals(true, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals("", userAccount3.EUA_ContactRelationshipStatus);
		}

		public void TestImportFromStaffReport_SameStaffNamesAndEmails()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = ZGuid.Empty;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			Assert(!importedContact1.IsInDatabase);

			var importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			Assert("matched to existing contact", importedContact2.IsInDatabase);
			importer.SaveIfNeeded();

			AssertEquals("Should have email and web access due to IsInDatabase sorting", true, importedContact1.OC_WebAccessEnabled);
			AssertEquals("Should not have email and web access due to IsInDatabase sorting", false, importedContact2.OC_WebAccessEnabled);

			AssertImportedContact(importedContact1, staffReport1, "User One (1)", true);
			AssertImportedContact(importedContact2, staffReport2, "User One", false);
			AssertEquals("Should have DER status", ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount2.EUA_ContactRelationshipStatus);
		}

		public void TestImportFromStaffReport_Deactivation()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence1.Company.Header;
			var licence2 = BillingTestHelper.CreateAnotherDatabase(licence1.Company, "PRD");
			var database1 = licence1.Database;
			database1.LD_OH_WebAccessOrg = org.PK;
			var database2 = licence2.Database;
			database2.LD_OH_WebAccessOrg = org.PK;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.three@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database1.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";

			var userAccount31 = Factory.New<EdiCustomerUserAccount>();
			userAccount31.EUA_LD = database1.PK;
			userAccount31.EUA_UserID = "US3";
			userAccount31.EUA_FullName = "User Three";
			userAccount31.EUA_Email = "user.three@test.org";
			userAccount31.EUA_OC_WebAccessContact = contact3.PK;

			var userAccount32 = Factory.New<EdiCustomerUserAccount>();
			userAccount32.EUA_LD = database2.PK;
			userAccount32.EUA_UserID = "US3";
			userAccount32.EUA_FullName = "User Three";
			userAccount32.EUA_Email = "user.three@test.org";
			userAccount32.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = false,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two@test.org",
				BranchCode = "AAA"
			};

			var staffReport31 = new StaffReport()
			{
				IsActive = false,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three@test.org",
				BranchCode = "AAA"
			};

			var importer1 = new VersionReportContactImporter(Factory, database1);
			var importedContact1 = importer1.ImportFromSingleStaffReport(staffReport1);
			var importedContact2 = importer1.ImportFromSingleStaffReport(staffReport2);
			var importedContact31 = importer1.ImportFromSingleStaffReport(staffReport31);
			importer1.SaveIfNeeded();

			AssertEquals("Contact should be deactivated", false, importedContact1.OC_IsActive);
			AssertEquals("Contact should be deactivated", false, importedContact2.OC_IsActive);
			AssertEquals("Contact should not be deactivated as still at least one linked user is active", true, importedContact31.OC_IsActive);

			AssertNotEquals("Contact Verified Date", ZDateTime.Empty, importedContact1.OC_DetailsVerified);
			AssertNotEquals("Contact Verified Date", ZDateTime.Empty, importedContact2.OC_DetailsVerified);
			AssertEquals("The sync should be disabled to avoid racing issue if there are multiple PRD databases linked", ZDateTime.Empty, importedContact31.OC_DetailsVerified);

			var staffReport32 = new StaffReport()
			{
				IsActive = false,
				Code = "US3",
				Name = "User Three",
				EmailAddress = "user.three@test.org",
				BranchCode = "AAA"
			};

			var importer2 = new VersionReportContactImporter(Factory, database2);
			var importedContact32 = importer2.ImportFromSingleStaffReport(staffReport32);
			importer2.SaveIfNeeded();
			AssertEquals("Contact should be deactivated as no more linked active users", false, importedContact32.OC_IsActive);

			staffReport32.IsActive = true;
			importedContact32 = importer2.ImportFromSingleStaffReport(staffReport32);
			importer2.SaveIfNeeded();
			AssertEquals("Contact should be reactivated as there is one active user", true, importedContact32.OC_IsActive);
		}

		public void TestImportFromStaffReport_BlankEmail()
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

			var staffReport = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User Two",
				EmailAddress = string.Empty,
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, db);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			staffReport.EmailAddress = "aaa@test.com";
			var importedContact = importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			AssertEquals("aaa@test.com", importedContact.OC_Email);
			AssertEquals(true, importedContact.OC_WebAccessEnabled);

			staffReport.EmailAddress = string.Empty;
			importedContact = importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();

			AssertEquals(string.Empty, importedContact.OC_Email);
			AssertEquals(false, importedContact.OC_WebAccessEnabled);
		}

		[ExpectNoExceptions]
		public void TestImportFromStaffReport_UnicodeCharInContactName()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var charMap = new Dictionary<char, string>() {
				{ 'À', "A" }, { 'Á', "A" }, { 'Â', "A" }, { 'Ã', "A" }, { 'Ä', "A" }, { 'Å', "A" }, { 'Æ', "Ae" },
				{ 'Ç', "C" },
				{ 'È', "E" }, { 'É', "E" }, { 'Ê', "E" }, { 'Ë', "E" },
				{ 'Ì', "I" }, { 'Í', "I" }, { 'Î', "I" }, { 'Ï', "I" },
				{ 'Ð', "Dh" }, { 'Þ', "Th" },
				{ 'Ñ', "N" },
				{ 'Ò', "O" }, { 'Ó', "O" }, { 'Ô', "O" }, { 'Õ', "O" }, { 'Ö', "O" }, { 'Ø', "Oe" },
				{ 'Ù', "U" }, { 'Ú', "U" }, { 'Û', "U" }, { 'Ü', "U" },
				{ 'Ý', "Y" },
				{ 'ß', "ss" },
				{ 'à', "a" }, { 'á', "a" }, { 'â', "a" }, { 'ã', "a" }, { 'ä', "a" }, { 'å', "a" }, { 'æ', "ae" },
				{ 'ç', "c" },
				{ 'è', "e" }, { 'é', "e" }, { 'ê', "e" }, { 'ë', "e" },
				{ 'ì', "i" }, { 'í', "i" }, { 'î', "i" }, { 'ï', "i" },
				{ 'ð', "dh" }, { 'þ', "th" },
				{ 'ñ', "n" },
				{ 'ò', "o" }, { 'ó', "o" }, { 'ô', "o" }, { 'õ', "o" }, { 'ö', "o" }, { 'ø', "oe" },
				{ 'ù', "u" }, { 'ú', "u" }, { 'û', "u" }, { 'ü', "u" },
				{ 'ý', "y" }, { 'ÿ', "y" }
			};

			foreach (var pairValue in charMap.Values.Select(x => x.ToUpper()).Distinct())
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"User {pairValue}";
				contact.OC_Email = $"User{pairValue}@test.org";
			}

			Factory.Save();

			var index = 1;
			var staffReportList = new List<StaffReport>();
			foreach (var pairKey in charMap.Keys)
			{
				var staffReport = new StaffReport()
				{
					IsActive = true,
					Code = $"U{index}",
					Name = $"User {pairKey}",
					EmailAddress = $"User{pairKey}@test.com"
				};
				staffReportList.Add(staffReport);
				index++;
			}

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromStaffReports(staffReportList);
			importer.SaveIfNeeded();
			ExceptionReporterTestListener.Instance.Clear();
		}

		void AssertImportedContact(OrgContact contact, StaffReport staffReport, string expectedContactName = "", bool webAccessEnabled = true, bool emptyEmail = false)
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
					AssertEquals("Contact Name", staffReport.Name, contact.OC_ContactName);
				}

				AssertEquals("Contact Active", true, contact.OC_IsActive);
				AssertEquals("Contact Work Phone", staffReport.WorkPhone ?? string.Empty, contact.OC_Phone);
				AssertEquals("Contact Work Phone Extension", staffReport.WorkPhoneExtension ?? string.Empty, contact.OC_PhoneExtension);
				AssertEquals("Contact Job Title", staffReport.JobTitle ?? string.Empty, contact.OC_Title);
				AssertNotEquals("Contact Job Catagory should be always populated", string.Empty, contact.OC_JobCategory);
				AssertEquals("Contact Language", staffReport.LanguageCode ?? "EN", contact.OC_Language);

				if (emptyEmail)
				{
					AssertEquals("Contact Email", "", contact.OC_Email);
					AssertEquals("Contact Notify Mode", "PRN", contact.OC_NotifyMode);
				}
				else
				{
					AssertEquals("Contact Email", staffReport.EmailAddress, contact.OC_Email);
					AssertEquals("Contact Notify Mode", "EML", contact.OC_NotifyMode);
				}
				AssertNotEquals("Contact Address", ZGuid.Empty, contact.OC_OA_OrgAddress);
				AssertNotEquals("Contact Verified Date", ZDateTime.Empty, contact.OC_DetailsVerified);
				AssertEquals("OC_WebAccessEnabled", webAccessEnabled, contact.OC_WebAccessEnabled);
				AssertEquals(contact.PK, contact.Person.PrimaryRelationship.PPR_PrimaryId);
			});
		}

		#endregion

		#region Contact Relationship

		public void TestUpdateEmail_Overridden_DiffEmails_EmailChanged()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "new.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			userAccount2.EUA_IsEmailOverridden = true;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "very.new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);
			AssertEquals("user.one@test.org", userAccount1.EUA_Email);
			AssertEquals("very.new.user.two@test.org", userAccount2.EUA_Email);
			AssertEquals(false, userAccount1.EUA_IsEmailOverridden);
			AssertEquals("Should not override when EUA_OC_WebAccessContact is false", false, userAccount2.EUA_IsEmailOverridden);
		}

		public void TestUpdateEmail_Overridden_DiffEmails_EmailNotChanged()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "new.user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "new.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			userAccount2.EUA_IsEmailOverridden = true;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);
			AssertEquals("user.one@test.org", userAccount1.EUA_Email);
			AssertEquals("new.user.two@test.org", userAccount2.EUA_Email);
			AssertEquals(false, userAccount1.EUA_IsEmailOverridden);
			AssertEquals("Should not override when EUA_OC_WebAccessContact is false", false, userAccount2.EUA_IsEmailOverridden);
		}

		public void TestUpdateEmail_Overridden_SameEmails_EmailChanged()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "very.new.user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "new.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			userAccount2.EUA_IsEmailOverridden = true;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "very.new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);
			AssertEquals("user.one@test.org", userAccount1.EUA_Email);
			AssertEquals("very.new.user.two@test.org", userAccount2.EUA_Email);
			AssertEquals(false, userAccount1.EUA_IsEmailOverridden);
			AssertEquals("Should not override when EUA_OC_WebAccessContact is false", false, userAccount2.EUA_IsEmailOverridden);
		}

		public void TestUpdateEmail_Overridden_SameEmails_EmailNotChanged()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "new.user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "new.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			userAccount2.EUA_IsEmailOverridden = true;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);
			AssertEquals("user.one@test.org", userAccount1.EUA_Email);
			AssertEquals("new.user.two@test.org", userAccount2.EUA_Email);
			AssertEquals(false, userAccount1.EUA_IsEmailOverridden);
			AssertEquals("Should not override when EUA_OC_WebAccessContact is false", false, userAccount2.EUA_IsEmailOverridden);
		}

		public void TestUpdateEmail_Distinct()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "New User Two";
			contact3.OC_Email = "new.user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "xxx.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);

			AssertEquals(ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount2.EUA_ContactRelationshipStatus);
			AssertEquals(false, userAccount2.EUA_IsContactRelationshipActive);

			AssertEquals(true, userAccount1.EUA_UserVerifiedDateUtc.IsEmpty);
			AssertEquals(true, userAccount2.EUA_UserVerifiedDateUtc.IsEmpty);
		}

		public void TestUpdateEmail_Distinct_WebEnabled()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";
			contact2.OC_WebAccessEnabled = true;

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "New User Two";
			contact3.OC_Email = "new.user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "xxx.user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			var contact = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertNotNull(contact);

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);

			AssertEquals(ContactRelationshipStatusList.Codes.EmailChanged, userAccount2.EUA_ContactRelationshipStatus);
			AssertEquals(false, userAccount2.EUA_IsContactRelationshipActive);

			AssertEquals(true, userAccount1.EUA_UserVerifiedDateUtc.IsEmpty);
			AssertEquals(true, userAccount2.EUA_UserVerifiedDateUtc.IsEmpty);
		}

		public void TestUpdateEmail_Distinct_EmptyEmail()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			Factory.Save();

			var staffReport = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var contact = importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();
			AssertNotNull(contact);
			AssertEquals(true, contact.OC_Email.IsEmpty);

			var userAccount = Factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK)).Single();
			AssertEquals(true, userAccount.EUA_Email.IsEmpty);
			AssertEquals("", userAccount.EUA_ContactRelationshipStatus);
			AssertEquals(true, userAccount.EUA_IsContactRelationshipActive);
		}

		public void TestUpdateContactRelationship_DiffEmails()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = false,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "new.user.two@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1);
			importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			AssertEquals(userAccount1.EUA_OC_WebAccessContact, contact1.PK);
			AssertEquals("Should remove the link if contact is inactive", userAccount2.EUA_OC_WebAccessContact, ZGuid.Empty);

			AssertEquals(true, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals(false, userAccount2.EUA_IsContactRelationshipActive);
			AssertEquals(ContactRelationshipStatusList.Codes.EmailChanged, userAccount2.EUA_ContactRelationshipStatus);

			AssertEquals(true, userAccount1.EUA_UserVerifiedDateUtc.IsEmpty);
			AssertEquals(true, userAccount2.EUA_UserVerifiedDateUtc.IsEmpty);
		}

		public void TestUpdateContactRelationship_Reactivation()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_IsActive = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_IsActive = false;

			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			importer.SaveIfNeeded();

			userAccount1.Reload();
			AssertEquals(false, importedContact1.OC_IsActive);
			AssertEquals(true, userAccount1.EUA_IsActive);
			AssertEquals(false, userAccount1.EUA_IsContactRelationshipActive);
			AssertEquals(ContactRelationshipStatusList.Codes.AccountReactivated, userAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestUpdateContactRelationship_ReactivationWithActiveContact()
		{
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;

			var org = database.LicEnterprise.Organisation;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			ImportStaffReport(database, isActive: false);
			AssertActiveStatus(contact1, userAccount1, isActive: false, isContactRelationshipActive: false, contactRelationshipStatus: string.Empty);

			contact1.OC_IsActive = true;
			Factory.Save();

			ImportStaffReport(database, false);
			AssertActiveStatus(contact1, userAccount1, isActive: false, isContactRelationshipActive: false, contactRelationshipStatus: string.Empty);

			contact1.OC_IsActive = true;
			Factory.Save();

			ImportStaffReport(database, true);
			Factory.Save();
			AssertActiveStatus(contact1, userAccount1, isActive: true, isContactRelationshipActive: false, contactRelationshipStatus: ContactRelationshipStatusList.Codes.AccountReactivated);
		}

		void ImportStaffReport(LicenceDatabase database, bool isActive)
		{
			var staffReport = new StaffReport()
			{
				IsActive = isActive,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org"
			};
			var staffReports = new List<StaffReport>() { staffReport };

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromStaffReports(staffReports);
			importer.SaveIfNeeded();
		}

		static void AssertActiveStatus(OrgContact contact, EdiCustomerUserAccount userAccount, bool isActive, bool isContactRelationshipActive, string contactRelationshipStatus)
		{
			userAccount.Reload();
			AssertEquals(isActive, userAccount.EUA_IsActive);
			AssertEquals(isContactRelationshipActive, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals(contactRelationshipStatus, userAccount.EUA_ContactRelationshipStatus);

			contact.Reload();
			AssertEquals(isActive, contact.OC_IsActive);
		}

		public void TestUpdateContactRelationship_ContactLinkedToDifferentDatabaseUser()
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
			userAccount1.EUA_Email = "userx@cw1.com";
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
			AssertEquals(ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked, userAccount2.EUA_ContactRelationshipStatus);
		}

		#endregion

		#region Web access handling

		public void TestImportFromStaffReport_WebAccess()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";
			Factory.Save();
			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA"
			};
			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User Two",
				EmailAddress = "user.two@test.org",
				BranchCode = "AAA"
			};
			var importer = new VersionReportContactImporter(Factory, database);
			var importedContact1 = importer.ImportFromSingleStaffReport(staffReport1);
			var importedContact2 = importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();
			AssertEquals("User One", importedContact1.OC_ContactName);
			AssertEquals("User Two", importedContact2.OC_ContactName);
			AssertEquals(true, importedContact1.OC_WebAccessEnabled);
			AssertEquals(true, importedContact2.OC_WebAccessEnabled);
		}

		public void TestImportFromStaffReport_WebAccess_DuplicateEmails()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User 1";
			contact1a.OC_Email = "u1@cw1.com";
			contact1a.OC_WebAccessEnabled = false;

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "User 1 (1)";
			contact1b.OC_Email = "u1@cw1.com";
			contact1b.OC_WebAccessEnabled = true;

			var contact2a = org.Contacts.AddNew();
			contact2a.OC_ContactName = "User 2A";
			contact2a.OC_Email = "u2@cw1.com";
			contact2a.OC_WebAccessEnabled = true;

			var contact2b = org.Contacts.AddNew();
			contact2b.OC_ContactName = "User 2";
			contact2b.OC_Email = "u2@cw1.com";
			contact2b.OC_WebAccessEnabled = false;

			var userAccount1a = Factory.New<EdiCustomerUserAccount>();
			userAccount1a.EUA_LD = database.PK;
			userAccount1a.EUA_UserID = "US1";
			userAccount1a.EUA_FullName = "User 1";
			userAccount1a.EUA_Email = "u1@cw1.com";
			userAccount1a.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount1b = Factory.New<EdiCustomerUserAccount>();
			userAccount1b.EUA_LD = database.PK;
			userAccount1b.EUA_UserID = "USO";
			userAccount1b.EUA_FullName = "User 1";
			userAccount1b.EUA_Email = "u1@cw1.com";
			userAccount1b.EUA_OC_WebAccessContact = contact1b.PK;

			var userAccount1c = Factory.New<EdiCustomerUserAccount>();
			userAccount1c.EUA_LD = database.PK;
			userAccount1c.EUA_UserID = "USX";
			userAccount1c.EUA_FullName = "User 1";
			userAccount1c.EUA_Email = "u1@cw1.com";

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User 2";
			userAccount2.EUA_Email = "u2@cw1.com";
			userAccount2.EUA_OC_WebAccessContact = contact2b.PK;

			Factory.Save();

			var staffReport1a = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User 1",
				EmailAddress = "u1@cw1.com"
			};

			var staffReport1b = new StaffReport()
			{
				IsActive = true,
				Code = "USO",
				Name = "User 1",
				EmailAddress = "u1@cw1.com"
			};

			var staffReport1c = new StaffReport()
			{
				IsActive = true,
				Code = "USX",
				Name = "User 1",
				EmailAddress = "u1@cw1.com"
			};

			var staffReport2 = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User 2",
				EmailAddress = "u2@cw1.com"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			var importContact1c = importer.ImportFromSingleStaffReport(staffReport1c);
			importer.SaveIfNeeded();

			importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1a);
			importer.ImportFromSingleStaffReport(staffReport1b);
			importer.ImportFromSingleStaffReport(staffReport2);
			importer.SaveIfNeeded();

			contact1a.Reload();
			contact1b.Reload();
			importContact1c.Reload();
			contact2a.Reload();
			contact2b.Reload();

			AssertEquals("Another staff linked contact with same email already has web access", false, contact1a.OC_WebAccessEnabled);
			AssertEquals("Another staff linked contact with same email already has web access", ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount1a.EUA_ContactRelationshipStatus);
			AssertEquals("This linked contact with same email should keep web access", true, contact1b.OC_WebAccessEnabled);
			AssertEquals("Another staff linked contact with same email already has web access", false, importContact1c.OC_WebAccessEnabled);

			AssertEquals("Contact without linked staff should not keep web access", false, contact2a.OC_WebAccessEnabled);
			AssertEquals("This linked contact with same email should grant web access", true, contact2b.OC_WebAccessEnabled);
		}

		public void TestImportFromStaffReport_WebAccess_DuplicateEmails_AccessShouldNotBounce()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;
			var commonEmail = "u2@cw1.com";

			var contact2a = org.Contacts.AddNew();
			contact2a.OC_ContactName = "User 2A";
			contact2a.OC_Email = "ux@cw1.com";
			contact2a.OC_WebAccessEnabled = true;

			var contact2b = org.Contacts.AddNew();
			contact2b.OC_ContactName = "User 2";
			contact2b.OC_Email = commonEmail;
			contact2b.OC_WebAccessEnabled = true;

			var userAccount2a = Factory.New<EdiCustomerUserAccount>();
			userAccount2a.EUA_LD = database.PK;
			userAccount2a.EUA_UserID = "U2A";
			userAccount2a.EUA_FullName = "User 2";
			userAccount2a.EUA_Email = "ux@cw1.com";
			userAccount2a.EUA_OC_WebAccessContact = contact2a.PK;

			var userAccount2b = Factory.New<EdiCustomerUserAccount>();
			userAccount2b.EUA_LD = database.PK;
			userAccount2b.EUA_UserID = "U2B";
			userAccount2b.EUA_FullName = "User 2";
			userAccount2b.EUA_Email = commonEmail;
			userAccount2b.EUA_OC_WebAccessContact = contact2b.PK;

			Factory.Save();

			var staffReport2a = new StaffReport()
			{
				IsActive = true,
				Code = "U2A",
				Name = "User 2",
				EmailAddress = commonEmail
			};

			var staffReport2b = new StaffReport()
			{
				IsActive = true,
				Code = "U2B",
				Name = "User 2",
				EmailAddress = commonEmail
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport2a);
			importer.SaveIfNeeded();

			contact2a.Reload();
			contact2b.Reload();

			AssertEquals("Web access should be revoked, since this email is already used for contact 2b's web access which has a user account so it should stay there", false, contact2a.OC_WebAccessEnabled);
			AssertEquals("DER status should be applied", ContactRelationshipStatusList.Codes.DistinctEmailRequired, userAccount2a.EUA_ContactRelationshipStatus);
			AssertEquals("Email should be updated to the common email", commonEmail, userAccount2a.EUA_Email);
			AssertEquals("Email should be updated to the common email", commonEmail, contact2a.OC_Email);
			AssertEquals("Web access is already linked to contact 2b which has a user account so it should stay there", true, contact2b.OC_WebAccessEnabled);
			AssertEquals("Precondition: Status should be empty", string.Empty, userAccount2b.EUA_ContactRelationshipStatus);

			importer.ImportFromSingleStaffReport(staffReport2b);
			importer.SaveIfNeeded();

			contact2a.Reload();
			contact2b.Reload();
			AssertEquals("Web access is already linked to contact 2b which has a user account so it should stay there", true, contact2b.OC_WebAccessEnabled);
			AssertEquals("Status should be remain empty, so web access is allowed", string.Empty, userAccount2b.EUA_ContactRelationshipStatus);
		}

		public void TestImportFromStaffReport_WebAccess_DuplicateByChangedEmail_ConflictIsLinked()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User 1";
			contact1a.OC_Email = "u1@cw1.com";
			contact1a.OC_WebAccessEnabled = true;

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "User 1 (1)";
			contact1b.OC_Email = "u1.additional@cw1.com";
			contact1b.OC_WebAccessEnabled = true;

			var userAccount1a = Factory.New<EdiCustomerUserAccount>();
			userAccount1a.EUA_LD = database.PK;
			userAccount1a.EUA_UserID = "US1";
			userAccount1a.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount1b = Factory.New<EdiCustomerUserAccount>();
			userAccount1b.EUA_LD = database.PK;
			userAccount1b.EUA_UserID = "USO";
			userAccount1b.EUA_OC_WebAccessContact = contact1b.PK;

			Factory.Save();

			var staffReport1b = new StaffReport()
			{
				IsActive = true,
				Code = "USO",
				Name = "User 1",
				EmailAddress = "u1@cw1.com"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1b);
			importer.SaveIfNeeded();
			contact1b.Reload();

			AssertEquals("Another staff linked contact with same email already has web access", false, contact1b.OC_WebAccessEnabled);
		}

		public void TestImportFromStaffReport_WebAccess_DuplicateByChangedEmail_ConflictIsNotLinked()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User 1";
			contact1a.OC_Email = "u1@cw1.com";
			contact1a.OC_WebAccessEnabled = true;

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "User 1 (1)";
			contact1b.OC_Email = "u1.additional@cw1.com";
			contact1b.OC_WebAccessEnabled = true;

			var userAccount1b = Factory.New<EdiCustomerUserAccount>();
			userAccount1b.EUA_LD = database.PK;
			userAccount1b.EUA_UserID = "USO";
			userAccount1b.EUA_OC_WebAccessContact = contact1b.PK;

			Factory.Save();

			var staffReport1b = new StaffReport()
			{
				IsActive = true,
				Code = "USO",
				Name = "User 1",
				EmailAddress = "u1@cw1.com"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1b);
			importer.SaveIfNeeded();
			contact1b.Reload();

			AssertEquals("This linked contact with same email should grant web access", true, contact1b.OC_WebAccessEnabled);
		}

		public void TestImportFromStaffReport_WebAccess_DuplicateByChangedEmail_MultipleCandidates()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1a = org.Contacts.AddNew();
			contact1a.OC_ContactName = "User 1";
			contact1a.OC_Email = "account@cw1.com";
			contact1a.OC_WebAccessEnabled = true;

			var contact1b = org.Contacts.AddNew();
			contact1b.OC_ContactName = "User ABC";
			contact1b.OC_Email = "abc@cw1.com";
			contact1b.OC_WebAccessEnabled = true;

			var contact1c = org.Contacts.AddNew();
			contact1c.OC_ContactName = "User DEF";
			contact1c.OC_Email = "def@cw1.com";
			contact1c.OC_WebAccessEnabled = true;

			var userAccount1b = Factory.New<EdiCustomerUserAccount>();
			userAccount1b.EUA_LD = database.PK;
			userAccount1b.EUA_UserID = "US2";
			userAccount1b.EUA_OC_WebAccessContact = contact1b.PK;

			var userAccount1c = Factory.New<EdiCustomerUserAccount>();
			userAccount1c.EUA_LD = database.PK;
			userAccount1c.EUA_UserID = "US3";
			userAccount1c.EUA_OC_WebAccessContact = contact1c.PK;

			Factory.Save();

			var staffReport1b = new StaffReport()
			{
				IsActive = true,
				Code = "US2",
				Name = "User 2",
				EmailAddress = "account@cw1.com"
			};

			var staffReport1c = new StaffReport()
			{
				IsActive = true,
				Code = "US3",
				Name = "User 3",
				EmailAddress = "account@cw1.com"
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport1b);
			importer.ImportFromSingleStaffReport(staffReport1c);
			importer.SaveIfNeeded();

			contact1a.Reload();
			contact1b.Reload();
			contact1c.Reload();

			AssertEquals("Conflict with no linked staff should disable web access", false, contact1a.OC_WebAccessEnabled);
			AssertEquals("First candidate should grant web access", true, contact1b.OC_WebAccessEnabled);
			AssertEquals("Second candidate should not grant web access", false, contact1c.OC_WebAccessEnabled);
		}

		[ExpectNoExceptions]
		public void TestImportFromStaffReport_WebAccess_ContactLinkedToAnotherStaff()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.OC_WebAccessEnabled = false;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 10000";
			contact2.OC_Email = "u1@cw1.com";
			contact2.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "USO";
			userAccount2.EUA_FullName = "User 10000";
			userAccount2.EUA_Email = "u1@cw1.com";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "USX";
			userAccount3.EUA_FullName = "User 1";
			userAccount3.EUA_Email = "";

			Factory.Save();

			var staffReport = new StaffReport()
			{
				IsActive = true,
				Code = "USX",
				Name = "User 1",
				EmailAddress = ""
			};

			var importer = new VersionReportContactImporter(Factory, database);
			importer.ImportFromSingleStaffReport(staffReport);
			importer.SaveIfNeeded();
		}

		public void TestImportFromStaffReport_WebAccess_NoOverriddenRightDenied()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			contact1.OC_WebAccessEnabled = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var webSecurityRight = EDIWebSecurityRightsList.CustomerService;
			var orgRight1 = org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, webSecurityRight.Code))[0] as OrgSecurity;
			var contactRightQuery1 = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight1.PK);
			contactRightQuery1.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact1.PK);
			var contactRight1 = contact1.SecurityRightsForBindingOnly.Find(contactRightQuery1)[0] as OrgSecurityContacts;
			orgRight1.OX_Granted = false;
			contactRight1.OZ_Granted = true;

			Factory.Save();

			var staffReport = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User 1A",
				EmailAddress = "u1@cw1.com"
			};

			var importer = new VersionReportContactImporter(new BusinessObjectFactory() { RefreshEnabled = false }, database);
			importer.ImportFromStaffReports(new List<StaffReport> { staffReport, staffReport });

			var loadFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadConact1 = loadFactory.Load<OrgContact>(contact1.PK);
			AssertEquals("Web access is still enabled", true, loadConact1.OC_WebAccessEnabled);
			var isRightStillGranted = OrgContactWebUser.IsRightGrantedWithoutCache(webSecurityRight, loadConact1);
			AssertEquals("Web security right should be still granted", true, isRightStillGranted);
		}

		public void TestImportFromStaffReport_UpdateFromProductionDB()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence1.Company.Header;
			var licence2 = BillingTestHelper.CreateAnotherDatabase(licence1.Company, "PRD");
			var licenceWTA = BillingTestHelper.CreateAnotherDatabase(licence1.Company, "WTA");
			var database1 = licence1.Database;
			database1.LD_OH_WebAccessOrg = org.PK;
			database1.LD_LicenceType = "TST";
			var database2 = licence2.Database;
			database2.LD_OH_WebAccessOrg = org.PK;
			database2.LD_LicenceType = "PRD";
			var databaseWTA = licenceWTA.Database;
			databaseWTA.LD_OH_WebAccessOrg = org.PK;
			databaseWTA.LD_LicenceType = "PRD";
			databaseWTA.LD_Product = "WTA";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_Title = "Mr TST";
			contact1.OC_Phone = "0123";
			contact1.OC_PhoneExtension = "456";
			contact1.OC_Language = "ENG";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccountWTA1 = Factory.New<EdiCustomerUserAccount>();
			userAccountWTA1.EUA_LD = databaseWTA.PK;
			userAccountWTA1.EUA_UserID = "US1";
			userAccountWTA1.EUA_FullName = "User One";
			userAccountWTA1.EUA_Email = "user.one@test.org";
			userAccountWTA1.EUA_OC_WebAccessContact = contact1.PK;
			Factory.Save();

			var staffReport1 = new StaffReport()
			{
				IsActive = true,
				Code = "US1",
				Name = "User One",
				EmailAddress = "user.one@test.org",
				BranchCode = "AAA",
				WorkPhone = "888",
				WorkPhoneExtension = "999",
				LanguageCode = "CHS",
				JobTitle = "Mr PRD"
			};

			var importer1 = new VersionReportContactImporter(Factory, database2);
			importer1.ImportFromStaffReports(new List<StaffReport> { staffReport1 });
			importer1.SaveIfNeeded();

			var newFactory = new BusinessObjectFactory();
			var contact1InNewFactory = newFactory.Load<OrgContact>(contact1.PK);

			// If the user account is PRD and there's a NON-PRD system linked
			// update everything even the relationship status is not active yet.
			AssertEquals(contact1InNewFactory.OC_Title, "Mr PRD");
			AssertEquals(contact1InNewFactory.OC_Phone, "+61888");
			AssertEquals(contact1InNewFactory.OC_PhoneExtension, "999");
			AssertEquals(contact1InNewFactory.OC_Language, "ZH-CN");

			AssertEquals(database2.LD_LicenceType, "PRD");
			var userAccount3PRDQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database2.PK);
			userAccount3PRDQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, "US1");
			userAccount3PRDQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact1.PK);
			var userAccount3PRD = newFactory.Load<EdiCustomerUserAccount>(userAccount3PRDQuery).Single();
			AssertEquals(false, userAccount3PRD.EUA_IsContactRelationshipActive);
		}

		#endregion

		#region DB hits

		public void TestImport_NewContactsDBHits()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			const int staffCount = 5;

			for (int i = 0; i < staffCount; i++)
			{
				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = $"US{i}";
				userAccount.EUA_FullName = $"User {i}";
				userAccount.EUA_Email = $"user{i}@test.org";
			}

			Factory.Save();

			var staffReports = new List<StaffReport>(staffCount);
			for (int i = 0; i < staffCount; i++)
			{
				var staffReport = new StaffReport()
				{
					Code = $"US{i}",
					Name = $"User {i}",
					EmailAddress = $"user{i}@test.org",
					BranchCode = "AAA",
					IsActive = true
				};
				staffReports.Add(staffReport);
			}

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var lookupFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var databaseInLookupFactory = lookupFactory.Load<LicenceDatabase>(database.PK);
				var importer = new VersionReportContactImporter(lookupFactory, databaseInLookupFactory);
				importer.ImportFromStaffReports(staffReports);
				importer.SaveIfNeeded();

				var expectedHitCounts = new Dictionary<string, int>
				{
					{ LicenceDatabaseSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 2 },	//One does fetch hint load and one does load with different query
				};
				AssertDbHits(expectedHitCounts, lookupFactory);
			}
		}

		public void TestImport_ExistingContactsDBHits()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			database.LD_StaffFirstReportUtc = ZDateTime.UtcNow;

			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			const int staffCount = 5;

			for (int i = 0; i < staffCount; i++)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"User {i}";
				contact.OC_Email = $"user{i}@test.org";
				contact.OC_WebAccessEnabled = true;

				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = $"US{i}";
				userAccount.EUA_FullName = $"User {i}";
				userAccount.EUA_Email = $"user{i}@test.org";
				userAccount.EUA_OC_WebAccessContact = contact.PK;
			}

			Factory.Save();

			var staffReports = new List<StaffReport>(staffCount);
			for (int i = 0; i < staffCount; i++)
			{
				var staffReport = new StaffReport()
				{
					Code = $"US{i}",
					Name = $"User {i}",
					EmailAddress = $"user{i}@test.org",
					BranchCode = "AAA",
					IsActive = true
				};
				staffReports.Add(staffReport);
			}

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var lookupFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var databaseInLookupFactory = lookupFactory.Load<LicenceDatabase>(database.PK);
				var importer = new VersionReportContactImporter(lookupFactory, databaseInLookupFactory);
				importer.ImportFromStaffReports(staffReports);
				importer.SaveIfNeeded();

				var expectedHitCounts = new Dictionary<string, int>
				{
					{ LicenceDatabaseSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 2 },	//Calling OrgHeader.Contacts to build contact name dictionary has two hits - one does fetch hint load and one does actual load
				};
				AssertDbHits(expectedHitCounts, lookupFactory);
			}
		}

		public void TestImport_NoChangeContactsDBHits()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			database.LD_StaffFirstReportUtc = ZDateTime.UtcNow;

			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			const int staffCount = 5;

			for (int i = 0; i < staffCount; i++)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"User {i}";
				contact.OC_Email = $"user{i}@test.org";
				contact.OC_OA_OrgAddress = org.MainAddress.PK;
				contact.OC_DetailsVerified = ZDateTime.Now;
				contact.OC_WebAccessEnabled = true;

				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = $"US{i}";
				userAccount.EUA_FullName = $"User {i}";
				userAccount.EUA_Email = $"user{i}@test.org";
				userAccount.EUA_OC_WebAccessContact = contact.PK;
			}

			Factory.Save();

			var staffReports = new List<StaffReport>(staffCount);
			for (int i = 0; i < staffCount; i++)
			{
				var staffReport = new StaffReport()
				{
					Code = $"US{i}",
					Name = $"User {i}",
					EmailAddress = $"user{i}@test.org",
					IsActive = true
				};
				staffReports.Add(staffReport);
			}

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var importFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var databaseInImportFactory = importFactory.Load<LicenceDatabase>(database.PK);
				var importer = new VersionReportContactImporter(importFactory, databaseInImportFactory);
				importer.ImportFromStaffReports(staffReports);
				importer.SaveIfNeeded();

				var expectedHitCounts = new Dictionary<string, int>
				{
					{ LicenceDatabaseSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 2 }	//One does fetch hint load and one does load with different query
				};
				AssertDbHits(expectedHitCounts, importFactory);
			}
		}

		#endregion

		class VersionReportContactImporterForRetryTest : VersionReportContactImporter
		{
			public Func<bool> ShouldThrow = () => false;

			public VersionReportContactImporterForRetryTest(BusinessObjectFactory factory, LicenceDatabase database) : base(factory, database)
			{
			}

			protected override bool NeedSave => true;
			protected override void GrantContactsWebAccess()
			{
				if (ShouldThrow())
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), null);
				}
				else
				{
					base.GrantContactsWebAccess();
				}
			}
		}

		class VersionReportContactImporterForTest : VersionReportContactImporter
		{
			public EdiCustomerUserAccount[] LastFetchHintsUserAccounts { get; private set; }

			public VersionReportContactImporterForTest(BusinessObjectFactory factory, LicenceDatabase database)
				: base(factory, database)
			{
			}

			protected override EdiCustomerUserAccount[] GetFetchHintsUserAccounts(List<StaffReport> batch)
			{
				LastFetchHintsUserAccounts = null;
				var result = base.GetFetchHintsUserAccounts(batch);
				LastFetchHintsUserAccounts = result;
				return result;
			}
		}
	}
}
