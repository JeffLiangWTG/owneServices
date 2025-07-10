using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaTransferHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondedWarehouseCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transferHeader = header.ArrivalHeaders.AddNew().TransferHeaders.AddNew();
			AssertEquals(typeof(BondedWarehouseCollection), transferHeader.Lookups.BondedWarehouseCollection.GetType());
		}

		public void TestShippingProviders()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var transferHeader = header.ArrivalHeaders.AddNew().TransferHeaders.AddNew();
			AssertEquals(typeof(AirShippingProviderCollection), transferHeader.Lookups.ShippingProviders.GetType());
		}
	}
}
