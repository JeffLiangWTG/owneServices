using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsCycleCountLocationVariance_PreventCreateIfParentCycleCountLocationIsNotCompleted))]
	class TG_WhsCycleCountLocationVariance_PreventCreateIfParentCycleCountLocationIsNotCompletedTest : DBCreateTriggerScriptTest
	{
		const string ErrorMessage = "The parent Cycle Count Location has not yet been completed, cannot create the new Variance.";

		#region TestTrigger_CycleCountLocationVariance

		public void TestTrigger_HasCompleteCycleCountLocationVariance()
		{
			AssertNoExceptionThrown("Should not throw exception", () => TestTrigger_CycleCountLocationVarianceCore(DateTimeOffset.Now.AddMinutes(10)));
		}

		public void TestTrigger_HasIncompleteCycleCountLocationVariance()
		{
			AssertExceptionThrown("Should throw exception", typeof(SqlException), ErrorMessage, () => TestTrigger_CycleCountLocationVarianceCore(), true);
		}

		void TestTrigger_CycleCountLocationVarianceCore(DateTimeOffset? locationEndTime = null)
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PID") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = locationEndTime, WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "APP", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK, WCC_WD_RelatedAdjustment = adjustment.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		#endregion

	}
}

