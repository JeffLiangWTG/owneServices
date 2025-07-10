using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Testing
{
	[TestedType(typeof(WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus))]
	class WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatusTest : DbCreateScriptTest
	{
		const string EnsureAllVariancesForSingleCycleCountLocationHasSameStatus = "TG_WhsCycleCountLocationVariance_HasSameStatus";

		#region TestAllVariancesForSingleCycleCountLocationHasSameStatus

		public void TestAllVariancesForSingleCycleCountLocationHasDifferentStatus()
		{
			TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(typeof(SqlException), "All variances do not have same status.");

			var sql = new SqlQueryBuilder();
			using (Db.Connection.BeginTransactionWithManager())
			{
				using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus,
					WhsCycleCountLocationVarianceSchema.Constants.TableName, TestConnection))
				{
					var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
					var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
					var areaPK = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
					var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
					var location1PK = new WhsLocation(row, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
					var location2PK = new WhsLocation(row, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;
					var cycleCountLocation1 = new WhsCycleCountLocation(location1PK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
					var cycleCountLocation2 = new WhsCycleCountLocation(location2PK, "PWS") { WCL_JobID = "WC00000002", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
					var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
					var openVarianceForLocation1 = new WhsCycleCountLocationVariance(cycleCountLocation1, "OPN") { WCC_Status = "OPN", WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
					var openVarianceForLocation2 = new WhsCycleCountLocationVariance(cycleCountLocation2, "OPN") { WCC_Status = "OPN", WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
					var approvedVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "APP") { WCC_PalletID = "PLT2", WCC_WD_RelatedAdjustment = adjustment.PK }.AppendInsertAndReturnObject(sql);
					var rejectedVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "REJ") { WCC_PalletID = "PLT3" }.AppendInsertAndReturnObject(sql);

					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

					TestUpdateWhsCycleCountVariance(new[] { openVarianceForLocation1.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { approvedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { openVarianceForLocation1.PK, approvedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { approvedVariance.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { openVarianceForLocation1.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { openVarianceForLocation1.PK, approvedVariance.PK, rejectedVariance.PK, openVarianceForLocation2.PK }, true);
					TestUpdateWhsCycleCountVariance(new[] { openVarianceForLocation2.PK }, false);
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
			TestAllVariancesForAllCycleCountLocationHasSameStatus(status: "APP", shouldCreateRelatedAdjustment: true);
		}

		public void TestAllVariancesForSingleCycleCountLocationHasDifferentStatus_RejectedStatus()
		{
			TestAllVariancesForAllCycleCountLocationHasSameStatus(status: "REJ");
		}

		void TestAllVariancesForAllCycleCountLocationHasSameStatus(string status, bool shouldCreateRelatedAdjustment = false)
		{
			var sql = new SqlQueryBuilder();
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus,
				WhsCycleCountLocationVarianceSchema.Constants.TableName, TestConnection))
			{
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
				var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;
				var cycleCountLocation = new WhsCycleCountLocation(locationPK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var variance1 = new WhsCycleCountLocationVariance(cycleCountLocation, status) { WCC_PalletID = "PLT1" };
				var variance2 = new WhsCycleCountLocationVariance(cycleCountLocation, status) { WCC_PalletID = "PLT2" };

				if (shouldCreateRelatedAdjustment)
				{
					var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
					var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
					variance1.WCC_WD_RelatedAdjustment = adjustment.PK;
					variance2.WCC_WD_RelatedAdjustment = adjustment.PK;
				}
				variance1.AppendInsertAndReturnObject(sql);
				variance2.AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestUpdateWhsCycleCountVariance(new[] { variance1.PK }, false);
				TestUpdateWhsCycleCountVariance(new[] { variance1.PK, variance2.PK }, false);
			}
		}

		#endregion

		#region TestAllVariancesForEachCycleCountLocationsHasSameStatus

		public void TestAllVariancesForEachCycleCountLocationsHasSameStatus()
		{
			var sql = new SqlQueryBuilder();
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureAllVariancesForSingleCycleCountLocationHasSameStatus,
				WhsCycleCountLocationVarianceSchema.Constants.TableName, TestConnection))
			{
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
				var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
				var row = new WhsRow(whs, "R1") { WR_Columns = 3 }.AppendInsertAndReturnObject(sql).PK;
				var location1PK = new WhsLocation(row, area, area) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
				var location2PK = new WhsLocation(row, area, area) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;
				var location3PK = new WhsLocation(row, area, area) { WL_Column = 3 }.AppendInsertAndReturnObject(sql).PK;

				var openCycleCountLocation = new WhsCycleCountLocation(location1PK, "PWS") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var openVariance1 = new WhsCycleCountLocationVariance(openCycleCountLocation, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
				var openVariance2 = new WhsCycleCountLocationVariance(openCycleCountLocation, "OPN") { WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

				var rejectedCycleCountLocation = new WhsCycleCountLocation(location2PK, "PWS") { WCL_JobID = "WC00000002", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var rejectedVariance1 = new WhsCycleCountLocationVariance(rejectedCycleCountLocation, "REJ") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
				var rejectedVariance2 = new WhsCycleCountLocationVariance(rejectedCycleCountLocation, "REJ") { WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				var approvedCycleCountLocation = new WhsCycleCountLocation(location3PK, "PWS") { WCL_JobID = "WC00000003", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var approvedVariance1 = new WhsCycleCountLocationVariance(approvedCycleCountLocation, "APP") { WCC_PalletID = "PLT1", WCC_WD_RelatedAdjustment = adjustment.PK }.AppendInsertAndReturnObject(sql);
				var approvedVariance2 = new WhsCycleCountLocationVariance(approvedCycleCountLocation, "APP") { WCC_PalletID = "PLT2", WCC_WD_RelatedAdjustment = adjustment.PK }.AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestUpdateWhsCycleCountVariance(new[] { openVariance1.PK, openVariance2.PK, rejectedVariance1.PK, rejectedVariance2.PK, approvedVariance1.PK, approvedVariance2.PK }, false);
			}
		}

		#endregion

		void TestUpdateWhsCycleCountVariance(Guid[] variancePKs, bool expectAnException)
		{
			const string sql =
@"
EXEC WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus @VariancePKs;
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

