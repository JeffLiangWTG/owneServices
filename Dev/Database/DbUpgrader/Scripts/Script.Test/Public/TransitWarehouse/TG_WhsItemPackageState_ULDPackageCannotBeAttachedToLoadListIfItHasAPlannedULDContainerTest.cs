using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer))]
	class TG_WhsItemPackageState_ULDCannotBeAttachedToLoadListIfItHasAPlannedULDTest : DBCreateTriggerScriptTest
	{
		const string ErrorMessage = "Cannot plan to load a Container into another Container.";

		public void TestTrigger_CannotAttachContainerToLoadListWhenItHasAPlannedContainerDTU_ULD_ULD()
		{
			Test_Core("ULD", "ULD");
		}

		public void TestTrigger_CannotAttachContainerToLoadListWhenItHasAPlannedContainerDTU_ULD_CNT()
		{
			Test_Core("ULD", "CNT");
		}

		public void TestTrigger_CannotAttachContainerToLoadListWhenItHasAPlannedContainerDTU_CNT_ULD()
		{
			Test_Core("CNT", "ULD");
		}

		public void TestTrigger_CannotAttachContainerToLoadListWhenItHasAPlannedContainerDTU_CNT_CNT()
		{
			Test_Core("CNT", "CNT");
		}

		void Test_Core(string freightUnitType, string containerUnitType)
		{
			var sql = SetupTestData();

			var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: freightUnitType).AppendInsertAndReturnObject(sql);
			var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: containerUnitType).AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs);
			var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU.PK, whs);

			var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertExceptionThrown(typeof(SqlException), ErrorMessage,
			() => WhsItemPackageState.UpdateWhere(uldHandlingUnitForDTU2ToBeAttached.PK).Set(p => p.WPS_WDL_LoadList, dll.PK)
				.Post(TestConnection), true);
		}

		public void TestTrigger_CanAttachContainerToLoadListWhenItDoesNotHaveAPlannedContainer()
		{
			var sql = SetupTestData();

			var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package3 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package4 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package5 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var freightDTU1 = new WhsItemDispatchTransportationUnit(whs, "freightDTU1", unitType: "CNT").AppendInsertAndReturnObject(sql);
			var freightDTU2 = new WhsItemDispatchTransportationUnit(whs, "freightDTU2", unitType: "ULD").AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, freightDTU1.PK, whs);
			var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU2.PK, whs);

			var handlingUnitAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "BKD", whs, null, isHandlingUnit: true, dll: dll);
			var packageLoadedOntoULDAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4.PK, "FLO", whs, rcn, isHandlingUnit: false, rtu: rtu, dcn: dcn, dtu: dtu1, dll: dll);
			var packageAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package5.PK, "BKD", whs, rcn, isHandlingUnit: false, dll: dll);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemPackageState.UpdateWhere(uldHandlingUnitForDTU1.PK).Set(p => p.WPS_WDL_LoadList, dll.PK)
				.Post(TestConnection));
			AssertNoExceptionThrown("Should not throw exception",
				() => WhsItemPackageState.UpdateWhere(uldHandlingUnitForDTU2.PK).Set(p => p.WPS_WDL_LoadList, dll.PK)
				.Post(TestConnection));
		}

		StringBuilder SetupTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			location = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", location, "rtu").AppendInsertAndReturnObject(sql);
			dtu1 = new WhsItemDispatchTransportationUnit(whs, "dtu1").AppendInsertAndReturnObject(sql);
			dll = new WhsItemDispatchLoadList("dll", whs).AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		WhsWarehouse whs;
		WhsLocation location;
		WhsItemReceiveConsignment rcn;
		WhsItemDispatchConsignment dcn;
		WhsItemReceiveTransportationUnit rtu;
		WhsItemDispatchTransportationUnit dtu1;
		WhsItemDispatchLoadList dll;
		PkgPackageJob packageJob_RCN;
	}

	class TG_WhsItemPackageState_ULDCannotBeAttachedToLoadListIfItHasAPlannedULDNonTransactionTestCase : TestCase
	{
		const string ProcedureName = "WhsItemCheckULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer";
		const string TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer = "TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer";

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
			var sql = SetupTestData();
			var package1 = new PkgPackage(packageJob_RCN, "BOX", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
			var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "CNT").AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs);
			var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU.PK, whs);

			var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll2, containerDTU).AppendInsertAndReturnObject(sql);

			ExecuteSqlInTransaction(sql.ToString());

			var updateSQL = WhsItemPackageState.UpdateWhere(uldHandlingUnitForDTU2ToBeAttached.PK).Set((p) => p.WPS_WDL_LoadList, dll2.PK).AsSQL();
			if (isDeferred)
			{
				// mocked procedure will always throw exception if run
				AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, $"SuspendTrigger '{TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer}'\r\n" + updateSQL));
			}
			else
			{
				AssertCheckProcedureRanForWhsItemPackageStates("If trigger is not suspended then check procedure should be run.", updateSQL, uldHandlingUnitForDTU2ToBeAttached);
			}
		}

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update()
		{
			var sql = SetupTestData();
			var standAlonePackage = new PkgPackage(packageJob_RCN, "BOX", 1).AppendInsertAndReturnObject(sql);
			var uldPackage1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var uldPackage2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var standAlonePackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, standAlonePackage.PK, "ARV", whs, rcn, rtu, dcn: dcn, dll: dll1, lastLocation: location);

			var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
			var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "CNT").AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, uldPackage1.PK, containerDTU.PK, whs);
			var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, uldPackage2.PK, freightDTU.PK, whs);

			var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll2, containerDTU).AppendInsertAndReturnObject(sql);

			ExecuteSqlInTransaction(sql.ToString());

			var updateSQL = WhsItemPackageState.UpdateWhere(uldHandlingUnitForDTU2ToBeAttached.PK).Set((p) => p.WPS_WDL_LoadList, dll2.PK).AsSQL();
			AssertCheckProcedureRanForWhsItemPackageStates("If trigger is not suspended then check procedure should be run.", updateSQL, uldHandlingUnitForDTU2ToBeAttached);
		}

		#endregion

		void AssertCheckProcedureRanForWhsItemPackageStates(string errorMessage, string sqlToRun, params WhsItemPackageState[] expectedPackagesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(Db.Connection, ProcedureName, sqlToRun, expectedPackagesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedPackagesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		void ExecuteSqlInTransaction(string sql)
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(sql);
				Db.Connection.CommitTransaction();
			}
		}

		StringBuilder SetupTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			location = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", location, "rtu").AppendInsertAndReturnObject(sql);
			dll1 = new WhsItemDispatchLoadList("dll1", whs).AppendInsertAndReturnObject(sql);
			dll2 = new WhsItemDispatchLoadList("dll2", whs).AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		WhsWarehouse whs;
		WhsLocation location;
		WhsItemReceiveConsignment rcn;
		WhsItemDispatchConsignment dcn;
		WhsItemReceiveTransportationUnit rtu;
		WhsItemDispatchLoadList dll1;
		WhsItemDispatchLoadList dll2;
		PkgPackageJob packageJob_RCN;
	}
}
