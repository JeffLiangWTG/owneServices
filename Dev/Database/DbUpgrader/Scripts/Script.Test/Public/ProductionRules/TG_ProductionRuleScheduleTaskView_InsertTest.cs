using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(TG_ProductionRuleScheduleTaskView_Insert))]
	class TG_ProductionRuleScheduleTaskView_InsertTest : DbCreateScriptTest
	{
		public void TestInsert() => TestInsert(true, true, true, true, true, true, true, true);
		public void TestInsert_Inactive() => TestInsert(false, true, true, true, true, true, true, true);

		public void TestInsert_RunOnSunday() => TestInsert(true, true, false, false, false, false, false, false);
		public void TestInsert_RunOnMonday() => TestInsert(true, false, true, false, false, false, false, false);
		public void TestInsert_RunOnTuesday() => TestInsert(true, false, false, true, false, false, false, false);
		public void TestInsert_RunOnWednesday() => TestInsert(true, false, false, false, true, false, false, false);
		public void TestInsert_RunOnThursday() => TestInsert(true, false, false, false, false, true, false, false);
		public void TestInsert_RunOnFriday() => TestInsert(true, false, false, false, false, false, true, false);
		public void TestInsert_RunOnSaturday() => TestInsert(true, false, false, false, false, false, false, true);

		void TestInsert(
			bool isActive,
			bool runOnSunday,
			bool runOnMonday,
			bool runOnTuesday,
			bool runOnWednesday,
			bool runOnThursday,
			bool runOnFriday,
			bool runOnSaturday)
		{
			var sql = new SqlQueryBuilder();

			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);

			var now = DateTime.Today;
			var startDate = now.AddDays(-20);
			var runTime = now.AddDays(-10);
			var nextRunDateTime = now.AddDays(-2);

			var createdTime = now.AddDays(-50);
			var lastEditTime = now.AddDays(-30);

			var rule1Schedule = new ProductionRuleScheduleTaskView(rule1.PK)
			{
				PRT_IsActive = isActive,
				PRT_WeeklyRecurrencePeriod = 42,
				PRT_LocalStartDate = startDate,
				PRT_LocalRunTime = runTime,
				PRT_NextScheduledRunTimeUtc = nextRunDateTime,
				PRT_RunOnSunday = runOnSunday,
				PRT_RunOnMonday = runOnMonday,
				PRT_RunOnTuesday = runOnTuesday,
				PRT_RunOnWednesday = runOnWednesday,
				PRT_RunOnThursday = runOnThursday,
				PRT_RunOnFriday = runOnFriday,
				PRT_RunOnSaturday = runOnSaturday,
				PRT_SystemCreateUser = "MMC",
				PRT_SystemCreateTimeUtc = createdTime,
				PRT_SystemLastEditUser = "BRS",
				PRT_SystemLastEditTimeUtc = lastEditTime,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var dayListString = BoolToCharFlag(runOnSunday) + BoolToCharFlag(runOnMonday) + BoolToCharFlag(runOnTuesday) + BoolToCharFlag(runOnWednesday) + BoolToCharFlag(runOnThursday) + BoolToCharFlag(runOnFriday) + BoolToCharFlag(runOnSaturday);

			StmScheduleTask
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ParentID), t => t.S5_ParentID, rule1.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ScheduleDescription), t => t.S5_ScheduleDescription, "TEST1")
				.ExpectEquals(nameof(StmScheduleTask.S5_IsActive), t => t.S5_IsActive, isActive)
				.ExpectEquals(nameof(StmScheduleTask.S5_TaskPeriod), t => t.S5_TaskPeriod, "W")
				.ExpectEquals(nameof(StmScheduleTask.S5_TaskPeriodCount), t => t.S5_TaskPeriodCount, 42)
				.ExpectEquals(nameof(StmScheduleTask.S5_DayList), t => t.S5_DayList, dayListString)
				.ExpectEquals(nameof(StmScheduleTask.S5_StartDate), t => t.S5_StartDate, startDate)
				.ExpectEquals(nameof(StmScheduleTask.S5_DailyStartTime), t => t.S5_DailyStartTime, runTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_NextScheduledPrintRunTimeUtc), t => t.S5_NextScheduledPrintRunTimeUtc, nextRunDateTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemCreateTimeUtc), t => t.S5_SystemCreateTimeUtc, createdTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemCreateUser), t => t.S5_SystemCreateUser, "MMC")
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemLastEditTimeUtc), t => t.S5_SystemLastEditTimeUtc, lastEditTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemLastEditUser), t => t.S5_SystemLastEditUser, "BRS")
				.VerifyAll();

			string BoolToCharFlag(bool value) => value ? "Y" : "N";
		}

		public void TestInsert_NoRule()
		{
			var sql = new SqlQueryBuilder();
			var pk = Guid.NewGuid();
			var rule1Schedule = new ProductionRuleScheduleTaskView(pk).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Should *not* have created the task.", false, StmScheduleTask.ExistsInDB(TestConnection, rule1Schedule.PK));
		}
	}
}

