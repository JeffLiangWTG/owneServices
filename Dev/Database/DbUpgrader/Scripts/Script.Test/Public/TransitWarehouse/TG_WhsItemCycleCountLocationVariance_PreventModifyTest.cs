using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemCycleCountLocationVariance_PreventModify))]
	class TG_WhsItemCycleCountLocationVariance_PreventModifyTest : DBCreateTriggerScriptTest
	{
		const string ChangingStatusTriggerErrorMessage = "Attempted to change APP/REJ status.";
		const string ChangingRCNErrorMessage = "Attempted to change Receive Consignment when already set.";
		const string ChangingTransferLineTriggerErrorMessage = "Attempted to change Transfer Line when already set.";

		public void TestTrigger_UpdateVarianceStatus()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;

			var receiveConsignmentPK = new WhsItemReceiveConsignment(whs, "123", "123", "STD", "SYD").AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsItemCycleCountLocation(locationPK, jobID: "CC001")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var cycleCountLocation2 = new WhsItemCycleCountLocation(locationPK, jobID: "CC002")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var variance1 = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "OPN")
			{
				WIV_VarianceQty = 1
			}.AppendInsertAndReturnObject(sql);
			var variance2 = new WhsItemCycleCountLocationVariance(cycleCountLocation2, "OPN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance1.PK)
					.Set(c => c.WIV_WRC_ReceiveConsignment, receiveConsignmentPK)
					.Set(c => c.WIV_VarianceQty, (short)1)
					.Set(c => c.WIV_Status, "APP")
					.Post(TestConnection));

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance2.PK).Set(c => c.WIV_Status, "REJ")
					.Post(TestConnection));
		}

		public void TestTrigger_UpdateVarianceStatus_APPtoOPN()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "ARV", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "APP")
			{
				WIV_WPS_PackageState = packageState.PK,
				WIV_VarianceQty = -1
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingStatusTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK).Set(c => c.WIV_Status, "OPN")
					.Post(TestConnection), true);
		}

		public void TestTrigger_UpdateVarianceStatus_APPtoREJ()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(pkgPackageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "ARV", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "APP")
			{
				WIV_WPS_PackageState = packageState.PK,
				WIV_VarianceQty = -1
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingStatusTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK).Set(c => c.WIV_Status, "REJ")
					.Post(TestConnection), true);
		}

		public void TestTrigger_UpdateVarianceStatus_REJtoOPN()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "REJ").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingStatusTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK).Set(c => c.WIV_Status, "OPN")
					.Post(TestConnection), true);
		}

		public void TestTrigger_UpdateVarianceStatus_REJtoAPP()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "ARV", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "REJ").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingStatusTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(variance.PK)
					.Set(c => c.WIV_Status, "APP")
					.Set(c => c.WIV_WPS_PackageState, packageState.PK)
					.Set(c => c.WIV_VarianceQty, (short)-1)
					.Post(TestConnection), true);
		}

		public void TestTrigger_UpdateReceiveConsignment()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var locationPK = new WhsLocation(row, area, area).AppendInsertAndReturnObject(sql).PK;
			var receiveConsignment1PK = new WhsItemReceiveConsignment(whs, "123", "123", "STD", "SYD").AppendInsertAndReturnObject(sql).PK;
			var receiveConsignment2PK = new WhsItemReceiveConsignment(whs, "456", "456", "STD", "SYD").AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsItemCycleCountLocation(locationPK, jobID: "CC001")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var cycleCountLocation2 = new WhsItemCycleCountLocation(locationPK, jobID: "CC002")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var varianceWithRCN = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "APP")
			{
				WIV_WRC_ReceiveConsignment = receiveConsignment1PK,
				WIV_VarianceQty = 1
			}.AppendInsertAndReturnObject(sql);

			var varianceWithoutRCN = new WhsItemCycleCountLocationVariance(cycleCountLocation2, "OPN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemCycleCountLocationVariance.UpdateWhere(varianceWithoutRCN.PK)
					.Set(c => c.WIV_VarianceQty, (short)1)
					.Set(c => c.WIV_WRC_ReceiveConsignment, receiveConsignment2PK)
					.Set(c => c.WIV_Status, "APP")
					.Post(TestConnection));

			AssertExceptionThrown("Should throw exception", typeof(SqlException), ChangingRCNErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(varianceWithRCN.PK)
					.Set(c => c.WIV_WRC_ReceiveConsignment, receiveConsignment2PK)
					.Post(TestConnection), true);
		}

		public void TestTrigger_UpdateTransferLine()
		{
			var today = DateTimeOffset.Now;
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql).PK;
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var location1 = new WhsLocation(row, area, area) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row, area, area) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var transfer = new WhsItemTransferHeader("TF1", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location1, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "CTT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location1, receiveLocation: location2);
			var package2 = new PkgPackage(pkgPackageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "CTT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location1, receiveLocation: location2);
			var transferLine1PK = new WhsItemTransferLine(packageState1, location1, location2, transfer).AppendInsertAndReturnObject(sql).PK;
			var transferLine2PK = new WhsItemTransferLine(packageState2, location1, location2, transfer).AppendInsertAndReturnObject(sql).PK;

			var cycleCountLocation1 = new WhsItemCycleCountLocation(location1.PK, jobID: "CC001")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);

			var cycleCountLocation2 = new WhsItemCycleCountLocation(location2.PK, jobID: "CC002")
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var varianceWithTransferLine = new WhsItemCycleCountLocationVariance(cycleCountLocation1, "APP")
			{
				WIV_WPS_PackageState = packageState1.PK,
				WIV_WTF_TransferLine = transferLine1PK,
				WIV_WL_ExpectedStockLocation = location2.PK
			}.AppendInsertAndReturnObject(sql);

			var varianceWithoutTransferLine = new WhsItemCycleCountLocationVariance(cycleCountLocation2, "OPN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemCycleCountLocationVariance.UpdateWhere(varianceWithoutTransferLine.PK)
			.Set(c => c.WIV_WTF_TransferLine, transferLine2PK)
			.Set(c => c.WIV_WPS_PackageState, packageState1.PK)
			.Set(c => c.WIV_WL_ExpectedStockLocation, location1.PK)
			.Set(c => c.WIV_Status, "APP")
			.Post(TestConnection));
			AssertExceptionThrown("Should throw exception", typeof(SqlException),
				ChangingTransferLineTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.UpdateWhere(varianceWithTransferLine.PK)
					.Set(c => c.WIV_WTF_TransferLine, transferLine2PK).Post(TestConnection), true);
		}
	}
}
