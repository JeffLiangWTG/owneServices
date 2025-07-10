using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemCycleCountLocationVariance_HasSameStatus))]
	class TG_WhsItemCycleCountLocationVariance_HasSameStatusTest : DbCreateScriptTest
	{
		const string ErrorMessage = "All variances do not have same status.";

		#region TestInsertWhsItemCycleCountVarianceWithAllSameStatus

		public void TestInsertWhsItemCycleCountVarianceWithStatusAllOpen()
		{
			TestInsertWhsItemCycleCountVarianceWithStatus(status: "OPN");
		}

		public void TestInsertWhsItemCycleCountVarianceWithStatusAllApproved()
		{
			TestInsertWhsItemCycleCountVarianceWithStatus(status: "APP");
		}

		public void TestInsertWhsItemCycleCountVarianceWithStatusAllRejected()
		{
			TestInsertWhsItemCycleCountVarianceWithStatus(status: "REJ");
		}

		void TestInsertWhsItemCycleCountVarianceWithStatus(string status)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
			var variance1 = new WhsItemCycleCountLocationVariance(cycleCountLocation, status);
			var variance2 = new WhsItemCycleCountLocationVariance(cycleCountLocation, status);

			if (status == "APP")
			{
				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
				variance1.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
				variance2.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
				variance1.WIV_VarianceQty = 1;
				variance2.WIV_VarianceQty = 1;
			}

			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestUpdateSomeCycleCountVariances

		public void TestUpdateWhsItemCycleCountVarianceFromOpenToApprove()
		{
			TestUpdateWhsItemCycleCountVariance(fromStatus: "OPN", toStatus: "APP");
		}

		public void TestUpdateWhsItemCycleCountVarianceFromOpenToRejected()
		{
			TestUpdateWhsItemCycleCountVariance(fromStatus: "OPN", toStatus: "REJ");
		}

		void TestUpdateWhsItemCycleCountVariance(string fromStatus, string toStatus)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
			var variance1 = new WhsItemCycleCountLocationVariance(cycleCountLocation, fromStatus).AppendInsertAndReturnObject(sql);
			var variance2 = new WhsItemCycleCountLocationVariance(cycleCountLocation, fromStatus).AppendInsertAndReturnObject(sql);
			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () =>
			{
				if (toStatus == "APP")
				{
					WhsItemCycleCountLocationVariance.UpdateWhere(variance1.PK)
						.Set(v => v.WIV_Status, toStatus)
						.Set(v => v.WIV_WRC_ReceiveConsignment, receiveConsignment.PK)
						.Set(v => v.WIV_VarianceQty, (short)1)
						.Post(TestConnection);
				}
				else
				{
					WhsItemCycleCountLocationVariance.UpdateWhere(variance1.PK)
						.Set(v => v.WIV_Status, toStatus)
						.Post(TestConnection);
				}
			}, true);
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
			var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(rowPK, areaPK, areaPK).AppendInsertAndReturnObject(sql).PK;
			var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, fromStatus).AppendInsertAndReturnObject(sql);
			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(() =>
			{
				if (toStatus == "APP")
				{
					WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK)
						.Set(v => v.WIV_Status, toStatus)
						.Set(v => v.WIV_WRC_ReceiveConsignment, receiveConsignment.PK)
						.Set(v => v.WIV_VarianceQty, (short)1)
						.Post(TestConnection);
				}
				else
				{
					WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK)
						.Set(v => v.WIV_Status, toStatus)
						.Post(TestConnection);
				}
			});
		}

		#endregion
	}

	class TG_WhsItemCycleCountLocationVariance_HasSameStatusNonTransactionTestCase : TestCase
	{
		const string ProcedureName = "WhsItemCheckAllVariancesForSingleCycleCountLocationHasSameStatus";
		const string TriggerName = "TG_WhsItemCycleCountLocationVariance_HasSameStatus";
		static readonly string WhsItemCycleCountLocationVarianceTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemCycleCountLocationVarianceSchema.Instance);

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert_Variance()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;
				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);

				var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
				var openVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var approvedVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "APP");
				approvedVariance.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
				approvedVariance.WIV_VarianceQty = 1;
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
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

				var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
				var openVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);
				var approvedVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);
				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);

				// defer triggers to save data into DB
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TriggerName, WhsItemCycleCountLocationVarianceTableNameForSQL, WhsItemCycleCountLocationVarianceSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				using (TestWhsDataSetupHelper.SuspendTrigger("dbo.TG_WhsItemCycleCountLocationVariance_HasSameStatus", WhsItemCycleCountLocationVarianceTableNameForSQL, mainConnection))
				{
					// update package id - should not affect trigger
					var updateSQL = WhsItemCycleCountLocationVariance.UpdateWhere(approvedVariance.PK).Set(v => v.WIV_PackageNotInWhsID, "bla").AsSQL();
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, updateSQL, commitChanges: true));
				}

				var updateSql = WhsItemCycleCountLocationVariance.UpdateWhere(approvedVariance.PK)
					.Set(a => a.WIV_Status, "APP")
					.Set(a => a.WIV_WRC_ReceiveConsignment, receiveConsignment.PK)
					.Set(a => a.WIV_VarianceQty, (short)1)
					.AsSQL();

				// update status - should call check procedure
				AssertCheckProcedureRanForCycleCountVariances(
					"Update of WIV_Status should trigger check procedure.",
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
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;
				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);

				var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
				var openVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);
				var approvedVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = WhsItemCycleCountLocationVariance.UpdateWhere(approvedVariance.PK)
					.Set(a => a.WIV_Status, "APP")
					.Set(a => a.WIV_WRC_ReceiveConsignment, receiveConsignment.PK)
					.Set(a => a.WIV_VarianceQty, (short)1)
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

		void AssertCheckProcedureRanForCycleCountVariances(string errorMessage, DbConnection connection, string sqlToRun, params WhsItemCycleCountLocationVariance[] expectedVariancesInTheProcedure)
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
