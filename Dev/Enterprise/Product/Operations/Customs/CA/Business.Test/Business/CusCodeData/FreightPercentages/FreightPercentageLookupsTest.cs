using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class FreightPercentageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var freightPercentage = Factory.New<FreightPercentage>();
			AssertEquals("TransportTypeList type", typeof(TransportTypeList), freightPercentage.Lookups.TransportTypeList.GetType());
		}
	}
}
