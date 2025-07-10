using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class TransportMeanLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVehicleCountryList()
		{
			var transportMean = Factory.New<TransportMean>();
			var list = transportMean.Lookups.VehicleCountryList;
			AssertNotNull(list);
			AssertType<RefCountryCollection>(list);
			AssertEquals("Cached", list, transportMean.Lookups.VehicleCountryList);
		}

		public void TestTruckKindList()
		{
			var transportMean = Factory.New<TransportMean>();
			var list = transportMean.Lookups.TruckKindList;
			AssertNotNull(list);
			AssertEquals(list.Count, 0);
		}
	}
}
