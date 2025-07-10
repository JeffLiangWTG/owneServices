using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal class LicenceEnterpriseKeyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInternalEnterpriseList()
		{
			#region Test Data

			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "ZQE";
			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "BET";

			Factory.Save();

			#endregion

			LicenceEnterpriseKey internalEnterprise = new LicenceEnterpriseKey(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertNotNull(internalEnterprise.Lookups.LicenceEnterpriseKeyList);
			internalEnterprise.Lookups.LicenceEnterpriseKeyList.Load();
			Assert(internalEnterprise.Lookups.LicenceEnterpriseKeyList.Contains(enterprise1));
			Assert(internalEnterprise.Lookups.LicenceEnterpriseKeyList.Contains(enterprise2));
		}
	}
}