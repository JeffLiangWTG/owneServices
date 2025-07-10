using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	public class WiseTechAcademyAutoLoginHelperTest : TestCaseWithFactory
	{
		public void TestGetOrCreateCustomerUserAccount_EnterpriseHasWTADatabaseWithNoLicenceHeader()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var licEnt = licence.Database.LicEnterprise;
			var org = licEnt.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";

			AssertEquals("Precondition", 1, licEnt.Companies.Count);
			var company = licEnt.Companies[0];

			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var existingWTA = licEnt.Databases.AddNew();
			existingWTA.LD_Product = ProductTypes.Codes.WiseTechAcademy;
			existingWTA.LD_AllowAutoLogin = true;
			existingWTA.LD_LicenceType = DatabaseTypes.Codes.Production;
			existingWTA.LD_Status = DatabaseStatusList.Codes.REG;
			existingWTA.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			existingWTA.LD_ServerCode = "WTA";
			existingWTA.LD_IsActive = true;
			existingWTA.LD_OH_WebAccessOrg = org.PK;
			Factory.Save();

			AssertEquals("Precondition: No LicenceHeaders so LicenceDatabase for WTA should not be attached", 0, company.LicDatabases.Cast<LicenceDatabase>().Count(ld => ld.LD_Product == ProductTypes.Codes.WiseTechAcademy));

			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);
			AssertEquals(userAccount.EUA_OC_WebAccessContact, contact.PK);
			AssertEquals("Should associate to existing database", existingWTA.PK, userAccount.EUA_LD);
			AssertEquals(licEnt.PK, userAccount.Database.LD_LE);
			AssertEquals(existingWTA.PK, userAccount.Database.PK);

			var wtaDBs = company.LicDatabases.Cast<LicenceDatabase>().Where(ld => ld.LD_Product == ProductTypes.Codes.WiseTechAcademy).ToArray();
			AssertEquals("LicenceHeader should be created to attach WTA LicenceDatabase", 1, wtaDBs.Length);
			var wtaDB = wtaDBs[0];
			AssertEquals("LicenceHeader should be created to attach WTA LicenceDatabase", existingWTA.PK, wtaDB.PK);
			var licHeaders = wtaDB.GetProductLicHeaders();
			AssertEquals("LicenceHeader should be created to attach WTA LicenceDatabase", 1, licHeaders.Length);
			var header = licHeaders[0];
			AssertEquals("LicenceHeader should be created to attach WTA LicenceDatabase", company.PK, header.LA_LC);
			AssertEquals("LicenceHeader should be created to attach WTA LicenceDatabase", wtaDB.PK, header.LA_LD);

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		public void TestGetOrCreateCustomerUserAccount()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Database.LicEnterprise.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";
			Factory.Save();
			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);
			AssertEquals(userAccount.EUA_OC_WebAccessContact, contact.PK);
			AssertEquals(userAccount.Database.LD_LE, licence.Database.LD_LE);
			AssertEquals("WTA", userAccount.Database.LD_Product);
			AssertEquals(org.PK, userAccount.Database.LD_OH_WebAccessOrg);
			AssertEquals(userAccount.Database.LD_DatabaseNumber.ToString(), userAccount.Database.LD_TenantID);
			AssertEquals(userAccount.PK.ToString(), userAccount.EUA_UserID);
			AssertEquals(org.PK, billingOrg.PK);
		}

		public void TestGetOrCreateCustomerUserAccount_OrgNotLicenced()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";
			Factory.Save();
			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);
			AssertEquals(userAccount.EUA_OC_WebAccessContact, contact.PK);
			AssertNotNull(userAccount.Database.LicEnterprise);
			AssertEquals("WTA", userAccount.Database.LD_Product);
			AssertEquals(org.PK, userAccount.Database.LD_OH_WebAccessOrg);
			AssertEquals(userAccount.Database.LD_DatabaseNumber.ToString(), userAccount.Database.LD_TenantID);
			AssertEquals(userAccount.PK.ToString(), userAccount.EUA_UserID);
			AssertEquals(org.PK, billingOrg.PK);
		}

		public void TestGetWorkingAddressCountryCode()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var org = licence.Database.LicEnterprise.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";
			Factory.Save();
			AssertEquals("AU", WiseTechAcademyAutoLoginHelper.GetWorkingAddressCountryCode(contact));
		}

		public void TestGetOrCreateCustomerUserAccountShouldReactivateLicenceDatabase()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "WTA", true);
			licence.Database.LD_IsActive = false;
			licence.Database.LD_Product = "WTA";
			var licEnt = licence.Database.LicEnterprise;
			var org = licEnt.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";

			AssertEquals("Precondition", 1, licEnt.Companies.Count);
			Factory.Save();

			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);

			var licenceDatabasePk = licence.Database.PK;
			var reloadLicenceDatabase = Factory.Load<LicenceDatabase>(licenceDatabasePk);
			AssertEquals("LicenceDatabase should be reactivated", true, reloadLicenceDatabase.LD_IsActive);
		}

		public void TestGetOrCreateCustomerUserAccountShouldNotOccurSaveError()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "WTA", true);
			licence.Database.LD_IsActive = false;
			var licEnt = licence.Database.LicEnterprise;
			var org = licEnt.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";

			AssertEquals("Precondition", 1, licEnt.Companies.Count);
			Factory.Save();

			AssertNoExceptionThrown(() => WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact));
		}

		public void TestGetOrCreateCustomerUserAccountShouldReactivateLicenceHeader()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var licEnt = licence.Database.LicEnterprise;
			var org = licEnt.Organisation;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "contact1@cw1.com";

			AssertEquals("Precondition", 1, licEnt.Companies.Count);
			var company = licEnt.Companies[0];

			var existingWTA = licEnt.Databases.AddNew();
			existingWTA.LD_Product = ProductTypes.Codes.WiseTechAcademy;
			existingWTA.LD_AllowAutoLogin = true;
			existingWTA.LD_LicenceType = DatabaseTypes.Codes.Production;
			existingWTA.LD_Status = DatabaseStatusList.Codes.REG;
			existingWTA.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			existingWTA.LD_ServerCode = "WTA";
			existingWTA.LD_IsActive = true;
			existingWTA.LD_OH_WebAccessOrg = org.PK;

			var licenceHeader = existingWTA.ActiveLicHeadersForAllCompanies.AddNew();
			licenceHeader.LA_LD = existingWTA.PK;
			licenceHeader.LA_LC = company.PK;
			licenceHeader.LA_IsActive = false;
			Factory.Save();

			var (userAccount, billingOrg) = WiseTechAcademyAutoLoginHelper.GetOrCreateCustomerUserAccount(org, contact);

			var licenceHeaderPk = licenceHeader.PK;
			var reloadLicenceHeader = Factory.Load<LicenceHeader>(licenceHeaderPk);
			AssertEquals("LicenceHeader should be reactivated", true, reloadLicenceHeader.LA_IsActive);
		}
	}
}
