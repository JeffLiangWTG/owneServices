using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsDockDoorAssignment_PreventChangingFirstPutawayToDockDoorUtcOnceSet))]
	class TG_WhsDockDoorAssignment_PreventChangingFirstPutawayToDockDoorUtcOnceSetTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsDockDoorAssignment_PreventChangingFirstPutawayToDockDoorUtcOnceSet : TransactionedTestCase
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
				WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
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

		#region TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcFromNull

		public void TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcFromNull()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor).AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertNoExceptionThrown(
					"Trigger should allow a First Putaway To Dock Door Utc to be changed from null to valid value.",
					() => TestConnection.ExecuteNonQuery(WhsDockDoorAssignment.UpdateWhere(ddA.PK).Set(pl => pl.WDA_FirstPutawayToDockDoorUtc, DateTime.UtcNow).AsSQL()));
		}

		#endregion

		#region TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValue

		public void TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValue()
		{
			TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValueCore(replaceWithNullDateTime: false);
		}

		public void TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValue_Null()
		{
			TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValueCore(replaceWithNullDateTime: true);
		}

		void TestTrigger_Update_UpdateValidFirstPutawayToDockDoorUtcToAnotherValueCore(bool replaceWithNullDateTime)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs1 = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area1 = new WhsArea(whs1.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs1, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor) { WDA_FirstPutawayToDockDoorUtc = DateTime.UtcNow.AddDays(-1) }.AppendInsertAndReturnObject(sql);
			SaveToDB(sql);

			AssertExceptionThrown(
					"Trigger should not allow a FirstPutawayToDockDoorUtc to be changed once set and valid.",
					typeof(SqlException),
					"Attempt to change First Putaway To Dock Door UTC value once set is invalid.",
					() =>
					{
						TestConnection.ExecuteNonQuery(WhsDockDoorAssignment.UpdateWhere(ddA.PK).Set(pl => pl.WDA_FirstPutawayToDockDoorUtc, replaceWithNullDateTime ? null : DateTime.UtcNow).AsSQL());
					},
					assertStartsWith: true);
		}

		#endregion

		#region Helper

		void SaveToDB(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(
				nameof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick),
				WhsDockDoorAssignmentSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion
	}
}
