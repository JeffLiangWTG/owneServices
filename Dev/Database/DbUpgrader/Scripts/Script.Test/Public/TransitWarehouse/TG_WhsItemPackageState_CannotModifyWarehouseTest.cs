using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemPackageState_CannotModifyWarehouse))]
	class TG_WhsItemPackageState_CannotModifyWarehouseTest : DBCreateTriggerScriptTest
	{
		class Data
		{
			public WhsWarehouse Warehouse { get; set; }
			public WhsWarehouse NewWarehouse { get; set; }
			public PkgPackage Package { get; set; }
			public PkgPackage Package1 { get; set; }
			public PkgPackageJob PackageJob { get; set; }
			public WhsItemPackageState PackageState { get; set; }
			public WhsItemReceiveConsignmentOld_V02 RCN { get; set; }
			public WhsItemDispatchConsignment DCN { get; set; }
			public WhsItemReceiveTransportationUnit RTU { get; set; }
			public WhsItemDispatchLoadList DLL { get; set; }
			public WhsItemDispatchTransportationUnit DTU { get; set; }

			public WhsArea area { get; set; }
			public WhsRow row { get; set; }
			public WhsLocation locationA { get; set; }
			public DateTimeOffset unloadedTime { get; set; }
			public DateTimeOffset loadedTime { get; set; }
			public WhsItemReceiveASN ASN { get; set; }
			public DateTime time1 { get; set; }
		}

		Data CreateAndSetupData(SqlQueryBuilder sql)
		{
			Data data = new Data();
			data.Warehouse = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			data.NewWarehouse = new WhsWarehouse("WH2", "TRW", branch.PK).WithDockDoor(TestConnection);
			data.time1 = new DateTime(2022, 05, 13, 1, 1, 0);
			data.area = new WhsArea(data.Warehouse.PK, "AREA1").AppendInsertAndReturnObject(sql);
			data.row = new WhsRow(data.Warehouse, "A").AppendInsertAndReturnObject(sql);
			data.locationA = new WhsLocation(data.row.PK, data.area.PK, data.area.PK).AppendInsertAndReturnObject(sql);

			data.RCN = new WhsItemReceiveConsignmentOld_V02(data.Warehouse, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			data.DCN = new WhsItemDispatchConsignment(data.Warehouse, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			data.RTU = new WhsItemReceiveTransportationUnit(data.Warehouse, "rtu", data.locationA, "rtu").AppendInsertAndReturnObject(sql);
			data.DLL = new WhsItemDispatchLoadList("dll1", data.Warehouse).AppendInsertAndReturnObject(sql);
			data.DTU = new WhsItemDispatchTransportationUnit(data.Warehouse, "dtu1").AppendInsertAndReturnObject(sql);
			data.PackageJob = new PkgPackageJob(data.RCN.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var packageHeader = new PkgPackageHeader("P1", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
			var packageHeader2 = new PkgPackageHeader("P2", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
			data.Package = new PkgPackage(data.PackageJob, packageHeader, "PLT", 1)
			{
				KP_SystemCreateTimeUtc = data.time1,
				KP_Weight = 1,
				KP_WeightUQ = "KG",
				KP_Length = 2,
				KP_Width = 3,
				KP_Height = 4,
				KP_DimensionUQ = "M"
			}.AppendInsertAndReturnObject(sql);
			data.Package1 = new PkgPackage(data.PackageJob, packageHeader2, "PLT", 2)
			{
				KP_SystemCreateTimeUtc = data.time1,
				KP_Weight = 1,
				KP_WeightUQ = "KG",
				KP_Length = 2,
				KP_Width = 3,
				KP_Height = 4,
				KP_DimensionUQ = "M"
			}.AppendInsertAndReturnObject(sql);
			data.unloadedTime = new DateTimeOffset(2022, 05, 13, 1, 2, 0, new TimeSpan(10, 0, 0));
			data.loadedTime = new DateTimeOffset(2022, 05, 14, 1, 2, 0, new TimeSpan(10, 0, 0));
			data.ASN = new WhsItemReceiveASN(data.Warehouse, "RC000001", "RC000001").AppendInsertAndReturnObject(sql);
			data.PackageState = new WhsItemPackageState(data.Package.PK, data.Warehouse, data.RCN.PK, "FLO")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WDH_TransitDispatchHeader = data.DTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_LoadedTime = data.loadedTime,
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			}.AppendInsertAndReturnObject(sql);
			return data;
		}

		public void TestTrigger_WPS_WRH_TransitReceiveHeader_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another RTU that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var area = new WhsArea(data.NewWarehouse.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(data.NewWarehouse, "A").AppendInsertAndReturnObject(sql);
			var locationA = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(data.NewWarehouse, "rtu1", locationA, "rtu1").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRH_TransitReceiveHeader, rtu.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WRH_TransitReceiveHeader_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var area = new WhsArea(data.Warehouse.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(data.Warehouse, "B").AppendInsertAndReturnObject(sql);
			var locationB = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(data.Warehouse, "rtu1", locationB, "rtu1").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRH_TransitReceiveHeader, rtu.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WRC_TransitReceiveConsignment_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another RCN that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var rcn = new WhsItemReceiveConsignment(data.NewWarehouse, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRC_TransitReceiveConsignment, rcn.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WRC_TransitReceiveConsignment_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var rcn = new WhsItemReceiveConsignment(data.Warehouse, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRC_TransitReceiveConsignment, rcn.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WDC_TransitDispatchConsignment_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DCN that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var dcn = new WhsItemDispatchConsignment(data.NewWarehouse, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDC_TransitDispatchConsignment, dcn.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WDC_TransitDispatchConsignment_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var dcn = new WhsItemDispatchConsignment(data.Warehouse, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDC_TransitDispatchConsignment, dcn.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WDH_TransitDispatchHeader_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DTU that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var dtu = new WhsItemDispatchTransportationUnit(data.NewWarehouse, "dtu2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDH_TransitDispatchHeader, dtu.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WDH_TransitDispatchHeader_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var dtu = new WhsItemDispatchTransportationUnit(data.Warehouse, "dtu2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDH_TransitDispatchHeader, dtu.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WRP_ReceiveExpectedPacking_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another ASN that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var dtu = new WhsItemReceiveASN(data.NewWarehouse, "RC000002", "RC000002").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRP_ReceiveExpectedPacking, dtu.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WRP_ReceiveExpectedPacking_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var dtu = new WhsItemReceiveASN(data.Warehouse, "RC000002", "RC000002").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WRP_ReceiveExpectedPacking, dtu.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WDL_LoadList_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DLL that in different warehouse.";
			var data = CreateAndSetupData(sql);
			var dll = new WhsItemDispatchLoadList("dll2", data.NewWarehouse).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDL_LoadList, dll.PK).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WDL_LoadList_NoChange()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateAndSetupData(sql);
			var dll = new WhsItemDispatchLoadList("dll2", data.Warehouse).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WDL_LoadList, dll.PK).AsSQL();
			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
		}

		public void TestTrigger_WPS_WW_Warehouse_Changed()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to change warehouse.";
			var data = CreateAndSetupData(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var updateSql = WhsItemPackageState.UpdateWhere(data.PackageState.PK).Set(p => p.WPS_WW_Warehouse, data.NewWarehouse).AsSQL();
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		public void TestTrigger_WPS_WRH_TransitReceiveHeader_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another RTU that in different warehouse.";
			var data = CreateAndSetupData(sql);

			var packageState = new WhsItemPackageState(data.Package1.PK, data.NewWarehouse, data.RCN.PK, "FLO")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WDH_TransitDispatchHeader = data.DTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_LoadedTime = data.loadedTime,
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}

		public void TestTrigger_WPS_WRC_TransitReceiveConsignment_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another RCN that in different warehouse.";
			var data = CreateAndSetupData(sql);

			var rcn = new WhsItemReceiveConsignment(data.NewWarehouse, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(data.Package1.PK, data.Warehouse, rcn.PK, "ARV")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}

		public void TestTrigger_WPS_WDC_TransitDispatchConsignment_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DCN that in different warehouse.";
			var data = CreateAndSetupData(sql);

			var dcn = new WhsItemDispatchConsignment(data.NewWarehouse, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(data.Package1.PK, data.Warehouse, data.RCN.PK, "ARV")
			{
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}

		public void TestTrigger_WPS_WDH_TransitDispatchHeader_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DTU that in different warehouse.";
			var data = CreateAndSetupData(sql);
		
			var dtu = new WhsItemDispatchTransportationUnit(data.NewWarehouse, "dtu2").AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(data.Package1.PK, data.Warehouse, data.RCN.PK, "FIN")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WDH_TransitDispatchHeader = dtu.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_LoadedTime = data.loadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}

		public void TestTrigger_WRP_WDC_ReceiveExpectedPacking_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another ASN that in different warehouse.";
			var data = CreateAndSetupData(sql);

			var asn = new WhsItemReceiveASN(data.NewWarehouse, "RC000002", "RC000002").AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(data.Package1.PK, data.Warehouse, data.RCN.PK, "FIN")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = data.DLL.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WDH_TransitDispatchHeader = data.DTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_LoadedTime = data.loadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = asn.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}

		public void TestTrigger_WDL_LoadList_Insert()
		{
			var sql = new SqlQueryBuilder();
			const string ExpectedErrorMessage = "Attempt to attach package state into another DLL that in different warehouse.";
			var data = CreateAndSetupData(sql);
			
			var dll = new WhsItemDispatchLoadList("dll2", data.NewWarehouse).AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(data.Package1.PK, data.Warehouse, data.RCN.PK, "FIN")
			{
				WPS_WDC_TransitDispatchConsignment = data.DCN.PK,
				WPS_WDL_LoadList = dll.PK,
				WPS_WRH_TransitReceiveHeader = data.RTU.PK,
				WPS_WDH_TransitDispatchHeader = data.DTU.PK,
				WPS_WL_LastLocation = data.locationA.PK,
				WPS_UnloadedTime = data.unloadedTime,
				WPS_UnloadedNotYetProcessedTime = data.unloadedTime,
				WPS_LoadedTime = data.loadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = data.time1,
				WPS_WRP_ReceiveExpectedPacking = data.ASN.PK
			};

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), ExpectedErrorMessage, () => TestConnection.ExecuteNonQuery(packageState.GetInsertStatement()), true);
		}
	}
}
