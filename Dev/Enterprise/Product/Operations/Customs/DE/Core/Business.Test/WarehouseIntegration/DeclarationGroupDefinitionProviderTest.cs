using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.DE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DeclarationGroupDefinitionProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_NullParameter()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("inventoryWrapper is null", () => new DeclarationGroupingDefinitionProvider(null));

				AssertExceptionThrown<ArgumentNullException>("bwhAttribute is null", () => new DeclarationGroupingDefinitionProvider(null, wrapper.Factory));

				AssertExceptionThrown<ArgumentNullException>("factory is null", () => new DeclarationGroupingDefinitionProvider(BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsInventoryWrapper(wrapper), null));
			});
		}

		public void TestSupplierAddressPK()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "SUPPLIER";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "SUPP_ADDRESS";
			orgAddress.OA_OH = orgHeader.PK;
			Factory.Save();
			AssertEquals(orgAddress.PK, provider.SupplierAddressPK);
		}

		public void TestImporterAddressPK()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "IMPORTER";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = "IMP_ADDRESS";
			orgAddress.OA_OH = orgHeader.PK;
			Factory.Save();
			AssertEquals(orgAddress.PK, provider.ImporterAddressPK);
		}

		public void TestPortOfLoading()
		{
			AssertEquals("DEWIB", provider.PortOfLoading);
		}

		public void TestFirstEUArrival()
		{
			AssertEquals("DEFRA", provider.FirstEUArrival);
		}

		public void TestTransport()
		{
			AssertEquals("AIR", provider.Transport);
		}

		public void TestInventoryWrapper()
		{
			AssertSame(wrapper, provider.InventoryWrapper);
		}

		protected override void SetUp()
		{
			var helper = new WhsDataTestHelper(Factory);
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, "ENT1234", 1);
			Factory.Save();

			var whsInventory = receiveLine.Inventory;
			wrapper = new WhsInventoryWrapper(whsInventory, new ImportInventorySelectionHeader(Factory.New<JobDeclaration>()));
			provider = new DeclarationGroupingDefinitionProvider(wrapper);
		}
		WhsInventoryWrapper wrapper;
		DeclarationGroupingDefinitionProvider provider;
	}
}
