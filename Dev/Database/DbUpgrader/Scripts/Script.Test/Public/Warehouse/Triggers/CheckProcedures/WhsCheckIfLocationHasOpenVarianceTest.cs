using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckIfLocationHasOpenVariance))]
	class WhsCheckIfLocationHasOpenVaranceTest : DbCreateScriptTest
	{
		const string PreventCreateIfLocationHasOpenVariance = "TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance";
		const string ErrorMessage = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";

		#region TestCheckProcedure_HasOpenVarianceCycleCount

		public void TestCheckProcedure_HasOpenVarianceCycleCount()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var location1PK = new WhsLocation(row, area, area) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var location2PK = new WhsLocation(row, area, area) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsCycleCountLocation(location1PK, "PID") { WCL_JobID = "WC00000001" }.AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsCycleCountLocation(location2PK, "PID") { WCL_JobID = "WC00000002", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation2, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
			var cycleCountLocation3 = new WhsCycleCountLocation(location2PK, "PID") { WCL_JobID = "WC00000003" }.AppendInsertAndReturnObject(sql);

			// suspend trigger to save bad data into DB
			using (TestWhsDataSetupHelper.SuspendTrigger(PreventCreateIfLocationHasOpenVariance, WhsCycleCountLocationSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(Array.Empty<Guid>()));
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(new[] { cycleCountLocation1.PK }));
			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () => RunCheckProcedure(new[] { cycleCountLocation2.PK, cycleCountLocation3.PK }), true);
		}

		#endregion

		#region TestCheckProcedure_NoOpenVarianceCycleCount

		public void TestCheckProcedure_NoOpenVarianceCycleCount_Approved()
		{
			TestCheckProcedure_NoOpenVarianceCycleCountCore("APP");
		}

		public void TestCheckProcedure_NoOpenVarianceCycleCount_Rejected()
		{
			TestCheckProcedure_NoOpenVarianceCycleCountCore("REJ");
		}

		void TestCheckProcedure_NoOpenVarianceCycleCountCore(string status)
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance = new WhsCycleCountLocationVariance(cycleCountLocation1, status) { WCC_PalletID = "PLT1" };
			if (status == "APP")
			{
				var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "D1", "D1").AppendInsertAndReturnObject(sql);
				variance.WCC_WD_RelatedAdjustment = adjustment.PK;
			}
			variance.AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsCycleCountLocation(locationPK, "PID") { WCL_JobID = "WC00000002" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(new[] { cycleCountLocation1.PK, cycleCountLocation2.PK }));
		}

		#endregion

		#region TestCheckProcedure_DifferentLocation

		public void TestCheckProcedure_DifferentLocation()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var location1PK = new WhsLocation(row, area, area) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var location2PK = new WhsLocation(row, area, area) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsCycleCountLocation(location1PK, "PID") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(2), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsCycleCountLocationVariance(cycleCountLocation1, "OPN") { WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsCycleCountLocation(location2PK, "PID") { WCL_JobID = "WC00000002" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(new[] { cycleCountLocation1.PK, cycleCountLocation2.PK }));
		}

		#endregion

		#region TestProcedure

		void RunCheckProcedure(Guid[] cycleCountPKs)
		{
			const string sql = "EXEC WhsCheckIfLocationHasOpenVariance @CycleCountPKs";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@CycleCountPKs", "dbo.TVP_uniqueidentifier", cycleCountPKs);
				sqlCommand.ExecuteNonQuery();
			}
		}
		#endregion
	}
}

