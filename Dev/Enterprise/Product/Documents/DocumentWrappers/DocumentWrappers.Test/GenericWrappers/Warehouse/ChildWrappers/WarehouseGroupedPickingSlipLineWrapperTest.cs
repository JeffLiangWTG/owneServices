using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseGroupedPickingSlipLineWrapper))]
	public class WarehouseGroupedPickingSlipLineWrapperTest : WarehousePickingSlipLineWrapperTest
	{
		public void TestGroupedPickingSlipLineWrapper()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_StockKeepingUnit = "UNT";

			var docWrapper = GetNewDocWrapper(new GroupedQuantityItem(9.5m, "BOX", "", 36.2m, "UNT"), new GroupedQuantityItem(Math.PI, "PCK", "", Math.E, "UNT"));
			AssertEquals("9.5" + System.Environment.NewLine + "3.142", docWrapper.UnitsGroupedPackQty);
			AssertEquals("BOX" + System.Environment.NewLine + "PCK", docWrapper.UnitsGroupedPackType);
			AssertEquals("36.2" + System.Environment.NewLine + "2.718", docWrapper.UnitsGroupedQty);
			AssertEquals("UNT" + System.Environment.NewLine + "UNT", docWrapper.UnitsGroupedStockKeepingUnit);
		}

		public void TestGroupedUOMType_Case()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_StockKeepingUnit = "UNT";

			var docWrapper = GetNewDocWrapper(new GroupedQuantityItem(9.5m, "BOX", "CAS", 36.2m, "UNT"), new GroupedQuantityItem(Math.PI, "PCK", "CAS", Math.E, "UNT"));
			AssertEquals("CAS" + System.Environment.NewLine + "CAS", docWrapper.UnitsGroupedUOMType);
		}

		public void TestGroupedUOMType_Pallet()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_StockKeepingUnit = "UNT";

			var docWrapper = GetNewDocWrapper(new GroupedQuantityItem(9.5m, "BOX", "PLT", 36.2m, "UNT"), new GroupedQuantityItem(Math.PI, "PCK", "PLT", Math.E, "UNT"));
			AssertEquals("PLT" + System.Environment.NewLine + "PLT", docWrapper.UnitsGroupedUOMType);
		}

		public void TestGroupedUOMType_SplitCase()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_StockKeepingUnit = "UNT";

			var docWrapper = GetNewDocWrapper(new GroupedQuantityItem(9.5m, "BOX", "SPC", 36.2m, "UNT"), new GroupedQuantityItem(Math.PI, "PCK", "SPC", Math.E, "UNT"));
			AssertEquals("SPC" + System.Environment.NewLine + "SPC", docWrapper.UnitsGroupedUOMType);
		}

		WarehouseGroupedPickingSlipLineWrapper GetNewDocWrapper(params GroupedQuantityItem[] items)
		{
			return new WarehouseGroupedPickingSlipLineWrapper(PickLine, Factory, new List<GroupedQuantityItem>(items));
		}

		// this method is overriden to make a reflection test happy
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WarehouseGroupedPickingSlipLineWrapper(null, Factory, new List<GroupedQuantityItem>());
		}
	}
}
