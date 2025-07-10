using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(WhsItemCheckULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer))]
	class WhsItemCheckULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainerTest : DbCreateScriptTest
	{
		const string ProcedureName = "dbo.WhsItemCheckULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer";
		const string TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer = "dbo.TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer";
		const string TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached = "dbo.TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached";
		const string expectedError = "Cannot plan to load a Container into another Container.";
		static readonly string WhsItemPackageStateTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemPackageStateSchema.Instance);
		static readonly string WhsItemDispatchLoadListDTUPivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(WhsItemDispatchLoadListDTUPivotSchema.Instance);

		public void TestCannotAttachContainerToLoadListWhenItHasAPlannedContainer_ULD_ULD()
		{
			Test_Core("ULD", "ULD");
		}

		public void TestCannotAttachContainerToLoadListWhenItHasAPlannedContainer_ULD_CNT()
		{
			Test_Core("ULD", "CNT");
		}

		public void TestCannotAttachContainerToLoadListWhenItHasAPlannedContainer_CNT_ULD()
		{
			Test_Core("CNT", "ULD");
		}

		public void TestCannotAttachContainerToLoadListWhenItHasAPlannedContainer_CNT_CNT()
		{
			Test_Core("CNT", "CNT");
		}

		void Test_Core(string freightUnitType, string containerUnitType)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: freightUnitType).AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: containerUnitType).AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs);
				var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU.PK, whs, dll: dll);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @PackageStatePKs"))
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", new[] { uldHandlingUnitForDTU2ToBeAttached.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					AssertExceptionThrown(typeof(SqlException), expectedError, () => sqlCommand.ExecuteNonQuery(), true);
				}
			}
		}

		public void TestNoError_AttachULDToLoadListWhenItDoesNotHaveAPlannedULD()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package3 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package4 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package5 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "CNT").AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs);
				var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU.PK, whs);

				var handlingUnitAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "BKD", whs, null, isHandlingUnit: true, dll: dll);
				var packageLoadedOntoULDAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4.PK, "FLO", whs, rcn, isHandlingUnit: false, rtu: rtu, dcn: dcn, dtu: dtu1, dll: dll);
				var packageAttachedToDLL = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package5.PK, "BKD", whs, rcn, isHandlingUnit: false, dll: dll);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @PackageStatePKs"))
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", new[] { uldHandlingUnitForDTU2ToBeAttached.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					AssertNoExceptionThrown(expectedError, () => sqlCommand.ExecuteNonQuery());
				}
			}
		}

		public void TestNoError_LoadListIsEmpty()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var uldPackage1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var uldPackage2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "CNT").AppendInsertAndReturnObject(sql);

				var uldPackageStateToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, uldPackage1.PK, freightDTU.PK, whs);
				var uldPackageStateForPlannedDTU = TestWhsDataSetupHelper.CreateContainerPackageState(sql, uldPackage2.PK, containerDTU.PK, whs);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @PackageStatePKs"))
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", new[] { uldPackageStateToBeAttached.PK });
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == expectedError);
					AssertNoExceptionThrown(expectedError, () => sqlCommand.ExecuteNonQuery());
				}
			}
		}

		public void TestCheckULDPackageAttachedToLoadList_ULDIsAcitve_ThrowError()
		{
			CheckULDPackageAttachedToLoadList_ULDIsAcitve_Core(true);
		}

		public void TestCheckULDPackageAttachedToLoadList_ULDIsDeacitve_NoError()
		{
			CheckULDPackageAttachedToLoadList_ULDIsAcitve_Core(false);
		}

		public void CheckULDPackageAttachedToLoadList_ULDIsAcitve_Core(bool isContainerizedDTUActive)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached, WhsItemDispatchLoadListDTUPivotTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer, WhsItemPackageStateTableNameForSQL, TestConnection))
			{
				var sql = SetupTestData();

				var package1 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob_RCN, "PLT", 1).AppendInsertAndReturnObject(sql);

				var freightDTU = new WhsItemDispatchTransportationUnit(whs, "freightDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);
				var containerDTU = new WhsItemDispatchTransportationUnit(whs, "containerDTU", unitType: "ULD").AppendInsertAndReturnObject(sql);

				var uldHandlingUnitForDTU1 = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package1.PK, containerDTU.PK, whs, isContainerizedDTUActive: isContainerizedDTUActive);
				var uldHandlingUnitForDTU2ToBeAttached = TestWhsDataSetupHelper.CreateContainerPackageState(sql, package2.PK, freightDTU.PK, whs, dll: dll, isContainerizedDTUActive: isContainerizedDTUActive);

				var loadListDTUPivot = new WhsItemDispatchLoadListDTUPivot(dll, containerDTU).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());

				using (var sqlCommand = Db.Connection.Command($"EXEC {ProcedureName} @PackageStatePKs"))
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", new[] { uldHandlingUnitForDTU2ToBeAttached.PK });
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
}
