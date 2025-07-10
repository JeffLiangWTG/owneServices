using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class StaffContactValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestFindOrCreateContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var newOrg = licence.Company.Header;
			newOrg.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "Your Name", EmailAddress = "bob@wisetechglobal.com" };
			SecureQueryString queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd) };
			var helper = new StaffContactHelper(queryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			newOrg.Contacts.Reload(true);
			AssertEquals("Should be created automatically", 1, newOrg.Contacts.Count);
			Assert(newOrg.Contacts[0].IsInDatabase);
			Assert("Should enable web access", newOrg.Contacts[0].OC_WebAccessEnabled);
			Assert("Should have an empty web access password", newOrg.Contacts[0].OC_PasswordHash.IsEmpty);
			AssertEquals(contact.PK, newOrg.Contacts[0].PK);
			helper = new StaffContactHelper(queryString.ToString());
			var contact2 = helper.FindOrCreateContact().Contact;
			AssertEquals("Should load instead of create", 1, newOrg.Contacts.Count);
			AssertEquals(contact.PK, contact2.PK);
			newOrg.Contacts[0].OC_IsActive = false;
			Factory.Save();
			helper = new StaffContactHelper(queryString.ToString());
			var contact3 = helper.FindOrCreateContact();
			newOrg.Contacts.Reload(false);
			AssertEquals("Should not create new active contact", 1, newOrg.Contacts.Count);
		}

		public void TestFindOrCreateContact_ByStaffCode()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
			var database = licence.Database;
			var staff = Factory.New<EdiCustomerUserAccount>();
			staff.EUA_LD = database.PK;
			staff.EUA_UserID = "US1";
			staff.EUA_FullName = "User 1";
			staff.EUA_Email = "user1@test.com";
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "User 1", EmailAddress = "user1@test.com" };
			SecureQueryString queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), [StaffContactValueObjectHelper.QueryStringKeys.StaffCode] = "US1" };
			var helper = new StaffContactHelper(queryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			var logCount = contact.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecutedCode);
			AssertEquals(1, logCount);
		}

		public void TestFindOrCreateContact_NoCustomerUserAccount_ByStaffCode()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "User 1", EmailAddress = "user1@test.com" };
			SecureQueryString queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), [StaffContactValueObjectHelper.QueryStringKeys.StaffCode] = "US1" };
			var helper = new StaffContactHelper(queryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "US1"));
			AssertNotNull(userAccount);
			AssertNotNull(contact);
			AssertEquals("Full name should be populated from Xsd.OrgContact", contactXsd.Name, userAccount.EUA_FullName);
			AssertEquals("Email should be populated from Xsd.OrgContact", contactXsd.EmailAddress, userAccount.EUA_Email);
			AssertEquals("Should not require email verification", true, userAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Contact relationship should be active", true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals(string.Empty, userAccount.EUA_ContactRelationshipStatus);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedUserAccount = newFactory.Load<EdiCustomerUserAccount>(userAccount.PK);
			reloadedUserAccount.EUA_IsEmailVerificationRequired = false;
			newFactory.Save();
			helper = new StaffContactHelper(queryString.ToString());
			contact = helper.FindOrCreateContact().Contact;
			userAccount.Reload();
			AssertEquals("Account should be connected to the contact", contact.PK, userAccount.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be active", true, userAccount.EUA_IsContactRelationshipActive);
			AssertEquals(string.Empty, userAccount.EUA_ContactRelationshipStatus);
		}

		public void TestFindOrCreateContact_InvalidStaffRecords()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactValueObject = new Xsd.OrgContact { Name = "Andrew", EmailAddress = "alex@andrew.com", HomePhone = "+61", Fax = "asd", Mobile = "+61 (*) 2222-2222" };
			var securedQueryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactValueObject) };
			var helper = new StaffContactHelper(securedQueryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			org.Contacts.Reload(true);
			AssertEquals("Should be created automatically", 1, org.Contacts.Count);
			CombineAssertions(() =>
			{
				Assert("IsInDatabase", org.Contacts[0].IsInDatabase);
				AssertEquals("PK", contact.PK, org.Contacts[0].PK);
				AssertEquals("OC_HomePhone", ZString.Empty, org.Contacts[0].OC_HomePhone);
				AssertEquals("OC_Fax", ZString.Empty, org.Contacts[0].OC_Fax);
				AssertEquals("OC_Mobile", ZString.Empty, org.Contacts[0].OC_Mobile);
			});
		}

		[ExpectNoExceptions]
		public void TestFindOrCreateContact_Concurrency()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Company.Header;
			org.Contacts.Load();
			Factory.Save();
			var contactValueObject = new Xsd.OrgContact { Name = "Edward", HomePhone = "+61", Fax = "asd", Mobile = "+61 (*) 2222-2222", EmailAddress = "a@test.com" };
			var securedQueryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactValueObject) };
			var helper1 = new StaffContactHelper(securedQueryString.ToString());
			var helper2 = new StaffContactHelper(securedQueryString.ToString());
			var contact1 = helper1.FindOrCreateContact().Contact;
			var contact2 = helper2.FindOrCreateContact().Contact;
			AssertEquals(contact1.PK, contact2.PK);
		}

		public void TestFindOrCreateContact_NoLoadingOtherContacts()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Company.Header;
			var existingContact1 = org.Contacts.AddNew();
			existingContact1.OC_ContactName = "Existing One";
			existingContact1.OC_Email = "existing1@cargowise.com";
			var existingContact2 = org.Contacts.AddNew();
			existingContact2.OC_ContactName = "Existing TWO";
			existingContact2.OC_Email = "existing2@cargowise.com";
			Factory.Save();
			var contactValueObject = new Xsd.OrgContact { Name = "Name1 Name1", EmailAddress = "a@test.com" };
			var securedQueryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactValueObject) };
			var helper = new StaffContactHelper(securedQueryString.ToString());
			var contact1 = helper.FindOrCreateContact().Contact;
			var factory2 = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var loadedContact1 = factory2.Load<OrgContact>(contact1.PK);
			AssertNotNull(contact1);
			var loadedContacts = ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.Select(x => x as OrgContact).WhereNotNull().ToList();
			AssertEquals("only the new contact is in the import factory", 1, loadedContacts.Count);
			AssertEquals(loadedContact1.PK, loadedContacts[0].PK);
			AssertNotEquals(loadedContact1.PK, existingContact1.PK);
			AssertNotEquals(loadedContact1.PK, existingContact2.PK);
		}

		public void TestFindOrCreateContact_NoEmailAddress_ShouldReturnNull()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var newOrg = licence.Company.Header;
			newOrg.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "Your Name" };
			SecureQueryString queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd) };
			var helper = new StaffContactHelper(queryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			newOrg.Contacts.Reload(true);
			AssertEquals("Should not create a contact since email addres is not populated", 0, newOrg.Contacts.Count);
			AssertNull(contact);
		}

		public void TestUserAccount_New()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "User 1", EmailAddress = "user1@test.com" };
			var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), [StaffContactValueObjectHelper.QueryStringKeys.StaffCode] = "US1", };
			var helper = new StaffContactHelper(queryString.ToString());
			helper.FindOrCreateContact();
			var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "US1"));
			AssertNotNull(userAccount);
			AssertEquals(database.PK, userAccount.EUA_LD);
			AssertEquals(contactXsd.EmailAddress, userAccount.EUA_Email);
			AssertEquals(queryString[StaffContactValueObjectHelper.QueryStringKeys.StaffCode], userAccount.EUA_UserID);
			AssertEquals("Newly created user account should require email verification by default", true, userAccount.EUA_IsEmailVerificationRequired);
			AssertEquals("Newly created user account should be saved", true, userAccount.IsInDatabase);
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();
			userAccount.Delete();
			userAccount.Factory.Save();
			helper = new StaffContactHelper(queryString.ToString());
			helper.FindOrCreateContact();
			userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "US1"));
			AssertEquals("Should also require email verification", true, userAccount.EUA_IsEmailVerificationRequired);
		}

		public void TestUserAccount_Existing()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
			var database = licence.Database;
			database.LD_LicenceType = DatabaseTypes.Codes.Test;
			var org = licence.Company.Header;
			org.Contacts.RemoveAndDeleteAll();
			var staff = Factory.New<EdiCustomerUserAccount>();
			staff.EUA_LD = database.PK;
			staff.EUA_UserID = "US1";
			staff.EUA_FullName = "User 1";
			staff.EUA_Email = "contact@wisetech.com";
			staff.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			var contactXsd = new Xsd.OrgContact { Name = "User 1", EmailAddress = "user1@test.com" };
			var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), [StaffContactValueObjectHelper.QueryStringKeys.StaffCode] = "US1", };
			var helper = new StaffContactHelper(queryString.ToString());
			helper.FindOrCreateContact();
			var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, "US1"));
			AssertNotNull(userAccount);
			AssertEquals("Still requires email verification", true, userAccount.EUA_IsEmailVerificationRequired);
		}

		public void TestFindOrCreateContact_GetOrgHeader()
		{
			var entOrg = Factory.NewWithValidTestData<OrgHeader>();
			entOrg.OH_Code = "DDDCOMSYD";
			var companyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			companyOrg1.OH_Code = "NEXCORMEL";
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = entOrg.PK;
			var licenceCompany1 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany1.LC_LE = enterprise.PK;
			licenceCompany1.LC_CompanyCode = "SYD";
			licenceCompany1.LC_OH = entOrg.PK;
			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LE = enterprise.PK;
			database1.LD_ServerCode = "PRD";
			database1.LD_DatabaseNumber = 2200;
			database1.LD_OH_WebAccessOrg = entOrg.PK;
			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LC = licenceCompany1.PK;
			licence1.LA_LD = database1.PK;
			var clientCompany1 = database1.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = database1.PK;
			clientCompany1.LCC_Code = "ABC";
			var clientCompany2 = database1.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = database1.PK;
			clientCompany2.LCC_Code = "NXT";
			clientCompany2.LCC_OH = companyOrg1.PK;
			Factory.Save();
			AssertCreatedContactOrg("Use enterprise org, when no client company org and ent org is attched to database", "DDDSYDPRD", "2200", "contact1@test.com", "contact 1", entOrg.PK);
			AssertCreatedContactOrg("Use enterprise org, no client company org and ent org is attched to database", "DDDABCPRD", "2200", "contact2@test.com", "contact 2", entOrg.PK);
			AssertCreatedContactOrg("Use client company org if exists", "DDDNXTPRD", "2200", "contact3@test.com", "contact 3", companyOrg1.PK);
			AssertCreatedContactOrg("Use client company org if exists, database number is not provided", "DDDNXTPRD", "", "contact4@test.com", "contact 4", companyOrg1.PK);
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = enterprise.PK;
			database2.LD_ServerCode = "FOR";
			database2.LD_DatabaseNumber = 2210;
			var companyOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			companyOrg2.OH_Code = "BICKKKSYD";
			var licenceCompany2 = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany2.LC_LE = enterprise.PK;
			licenceCompany2.LC_CompanyCode = "BIC";
			licenceCompany2.LC_OH = companyOrg2.PK;
			var licence2a = Factory.New<LicenceHeader>();
			licence2a.LA_LC = licenceCompany2.PK;
			licence2a.LA_LD = database2.PK;
			licence2a.LA_AgreedLiveDate = new ZDateTime(2018, 7, 1);
			database2.LD_OH_WebAccessOrg = companyOrg2.PK;
			Factory.Save();
			AssertCreatedContactOrg("Ent org is not attched to database, use web access org", "DDDBICFOR", "2210", "contact5@test.com", "contact 5", companyOrg2.PK);
		}

		void AssertCreatedContactOrg(string message, string licenceCode, string databaseNumberAsText, string contactEmail, string contactName, ZGuid expectedOrgPk)
		{
			var contactXsd = new Xsd.OrgContact { Name = contactName, EmailAddress = contactEmail };
			SecureQueryString queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = databaseNumberAsText, [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd) };
			var helper = new StaffContactHelper(queryString.ToString());
			var contact = helper.FindOrCreateContact().Contact;
			AssertEquals(message, expectedOrgPk, contact.OC_OH);
		}
	}
}
