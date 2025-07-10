using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class WarehouseIdentificationProviderTest : DataProviderTestCase<WarehouseIdentificationProvider>
	{
		public void TestIWarehouseIdentification()
		{
			Assert("Should implement IIdType", Provider is IWarehouseIdentification);
		}

		public void TestType()
		{
			AssertEquals("TYPE", Provider.Type);
		}

		public void TestID()
		{
			AssertEquals("ID", Provider.ID);
		}

		protected override WarehouseIdentificationProvider GetProvider() => WarehouseIdentificationProvider.New("TYPE", "ID");
	}
}
