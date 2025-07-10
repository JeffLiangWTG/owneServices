using System;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules.ProductionRuleScheduleTaskView))]
	class ProductionRuleScheduleTaskViewTest : DbCreateScriptTest
	{
		public void TestView() => TestView(true, true, true, true, true, true, true, true);
		public void TestView_Inactive() => TestView(false, true, true, true, true, true, true, true);

		public void TestView_RunOnSunday() => TestView(true, true, false, false, false, false, false, false);
		public void TestView_RunOnMonday() => TestView(true, false, true, false, false, false, false, false);
		public void TestView_RunOnTuesday() => TestView(true, false, false, true, false, false, false, false);
		public void TestView_RunOnWednesday() => TestView(true, false, false, false, true, false, false, false);
		public void TestView_RunOnThursday() => TestView(true, false, false, false, false, true, false, false);
		public void TestView_RunOnFriday() => TestView(true, false, false, false, false, false, true, false);
		public void TestView_RunOnSaturday() => TestView(true, false, false, false, false, false, false, true);

		void TestView(
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

			var dayListString = BoolToCharFlag(runOnSunday) + BoolToCharFlag(runOnMonday) + BoolToCharFlag(runOnTuesday) + BoolToCharFlag(runOnWednesday) + BoolToCharFlag(runOnThursday) + BoolToCharFlag(runOnFriday) + BoolToCharFlag(runOnSaturday);
			var rule1Schedule = new StmScheduleTask(rule1.PK, "PRL")
			{
				S5_IsActive = isActive,
				S5_DayList = dayListString,
				S5_StartDate = startDate,
				S5_DailyStartTime = runTime,
				S5_NextScheduledPrintRunTimeUtc = nextRunDateTime,
				S5_TaskPeriodCount = 42,
				S5_SystemCreateUser = "MMC",
				S5_SystemCreateTimeUtc = createdTime,
				S5_SystemLastEditUser = "BRS",
				S5_SystemLastEditTimeUtc = lastEditTime,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			ProductionRuleScheduleTaskView
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_PRL_Rule), t => t.PRT_PRL_Rule, rule1.PK)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_IsActive), t => t.PRT_IsActive, isActive)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_LocalStartDate), t => t.PRT_LocalStartDate, startDate)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_LocalRunTime), t => t.PRT_LocalRunTime, runTime)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_NextScheduledRunTimeUtc), t => t.PRT_NextScheduledRunTimeUtc, nextRunDateTime)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_WeeklyRecurrencePeriod), t => t.PRT_WeeklyRecurrencePeriod, 42)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnSunday), t => t.PRT_RunOnSunday, runOnSunday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnMonday), t => t.PRT_RunOnMonday, runOnMonday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnTuesday), t => t.PRT_RunOnTuesday, runOnTuesday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnWednesday), t => t.PRT_RunOnWednesday, runOnWednesday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnThursday), t => t.PRT_RunOnThursday, runOnThursday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnFriday), t => t.PRT_RunOnFriday, runOnFriday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_RunOnSaturday), t => t.PRT_RunOnSaturday, runOnSaturday)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_SystemCreateTimeUtc), t => t.PRT_SystemCreateTimeUtc, createdTime)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_SystemCreateUser), t => t.PRT_SystemCreateUser, "MMC")
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_SystemLastEditTimeUtc), t => t.PRT_SystemLastEditTimeUtc, lastEditTime)
				.ExpectEquals(nameof(ProductionRuleScheduleTaskView.PRT_SystemLastEditUser), t => t.PRT_SystemLastEditUser, "BRS")
				.VerifyAll();

			string BoolToCharFlag(bool value) => value ? "Y" : "N";
		}

		public void TestView_DifferentTableCode()
		{
			var sql = new SqlQueryBuilder();

			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);
			var rule1Schedule = new StmScheduleTask(rule1.PK, "AC").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals(false, ProductionRuleScheduleTaskView.ExistsInDB(TestConnection, rule1Schedule.PK));
		}

		public void TestView_DifferentTaskPeriod()
		{
			var sql = new SqlQueryBuilder();

			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);
			var rule1Schedule = new StmScheduleTask(rule1.PK, "PRL") { S5_TaskPeriod = "D" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals(false, ProductionRuleScheduleTaskView.ExistsInDB(TestConnection, rule1Schedule.PK));
		}

		public void TestView_NoRule()
		{
			var sql = new SqlQueryBuilder();
			var rule1Schedule = new StmScheduleTask(Guid.NewGuid(), "PRL").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals(false, ProductionRuleScheduleTaskView.ExistsInDB(TestConnection, rule1Schedule.PK));
		}
	}
}

