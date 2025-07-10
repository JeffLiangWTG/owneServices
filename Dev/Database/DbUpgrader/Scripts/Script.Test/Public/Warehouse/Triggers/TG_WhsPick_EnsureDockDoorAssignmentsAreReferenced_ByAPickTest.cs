using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick))]
	class TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPickTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row.PK, area.PK, area.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor) { WDA_FirstPutawayToDockDoorUtc = DateTime.UtcNow.AddDays(-1) }.AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "P1", "NEW") { WP_WDA_DockDoorAssignment = null }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(
					nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick),
					WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			{
				AssertNoExceptionThrown(
				"Expected no trigger to prevent insert of Pick",
				() =>
				{
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				});
			}
		}

		public void TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_Update_AnotherValue()
		{
			TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_UpdateCore(replaceWithNull: false);
		}

		public void TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_Update_Null()
		{
			TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_UpdateCore(replaceWithNull: true);
		}

		void TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_UpdateCore(bool replaceWithNull)
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
			var ddA2 = new WhsDockDoorAssignment(dockdoor2).AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P1", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = ddA1 }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertExceptionThrown(
					"Procedure should not allow to change",
					typeof(SqlException),
					ErrorMessage,
					() =>
					{
						WhsPick
						.UpdateWhere(pick.PK)
						.Set(p => p.WP_WDA_DockDoorAssignment, replaceWithNull ? null : ddA2)
						.Set(p => p.WP_WL_DockDoor, replaceWithNull ? dockdoor1 : null)
						.Post(TestConnection);
					},
					assertStartsWith: true);
		}

		public void TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick_Delete()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row.PK, area.PK, area.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor).AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "P1", "NEW") { WP_WL_DockDoor = null, WP_WDA_DockDoorAssignment = ddA }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertExceptionThrown(
				"Trigger should not allow to change",
				typeof(SqlException),
				ErrorMessage,
				() =>
				{
					WhsPick.DeleteInDB(TestConnection, pick.PK);
				},
				assertStartsWith: true);
		}

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(WhsCheckForUnreferencedWhsDockDoorAssignment), WhsPickSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick), WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		const string ErrorMessage = "Attempt to leave a Dock Door Assignment with no referencing Picks.";

		#endregion
	}
}

