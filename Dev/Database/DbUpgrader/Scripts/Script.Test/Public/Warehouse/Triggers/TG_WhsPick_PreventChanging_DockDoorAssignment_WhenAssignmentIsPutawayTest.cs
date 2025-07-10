using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPick_PreventChanging_DockDoorAssignment_WhenAssignmentIsPutaway))]
	class TG_WhsPick_PreventChanging_DockDoorAssignment_WhenAssignmentIsPutawayTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs, "B").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor1 = new WhsLocation(row1.PK, area.PK, area.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);
			var dockdoor2 = new WhsLocation(row2.PK, area.PK, area.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA1 = new WhsDockDoorAssignment(dockdoor1).AppendInsertAndReturnObject(sql);
			new WhsPick(whs, "P2", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = ddA1 }.AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "P1", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = ddA1 }.AppendInsertAndReturnObject(sql);
			var ddA2 = new WhsDockDoorAssignment(dockdoor2).AppendInsertAndReturnObject(sql);
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick), WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertNoExceptionThrown(() => WhsDockDoorAssignment.UpdateWhere(ddA1.PK).Set(d => d.WDA_FirstPutawayToDockDoorUtc, DateTime.UtcNow.AddDays(-1)).Post(TestConnection));

			AssertExceptionThrown(
					"Trigger should not allow to change",
					typeof(SqlException),
					"Attempt to change Dock Door Assignment value after assignment is already putaway.",
					() =>
					{
						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WDA_DockDoorAssignment, ddA2).Post(TestConnection);
					},
					assertStartsWith: true);
		}
	}
}

