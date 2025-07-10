using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	internal class LicenceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAMSModeList()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			AssertNotNull(header.Lookups.AMSModeList);
			Assert(header.Lookups.AMSModeList.Count > 0);
		}

		public void TestSupportModeList()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			AssertNotNull(header.Lookups.SupportModeList);
			Assert(header.Lookups.SupportModeList.Count > 0);
		}

		public void TestLAProductType()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			LicenceHeader header = Factory.New<LicenceHeader>();
			var actual = header.Lookups.ProductType;
			AssertNotNull("List should not be null", actual);
			Assert("List should have more then zero elements", actual.Count > 0);
			AssertArrayEqualsByElements(new ProductTypes(true).ToArray(), actual.ToArray());
		}

		public void TestLicenceAdvStdOth()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			AssertNotNull("List should not be null", header.Lookups.LicenceAdvStdOth);
			Assert("List should have more then zero elements", header.Lookups.LicenceAdvStdOth.Count > 0);
		}
	}
}