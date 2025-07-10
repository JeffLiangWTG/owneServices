using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached))]
	class TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttachedTest : DbCreateScriptTest
	{
		const string ErrorMessage = "Cannot plan to load a Container into another Container.";

		public void TestTrigger_ContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_ULD_ULD()
		{
			Test_Core("ULD", "ULD");
		}

		public void TestTrigger_ContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_ULD_CNT()
		{
			Test_Core("ULD", "CNT");
		}

		public void TestTrigger_ContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_CNT_ULD()
		{
			Test_Core("CNT", "ULD");
		}

		public void TestTrigger_ContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_CNT_CNT()
		{
			Test_Core("CNT", "CNT");
		}

		void Test_Core(string freightUnitType,  string containerUnitType)
		{
			var sql = SetupTestData();

			var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: freightUnitType).AppendInsertAndReturnObject(sql);
			var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: containerUnitType).AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1AttachedToDLL = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, freightDTU.PK, whs, dll: dll);
			var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, containerDTU.PK, whs);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var updateSQL = new SqlQueryBuilder();
			var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(updateSQL);
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ErrorMessage, () => TestConnection.ExecuteNonQuery(
				updateSQL.ToStringWithNewLineBetweenAppends()), true);
		}

		public void TestTrigger_ContainerCanBePlannedForLoadListIfItDoesNotHaveAContainerPackageAttached()
		{
			var sql = SetupTestData();

			var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package3 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package4 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package5 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var containerDTU1 = new WhsItemDispatchTransportationUnit(whs, "containerDTU1", unitType: "CNT").AppendInsertAndReturnObject(sql);
			var containerDTU2 = new WhsItemDispatchTransportationUnit(whs, "containerDTU2", unitType: "ULD").AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU1.PK, whs);
			var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, containerDTU2.PK, whs);

			var handlingUnitAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "BKD", whs, null, isHandlingUnit: true, dll: dll);
			var packageLoadedOntoULDAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4.PK, "FLO", whs, rcn, isHandlingUnit: false, dcn: dcn, rtu: rtu, dtu: dtu, dll: dll);
			var packageAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package5.PK, "BKD", whs, rcn, isHandlingUnit: false, dll: dll);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var updateSQL = new SqlQueryBuilder();
			var loadListDTUPivot1 = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU1).AppendInsertAndReturnObject(updateSQL);
			var loadListDTUPivot2 = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU2).AppendInsertAndReturnObject(updateSQL);
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSQL.ToStringWithNewLineBetweenAppends()));
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
			dtu = new WhsItemDispatchTransportationUnit(whs, "dtu").AppendInsertAndReturnObject(sql);
			dll = new WhsItemDispatchLoadList("dll", whs).AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		WhsLocation location;
		WhsWarehouse whs;
		WhsItemReceiveConsignment rcn;
		WhsItemDispatchConsignment dcn;
		WhsItemReceiveTransportationUnit rtu;
		WhsItemDispatchTransportationUnit dtu;
		WhsItemDispatchLoadList dll;
		PkgPackageJob packageJob_RCN;
	}

	class TG_WhsItemDispatchLoadListDTUPivot_ContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttachedTestNonTransactionTestCase : TestCase
	{
		const string ProcedureName = "WhsItemCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached";
		const string TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached = "TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached";

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
			var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

			var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
			var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);

			var uldHandlingUnitForDTU1AttachedToDLL = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, freightDTU.PK, whs, dll: dll);
			var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, containerDTU.PK, whs);

			ExecuteSqlInTransaction(sql.ToString());

			var updateSQL = new SqlQueryBuilder();
			var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(updateSQL);
			if (isDeferred)
			{
				// mocked procedure will always throw exception if run
				AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, $"SuspendTrigger '{TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached}'\r\n" + updateSQL.ToStringWithNewLineBetweenAppends()));
			}
			else
			{
				AssertCheckProcedureRanForWhsItemDispatchLoadListDTUPivots("If trigger is not suspended then check procedure should be run.", updateSQL.ToStringWithNewLineBetweenAppends(), loadListDTUPivot);
			}
		}

		void AssertCheckProcedureRanForWhsItemDispatchLoadListDTUPivots(string errorMessage, string sqlToRun, params WhsItemDispatchLoadListDTUPivot[] expectedPivotsInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(Db.Connection, ProcedureName, sqlToRun, expectedPivotsInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedPivotsInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
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

			rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			dll = new WhsItemDispatchLoadList("dll", whs).AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		WhsWarehouse whs;
		WhsItemReceiveConsignment rcn;
		WhsItemDispatchLoadList dll;
		PkgPackageJob packageJob_RCN;
	}
}
