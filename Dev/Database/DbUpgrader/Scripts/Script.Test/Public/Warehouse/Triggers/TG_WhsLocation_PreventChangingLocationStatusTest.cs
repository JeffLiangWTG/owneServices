using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocation_PreventChangingLocationStatus))]
	class TG_WhsLocation_PreventChangingLocationStatusTest : DBCreateTriggerScriptTest
	{
		#region TestAllowStatusChange_IfUsedAsDefaultDockDoor_Normal

		public void TestAllowStatusChange_IfUsedAsDefaultDockDoor_Inbound_Normal()
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(warehouse.WW_DefaultInboundDockDoor.FK).Set(l => l.WL_LocationStatus, "NOR").Post(TestConnection)
			);
		}

		public void TestAllowStatusChange_IfUsedAsDefaultDockDoor_Outbound_Normal()
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(warehouse.WW_DefaultOutboundDockDoor.FK).Set(l => l.WL_LocationStatus, "NOR").Post(TestConnection)
			);
		}

		#endregion

		#region TestPreventStatusChange_IfUsedAsDefaultDockDoor

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound_Damaged()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound("DAM");
		}

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound_Damaged()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound("DAM");
		}

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound_Held()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound("HEL");
		}

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound_Held()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound("HEL");
		}

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound_Void()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound("VOI");
		}

		public void TestPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound_Void()
		{
			AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound("VOI");
		}

		void AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Inbound(string locationStatus)
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();
			AssertTriggerPreventsStatusChange(
				"Attempt to change Location Status for a Dock Door Location that is used by the Warehouse.",
				() => WhsLocation.UpdateWhere(warehouse.WW_DefaultInboundDockDoor.FK).Set(l => l.WL_LocationStatus, locationStatus).Post(TestConnection)
			);
		}

		void AssertPreventStatusChange_IfUsedAsDefaultDockDoor_Outbound(string locationStatus)
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();

			AssertTriggerPreventsStatusChange(
				"Attempt to change Location Status for a Dock Door Location that is used by the Warehouse.",
				() => WhsLocation.UpdateWhere(warehouse.WW_DefaultOutboundDockDoor.FK).Set(l => l.WL_LocationStatus, locationStatus).Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedForStockOnHand_NonVoid

		public void TestAllowStatusChange_IfUsedForStockOnHand_NonVoid()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1).Single();
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = new DateTime(2018, 1, 1) }.InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_LocationStatus, "HEL").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfStockOnHandIsZero

		public void TestAllowStatusChange_IfStockOnHandIsZero()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1).Single();
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = new DateTime(2018, 1, 1) }.InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 0m, location.PK) { WE_StockOnHand = 0m }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestPreventStatusChange_IfUsedForStockOnHand

		public void TestPreventStatusChange_IfUsedForStockOnHand()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1).Single();
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = new DateTime(2018, 1, 1) }.InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m }.Insert(TestConnection);

			AssertTriggerPreventsStatusChange(
				"Attempted to Void out a Location that has Existing or Pending Stock.",
				() => WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedForPendingStock_NonVoid

		public void TestAllowStatusChange_IfUsedForPendingStock_NonVoid()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1).Single();
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m, WE_CurrentInventoryStatus = "PUT", WE_OriginalInventoryStatus = "PUT" }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_LocationStatus, "HEL").Post(TestConnection)
			);
		}

		#endregion

		#region TestPreventStatusChange_IfUsedForPendingStock

		public void TestPreventStatusChange_IfUsedForPendingStock_Receive()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var product = new OrgSupplierPart("P1").InsertAndReturnObject(TestConnection);
			var location = warehouse.CreateLocations(TestConnection, 1).Single();
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_StockOnHand = 10m, WE_CurrentInventoryStatus = "PUT", WE_OriginalInventoryStatus = "PUT" }.Insert(TestConnection);

			AssertTriggerPreventsStatusChange(
				"Attempted to Void out a Location that has Existing or Pending Stock.",
				() => WhsLocation.UpdateWhere(location.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		public void TestPreventStatusChange_IfUsedForPendingStock_Transfer()
		{
			var sql = new SqlQueryBuilder();
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var locations = warehouse.CreateLocations(TestConnection, 2);
			var sourceLocation = locations[0];
			var destinationLocation = locations[1];

			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = new DateTime(2018, 1, 1) }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, sourceLocation.PK).AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, destinationLocation.PK) { WE_StockOnHand = 10m, WE_WL_TransferFrom = sourceLocation.PK, WE_DocketLineStatus = "HFT", WE_CurrentInventoryStatus = "INT", WE_OriginalInventoryStatus = "INT" }.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, transferLine, 10m) { WZ_GS_NKAssignedTo = "E", WZ_PickedDateTime = new DateTime(2018, 1, 2) }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertTriggerPreventsStatusChange(
				"Attempted to Void out a Location that has Existing or Pending Stock.",
				() => WhsLocation.UpdateWhere(destinationLocation.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedAsDockDoorOnUnfinalisedPick_Normal

		public void TestAllowStatusChange_IfUsedAsDockDoorOnUnfinalisedPick_Normal()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorLocationType).InsertAndReturnObject(TestConnection);
			new WhsPick(warehouse, "P1", "PIC") { WP_WL_DockDoor = dockDoorLocation }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(dockDoorLocation.PK).Set(l => l.WL_LocationStatus, "NOR").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedAsDockDoorOnCancelledPick

		public void TestAllowStatusChange_IfUsedAsDockDoorOnCancelledPick()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorLocationType).InsertAndReturnObject(TestConnection);
			new WhsPick(warehouse, "P1", "CAN") { WP_WL_DockDoor = dockDoorLocation }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(dockDoorLocation.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedAsDockDoorOnFinalisedPick

		public void TestAllowStatusChange_IfUsedAsDockDoorOnFinalisedPick()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorLocationType).InsertAndReturnObject(TestConnection);
			new WhsPick(warehouse, "P1", "FIN") { WP_WL_DockDoor = dockDoorLocation, WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "A" }.Insert(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(dockDoorLocation.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestPreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick

		public void TestPreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick_Damaged()
		{
			PreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick("DAM");
		}

		public void TestPreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick_Held()
		{
			PreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick("HEL");
		}

		public void TestPreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick_Void()
		{
			PreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick("VOI");
		}

		void PreventStatusChange_IfUsedAsDockDoorOnUnfinalisedPick(string locationStatus)
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocationType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorLocationType).InsertAndReturnObject(TestConnection);
			new WhsPick(warehouse, "P1", "PIC") { WP_WL_DockDoor = dockDoorLocation }.Insert(TestConnection);
			AssertTriggerPreventsStatusChange(
				"Attempt to change Location Status for a Dock Door Location that is used by unfinalised Picks.",
				() => WhsLocation.UpdateWhere(dockDoorLocation.PK).Set(l => l.WL_LocationStatus, locationStatus).Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedAsPackingStationOnCancelledPick

		public void TestAllowStatusChange_IfUsedAsPackingStationOnCancelledPick()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CL1").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "A").InsertAndReturnObject(TestConnection);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.InsertAndReturnObject(TestConnection);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(whs1, "PICK1", "NEW").InsertAndReturnObject(TestConnection);
			var order = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.InsertAndReturnObject(TestConnection);
			WhsPick.UpdateWhere(pick.PK).Set(p => p.WP_PickStatus, "CAN");

			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(packingStation.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestAllowStatusChange_IfUsedAsPackingStationOnFinalisedPick

		public void TestAllowStatusChange_IfUsedAsPackingStationOnFinalisedPick()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CL1").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "A").InsertAndReturnObject(TestConnection);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.InsertAndReturnObject(TestConnection);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(whs1, "PICK1", "NEW").InsertAndReturnObject(TestConnection);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick.PK }.InsertAndReturnObject(TestConnection);
			WhsPick.UpdateWhere(pick.PK)
				.Set(p => p.WP_PickStatus, "FIN")
				.Set(p => p.WP_GS_NKFinalizedBy, "A")
				.Set(pl => pl.WP_WL_PackingStation, packingStation)
				.Set(p => p.WP_FinalizedDateUtc, DateTime.UtcNow).Post(TestConnection);
			AssertNoExceptionThrown(
				"Update of Location Status should succeed.",
				() => WhsLocation.UpdateWhere(packingStation.PK).Set(l => l.WL_LocationStatus, "VOI").Post(TestConnection)
			);
		}

		#endregion

		#region TestPreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick

		public void TestPreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick_Damaged()
		{
			PreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick("DAM");
		}

		public void TestPreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick_Held()
		{
			PreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick("HEL");
		}

		public void TestPreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick_Void()
		{
			PreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick("VOI");
		}

		void PreventStatusChange_IfUsedAsPackingStationOnUnfinalisedPick(string locationStatus)
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var client = new OrgHeader("CL1").InsertAndReturnObject(TestConnection);

			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "A").InsertAndReturnObject(TestConnection);

			var packingStationType = new WhsLocationType("PST") { WLT_LocationClass = "PST", WLT_DefaultCycleCountGranularity = string.Empty }.InsertAndReturnObject(TestConnection);
			var packingStation = new WhsLocation(row1.PK, area1.PK, area1.PK, packingStationType.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(whs1, "PICK1", "NEW").InsertAndReturnObject(TestConnection);
			new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick.PK }.InsertAndReturnObject(TestConnection);
			WhsPick.UpdateWhere(pick.PK).Set(pl => pl.WP_WL_PackingStation, packingStation).Post(TestConnection);

			AssertTriggerPreventsStatusChange(
				"Attempt to change Location Status for a Packing Station Location that is used by unfinalised Picks.",
				() => WhsLocation.UpdateWhere(packingStation.PK).Set(l => l.WL_LocationStatus, locationStatus).Post(TestConnection)
			);
		}

		#endregion

		#region TestAssertions

		void AssertTriggerPreventsStatusChange(string expectedTriggerError, AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent update of Location Status on Location",
				typeof(SqlException),
				expectedTriggerError + "\r\nThe transaction ended in the trigger. The batch has been aborted.",
				codeToRun
			);
		}
		#endregion

		#region Implementation

		WhsWarehouse CreateWarehouseWithUniqueDefaultDockDoors()
		{
			var warehouse = new WhsWarehouse("WHS");

			var inboundRow = new WhsRow(warehouse, "INBOUND DOCKDOOR");
			var inboundArea = new WhsArea(warehouse.PK, "[DEFAULT INBOUND DOCK DOOR]") { WA_AreaType = "DDA" };
			var inboundDockDoor = new WhsLocation(inboundRow.PK, inboundArea.PK, inboundArea.PK)
			{
				WL_LocationStatus = "NOR",
				WL_PickMethod = "ANY",
				WL_PickPathSequence = 1,
				WL_MaxQuantityUnit = "UNT"
			};

			var outboundRow = new WhsRow(warehouse, "OUTBOUND DOCKDOOR");
			var outboundArea = new WhsArea(warehouse.PK, "[DEFAULT OUTBOUND DOCK DOOR]") { WA_AreaType = "DDA" };
			var outboundDockDoor = new WhsLocation(outboundRow.PK, outboundArea.PK, outboundArea.PK)
			{
				WL_LocationStatus = "NOR",
				WL_PickMethod = "ANY",
				WL_PickPathSequence = 1,
				WL_MaxQuantityUnit = "UNT"
			};

			warehouse.WW_DefaultInboundDockDoor = inboundDockDoor;
			warehouse.WW_DefaultOutboundDockDoor = outboundDockDoor;

			warehouse.Insert(TestConnection);
			inboundRow.Insert(TestConnection);
			inboundArea.Insert(TestConnection);
			inboundDockDoor.Insert(TestConnection);
			outboundRow.Insert(TestConnection);
			outboundArea.Insert(TestConnection);
			outboundDockDoor.Insert(TestConnection);

			return warehouse;
		}
		#endregion
	}
}

