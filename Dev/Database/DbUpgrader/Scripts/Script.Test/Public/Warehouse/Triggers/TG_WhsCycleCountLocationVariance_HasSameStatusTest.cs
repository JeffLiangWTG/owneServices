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
	[TestedType(typeof(TG_WhsCycleCountLocationVariance_HasSameStatus))]
	class TG_WhsCycleCountLocationVariance_HasSameStatusTest : DbCreateScriptTest
	{
		const string ErrorMessage = "All variances do not have same status.";

		#region TestInsertWhsCycleCountVarianceWithAllSameStatus

		public void TestInsertWhsCycleCountVarianceWithStatusAllOpen()
		{
			TestInsertWhsCycleCountVarianceWithStatus(status: "OPN");
		}

		public void TestInsertWhsCycleCountVarianceWithStatusAllApproved()
		{
			TestInsertWhsCycleCountVarianceWithStatus(status: "APP", shouldCreateAdjustment: true);
		}

		public void TestInsertWhsCycleCountVarianceWithStatusAllRejected()
		{
			TestInsertWhsCycleCountVarianceWithStatus(status: "REJ");
		}

		void TestInsertWhsCycleCountVarianceWithStatus(string status, bool shouldCreateAdjustment = false)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001" }.AppendInsertAndReturnObject(sql);
			var variance1 = new WhsCycleCountLocationVariance(cycleCountLocation, status) { WCC_PalletID = "PLT1" };
			var variance2 = new WhsCycleCountLocationVariance(cycleCountLocation, status) { WCC_PalletID = "PLT2" };
			if (shouldCreateAdjustment)
			{
				var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				variance1.WCC_WD_RelatedAdjustment = adjustment.PK;
				variance2.WCC_WD_RelatedAdjustment = adjustment.PK;
			}

			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestUpdateSomeCycleCountVariances

		public void TestUpdateWhsCycleCountVarianceFromOpenToApprove()
		{
			TestUpdateWhsCycleCountVariance(fromStatus: "OPN", toStatus: "APP");
		}

		public void TestUpdateWhsCycleCountVarianceFromOpenToRejected()
		{
			TestUpdateWhsCycleCountVariance(fromStatus: "OPN", toStatus: "REJ");
		}

		void TestUpdateWhsCycleCountVariance(string fromStatus, string toStatus)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance1 = new WhsCycleCountLocationVariance(cycleCountLocation, fromStatus) { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
			var variance2 = new WhsCycleCountLocationVariance(cycleCountLocation, fromStatus) { WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

			var adjustmentPK = Guid.Empty;
			if (toStatus == "APP")
			{
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				adjustmentPK = adjustment.PK;
			}
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw an exception.", typeof(SqlException), ErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(variance1.PK)
				.Set(v => v.WCC_Status, toStatus)
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustmentPK == Guid.Empty ? null : adjustmentPK).Post(TestConnection), true);
		}

		#endregion

		#region TestUpdateAllCycleCountVariances

		public void TestUpdateAllCycleCountVariancesFromOpenToApprove()
		{
			TestUpdateAllCycleCountVariances(fromStatus: "OPN", toStatus: "APP");
		}

		public void TestUpdateAllCycleCountVariancesFromOpenToRejected()
		{
			TestUpdateAllCycleCountVariances(fromStatus: "OPN", toStatus: "REJ");
		}

		void TestUpdateAllCycleCountVariances(string fromStatus, string toStatus)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance = new WhsCycleCountLocationVariance(cycleCountLocation, fromStatus) { WCC_Status = "OPN", WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);

			var adjustmentPK = Guid.Empty;
			if (toStatus == "APP")
			{
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				adjustmentPK = adjustment.PK;
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown(
				() => WhsCycleCountLocationVariance
				.UpdateWhere(variance.PK)
				.Set(v => v.WCC_Status, toStatus)
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustmentPK == Guid.Empty ? null : adjustmentPK).Post(TestConnection));
		}

		#endregion
	}

	class TG_WhsCycleCountLocationVariance_HasSameStatusNonTransactionTestCase : TestCase
	{
		const string ProcedureName = "WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus";
		const string TriggerName = "TG_WhsCycleCountLocationVariance_HasSameStatus";

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert_Variance()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var approvedVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "APP") { WCC_PalletID = "PLT2", WCC_WD_RelatedAdjustment = adjustment.PK };
				AssertCheckProcedureRanForCycleCountVariances("Insert of new Approved variance should trigger check procedure.", mainConnection, approvedVariance.GetInsertStatement(), approvedVariance);
			}
		}

		#endregion

		#region TestTrigger_Update_VarianceStatus

		[UseSnapshotProtection]
		public void TestTrigger_Update_VarianceStatus()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
				var approvedVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN") { WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TriggerName, WhsCycleCountLocationVarianceSchema.Constants.TableName, WhsCycleCountLocationVarianceSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				using (TestWhsDataSetupHelper.SuspendTrigger("TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeleted", WhsCycleCountLocationVarianceSchema.Constants.TableName, mainConnection))
				{
					// update pallet id - should not effect trigger
					var updateSQL = WhsCycleCountLocationVariance.UpdateWhere(approvedVariance.PK).Set(v => v.WCC_PalletID, "bla").AsSQL();
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, updateSQL, commitChanges: true));
				}

				var updateSql = WhsCycleCountLocationVariance.UpdateWhere(approvedVariance.PK)
					.Set(a => a.WCC_Status, "APP")
					.Set(a => a.WCC_WD_RelatedAdjustment, adjustment.PK)
					.AsSQL();

				// update status - should call check procedure
				AssertCheckProcedureRanForCycleCountVariances(
					"Update of WCC_Status should trigger check procedure.",
					mainConnection,
					updateSql,
					approvedVariance);
			}
		}

		#endregion

		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
				var approvedVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN") { WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = WhsCycleCountLocationVariance.UpdateWhere(approvedVariance.PK)
					.Set(a => a.WCC_Status, "APP")
					.Set(a => a.WCC_WD_RelatedAdjustment, adjustment.PK)
					.AsSQL();

				if (isDeferred)
				{
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, $"SuspendTrigger '{TriggerName}'\r\n" + updateSql));
				}
				else
				{
					AssertCheckProcedureRanForCycleCountVariances("If trigger is not suspended then check procedure should be run.", mainConnection, updateSql, approvedVariance);
				}
			}
		}

		void AssertCheckProcedureRanForCycleCountVariances(string errorMessage, DbConnection connection, string sqlToRun, params WhsCycleCountLocationVariance[] expectedVariancesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, ProcedureName, sqlToRun, expectedVariancesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedVariancesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
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

