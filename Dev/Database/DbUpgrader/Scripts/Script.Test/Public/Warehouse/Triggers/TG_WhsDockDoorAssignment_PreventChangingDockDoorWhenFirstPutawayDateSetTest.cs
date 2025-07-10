using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDockDoorAssignment_PreventChangingDockDoorWhenFirstPutawayDateSet))]
	class TG_WhsDockDoorAssignment_PreventChangingDockDoorWhenFirstPutawayDateSetTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsDockDoorAssignment_PreventChangingDockDoorWhenFirstPutawayDateSet : TransactionedTestCase
	{
		#region TestTrigger_Insert

		public void TestTrigger_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			new WhsDockDoorAssignment(dockdoor) { WDA_FirstPutawayToDockDoorUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			using (TestWhsDataSetupHelper.SuspendTrigger(
				nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick),
				WhsDockDoorAssignmentSchema.Constants.TableName,
				TestConnection))
			{
				AssertNoExceptionThrown(
					"Trigger should allow insert.",
					() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#region TestTrigger_Delete

		public void TestTrigger_Delete()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor) { WDA_FirstPutawayToDockDoorUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertNoExceptionThrown(
					"Trigger should allow delete.",
					() => WhsPick.DeleteInDB(TestConnection, ddA.PK));
		}

		#endregion

		#region TestTrigger_Update

		public void TestTrigger_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var area2 = new WhsArea(whs1.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs1, "B").AppendInsertAndReturnObject(sql);
			var dockdoor2 = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor1) { WDA_FirstPutawayToDockDoorUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a Dock Door Assignment to be changed once FirstPutawayToDockDoorUtc set and valid.",
					typeof(SqlException),
					"Attempt to change Dock Door Location after stock has already been moved to Dock Door Location.",
					() =>
					{
						TestConnection.ExecuteNonQuery(WhsDockDoorAssignment.UpdateWhere(ddA.PK).Set(pl => pl.WDA_WL_AssignedDockDoor, dockdoor2).AsSQL());
					},
					assertStartsWith: true);
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(
				nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick),
				WhsDockDoorAssignmentSchema.Constants.TableName,
				TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion
	}
}

