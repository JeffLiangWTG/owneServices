using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Client.EDI.Web.Login.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class EDILoginContactTest : TestCaseWithFactory
	{
		public void TestLinkedSystems()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DAD", "ABC", "SYD");
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, licence.Database.EnterpriseCode));

			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";

			var cw1Db = licence.Database;
			cw1Db.LD_Product = ProductTypes.Codes.CargoWiseOne;
			cw1Db.LD_ServerCode = "LVE";

			var cwnDb = BillingTestHelper.CreateAnotherDatabase(licence, "CWN").Database;
			cwnDb.LD_Product = ProductTypes.Codes.CargoWiseNext;

			var nonCW1Db1 = licenceEnterprise.Databases.AddNew();
			nonCW1Db1.LD_ServerCode = "TD1";
			nonCW1Db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			nonCW1Db1.LD_Product = ProductTypes.Codes.Sapphire;

			var nonCW1Db2 = licenceEnterprise.Databases.AddNew();
			nonCW1Db2.LD_ServerCode = "AKA";
			nonCW1Db2.LD_LicenceType = DatabaseTypes.Codes.Training;
			nonCW1Db2.LD_Product = ProductTypes.Codes.BorderWise;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = cw1Db.PK;
			userAccount1.EUA_UserID = "TST";
			userAccount1.EUA_FullName = "Test User";
			userAccount1.EUA_OC_WebAccessContact = contact.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = nonCW1Db1.PK;
			userAccount2.EUA_UserID = "TS2";
			userAccount2.EUA_FullName = "Test User 2";
			userAccount2.EUA_OC_WebAccessContact = contact.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = nonCW1Db2.PK;
			userAccount3.EUA_UserID = "TS2";
			userAccount3.EUA_FullName = "Test User 2";
			userAccount3.EUA_OC_WebAccessContact = contact.PK;
			userAccount3.EUA_IsActive = false;

			var userAccount4 = Factory.New<EdiCustomerUserAccount>();
			userAccount4.EUA_LD = cwnDb.PK;
			userAccount4.EUA_UserID = "TS4";
			userAccount4.EUA_FullName = "Test User 4";
			userAccount4.EUA_OC_WebAccessContact = contact.PK;

			Factory.Save();

			var loginContact = LoginContact.New(contact);
			AssertEquals("CW1_DADLVE, SPH_TST, CWN_DADCWN", loginContact.LinkedSystems);
		}

		public void TestLinkedSystemsUnlinkedContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DAD", "ABC", "SYD");

			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "tester@test.org";
			Factory.Save();

			var loginContact = LoginContact.New(contact);
			AssertEquals(string.Empty, loginContact.LinkedSystems);
		}
	}
}
