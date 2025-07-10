using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(UpgradeRequestCollection))]
	public class UpgradeRequestCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UpgradeRequestCollection>
	{
		public void TestConstructor()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1", false);
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2", false);
			var lic3 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB3", false);
			var lic4 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB4", false);
			lic3.Database.LD_IsActive = false;
			lic4.LA_IsActive = false;
			var org1 = lic1.Company.Header;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			var collection = new UpgradeRequestCollection(Factory, org1, org2);

			AssertEquals("Two elements in collection", 2, collection.Count);
			AssertEquals("Element 1 Org1", org1.PK, collection[0].OrganisationPk);
			AssertEquals("Element 1 LicDB1", lic1.Database, collection[0].LicDatabase);
			AssertEquals("Element 2 Org1", org1.PK, collection[1].OrganisationPk);
			AssertEquals("Element 2 LicDB2", lic2.Database, collection[1].LicDatabase);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			var licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licDB = Factory.NewWithValidTestData<LicenceDatabase>();

			org.LicCompany.LC_LE = licEnt.PK;
			licDB.LD_LE = org.LicEnterprise.PK;
			LicenceHeader header = org.LicCompany.LicHeadersForAllDatabases.AddNew();
			header.LA_LC = org.LicCompany.PK;
			header.LA_LD = licDB.PK;

			return new UpgradeRequest(Factory, org, org.LicCompany.LicHeadersForAllDatabases[0].Database);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(UpgradeRequestCollection);
		}

		protected override UpgradeRequestCollection GetCollectionToTest()
		{
			return new UpgradeRequestCollection(Factory, Factory.NewWithValidTestData<EDIOrgHeader>());
		}
	}
}
