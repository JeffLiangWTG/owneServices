using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(WhsItemCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached))]
	class WhsItemCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttachedTest : DbCreateScriptTest
	{
		const string ProcedureName = "dbo.WhsItemCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached";
		const string TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached = "dbo.TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached";
		const string TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer = "dbo.TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer";
		const string expectedError = "Cannot plan to load a Container into another Container.";
		static readonly string WhsItemPackageStateTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemPackageStateSchema.Instance);
		static readonly string WhsItemDispatchLoadListDTUPivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemDispatchLoadListDTUPivotSchema.Instance);

		public void TestContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_ULD_ULD()
		{
			Test_Core("ULD", "ULD");
		}

		public void TestContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_ULD_CNT()
		{
			Test_Core("ULD", "CNT");
		}

		public void TestContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_CNT_ULD()
		{
			Test_Core("CNT", "ULD");
		}

		public void TestContainerCannotBePlannedForLoadListIfItHasAContainerPackageAttached_CNT_CNT()
		{
			Test_Core("CNT", "CNT");
		}

		void Test_Core(string freightUnitType, string containerUnitType)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: freightUnitType).AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: containerUnitType).AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1AttachedToDLL = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, freightDTU.PK, whs, dll: dll);
				var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, containerDTU.PK, whs);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @LoadListDTUPivotPKs"))
				{
					sqlCommand.AddTableValuedParameter("@LoadListDTUPivotPKs", "dbo.TVP_uniqueidentifier", new[] { loadListDTUPivot.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					AssertExceptionThrown(typeof(SqlException), expectedError, () => sqlCommand.ExecuteNonQuery(), true);
				}
			}
		}

		public void TestNoError_PlanULDContainerForALoadListIfItHasNoULDPackageAttached()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package3 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package4 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package5 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "CNT").AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs);

				var handlingUnitAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "BKD", whs, null, isHandlingUnit: true, dll: dll);
				var packageLoadedOntoULDAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4.PK, "FLO", whs, rcn, isHandlingUnit: false, rtu: rtu, dcn: dcn, dtu: dtu, dll: dll);
				var packageAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package5.PK, "BKD", whs, rcn, isHandlingUnit: false, dll: dll);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @LoadListDTUPivotPKs"))
				{
					sqlCommand.AddTableValuedParameter("@LoadListDTUPivotPKs", "dbo.TVP_uniqueidentifier", new[] { loadListDTUPivot.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					AssertNoExceptionThrown(expectedError, () => sqlCommand.ExecuteNonQuery());
				}
			}
		}

		public void TestCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached_HasActiveULD_ThrowError()
		{
			TestCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached_Core(true);
		}

		public void TestCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached_HasNoActiveULD_NoError()
		{
			TestCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached_Core(false);
		}

		public void TestCheckULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached_Core(bool isContainerizedDTUActive)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1AttachedToDLL = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, freightDTU.PK, whs, dll: dll, isContainerizedDTUActive: isContainerizedDTUActive);
				var uldHandlingUnitForDTU2 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, containerDTU.PK, whs, isContainerizedDTUActive: isContainerizedDTUActive);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @LoadListDTUPivotPKs"))
				{
					sqlCommand.AddTableValuedParameter("@LoadListDTUPivotPKs", "dbo.TVP_uniqueidentifier", new[] { loadListDTUPivot.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					if (isContainerizedDTUActive)
					{
						AssertExceptionThrown(typeof(SqlException), expectedError, () => sqlCommand.ExecuteNonQuery(), true);
					}
					else
					{
						AssertNoExceptionThrown(expectedError, () => sqlCommand.ExecuteNonQuery());
					}
				}
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
			dtu = new WhsItemDispatchTransportationUnit(whs, "dtu").AppendInsertAndReturnObject(sql);
			dll = new WhsItemDispatchLoadList("dll", whs).AppendInsertAndReturnObject(sql);
			packageJob_RCN = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		WhsWarehouse whs;
		WhsLocation location;
		WhsItemReceiveConsignment rcn;
		WhsItemDispatchConsignment dcn;
		WhsItemReceiveTransportationUnit rtu;
		WhsItemDispatchTransportationUnit dtu;
		WhsItemDispatchLoadList dll;
		PkgPackageJob packageJob_RCN;
	}
}
