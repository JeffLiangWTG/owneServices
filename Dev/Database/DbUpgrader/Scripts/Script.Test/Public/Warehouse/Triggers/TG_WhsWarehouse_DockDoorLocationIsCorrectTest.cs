using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsWarehouse_DockDoorLocationIsCorrect))]
	class TG_WhsWarehouse_DockDoorLocationIsCorrectTest : DBCreateTriggerScriptTest
	{
		#region TestLocationMustBeADockDoor

		public void TestLocationMustBeADockDoor_Inbound()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "NOTADOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsDefaultDockDoorChange(() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultInboundDockDoor, location).Post(TestConnection), true);
		}

		public void TestLocationMustBeADockDoor_Outbound()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "NOTADOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsDefaultDockDoorChange(() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultOutboundDockDoor, location).Post(TestConnection), false);
		}

		#endregion

		#region TestLocationMustHaveANormalLocationStatus

		public void TestLocationMustHaveANormalLocationStatus_Inbound_Damaged()
		{
			AssertLocationMustHaveANormalLocationStatus_Inbound("DAM");
		}

		public void TestLocationMustHaveANormalLocationStatus_Inbound_Held()
		{
			AssertLocationMustHaveANormalLocationStatus_Inbound("HEL");
		}

		public void TestLocationMustHaveANormalLocationStatus_Inbound_Void()
		{
			AssertLocationMustHaveANormalLocationStatus_Inbound("VOI");
		}

		public void TestLocationMustHaveANormalLocationStatus_Outbound_Damaged()
		{
			AssertLocationMustHaveANormalLocationStatus_Outbound("DAM");
		}

		public void TestLocationMustHaveANormalLocationStatus_Outbound_Held()
		{
			AssertLocationMustHaveANormalLocationStatus_Outbound("HEL");
		}

		public void TestLocationMustHaveANormalLocationStatus_Outbound_Void()
		{
			AssertLocationMustHaveANormalLocationStatus_Outbound("VOI");
		}

		void AssertLocationMustHaveANormalLocationStatus_Inbound(string locationStatus)
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockDoorType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var row = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK, dockDoorType) { WL_LocationStatus = locationStatus }.InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsDefaultDockDoorChange(() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultInboundDockDoor, location).Post(TestConnection), true);
		}

		void AssertLocationMustHaveANormalLocationStatus_Outbound(string locationStatus)
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockDoorType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var row = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK, dockDoorType) { WL_LocationStatus = locationStatus }.InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsDefaultDockDoorChange(() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultOutboundDockDoor, location).Post(TestConnection), false);
		}

		#endregion

		#region TestLocationUpdateSucceedsIfDockDoor

		public void TestLocationUpdateSucceedsIfDockDoorAndLocationStatusIsNormal_Inbound()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockDoorType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorType) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(
				"Update of Dock Door should succeed.",
				() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultInboundDockDoor, dockDoorLocation).Post(TestConnection)
			);
		}

		public void TestLocationUpdateSucceedsIfDockDoorAndLocationStatusIsNormal_Outbound()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var dockDoorType = warehouse.WW_DefaultOutboundDockDoor.LoadAs<WhsLocation>().WL_WLT_LocationType;
			var dockRow = new WhsRow(warehouse, "DOCK").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);
			var dockDoorLocation = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorType) { WL_LocationStatus = "NOR" }.InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(
				"Update of Dock Door should succeed.",
				() => WhsWarehouse.UpdateWhere(warehouse.PK).Set(l => l.WW_DefaultOutboundDockDoor, dockDoorLocation).Post(TestConnection)
			);
		}
		void AssertTriggerPreventsDefaultDockDoorChange(AnonymousMethod codeToRun, bool isInboundDockDoor)
		{
			var exceptionMsg = string.Empty;

			if (isInboundDockDoor)
			{
				exceptionMsg = "The Warehouses Default Inbound Dock Door Locations must be of type DDL, with a status of NOR.\r\nThe transaction ended in the trigger. The batch has been aborted.";
			}
			else
			{
				exceptionMsg = "The Warehouses Default Outbound Dock Door Locations must be of type DDL, with a status of NOR.\r\nThe transaction ended in the trigger. The batch has been aborted.";
			}

			AssertExceptionThrown(
				"Expected trigger to prevent update of Dock Door on Warehouse",
				typeof(SqlException),
				exceptionMsg,
				codeToRun
			);
		}

		#endregion

		#region TestLocationIsFromCorrectWarehouse

		public void TestLocationIsFromCorrectWarehouse_InboundDDL()
		{
			TestLocationIsFromCorrectWarehouseCore(whs => whs.WW_DefaultInboundDockDoor, true);
		}

		public void TestLocationIsFromCorrectWarehouse_OutboundDDL()
		{
			TestLocationIsFromCorrectWarehouseCore(whs => whs.WW_DefaultOutboundDockDoor, false);
		}

		void TestLocationIsFromCorrectWarehouseCore(Expression<Func<WhsWarehouse, ForeignKey<IWhsLocationSQL>>> ddlPropertyToSet, bool isInboundDockDoor)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(sql);

			var dockDoorType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var dockRow = new WhsRow(whs1, "DDL ROW").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs1.PK, "DDL AREA").AppendInsertAndReturnObject(sql);
			var dockDoorLocationInWhs1 = new WhsLocation(dockRow.PK, area.PK, area.PK, dockDoorType.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Update of Dock Door for correct warehouse should succeed.", () => WhsWarehouse.UpdateWhere(whs1.PK).Set(ddlPropertyToSet, dockDoorLocationInWhs1).Post(TestConnection));

			// update of Dock Door for incorrect warehouse should fail
			AssertTriggerPreventsDefaultDockDoorChange(() => WhsWarehouse.UpdateWhere(whs2.PK).Set(ddlPropertyToSet, dockDoorLocationInWhs1).Post(TestConnection), isInboundDockDoor);
		}
		#endregion

		#region TestInboundDockDoorUpdateIfOutboundDockDoorIsIncorrect

		public void TestInboundDockDoorUpdateIfOutboundDockDoorIsIncorrect()
		{
			TestDockDoorUpdateIfAnotherDockDoorIsIncorrectCore(true);
		}

		#endregion

		#region TestOutboundDockDoorUpdateIfInboundDockDoorIsIncorrect

		public void TestOutboundDockDoorUpdateIfInboundDockDoorIsIncorrect()
		{
			TestDockDoorUpdateIfAnotherDockDoorIsIncorrectCore(false);
		}

		#endregion

		void TestDockDoorUpdateIfAnotherDockDoorIsIncorrectCore(bool isUpdateInboundDockDoor)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(sql);

			var dockDoorType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL").First();
			var dockRowWh1 = new WhsRow(whs1, "DDL ROW").AppendInsertAndReturnObject(sql);
			var areaWh1 = new WhsArea(whs1.PK, "DDL AREA").AppendInsertAndReturnObject(sql);
			var dockDoorLocationInWhs1 = new WhsLocation(dockRowWh1.PK, areaWh1.PK, areaWh1.PK, dockDoorType.PK).AppendInsertAndReturnObject(sql);

			var dockRowWh2 = new WhsRow(whs2, "DDL ROW").AppendInsertAndReturnObject(sql);
			var areaWh2 = new WhsArea(whs2.PK, "DDL AREA").AppendInsertAndReturnObject(sql);
			var dockDoorLocationInWhs2 = new WhsLocation(dockRowWh2.PK, areaWh2.PK, areaWh2.PK, dockDoorType.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsWarehouse_DockDoorLocationIsCorrect ON WhsWarehouse");
			if (isUpdateInboundDockDoor)
			{
				WhsWarehouse.UpdateWhere(whs1.PK).Set(whs => whs.WW_DefaultOutboundDockDoor, dockDoorLocationInWhs2).Post(TestConnection);
				TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_WhsWarehouse_DockDoorLocationIsCorrect ON WhsWarehouse");
				AssertNoExceptionThrown("Update of Inbound Dock Door should succeed.", () => WhsWarehouse.UpdateWhere(whs1.PK).Set(whs => whs.WW_DefaultInboundDockDoor, dockDoorLocationInWhs1).Post(TestConnection));
			}
			else
			{
				WhsWarehouse.UpdateWhere(whs1.PK).Set(whs => whs.WW_DefaultInboundDockDoor, dockDoorLocationInWhs2).Post(TestConnection);
				TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_WhsWarehouse_DockDoorLocationIsCorrect ON WhsWarehouse");
				AssertNoExceptionThrown("Update of Outbound Dock Door should succeed.", () => WhsWarehouse.UpdateWhere(whs1.PK).Set(whs => whs.WW_DefaultOutboundDockDoor, dockDoorLocationInWhs1).Post(TestConnection));
			}
		}
	}
}

