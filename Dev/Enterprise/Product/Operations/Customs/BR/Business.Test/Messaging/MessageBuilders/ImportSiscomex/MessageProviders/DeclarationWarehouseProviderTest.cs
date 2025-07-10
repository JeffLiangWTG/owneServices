using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationWarehouseProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationWarehouseProvider()
		{
			var warehouse = Factory.New<WarehouseArea>();
			warehouse.CY_Code = "W1";

			var warehouseProvider = new DeclarationWarehouseProvider(warehouse);
			AssertEquals("Warehouse should be equal to W1", "W1", warehouseProvider.WarehouseCode);
		}
	}
}
