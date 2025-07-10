using System;
using System.Linq.Expressions;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(TG_ProductionRuleScheduleTaskView_Update))]
	class TG_ProductionRuleScheduleTaskView_UpdateTest : DbCreateScriptTest
	{
		public void TestUpdate_IsActive() => TestUpdate(p => p.PRT_IsActive, false, s => s.S5_IsActive);
		public void TestUpdate_WeeklyRecurrencePeriod() => TestUpdate(p => p.PRT_WeeklyRecurrencePeriod, 7, s => s.S5_TaskPeriodCount);
		public void TestUpdate_RunOnSunday() => TestUpdate(p => p.PRT_RunOnSunday, false, s => s.S5_DayList, "NYYYYYY");
		public void TestUpdate_RunOnSunday_True() => TestUpdate(p => p.PRT_RunOnSunday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnMonday() => TestUpdate(p => p.PRT_RunOnMonday, false, s => s.S5_DayList, "YNYYYYY");
		public void TestUpdate_RunOnMonday_True() => TestUpdate(p => p.PRT_RunOnMonday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnTuesday() => TestUpdate(p => p.PRT_RunOnTuesday, false, s => s.S5_DayList, "YYNYYYY");
		public void TestUpdate_RunOnTuesday_True() => TestUpdate(p => p.PRT_RunOnTuesday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnWednesday() => TestUpdate(p => p.PRT_RunOnWednesday, false, s => s.S5_DayList, "YYYNYYY");
		public void TestUpdate_RunOnWednesday_True() => TestUpdate(p => p.PRT_RunOnWednesday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnThursday() => TestUpdate(p => p.PRT_RunOnThursday, false, s => s.S5_DayList, "YYYYNYY");
		public void TestUpdate_RunOnThursday_True() => TestUpdate(p => p.PRT_RunOnThursday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnFriday() => TestUpdate(p => p.PRT_RunOnFriday, false, s => s.S5_DayList, "YYYYYNY");
		public void TestUpdate_RunOnFriday_True() => TestUpdate(p => p.PRT_RunOnFriday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_RunOnSaturday() => TestUpdate(p => p.PRT_RunOnSaturday, false, s => s.S5_DayList, "YYYYYYN");
		public void TestUpdate_RunOnSaturday_True() => TestUpdate(p => p.PRT_RunOnSaturday, true, s => s.S5_DayList, "YYYYYYY");
		public void TestUpdate_LocalRunTime() => TestUpdate(p => p.PRT_LocalRunTime, DateTime.Today.AddDays(-7), s => s.S5_DailyStartTime);
		public void TestUpdate_LocalStartDate() => TestUpdate(p => p.PRT_LocalStartDate, DateTime.Today.AddDays(-7), s => s.S5_StartDate);
		public void TestUpdate_NextScheduledRunTimeUtc() => TestUpdate(p => p.PRT_NextScheduledRunTimeUtc, DateTime.Today.AddDays(-7), s => s.S5_NextScheduledPrintRunTimeUtc);
		public void TestUpdate_SystemLastEditTimeUtc() => TestUpdate(p => p.PRT_SystemLastEditTimeUtc, DateTime.Today.AddDays(-7), s => s.S5_SystemLastEditTimeUtc);
		public void TestUpdate_SystemLastEditUser() => TestUpdate(p => p.PRT_SystemLastEditUser, "DBE", s => s.S5_SystemLastEditUser);

		void TestUpdate<T>(
			Expression<Func<ProductionRuleScheduleTaskView, T>> valueSetter,
			T value,
			Func<StmScheduleTask, T> valueGetter)
		{
			TestUpdate(valueSetter, value, valueGetter, value);
		}

		void TestUpdate<TSet, TGet>(
			Expression<Func<ProductionRuleScheduleTaskView, TSet>> valueSetter,
			TSet valueToSet,
			Func<StmScheduleTask, TGet> valueGetter,
			TGet expectedValue,
			Expression<Func<ProductionRuleScheduleTaskView, TSet>> optionalInitialSetter = null)
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
				PRT_IsActive = true,
				PRT_WeeklyRecurrencePeriod = 42,
				PRT_LocalStartDate = startDate,
				PRT_LocalRunTime = runTime,
				PRT_NextScheduledRunTimeUtc = nextRunDateTime,
				PRT_RunOnSunday = true,
				PRT_RunOnMonday = true,
				PRT_RunOnTuesday = true,
				PRT_RunOnWednesday = true,
				PRT_RunOnThursday = true,
				PRT_RunOnFriday = true,
				PRT_SystemCreateUser = "MMC",
				PRT_SystemCreateTimeUtc = createdTime,
				PRT_SystemLastEditUser = "BRS",
				PRT_SystemLastEditTimeUtc = lastEditTime,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// Precondition
			StmScheduleTask
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ParentID), t => t.S5_ParentID, rule1.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ScheduleDescription), t => t.S5_ScheduleDescription, "TEST1")
				.ExpectEquals(nameof(StmScheduleTask.S5_IsActive), t => t.S5_IsActive, true)
				.ExpectEquals(nameof(StmScheduleTask.S5_TaskPeriod), t => t.S5_TaskPeriod, "W")
				.ExpectEquals(nameof(StmScheduleTask.S5_TaskPeriodCount), t => t.S5_TaskPeriodCount, 42)
				.ExpectEquals(nameof(StmScheduleTask.S5_DayList), t => t.S5_DayList, "YYYYYYY")
				.ExpectEquals(nameof(StmScheduleTask.S5_StartDate), t => t.S5_StartDate, startDate)
				.ExpectEquals(nameof(StmScheduleTask.S5_DailyStartTime), t => t.S5_DailyStartTime, runTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_NextScheduledPrintRunTimeUtc), t => t.S5_NextScheduledPrintRunTimeUtc, nextRunDateTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemCreateTimeUtc), t => t.S5_SystemCreateTimeUtc, createdTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemCreateUser), t => t.S5_SystemCreateUser, "MMC")
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemLastEditTimeUtc), t => t.S5_SystemLastEditTimeUtc, lastEditTime)
				.ExpectEquals(nameof(StmScheduleTask.S5_SystemLastEditUser), t => t.S5_SystemLastEditUser, "BRS")
				.VerifyAll();

			ProductionRuleScheduleTaskView
				.UpdateWhere(rule1Schedule.PK)
				.Set(valueSetter, valueToSet)
				.Post(TestConnection);

			StmScheduleTask
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals("Should have updated value.", valueGetter, expectedValue)
				.VerifyAll();
		}

		public void TestUpdate_RuleName()
		{
			var sql = new SqlQueryBuilder();

			var ruleSet = new ProductionRuleSet("PWP", "TEST") { PRS_IsLive = false }.AppendInsertAndReturnObject(sql);
			var rule1 = new ProductionRule(ruleSet, "TEST1", "{}").AppendInsertAndReturnObject(sql);

			var rule1Schedule = new ProductionRuleScheduleTaskView(rule1.PK) { PRT_IsActive = true }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			// Precondition
			StmScheduleTask
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ParentID), t => t.S5_ParentID, rule1.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ScheduleDescription), t => t.S5_ScheduleDescription, "TEST1")
				.ExpectEquals(nameof(StmScheduleTask.S5_IsActive), t => t.S5_IsActive, true)
				.VerifyAll();

			ProductionRule
				.UpdateWhere(rule1.PK)
				.Set(p => p.PRL_Name, "TEST2")
				.Post(TestConnection);

			ProductionRuleScheduleTaskView
				.UpdateWhere(rule1Schedule.PK)
				.Set(p => p.PRT_IsActive, false)
				.Post(TestConnection);

			StmScheduleTask
				.AssertFromDB(TestConnection, rule1Schedule.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ParentID), t => t.S5_ParentID, rule1.PK)
				.ExpectEquals(nameof(StmScheduleTask.S5_ScheduleDescription), t => t.S5_ScheduleDescription, "TEST2")
				.ExpectEquals(nameof(StmScheduleTask.S5_IsActive), t => t.S5_IsActive, false)
				.VerifyAll();
		}
	}
}

