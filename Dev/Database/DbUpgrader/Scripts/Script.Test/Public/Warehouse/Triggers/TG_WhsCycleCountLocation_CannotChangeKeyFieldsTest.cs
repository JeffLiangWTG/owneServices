using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsCycleCountLocation_CannotChangeKeyFields))]
	class TG_WhsCycleCountLocation_CannotChangeKeyFieldsTest : DBCreateTriggerScriptTest
	{
		const string ChangingLocationTriggerErrorMessage = "Attempted to change Location on Cycle Count task.";
		const string ChangingTimeTriggerErrorMessage = "Attempted to change StartTime/EndTime when Cycle Count has completed.";
		const string ChangingAssignedToTriggerErrorMessage = "Attempted to change AssignedTo when Cycle Count has started/completed.";

		public void TestTrigger_UpdateLocation()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK1 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK2 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK1, "PWS") { WCL_JobID = "WC00000001" }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingLocationTriggerErrorMessage, () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_WL_Location, locationPK2).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateStartTime_NotStarted()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_StartTime, DateTimeOffset.Now).Post(TestConnection));
		}

		public void TestTrigger_UpdateStartTime_HasStarted()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_StartTime, today.AddDays(1)).Post(TestConnection));
		}

		public void TestTrigger_UpdateStartTime_HasCompleted()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(10) }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingTimeTriggerErrorMessage,
				() => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_StartTime, today.AddMinutes(1)).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateEndTime_NotCompleted()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_EndTime, today.AddMinutes(1)).Post(TestConnection));
		}

		public void TestTrigger_UpdateEndTime_HasCompleted()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(1) }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingTimeTriggerErrorMessage,
				() => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_EndTime, today.AddHours(1)).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateAssignedTo_NotAssigned()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001" }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_GS_NKAssignedTo, "AA").Post(TestConnection));
		}

		public void TestTrigger_UpdateAssignedTo_HasAssignedButNotStarted()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "BB" }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_GS_NKAssignedTo, "AA").Post(TestConnection));
		}

		public void TestTrigger_UpdateAssignedTo_HasStarted()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_GS_NKAssignedTo, "AA").Post(TestConnection));

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingAssignedToTriggerErrorMessage,
				() => WhsCycleCountLocation.UpdateWhere(cycleCountPK).Set(c => c.WCL_GS_NKAssignedTo, "BB").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateAssignedTo_AndClearStartTime()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var cycleCountPK = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_GS_NKAssignedTo = "AA", WCL_StartTime = today }.AppendInsertAndReturnObject(sql).PK;

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsCycleCountLocation
				.UpdateWhere(cycleCountPK)
				.Set(c => c.WCL_GS_NKAssignedTo, "BB")
				.Set(c => c.WCL_StartTime, null).Post(TestConnection));
		}
	}
}

