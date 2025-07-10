using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseBOMStagingLocationPart))]
	sealed class WarehouseBOMStagingLocationPartTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestTotalUnits()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, data.Whs1.Rows[0].Locations[0]);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			var wheelRimPartLine = FindChildWorkOrderLine(workOrder.Lines[0], data.BOM.WheelRim);

			var bomStagingLocationPart = new WarehouseBOMStagingLocationPart();
			bomStagingLocationPart.AddWorkOrderLine(wheelRimPartLine);

			AssertEquals("Correct TotalUnits", 1m, bomStagingLocationPart.TotalUnits);
		}

		public void TestTotalUnits_ShortfallPick()
		{
			// Only create enough product for 5 of the bom product to be produced...
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.CreateProductInInventory("Rims for 10 wheels.", data.BOM.WheelRim, 10m);
			data.CreateProductInInventory("Polish for 10 wheels.", data.BOM.Polish, 10m);
			data.CreateProductInInventory("Tyres for 5 wheels (shortfall).", data.BOM.WheelTyre, 5m);
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, data.Whs1.Rows[0].Locations[0]);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 10);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition - Has Shortfalls.", true, pick.HasShortfallItems);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);
			AssertEquals("Precondition - Correct Number of Child Order Lines.", 3, workOrder.Lines[0].BOM.ChildComponentLines.Count);

			foreach (var childOrderLine in workOrder.Lines[0].BOM.ChildComponentLines)
			{
				var bomStagingLocationPart = new WarehouseBOMStagingLocationPart();
				bomStagingLocationPart.AddWorkOrderLine(childOrderLine);
				AssertEquals("Correct TotalUnits - " + childOrderLine.SupplierPart.OP_Desc + ".", 5m, bomStagingLocationPart.TotalUnits);
			}
		}

		public void TestPackType()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, data.Whs1.Rows[0].Locations[0]);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			var wheelRimPartLine = FindChildWorkOrderLine(workOrder.Lines[0], data.BOM.WheelRim);
			var wheelTyrePartLine = FindChildWorkOrderLine(workOrder.Lines[0], data.BOM.WheelTyre);
			var wheelPolishPartLine = FindChildWorkOrderLine(workOrder.Lines[0], data.BOM.Polish);

			var rimStagingLocationPart = new WarehouseBOMStagingLocationPart();
			rimStagingLocationPart.AddWorkOrderLine(wheelRimPartLine);
			AssertEquals("Matching PackType (WheelRim).", wheelRimPartLine.SupplierPart.StockKeepingUnitProxy, rimStagingLocationPart.PackType);

			var tyreStagingLocationPart = new WarehouseBOMStagingLocationPart();
			tyreStagingLocationPart.AddWorkOrderLine(wheelTyrePartLine);
			AssertEquals("Matching PackType (WheelTyre).", wheelTyrePartLine.SupplierPart.StockKeepingUnitProxy, tyreStagingLocationPart.PackType);

			var polishStagingLocationPart = new WarehouseBOMStagingLocationPart();
			polishStagingLocationPart.AddWorkOrderLine(wheelPolishPartLine);
			AssertEquals("Matching PackType (WheelPolish).", wheelPolishPartLine.SupplierPart.StockKeepingUnitProxy, polishStagingLocationPart.PackType);
		}

		public void TestStagingLocation()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, data.Whs1.Rows[0].Locations[0]);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			foreach (var childOrderLine in workOrder.Lines[0].BOM.ChildComponentLines)
			{
				var bomStagingLocationPart = new WarehouseBOMStagingLocationPart();
				bomStagingLocationPart.AddWorkOrderLine(childOrderLine);
				AssertEquals("Correct StagingLocation - " + childOrderLine.SupplierPart.OP_Desc + ".", data.Whs1.Rows[0].Locations[0], bomStagingLocationPart.BOMStagingLocation);
			}
		}

		#endregion

		#region AddWorkOrderLine

		public void TestAddWorkOrderLine_NoStagingLocation()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1);
			var pick = Helper.CreatePickNew(workOrder);

			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			var childLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);

			var bomStagingLocationPart = new WarehouseBOMStagingLocationPart();
			bomStagingLocationPart.AddWorkOrderLine(childLine);
			AssertEquals("StagingLocationPart should be empty.", null, bomStagingLocationPart.BOMStagingLocation);
			AssertEquals("StagingLocationPart should be empty.", 0m, bomStagingLocationPart.TotalUnits);
		}

		#endregion

		#region Implementation

		WhsWorkOrderLine FindChildWorkOrderLine(WhsWorkOrderLine workOrderLine, OrgSupplierPart locatePart)
		{
			return workOrderLine.BOM.ChildComponentLines.Single(line => line.SupplierPart.PK == locatePart.PK);
		}

		#endregion
	}
}
