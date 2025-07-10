using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemCycleCountLocation_PreventStartWhenVarianceExpectLocationExisted))]
	class TG_WhsItemCycleCountLocation_PreventStartWhenVarianceExpectLocationExistedTest : DBCreateTriggerScriptTest
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
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "P1").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R1") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var location1 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location1, "VEH123").AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location1, receiveLocation: location1);

			var cycleCount1 = new WhsItemCycleCountLocation(location1.PK, jobID: "CC001") { WIC_Status = isCycleCountStarted ? "INP" : "NST", WIC_StartTime = isCycleCountStarted ? today : null, WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);

			var cycleCount2 = new WhsItemCycleCountLocation(location2.PK, jobID: "CC002") { WIC_Status = "CMP", WIC_StartTime = today, WIC_ProcessingTime = today.AddMinutes(10), WIC_EndTime = today.AddMinutes(10), WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance2 = new WhsItemCycleCountLocationVariance(cycleCount2, statusOfVarianceWithExpectedStockLocation, 1) { WIV_WL_ExpectedStockLocation = location1.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			if (shouldFail)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), TriggerErrorMessage,
					() => WhsItemCycleCountLocation.UpdateWhere(cycleCount1.PK).Set(c => c.WIC_StartTime, DateTimeOffset.Now).Set(c => c.WIC_Status, "INP").Post(TestConnection), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception", () => WhsItemCycleCountLocation.UpdateWhere(cycleCount1.PK).Set(c => c.WIC_StartTime, DateTimeOffset.Now.AddHours(1)).Set(c => c.WIC_Status, "INP").Post(TestConnection));
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
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH2").WithDockDoor(TestConnection);
			var areaPK = new WhsArea(whs.PK, "P2").AppendInsertAndReturnObject(sql).PK;
			var rowPK = new WhsRow(whs, "R2") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql).PK;
			var location1 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(rowPK, areaPK, areaPK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location1, "VEH123").AppendInsertAndReturnObject(sql);

			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location1, receiveLocation: location1);

			var cycleCount1 = new WhsItemCycleCountLocation(location1.PK, jobID: "CC001") { WIC_Status = "CMP", WIC_StartTime = today, WIC_ProcessingTime = today.AddMinutes(10), WIC_EndTime = today.AddMinutes(10), WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var variance1 = new WhsItemCycleCountLocationVariance(cycleCount1, statusOfVarianceWithExpectedStockLocation, 1) { WIV_WPS_PackageState = packageState.PK, WIV_WL_ExpectedStockLocation = location2.PK }.AppendInsertAndReturnObject(sql);

			var cycleCount2 = new WhsItemCycleCountLocation(location2.PK, jobID: "CC002") { WIC_Status = isCycleCountStarted ? "INP" : "NST", WIC_StartTime = isCycleCountStarted ? today : null, WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);

			if (shouldFail)
			{
				AssertExceptionThrown("Should throw exception", typeof(SqlException), TriggerErrorMessage, () => TestConnection.ExecuteNonQuery(sql.ToString()), true);
			}
			else
			{
				AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToString()));
			}
		}

		#endregion
	}
}
