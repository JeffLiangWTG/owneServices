using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCustomsDataRegistry))]
	sealed class NctsCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<NctsCustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, NctsCustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestEnableWarehouseOrderImportNCTS()
		{
			TestRegistryItem(NctsCustomsDataRegistry.Instance.EnableInventoryManagement,
				"EnableInventoryManagementNCTS",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Enable Inventory Management",
				"Enable Inventory Management",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestEnableMultipleMovements()
		{
			TestRegistryItem(NctsCustomsDataRegistry.Instance.EnableMultipleMovements,
				"EnableMultipleMovements",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Enable Multiple Movements",
				"Enable Multiple Movements",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}
	}
}
