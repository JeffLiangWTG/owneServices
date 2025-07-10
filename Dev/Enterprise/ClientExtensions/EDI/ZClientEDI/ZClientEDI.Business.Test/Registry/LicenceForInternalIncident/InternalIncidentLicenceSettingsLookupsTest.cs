using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal class InternalIncidentLicenceSettingsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLicenceList()
		{
			#region Test Data

			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "EDI";

			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			licence1.LA_LC = company1.PK;
			LicenceCompany company2 = Factory.NewWithValidTestData<LicenceCompany>();
			company2.LC_LE = enterprise1.PK;
			LicenceHeader licence2 = Factory.NewWithValidTestData<LicenceHeader>();
			licence2.LA_LC = company2.PK;

			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "CAR";

			LicenceCompany company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company3.LC_LE = enterprise2.PK;
			LicenceHeader licence3 = Factory.NewWithValidTestData<LicenceHeader>();
			licence3.LA_LC = company3.PK;

			Factory.Save();

			#endregion

			InternalIncidentLicenceSettings licence = new InternalIncidentLicenceSettings(Factory);
			AssertNotNull(licence.Lookups);
			AssertEquals(0, licence.Lookups.LicenceList.Count);

			LicenceEnterpriseKey internalEnterprise1 = licence.LicenceEnterpriseKeys.AddNew();
			internalEnterprise1.LE_PK = enterprise1.PK;
			LicenceHeaderCollection licenceCollection = licence.Lookups.LicenceList;
			licenceCollection.Load();
			AssertEquals(2, licenceCollection.Count);
			Assert(licenceCollection.Contains(licence1));
			Assert(licenceCollection.Contains(licence2));

			LicenceEnterpriseKey internalEnterprise2 = licence.LicenceEnterpriseKeys.AddNew();
			internalEnterprise2.LE_PK = enterprise2.PK;
			licenceCollection = licence.Lookups.LicenceList;
			licenceCollection.Load();
			AssertEquals(3, licenceCollection.Count);
			Assert(licenceCollection.Contains(licence1));
			Assert(licenceCollection.Contains(licence2));
			Assert(licenceCollection.Contains(licence3));
		}
	}
}