using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQuantityUnits()
		{
			var item = Factory.New<CusCAeMHMaster>().HouseBills.AddNew().Items.AddNew();
			AssertType(typeof(ACROSSPackageTypes), item.Lookups.QuantityUnits);
		}
	}
}
