using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class WarehouseProviderTest : DataProviderTestCase<WarehouseProvider>
	{
		#region Public test methods

		public void TestType()
		{
			var provider = Provider;
			IWarehouse warehouse = provider;
			AssertEquals("provider.Type", "ABC", provider.Type);
			AssertEquals("warehouse.Type", "ABC", warehouse.Type);
		}

		public void TestIdentifier()
		{
			var provider = Provider;
			IWarehouse warehouse = provider;
			AssertEquals("provider.Identifier", "ABC123", provider.Identifier);
			AssertEquals("warehouse.Identifier", "ABC123", warehouse.Identifier);
		}

		#endregion

		#region Overridings & inherits
		readonly string warehouseType = "ABC";
		readonly string warehouseIdentifier = "ABC123";
		protected override WarehouseProvider GetProvider() => WarehouseProvider.New(warehouseType, warehouseIdentifier);
		#endregion
	}
}
