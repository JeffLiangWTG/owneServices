using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsCycleCountLocation_PreventStartWhenVarianceExpectLocationExisted))]
	class TG_WhsCycleCountLocation_PreventStartWhenVarianceExpectLocationExistedTest : DBCreateTriggerScriptTest
	{
		const string TriggerErrorMessage = "Attempted to start a Cycle Count that is blocked by open variance from another cycle count.";

		#region Trigger Tests for Update Cycle Count
		public void TestTrigger_UpdateStartTimeOnStartedCycleCountWithCompletedVariance()
		{
			TestTrigger_StartCycleCountCore(isCycleCountStarted: true, statusOfVarianceWithExpectedStockLocation: "REJ", shouldFail: false);
		}

		public void TestTrigger_StartCycleCountWithCompletedVariance()
		{
			TestTrigger_StartCycleCountCore(isCycleCountStarted: false, statusOfVarianceWithExpectedStockLocation: "REJ", shouldFail: false);
		}

		public void TestTrigger_UpdateStartTimeOnStartedCycleCountWithOpenVarianceHasExpectedStock()
		{
			TestTrigger_StartCycleCountCore(isCycleCountStarted: true, statusOfVarianceWithExpectedStockLocation: "OPN", shouldFail: true);
		}

		public void TestTrigger_StartCycleCountWithOpenVarianceHasExpectedStock()
		{
			TestTrigger_StartCycleCountCore(isCycleCountStarted: false, statusOfVarianceWithExpectedStockLocation: "OPN", shouldFail: true);
		}

		void TestTrigger_StartCycleCountCore(bool isCycleCountStarted, string statusOfVarianceWithExpectedStockLocation, bool shouldFail)
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var clientPK = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql).PK;
			var productPK = new OrgSupplierPart("PRD1").AppendInsertAndReturnObject(sql).PK;
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "P1").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK1 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK2 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;

			var cycleCount1 = new WhsCycleCountLocation(locationPK1, "PWA") { WCL_JobID = "WC00000001", WCL_StartTime = isCycleCountStarted ? today : null, WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);

			var cycleCount2 = new WhsCycleCountLocation(locationPK2, "PWA") { WCL_JobID = "WC00000002", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variancePK = new WhsCycleCountLocationVariance(cycleCount2, statusOfVarianceWithExpectedStockLocation, 2m) { WCC_WL_ExpectedStockLocation = locationPK1, WCC_OH_Client = clientPK, WCC_OP_Product = productPK, WCC_PalletID = "PLT1" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			if (shouldFail)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), TriggerErrorMessage,
					() => WhsCycleCountLocation.UpdateWhere(cycleCount1.PK).Set(c => c.WCL_StartTime, DateTimeOffset.Now).Post(TestConnection), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception", () => WhsCycleCountLocation.UpdateWhere(cycleCount1.PK).Set(c => c.WCL_StartTime, DateTimeOffset.Now.AddHours(1)).Post(TestConnection));
			}
		}
		#endregion

		#region Trigger Tests for Insert Cycle Count
		public void TestTrigger_InsertCycleCountWithCompletedVariance()
		{
			TestTrigger_InsertCycleCountCore(isCycleCountStarted: false, statusOfVarianceWithExpectedStockLocation: "REJ", shouldFail: false);
		}

		public void TestTrigger_InsertAndStartCycleCountWithCompletedVariance()
		{
			TestTrigger_InsertCycleCountCore(isCycleCountStarted: true, statusOfVarianceWithExpectedStockLocation: "REJ", shouldFail: false);
		}

		public void TestTrigger_INsertCycleCountWithOpenVarianceHasExpectedStock()
		{
			TestTrigger_InsertCycleCountCore(isCycleCountStarted: false, statusOfVarianceWithExpectedStockLocation: "OPN", shouldFail: false);
		}

		public void TestTrigger_InsertAndStartCycleCountWithOpenVarianceHasExpectedStock()
		{
			TestTrigger_InsertCycleCountCore(isCycleCountStarted: true, statusOfVarianceWithExpectedStockLocation: "OPN", shouldFail: true);
		}

		void TestTrigger_InsertCycleCountCore(bool isCycleCountStarted, string statusOfVarianceWithExpectedStockLocation, bool shouldFail)
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var clientPK = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql).PK;
			var productPK = new OrgSupplierPart("PRD2").AppendInsertAndReturnObject(sql).PK;
			var whs = new WhsWarehouse("WH2").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "P2").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK1 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql).PK;
			var locationPK2 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql).PK;

			var cycleCount1 = new WhsCycleCountLocation(locationPK1, "PWA") { WCL_JobID = "WC00000001", WCL_StartTime = today, WCL_EndTime = today.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance1PK = new WhsCycleCountLocationVariance(cycleCount1, statusOfVarianceWithExpectedStockLocation, 2m) { WCC_WL_ExpectedStockLocation = locationPK2, WCC_OH_Client = clientPK, WCC_OP_Product = productPK, WCC_PalletID = "PLT2" }.AppendInsertAndReturnObject(sql);

			var cycleCount2 = new WhsCycleCountLocation(locationPK2, "PWA") { WCL_JobID = "WC00000002", WCL_StartTime = isCycleCountStarted ? today : null, WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);

			if (shouldFail)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), TriggerErrorMessage, () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
			}
		}
		#endregion
	}
}

