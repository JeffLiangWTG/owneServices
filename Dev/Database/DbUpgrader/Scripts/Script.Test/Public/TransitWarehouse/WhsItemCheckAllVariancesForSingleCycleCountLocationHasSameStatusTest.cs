using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(WhsItemCheckAllVariancesForSingleCycleCountLocationHasSameStatus))]
	class WhsItemCheckAllVariancesForSingleCycleCountLocationHasSameStatusTest : DbCreateScriptTest
	{
		const string EnsureAllVariancesForSingleCycleCountLocationHasSameStatus = "dbo.TG_WhsItemCycleCountLocationVariance_HasSameStatus";
		static readonly string WhsItemCycleCountLocationVarianceTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemCycleCountLocationVarianceSchema.Instance);

		#region TestAllVariancesForSingleCycleCountLocationHasSameStatus

		public void TestAllVariancesForSingleCycleCountLocationHasDifferentStatus()
		{
			TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(typeof(SqlException), "All variances do not have same status.");

			var sql = new SqlQueryBuilder();
			using (Db.Connection.BeginTransactionWithManager())
			{
				using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus, WhsItemCycleCountLocationVarianceTableNameForSQL, TestConnection))
				{
					var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
					var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
					var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
					var location1PK = new WhsLocation(row, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
					var location2PK = new WhsLocation(row, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;
					var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
					var cycleCountLocation1 = new WhsItemCycleCountLocation(location1PK, jobID: "CC001").AppendInsertAndReturnObject(sql);
					var cycleCountLocation2 = new WhsItemCycleCountLocation(location2PK, jobID: "CC002").AppendInsertAndReturnObject(sql);
					var openVarianceForLocation1 = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "OPN").AppendInsertAndReturnObject(sql);
					var openVarianceForLocation2 = new WhsItemCycleCountLocationVariance(cycleCountLocation2, "OPN").AppendInsertAndReturnObject(sql);
					var approvedVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "APP") { WIV_WRC_ReceiveConsignment = receiveConsignment.PK }.AppendInsertAndReturnObject(sql);
					var rejectedVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "REJ").AppendInsertAndReturnObject(sql);

					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

					TestUpdateWhsItemCycleCountVariance(new[] { openVarianceForLocation1.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { approvedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { openVarianceForLocation1.PK, approvedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { approvedVariance.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { openVarianceForLocation1.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { openVarianceForLocation1.PK, approvedVariance.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsItemCycleCountVariance(new[] { openVarianceForLocation2.PK }, false);
				}
			}
		}

		#endregion

		#region TestAllVariancesForAllCycleCountLocationHasSameStatus

		public void TestAllVariancesForAllCycleCountLocationHasSameStatus_OpenStatus()
		{
			TestAllVariancesForAllCycleCountLocationHasSameStatus(status: "OPN");
		}

		public void TestAllVariancesForAllCycleCountLocationHasSameStatus_ApprovedStatus()
		{
			TestAllVariancesForAllCycleCountLocationHasSameStatus(status: "APP");
		}

		public void TestAllVariancesForSingleCycleCountLocationHasDifferentStatus_RejectedStatus()
		{
			TestAllVariancesForAllCycleCountLocationHasSameStatus(status: "REJ");
		}

		void TestAllVariancesForAllCycleCountLocationHasSameStatus(string status)
		{
			var sql = new SqlQueryBuilder();
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus, WhsItemCycleCountLocationVarianceTableNameForSQL, TestConnection))
			{
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;
				var cycleCountLocation = new WhsItemCycleCountLocation(locationPK).AppendInsertAndReturnObject(sql);
				var variance1 = new WhsItemCycleCountLocationVariance(cycleCountLocation, status);
				var variance2 = new WhsItemCycleCountLocationVariance(cycleCountLocation, status);

				if (status == "APP")
				{
					var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
					variance1.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
					variance2.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
				}

				variance1.AppendInsertAndReturnObject(sql);
				variance2.AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestUpdateWhsItemCycleCountVariance(new[] { variance1.PK }, false);
				TestUpdateWhsItemCycleCountVariance(new[] { variance1.PK, variance2.PK }, false);
			}
		}

		#endregion

		#region TestAllVariancesForEachCycleCountLocationsHasSameStatus

		public void TestAllVariancesForEachCycleCountLocationsHasSameStatus()
		{
			var sql = new SqlQueryBuilder();
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus, WhsItemCycleCountLocationVarianceTableNameForSQL, TestConnection))
			{
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var location1PK = new WhsLocation(row, area, area) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
				var location2PK = new WhsLocation(row, area, area) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;
				var location3PK = new WhsLocation(row, area, area) { WL_Column = 3 }.AppendInsertAndReturnObject(sql).PK;

				var openCycleCountLocation = new WhsItemCycleCountLocation(location1PK, jobID: "CC001").AppendInsertAndReturnObject(sql);
				var openVariance1 = new WhsItemCycleCountLocationVariance(openCycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);
				var openVariance2 = new WhsItemCycleCountLocationVariance(openCycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);

				var rejectedCycleCountLocation = new WhsItemCycleCountLocation(location2PK, jobID: "CC002").AppendInsertAndReturnObject(sql);
				var rejectedVariance1 = new WhsItemCycleCountLocationVariance(rejectedCycleCountLocation, "REJ").AppendInsertAndReturnObject(sql);
				var rejectedVariance2 = new WhsItemCycleCountLocationVariance(rejectedCycleCountLocation, "REJ").AppendInsertAndReturnObject(sql);

				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
				var approvedCycleCountLocation = new WhsItemCycleCountLocation(location3PK, jobID: "CC003").AppendInsertAndReturnObject(sql);
				var approvedVariance1 = new WhsItemCycleCountLocationVariance(approvedCycleCountLocation, "APP") { WIV_WRC_ReceiveConsignment = receiveConsignment.PK }.AppendInsertAndReturnObject(sql);
				var approvedVariance2 = new WhsItemCycleCountLocationVariance(approvedCycleCountLocation, "APP") { WIV_WRC_ReceiveConsignment = receiveConsignment.PK }.AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestUpdateWhsItemCycleCountVariance(new[] { openVariance1.PK, openVariance2.PK, rejectedVariance1.PK, rejectedVariance2.PK, approvedVariance1.PK, approvedVariance2.PK }, false);
			}
		}

		#endregion

		void TestUpdateWhsItemCycleCountVariance(Guid[] variancePKs, bool expectAnException)
		{
			const string sql =
@"
EXEC dbo.WhsItemCheckAllVariancesForSingleCycleCountLocationHasSameStatus @VariancePKs;
";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@VariancePKs", "dbo.TVP_uniqueidentifier", variancePKs);
				if (expectAnException)
				{
					AssertExceptionThrown(typeof(SqlException), () => sqlCommand.ExecuteNonQuery());
				}
				else
				{
					AssertNoExceptionThrown(() => sqlCommand.ExecuteNonQuery());
				}
			}
		}
	}
}
