using System;
using System.Linq.Expressions;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(WhsItemCheckOverpackHUAndChildPackagesHaveSameRCNandDCN))]
	class WhsItemCheckOverpackHUAndChildPackagesHaveSameRCNandDCNTest : DbCreateScriptTest
	{
		const string TriggerErrorMessage_HandlingUnit = @"Attempted to update overpack HU RCN/DCN while its child packages are linked to different RCN/DCN.";
		const string TriggerErrorMessage_ChildPackage = @"Attempted to update child package RCN/DCN while its overpack HU is linked to different RCN/DCN.";

		const string TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN = "dbo.TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN";
		const string TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);
		static readonly string WhsItemPackageStateTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemPackageStateSchema.Instance);

		public void TestCannotChangeOverpackHURCNWhileItHasChild()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), true, (p) => p.WPS_WRC_TransitReceiveConsignment, rcn2.PK, TriggerErrorMessage_HandlingUnit);
		}

		public void TestCannotChangeOverpackHUDCNWhileItHasChild()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), true, (p) => p.WPS_WDC_TransitDispatchConsignment, dcn2.PK, TriggerErrorMessage_HandlingUnit);
		}

		public void TestCannotChangeChildPackageRCNWhileItsOnOverpackHU()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WRC_TransitReceiveConsignment, rcn2.PK, TriggerErrorMessage_ChildPackage);
		}

		public void TestCannotChangeChildPackageDCNWhileItsOnOverpackHU()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WDC_TransitDispatchConsignment, dcn2.PK, TriggerErrorMessage_ChildPackage);
		}

		public void TestCannotRemoveChildPackageRCNWhileItsOnOverpackHU()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WRC_TransitReceiveConsignment, null, TriggerErrorMessage_ChildPackage);
		}

		public void TestCannotRemoveChildPackageDCNWhileItsOnOverpackHU()
		{
			TestCannotChangeOverpackHUorChildPackageRCNorDCN(SetupTestData(), false, (p) => p.WPS_WDC_TransitDispatchConsignment, null, TriggerErrorMessage_ChildPackage);
		}

		void TestCannotChangeOverpackHUorChildPackageRCNorDCN<T>(StringBuilder sql, bool isChangingHU, Expression<Func<WhsItemPackageState, T>> property, T value, string expectError)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN, WhsItemPackageStateTableNameForSQL, TestConnection))
			{
				var handlingUnitPackage = new PkgPackage(packageJob_handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
				var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "ARV", whs, rcn1, null, dcn: dcn1, lastLocation: location, isHandlingUnit: true, unitType: "OVP");

				var childPackage = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var childPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, childPackage.PK, "ARV", whs, rcn1, rtu, dcn: dcn1, lastLocation: location);

				new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				var packageStatePKtoUpdate = isChangingHU ? handlingUnitPackageState.PK : childPackageState.PK;
				WhsItemPackageState.UpdateWhere(packageStatePKtoUpdate).Set(property, value).Post(TestConnection);

				using (var sqlCommand = Db.Connection.Command("EXEC dbo.WhsItemCheckOverpackHUAndChildPackagesHaveSameRCNandDCN @PackageStatePKs"))
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", new[] { packageStatePKtoUpdate });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectError);
					AssertExceptionThrown(typeof(SqlException), expectError, () => sqlCommand.ExecuteNonQuery(), true);
				}
			}
		}

		public void TestNoError_RemoveChildPackageRCNWhileItsOnNonOverpackHU()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, TestConnection))
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
		}

		public void TestNoError_RemoveUnpackedChildPackageRCNWhileItsOnOverpackHU()
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

		#region Implmentation

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

		WhsLocation location;
		WhsWarehouse whs;
		WhsItemReceiveConsignment rcn1;
		WhsItemReceiveConsignment rcn2;
		WhsItemDispatchConsignment dcn1;
		WhsItemDispatchConsignment dcn2;
		WhsItemReceiveTransportationUnit rtu;
		PkgPackageJob packageJob_RCN;
		PkgPackageJob packageJob_handlingUnit;

		#endregion
	}
}
