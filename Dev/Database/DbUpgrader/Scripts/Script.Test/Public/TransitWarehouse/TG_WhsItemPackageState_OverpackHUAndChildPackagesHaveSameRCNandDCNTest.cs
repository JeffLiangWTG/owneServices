using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN))]
	class TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCNTest : DBCreateTriggerScriptTest
	{
		const string TriggerErrorMessage_HandlingUnit = @"Attempted to update overpack HU RCN/DCN while its child packages are linked to different RCN/DCN.";
		const string TriggerErrorMessage_ChildPackage = @"Attempted to update child package RCN/DCN while its overpack HU is linked to different RCN/DCN.";

		public void TestTrigger_CannotChangeOverpackHURCNWhileItHasChild()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), true, (p) => p.WPS_WRC_TransitReceiveConsignment, rcn2.PK, TriggerErrorMessage_HandlingUnit);
		}

		public void TestTrigger_CannotChangeOverpackHUDCNWhileItHasChild()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), true, (p) => p.WPS_WDC_TransitDispatchConsignment, dcn2.PK, TriggerErrorMessage_HandlingUnit);
		}

		public void TestTrigger_CannotChangeChildPackageRCNWhileItsOnOverpackHU()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WRC_TransitReceiveConsignment, rcn2.PK, TriggerErrorMessage_ChildPackage);
		}

		public void TestTrigger_CannotChangeChildPackageDCNWhileItsOnOverpackHU()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WDC_TransitDispatchConsignment, dcn2.PK, TriggerErrorMessage_ChildPackage);
		}

		public void TestTrigger_CannotRemoveChildPackageRCNWhileItsOnOverpackHU()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WRC_TransitReceiveConsignment, null, TriggerErrorMessage_ChildPackage);
		}

		public void TestTrigger_CannotRemoveChildPackageDCNWhileItsOnOverpackHU()
		{
			TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WDC_TransitDispatchConsignment, null, TriggerErrorMessage_ChildPackage);
		}

		void TestTrigger_CannotChangeOverpackHUorChildPackageRCNorDCN<T>(StringBuilder sql, bool isChangingHU, Expression<Func<WhsItemPackageState, T>> property, T value, string expectError)
		{
			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, dcn: dcn1, lastLocation: location, isHandlingUnit: true, unitType: "OVP");

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn1, rtu, dcn: dcn1, lastLocation: location);

			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var packageStatePKtoUpdate = isChangingHU ? handlingUnitPackageState.PK : childPackageState.PK;
			AssertExceptionThrown(typeof(SqlException), expectError,
			() => WhsItemPackageState.UpdateWhere(packageStatePKtoUpdate).Set(property, value)
				.Post(TestConnection), true);
		}

		public void TestTrigger_NoError_RemoveChildPackageRCNWhileItsOnNonOverpackHU()
		{
			var sql = SetupTestData();

			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, dcn: dcn1, lastLocation: location, isHandlingUnit: true);

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn1, rtu, dcn: dcn1, lastLocation: location);

			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception", () => WhsItemPackageState.UpdateWhere(childPackageState.PK).Set(p => p.WPS_WRC_TransitReceiveConsignment, null).Post(TestConnection));
		}

		public void TestTrigger_NoError_RemoveUnpackedChildPackageRCNWhileItsOnOverpackHU()
		{
			var sql = SetupTestData();

			var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, dcn: dcn1, lastLocation: location, isHandlingUnit: true, unitType: "OVP");

			var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
			var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn1, rtu, dcn: dcn1, lastLocation: location);

			new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK, DateTime.Now.AddHours(10)).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertNoExceptionThrown("Should not throw exception", () => WhsItemPackageState.UpdateWhere(childPackageState.PK).Set(p => p.WPS_WRC_TransitReceiveConsignment, null).Post(TestConnection));
		}

		StringBuilder SetupTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			location = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			rcn1 = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			rcn2 = new WhsItemReceiveConsignment(whs, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			dcn1 = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			dcn2 = new WhsItemDispatchConsignment(whs, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", location, "rtu").AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		protected override void SetUp()
		{
			// drop triggers so we can insert incorrect data for view to find
			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT null from sys.triggers where name = 'TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit')
	DROP TRIGGER TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit");

			base.SetUp();
		}

		WhsWarehouse whs;
		WhsLocation location;
		WhsItemReceiveConsignment rcn1;
		WhsItemReceiveConsignment rcn2;
		WhsItemDispatchConsignment dcn1;
		WhsItemDispatchConsignment dcn2;
		WhsItemReceiveTransportationUnit rtu;
		PkgPackageJob packageJob_RCN;
		PkgPackageJob packageJob_handlingUnit;
	}

	class TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCNTransactionTestCase : TestCase
	{
		const string TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN = "TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN";
		const string TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		const string ProcedureName = "WhsItemCheckOverpackHUAndChildPackagesHaveSameRCNandDCN";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);

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
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, Db.Connection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				var sql = new StringBuilder();

				var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
				var whs = new WhsWarehouse("TR1", "TRW", branch.PK).WithDockDoor(sql);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var rcn1 = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
				var rcn2 = new WhsItemReceiveConsignment(whs, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
				var dcn1 = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
				var dcn2 = new WhsItemDispatchConsignment(whs, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
				var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", location, "rtu").AppendInsertAndReturnObject(sql);
				var packageJob_RCN = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
				var packageJob_handlingUnit = new PkgPackageJob(handlingUnit.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);

				var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
				var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, dcn: dcn1, lastLocation: location, isHandlingUnit: true, unitType: "OVP");

				var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn1, rtu, dcn: dcn1, lastLocation: location);

				new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(sql.ToString());

				var updateSql = WhsItemPackageState.UpdateWhere(childPackageState.PK)
					.Set(a => a.WPS_WDC_TransitDispatchConsignment, dcn2.PK)
					.AsSQL();

				if (isDeferred)
				{
					// mocked procedure will always throw exception if run
					AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(Db.Connection, ProcedureName, $"SuspendTrigger '{TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN}'\r\n" + updateSql));
				}
				else
				{
					AssertCheckProcedureRanForPkgPackages("If trigger is not suspended then check procedure should be run.", updateSql, childPackageState);
				}
			}
		}

		void AssertCheckProcedureRanForPkgPackages(string errorMessage, string sqlToRun, params WhsItemPackageState[] expectedPackagesInTheProcedure)
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

		#region Implmentation

		void ExecuteSqlInTransaction(string sql)
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(sql);
				Db.Connection.CommitTransaction();
			}
		}

		#endregion
	}
}
