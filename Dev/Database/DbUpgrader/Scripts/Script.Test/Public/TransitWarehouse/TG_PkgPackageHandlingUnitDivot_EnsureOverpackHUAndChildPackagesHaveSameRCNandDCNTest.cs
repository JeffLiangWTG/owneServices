using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_PkgPackageHandlingUnitDivot_EnsureOverpackHUAndChildPackagesHaveSameRCNandDCN))]
	class TG_PkgPackageHandlingUnitDivot_EnsureOverpackHUAndChildPackagesHaveSameRCNandDCNTest : DBCreateTriggerScriptTest
	{
		const string TriggerErrorMessage = @"Packed package should have the same RCN and DCN as its overpack handling unit.";

		public void TestTrigger_CannotPackPackageHasNoRCNOntoOverpackHandlingUnitHasRCN()
		{
			TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_RCNMismatch(false);
		}

		public void TestTrigger_CannotPackPackageHasDiffRCNOntoOverpackHandlingUnit()
		{
			TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_RCNMismatch(true);
		}

		void TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_RCNMismatch(bool childpackageHasRCN)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn1 = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rcn2 = new WhsItemReceiveConsignment(whs, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob_RCN = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);
			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, lastLocation: locationA1, isHandlingUnit: true, unitType: "OVP");

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, childpackageHasRCN ? rcn2 : null, rtu, lastLocation: locationA1);

			TestConnection.ExecuteNonQuery(sql.ToString());

			sql = new StringBuilder();
			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(typeof(SqlException), TriggerErrorMessage, () => TestConnection.Command(sql.ToString()).ExecuteNonQuery(), true);
		}

		public void TestTrigger_CannotPackPackageHasNoDCNOntoOverpackHandlingUnitHasDCN()
		{
			TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_DCNMismatch(false);
		}

		public void TestTrigger_CannotPackPackageHasDiffDCNOntoOverpackHandlingUnit()
		{
			TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_DCNMismatch(true);
		}

		void TestTrigger_CannotPackPackageOntoOverpackHandlingUnit_DCNMismatch(bool childpackageHasDCN)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dcn1 = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dcn2 = new WhsItemDispatchConsignment(whs, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);
			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, null, null, dcn: dcn1, lastLocation: locationA1, isHandlingUnit: true, unitType: "OVP");

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn, rtu, dcn: childpackageHasDCN ? dcn2 : null, lastLocation: locationA1);

			TestConnection.ExecuteNonQuery(sql.ToString());

			sql = new StringBuilder();
			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(typeof(SqlException), TriggerErrorMessage, () => TestConnection.Command(sql.ToString()).ExecuteNonQuery(), true);
		}

		public void TestTrigger_NoError_PackPackageOntoNonOverpackHandlingUnit()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dcn1 = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dcn2 = new WhsItemDispatchConsignment(whs, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);
			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, null, null, dcn: dcn1, lastLocation: locationA1, isHandlingUnit: true);

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn, rtu, dcn: dcn2, lastLocation: locationA1);

			TestConnection.ExecuteNonQuery(sql.ToString());

			sql = new StringBuilder();
			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should not throw error", () => TestConnection.Command(sql.ToString()).ExecuteNonQuery());
		}

		public void TestTrigger_NoError_InsertUnpackedDivotOntoOverpackHandlingUnit()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dcn1 = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dcn2 = new WhsItemDispatchConsignment(whs, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA1, "rtu").AppendInsertAndReturnObject(sql);
			var packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);
			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, null, null, dcn: dcn1, lastLocation: locationA1, isHandlingUnit: true);

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn, rtu, dcn: dcn2, lastLocation: locationA1);

			TestConnection.ExecuteNonQuery(sql.ToString());

			sql = new StringBuilder();
			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK, DateTime.Now.AddHours(10)).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should not throw error", () => TestConnection.Command(sql.ToString()).ExecuteNonQuery());
		}

		#region Implmentation

		protected override void SetUp()
		{
			// drop triggers so we can insert incorrect data for view to find
			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT null from sys.triggers where name = 'TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit')
	DROP TRIGGER TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit");

			base.SetUp();
		}

		#endregion
	}
}
