using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseBOMStagingLocationPartCollection))]
	sealed class WarehouseBOMStagingLocationPartCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WarehouseBOMStagingLocationPartCollection>
	{
		public void TestNoStagingLocationPick_WorkOrder()
			=> TestNoStagingLocationPick(data => Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1));

		public void TestNoStagingLocationPick_DynamicWorkOrder()
			=> TestNoStagingLocationPick(data => Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1));

		void TestNoStagingLocationPick(Func<TestDataForBOM, WhsComponentOrder> createWorkOrder)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			Factory.Save();

			var workOrder = createWorkOrder(data);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			var collection = new WarehouseBOMStagingLocationPartCollection(pick, Factory);
			AssertEquals("Correct Number of Part Lines in collection", 0, collection.Count);
		}

		public void TestSingleBOMProductPick()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			var wheelStagingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "R").Locations[0];
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, wheelStagingLocation);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 1, workOrder.Lines.Count);

			var collection = new WarehouseBOMStagingLocationPartCollection(pick, Factory);
			AssertEquals("Correct Number of Part Lines in collection.", 3, collection.Count);
		}

		public void TestMultiBOMProductPick()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			var wheelStagingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "W").Locations[0];
			var engineStagingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "E").Locations[0];
			data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, wheelStagingLocation);
			data.BOM.SetPartStagingLocation(data.BOM.BikeEngineProduct, engineStagingLocation);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var bikeWheelOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 1);
			var bikeEngineOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 1);

			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - IsWorkOrderPick.", true, pick.IsWorkOrderPick);
			AssertEquals("Precondition - No Shortfalls.", false, pick.HasShortfallItems);
			AssertEquals("Precondition - Correct Number of Work Order Lines.", 2, workOrder.Lines.Count);

			var collection = new WarehouseBOMStagingLocationPartCollection(pick, Factory);
			AssertEquals("Correct Number of Part Lines in collection (4xEngineParts + 3xWheelParts = 7).", 7, collection.Count);
		}

		#region overrides

		protected override WarehouseBOMStagingLocationPartCollection GetCollectionToTest()
		{
			Pick = Factory.New<WhsPick>();
			return new WarehouseBOMStagingLocationPartCollection(Pick, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WarehouseBOMStagingLocationPart();
		}

		WhsPick Pick;

		#endregion
	}
}
