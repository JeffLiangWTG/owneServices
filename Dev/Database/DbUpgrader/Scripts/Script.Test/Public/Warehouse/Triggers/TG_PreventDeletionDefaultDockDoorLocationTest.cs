using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventDeletionDefaultDockDoorLocation))]
	class TG_PreventDeletionDefaultDockDoorLocationTest : DBCreateTriggerScriptTest
	{
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

		public void TestPreventDeletionDefaultDockDoorLocation_CannotDeleteWhenDefaultDockDoor_Inbound()
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();

			AssertTriggerPreventsDeletionOfDockDoor(() => WhsLocation.DeleteInDB(TestConnection, warehouse.WW_DefaultInboundDockDoor.FK));
		}

		public void TestPreventDeletionDefaultDockDoorLocation_CannotDeleteWhenDefaultDockDoor_Outbound()
		{
			var warehouse = CreateWarehouseWithUniqueDefaultDockDoors();

			AssertTriggerPreventsDeletionOfDockDoor(() => WhsLocation.DeleteInDB(TestConnection, warehouse.WW_DefaultOutboundDockDoor.FK));
		}

		public void TestPreventDeletionDefaultDockDoorLocation_CanDeleteWhenNotDefaultDockDoor()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "NOTADOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown("Update of Dock Door should succeed.", () => WhsLocation.DeleteInDB(TestConnection, location.PK));
		}

		void AssertTriggerPreventsDeletionOfDockDoor(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent deletion of Dock Door on Warehouse",
				typeof(SqlException),
				"Attempt to delete a location which is a default dock door location for a warehouse.\r\nThe transaction ended in the trigger. The batch has been aborted.",
				codeToRun
			);
		}
	}
}

