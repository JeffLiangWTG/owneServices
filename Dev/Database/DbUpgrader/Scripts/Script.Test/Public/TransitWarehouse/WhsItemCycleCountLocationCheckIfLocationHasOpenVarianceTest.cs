using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse.WhsItemCycleCountLocationCheckIfLocationHasOpenVariance))]
	class WhsItemCycleCountLocationCheckIfLocationHasOpenVariance : DbCreateScriptTest
	{
		const string PreventCreateIfLocationHasOpenVariance = "dbo.TG_WhsItemCycleCountLocation_PreventCreateIfLocationHasOpenVariance";
		const string ErrorMessage = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";
		static readonly string WhsItemCycleCountLocationSchemaTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemCycleCountLocationSchema.Instance);

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

			var cycleCountLocation1 = new WhsItemCycleCountLocation(location1PK, jobID: "CC001").AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsItemCycleCountLocation(location2PK, jobID: "CC002")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation2, "OPN").AppendInsertAndReturnObject(sql);
			var cycleCountLocation3 = new WhsItemCycleCountLocation(location2PK, jobID: "CC003").AppendInsertAndReturnObject(sql);

			// suspend trigger to save bad data into DB
			using (TestWhsDataSetupHelper.SuspendTrigger(PreventCreateIfLocationHasOpenVariance, WhsItemCycleCountLocationSchemaTableNameForSQL, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(Array.Empty<Guid>()));
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(new[] { cycleCountLocation1.PK }));
			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () => RunCheckProcedure(new[] { cycleCountLocation3.PK }), true);
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
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsItemCycleCountLocation(locationPK, jobID: "CC001")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation1, status);
			if (status == "APP")
			{
				var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC1", "RCT00000001", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
				variance.WIV_WRC_ReceiveConsignment = receiveConsignment.PK;
			}
			variance.AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsItemCycleCountLocation(locationPK, jobID: "CC002").AppendInsertAndReturnObject(sql);

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

			var cycleCountLocation1 = new WhsItemCycleCountLocation(location1PK, jobID: "CC001")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var openVariance = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "OPN").AppendInsertAndReturnObject(sql);
			var cycleCountLocation2 = new WhsItemCycleCountLocation(location2PK, jobID: "CC002").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertNoExceptionThrown("Should not throw exception", () => RunCheckProcedure(new[] { cycleCountLocation1.PK, cycleCountLocation2.PK }));
		}

		#endregion

		#region TestProcedure

		void RunCheckProcedure(Guid[] cycleCountPKs)
		{
			const string sql = "EXEC dbo.WhsItemCycleCountLocationCheckIfLocationHasOpenVariance @CycleCountPKs";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@CycleCountPKs", "dbo.TVP_uniqueidentifier", cycleCountPKs);
				sqlCommand.ExecuteNonQuery();
			}
		}
		#endregion
	}
}
