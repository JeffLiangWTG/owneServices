using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance))]
	class TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVarianceTest : DbCreateScriptTest
	{
		const string ProcedureName = "WhsCheckIfLocationHasOpenVariance";
		const string TriggerName = "TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance";
		const string ErrorMessage = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";

		#region TestTrigger_HasOpenVarianceCycleCount

		public void TestTrigger_HasOpenVarianceCycleCount()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000002" }.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () =>
			{
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TriggerName, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.PK, ProcedureName))
				{
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				}
			});
		}

		public void TestTrigger_HasOpenVarianceCycleCount_ExistingCycleCount()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var cycleCountLocation2 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000002" };
			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () => cycleCountLocation2.Insert(TestConnection), true);
		}
		#endregion

	}

	class Trigger_TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVarianceTest : TestCase
	{
		const string ProcedureName = "WhsCheckIfLocationHasOpenVariance";
		const string TriggerName = "TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance";

		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var today = DateTimeOffset.Now;
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				var cycleCountLocation1 = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
				var cycleCountLocation2 = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000002" }.AppendInsertAndReturnObject(sql);

				if (isDeferred)
				{
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, $"SuspendTrigger '{TriggerName}'\r\n" + sql.ToStringWithNewLineBetweenAppends()));
				}
				else
				{
					AssertCheckProcedureRanForCycleCount("If trigger is not suspended then check procedure should be run.", mainConnection, sql.ToStringWithNewLineBetweenAppends(), cycleCountLocation1);
				}
			}
		}

		#endregion

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001" };
				AssertCheckProcedureRanForCycleCount(
					"Insert a new Cycle Count should trigger check procedure.",
					mainConnection,
					cycleCountLocation.GetInsertStatement(),
					cycleCountLocation);
			}
		}

		#endregion

		#region AssertCheckProcedureRanForCycleCount

		void AssertCheckProcedureRanForCycleCount(string errorMessage, DbConnection connection, string sqlToRun, params WhsCycleCountLocation[] expectedCycleCountsInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, ProcedureName, sqlToRun, expectedCycleCountsInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedCycleCountsInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
		#endregion
	}
}

