using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Client.EDI.MasterFiles.Business.Test.EDIGlbStaffTest;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class StaffContactImporterTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdateContactFromStaff()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Test User A";
			staff.GS_EmailAddress = "test.usera@test.org";
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staff);
			}

			org.Contacts.Reload(false);
			AssertEquals(1, org.Contacts.Count);
			var contact = org.Contacts[0];
			AssertEquals("Test User A", contact.OC_ContactName);
			AssertEquals("test.usera@test.org", contact.OC_Email);
			AssertEquals("Precondition", false, staff.GS_PER.IsEmpty);
			AssertEquals(staff.GS_PER, contact.OC_PER);
		}

		[ExpectNoExceptions]
		public void TestCreateOrUpdateContactFromStaff_NullRef()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(null);
			}
		}

		public void TestCreateOrUpdateContactFromStaff_ExistingEdiCustomerUserAccount()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_UserID = "TU0";
			userAccount.EUA_FullName = "User 1";
			userAccount.EUA_LD = licHeader.Database.PK;
			userAccount.EUA_Email = "u1@cw1.com";
			userAccount.EUA_IsActive = true;
			Factory.Save();

			var staff = Factory.NewWithValidTestData<EDIGlbStaffForTest>();
			staff.GS_Code = "TU0";
			staff.GS_FullName = "Test User1";
			staff.GS_EmailAddress = "test_user1@cw1.com";
			staff.DbNum = licHeader.Database.LD_DatabaseNumber;

			var staff2 = Factory.NewWithValidTestData<EDIGlbStaffForTest>();
			staff2.GS_Code = "TU2";
			staff2.GS_FullName = "Test User2";
			staff2.GS_EmailAddress = "test_user2@cw1.com";
			staff2.DbNum = licHeader.Database.LD_DatabaseNumber;

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(licHeader.Database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staff);
				importer.CreateOrUpdateContactFromStaff(staff2);
			}

			AssertEquals(userAccount.EUA_UserID, staff.GS_Code);
			AssertEquals(userAccount.EUA_FullName, staff.GS_FullName);
			AssertEquals(userAccount.EUA_Email, staff.GS_EmailAddress);
			AssertEquals(false, userAccount.EUA_OC_WebAccessContact.IsEmpty);

			var userAccount2 = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "TU2"));
			AssertEquals(userAccount2.EUA_UserID, staff2.GS_Code);
			AssertEquals(userAccount2.EUA_FullName, staff2.GS_FullName);
			AssertEquals(userAccount2.EUA_Email, staff2.GS_EmailAddress);
			AssertEquals(false, userAccount2.EUA_OC_WebAccessContact.IsEmpty);
		}

		public void TestCreateOrUpdateContactFromStaff_BlankEmail()
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

			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Test User A";
			staff.GS_EmailAddress = string.Empty;
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(db.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staff);
				Factory.Save();

				staff.GS_EmailAddress = "aaa@test.com";
				importer.CreateOrUpdateContactFromStaff(staff);
				Factory.Save();

				org.Contacts.Reload(true);
				var importedContact = org.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "Test User A");
				AssertEquals("aaa@test.com", importedContact.OC_Email);
				AssertEquals(true, importedContact.OC_WebAccessEnabled);

				staff.GS_EmailAddress = string.Empty;
				importer.CreateOrUpdateContactFromStaff(staff);
				Factory.Save();

				importedContact.Reload();
				AssertEquals(string.Empty, importedContact.OC_Email);
				AssertEquals(false, importedContact.OC_WebAccessEnabled);
			}
		}

		public void TestCreateOrUpdateContactFromStaff_MergeContactToStaffPerson()
		{
			// Simulate staff creation as GlbStaff instead of EDIGlbStaff
			var staffPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_EmailAddress, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES (@staffPK, 'AAA', 'Staff A', '', GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
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

			var staff = Factory.Load<EDIGlbStaff>(staffPK);
			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				staff.GS_EmailAddress = "aaa@test.com";
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staff);
				Factory.Save();
			}

			staff.Reload();
			org.Contacts.Load();
			var contact = org.Contacts[0];

			AssertNotNull(staff);
			AssertNotNull(contact);
			AssertEquals("Imported contact should link to staff person", staff.GS_PER, contact.OC_PER);
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

			var staffNonWTA = Factory.NewWithValidTestData<EDIGlbStaff>();
			staffNonWTA.GS_FullName = "John Test 2";
			staffNonWTA.GS_IsActive = false;
			staffNonWTA.GS_Code = "AAA";
			staffNonWTA.GS_EmailAddress = "b@b.com";
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(dbNonWTA.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staffNonWTA);
				Factory.Save();
			}

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

			var staffNonWTA = Factory.NewWithValidTestData<EDIGlbStaff>();
			staffNonWTA.GS_FullName = "John Test 2";
			staffNonWTA.GS_IsActive = false;
			staffNonWTA.GS_Code = "AAA";
			staffNonWTA.GS_EmailAddress = "b@b.com";
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(dbNonWTA.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staffNonWTA);
				Factory.Save();
			}

			org.Contacts.Load();

			AssertEquals("Active status should remain active because last account is on non-WTA Database", true, org.Contacts[0].OC_IsActive);
		}
	}
}
