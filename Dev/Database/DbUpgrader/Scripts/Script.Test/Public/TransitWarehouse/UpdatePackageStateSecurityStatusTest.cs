using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Definitions;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse.UpdatePackageStateSecurityStatus))]
	class UpdatePackageStateSecurityStatusTest : DbCreateScriptTest
	{
		const string TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);

		public void TestUpdateStandalonePackageSecurityStatusNotUpdatePackage_WhenSecurityStatusIsOVR()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "OVR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("OVR", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState1.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSCR_WhenLatestScreeningResultIsPassed()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);
			var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-1) }.AppendInsertAndReturnObject(BasicData.SQL);
			var childPkgPackageScreening3 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "", KPS_Time = DateTime.Now }.AppendInsertAndReturnObject(BasicData.SQL);

			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			var childPkgPackageScreening4 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);
			var childPkgPackageScreening5 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-1) }.AppendInsertAndReturnObject(BasicData.SQL);
			var childPkgPackageScreening6 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "", KPS_Time = DateTime.Now }.AppendInsertAndReturnObject(BasicData.SQL);
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("NOT", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsNull_WPSIsSecure_RegistryValueIsFalse_LastScreeningIsFailed()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = false, KPS_FailureReason = "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSEC_WhenTransitSecurityProcessingRequired_TransportModeIsNull_WPSIsSecure_RegistryValueIsFalse_NoLastScreening()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("SEC", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsNull_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsNull_WPSIsNotSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsNull_WPSIsNotSecure_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSEC_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_WPSIsSecure_RegistryValueIsFalse()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("SEC", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_WPSIsNotSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_WPSIsNotSecure_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSEC_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_WPSIsSecure_RegistryValueIsFalse()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("SEC", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_NoRCNDirection_NoDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore("", "");
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_NoRCNDirection_IMPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore("", TransitWarehouseConsignmentDirections.Codes.Import);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_NoRCNDirection_DOMDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore("", TransitWarehouseConsignmentDirections.Codes.Domestic);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_NoRCNDirection_EXPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore("", TransitWarehouseConsignmentDirections.Codes.Export);

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_IMPRCNDirection_NoDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Import, "");
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_IMPRCNDirection_IMPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Import);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_IMPRCNDirection_DOMDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Domestic);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_IMPRCNDirection_EXPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Export);

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_DOMRCNDirection_NoDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Domestic, "");
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_DOMRCNDirection_IMPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Import);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_DOMRCNDirection_DOMDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Domestic);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_DOMRCNDirection_EXPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Export);

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_EXPRCNDirection_NoDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Export, "");
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_EXPRCNDirection_IMPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Import);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_EXPRCNDirection_DOMDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Domestic);
		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_EXPRCNDirection_EXPDCNDirection() =>
			TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Export);

		void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_RegistryValueIsTrue_WithConsignmentDirectionsCore(string rcnDirection, string dcnDirection)
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true, rcn1Direction: rcnDirection, dcn1Direction: dcnDirection);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);

			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR")
			{
				WPS_WDL_LoadList = dll1.PK,
				WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK,
				WPS_WL_LastLocation = BasicData.Location1.PK,
				WPS_UnloadedTime = BasicData.UnloadedTime,
				WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = BasicData.Time,
				WPS_WDC_TransitDispatchConsignment = basicData.DCN1.PK
			};
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR")
			{
				WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK,
				WPS_WL_LastLocation = BasicData.Location1.PK,
				WPS_UnloadedTime = BasicData.UnloadedTime,
				WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_SystemLastEditTimeUtc = BasicData.Time,
			};
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var relevantConsignmentDirections = new string[] { TransitWarehouseConsignmentDirections.Codes.Export, string.Empty };
			var expectedSecurityStatus = "NOT";

			if (relevantConsignmentDirections.Contains(dcnDirection))
			{
				expectedSecurityStatus = "REQ";
			}

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals($"for rcn direction \"{rcnDirection}\" and dcn direction \"{dcnDirection}\" - security status should be {expectedSecurityStatus}",
				expectedSecurityStatus, standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			expectedSecurityStatus = "NOT";
			if (relevantConsignmentDirections.Contains(rcnDirection))
			{
				expectedSecurityStatus = "REQ";
			}

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals($"for rcn direction \"{rcnDirection}\" and without dcn direction - security status should be {expectedSecurityStatus}",
				expectedSecurityStatus, standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState2.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_WPSIsNotSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToREQ_WhenTransitSecurityProcessingRequired_TransportModeIsAIR_WPSIsNotSecure_RegistryValueIsTrue()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("REQ", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_WPSIsSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: false);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("NOT", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_WPSIsNotSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: false);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("NOT", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransportModeIsSEA_WPSIsSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = true;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = true;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("NOT", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransportModeIsSEA_WPSIsNotSecure()
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.WPS_IsSecure = false;
			packageState1.WPS_IsHandlingUnit = false;
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.WPS_IsSecure = false;
			packageState2.WPS_IsHandlingUnit = false;
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("NOT", standalonePackageState1.WPS_SecurityStatus);
			AssertEquals("TST", standalonePackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, standalonePackageState1.WPS_SystemLastEditTimeUtc);

			var standalonePackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals("SCR", standalonePackageState2.WPS_SecurityStatus);
			AssertEquals("A", standalonePackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, standalonePackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToREQ()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var scrPackageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState1 = new WhsItemPackageState(secPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState1.WPS_IsSecure = true;
				secPackageState1.WPS_IsHandlingUnit = false;
				secPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState2 = new WhsItemPackageState(secPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState2.WPS_IsSecure = true;
				secPackageState2.WPS_IsHandlingUnit = false;
				secPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll3 = new WhsItemDispatchLoadList("dll3", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var notPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var notPackageState1 = new WhsItemPackageState(notPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll3.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				notPackageState1.WPS_IsSecure = false;
				notPackageState1.WPS_IsHandlingUnit = false;
				notPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll4 = new WhsItemDispatchLoadList("dll4", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var notPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var notPackageState2 = new WhsItemPackageState(notPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll4.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				notPackageState2.WPS_IsSecure = false;
				notPackageState2.WPS_IsHandlingUnit = false;
				notPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll5 = new WhsItemDispatchLoadList("dll5", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
				var reqPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var reqpackageState1 = new WhsItemPackageState(reqPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll5.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				reqpackageState1.WPS_IsSecure = false;
				reqpackageState1.WPS_IsHandlingUnit = false;
				reqpackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll6 = new WhsItemDispatchLoadList("dll6", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
				var reqPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var reqpackageState2 = new WhsItemPackageState(reqPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll6.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				reqpackageState2.WPS_IsSecure = false;
				reqpackageState2.WPS_IsHandlingUnit = false;
				reqpackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("REQ", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedTopLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU2.PK).FirstOrDefault();
				AssertEquals("SCR", updatedTopLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedTopLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedTopLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToNOT()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var scrPackageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState1 = new WhsItemPackageState(secPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState1.WPS_IsSecure = true;
				secPackageState1.WPS_IsHandlingUnit = false;
				secPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState2 = new WhsItemPackageState(secPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState2.WPS_IsSecure = true;
				secPackageState2.WPS_IsHandlingUnit = false;
				secPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll3 = new WhsItemDispatchLoadList("dll3", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var notPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var notPackageState1 = new WhsItemPackageState(notPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll3.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				notPackageState1.WPS_IsSecure = false;
				notPackageState1.WPS_IsHandlingUnit = false;
				notPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll4 = new WhsItemDispatchLoadList("dll4", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var notPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var notPackageState2 = new WhsItemPackageState(notPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll4.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				notPackageState2.WPS_IsSecure = false;
				notPackageState2.WPS_IsHandlingUnit = false;
				notPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("NOT", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedTopLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU2.PK).FirstOrDefault();
				AssertEquals("SCR", updatedTopLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedTopLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedTopLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToSEC()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
				InsertRegistryWithFalseValue(BasicData.Whs1);
				InsertRegistryWithFalseValue(BasicData.Whs2);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var scrPackageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState1 = new WhsItemPackageState(secPkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState1.WPS_IsSecure = true;
				secPackageState1.WPS_IsHandlingUnit = false;
				secPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var secPkgPackage2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var secPackageState2 = new WhsItemPackageState(secPkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				secPackageState2.WPS_IsSecure = true;
				secPackageState2.WPS_IsHandlingUnit = false;
				secPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("SEC", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedTopLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU2.PK).FirstOrDefault();
				AssertEquals("SCR", updatedTopLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedTopLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedTopLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToOVR_WhenItOnlyHasOVRChildPakcages()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1);

				var pkgPackage = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, pkgPackage.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var ovrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "OVR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var ovrPackageState2 = new WhsItemPackageState(pkgPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "OVR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("OVR", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopHandlingUnitSecurityStatus_WhenItHasOVRChildPakcage()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1);

				var pkgPackage = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, pkgPackage.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var ovrPackageState1 = new WhsItemPackageState(pkgPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "OVR") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("SCR", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToSCR()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var scrPackageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals("SCR", updatedTopLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedTopLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU2.PK).FirstOrDefault();
				AssertEquals("NOT", updatedTopLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedTopLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedTopLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateTopOverpackSecurityStatusToREQ_WhenOverpackHasNoScreening_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: false, isLatestScreeningPassed: false, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "REQ");
		}

		public void TestUpdateTopOverpackSecurityStatusToREQ_WhenOverpackHasNoScreening_AllChildrenAreOverridden_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: false, isLatestScreeningPassed: false, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: true, "REQ");
		}

		public void TestUpdateTopOverpackSecurityStatusToREQ_WhenOverpackHasFailedLatestScreening_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: false, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "REQ");
		}

		public void TestUpdateTopOverpackSecurityStatusToREQ_WhenOverpackHasFailedlatestScreening_ScreeningTimeEarlierThanPackingTime_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: false, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "REQ");
		}

		public void TestUpdateTopOverpackSecurityStatusToREQ_WhenOverpackHasPassedLatestScreening_ScreeningTimeEarlierThanPackingTime_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "REQ");
		}

		public void TestUpdateTopOverpackSecurityStatusToSCR_WhenOverpackHasPassedLatestScreening_ScreeningTimeIsLaterThanPackingTime_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "SCR");
		}

		public void TestUpdateTopOverpackSecurityStatusToNOT_WhenOverpackHasPassedLatestScreening_ScreeningTimeIsLaterThanPackingTime_NotHighRisk_TransitSecurityProcessingNotRequired()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "NOT", transitSecurityProcessingRequired: false);
		}

		public void TestUpdateTopOverpackSecurityStatusToSCR_WhenOverpackHasPassedLatestScreening_FailedLastSecondScreening_ScreeningTimeIsLaterThanPackingTime_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "SCR");
		}

		public void TestUpdateTopOverpackSecurityStatusToSCR_WhenOverpackHasPassedLatestScreening_ScreeningTimeIsLaterThanPackingTime_AllChildrenAreOverridden_NotHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: false, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: true, "SCR");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRS_WhenOverpackHasNoScreening_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: false, isLatestScreeningPassed: false, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "HRS");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRS_WhenOverpackHasPassedLatestScreening_FailedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LastSecondScreeningTimeIsLaterThanPackingTime_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: false, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "HRS", "ME1", "ME2");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRS_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LatestScreeningTimeIsEarlierThanPackingTime_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: false, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "HRS", "ME1", "ME2");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRS_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LatestScreeningTimeIsLaterThanPackingTime_LastSecondScreeningTimeIsEarlierThanPackingTime_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: false, areAllChildPackagesOverridden: false, "HRS", "ME1", "ME2");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRS_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingSameScreeningMethodForScreening_LastSecondScreeningTimeIsLaterThanPackingTime_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "HRS", "ME1", "ME1");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRN_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LastSecondScreeningTimeIsLaterThanPackingTime_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "HRN", "ME1", "ME2");
		}

		public void TestUpdateTopOverpackSecurityStatusToHRN_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LastSecondScreeningTimeIsLaterThanPackingTime_AllChildrenAreOverridden_IsHighRisk()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: true, "HRN", "ME1", "ME2");
		}

		public void TestUpdateTopOverpackSecurityStatusToSCR_WhenOverpackHasPassedLatestScreening_PassedLastSecondScreening_UsingDifferentScreeningMethodsForScreening_LastSecondScreeningTimeIsLaterThanPackingTime_IsHighRisk_isHighRiskAuthorized()
		{
			TestUpdateTopOverpackSecurityStatusWithScreening_Core(isHighRisk: true, hasScreening: true, isLatestScreeningPassed: true, isLastScreeningTimeLaterThanLastPackingTime: true, isLastSecondScreenPassed: true, isLastSecondScreenTimeLaterThanLastPackingTime: true, areAllChildPackagesOverridden: false, "SCR", "ME1", "ME2", isHighRiskAuthorized: true);
		}

		void TestUpdateTopOverpackSecurityStatusWithScreening_Core(bool isHighRisk, bool hasScreening, bool isLatestScreeningPassed, bool isLastScreeningTimeLaterThanLastPackingTime, bool isLastSecondScreenPassed, bool isLastSecondScreenTimeLaterThanLastPackingTime, bool areAllChildPackagesOverridden, string expectedSecurityStatus, string lastScreeningMethod = "ME1", string lastSecondScreeningMethod = "ME2", bool isHighRiskAuthorized = false, bool transitSecurityProcessingRequired = true)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: transitSecurityProcessingRequired);

				var overpackPackage = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var overpackPackageState = new WhsItemPackageState(overpackPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_IsHandlingUnit = true, WPS_UnitType = "OVP", WPS_SystemLastEditTimeUtc = BasicData.Time };

				var subLeveloverpackPackage = new PkgPackage(BasicData.PackageJob1, overpackPackage, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var subLeveloverpackPackageState = new WhsItemPackageState(subLeveloverpackPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_IsHandlingUnit = true, WPS_UnitType = "OVP", WPS_SystemLastEditTimeUtc = BasicData.Time };

				var divot1 = new PkgPackageHandlingUnitDivot(overpackPackage.PK, subLeveloverpackPackage.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(overpackPackage, null, overpackPackage);
				overpackPackageState.WPS_WL_LastLocation = BasicData.Location1.PK;
				subLeveloverpackPackageState.WPS_WL_LastLocation = BasicData.Location1.PK;
				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", areAllChildPackagesOverridden ? "OVR" : "REQ") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var packageState3 = new WhsItemPackageState(BasicData.PkgPackage3.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", areAllChildPackagesOverridden ? "OVR" : "REQ") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };

				var divot2 = new PkgPackageHandlingUnitDivot(subLeveloverpackPackage.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(subLeveloverpackPackage.PK, BasicData.PkgPackage3.PK).AppendInsertAndReturnObject(BasicData.SQL);

				if (isHighRisk)
				{
					overpackPackageState.WPS_IsHighRisk = true;
					packageState1.WPS_IsHighRisk = true;
					packageState3.WPS_IsHighRisk = true;
				}

				if (isHighRiskAuthorized)
				{
					overpackPackageState.WPS_IsHighRiskAuthorized = true;
					packageState1.WPS_IsHighRiskAuthorized = true;
					packageState3.WPS_IsHighRiskAuthorized = true;
				}

				if (hasScreening)
				{
					var packedTime = divot2.KPD_PackedTime;
					var overpackLastScreening = new PkgPackageScreening(overpackPackage) { KPS_Passed = isLatestScreeningPassed, KPS_Method = lastScreeningMethod, KPS_FailureReason = "", KPS_Time = isLastScreeningTimeLaterThanLastPackingTime ? packedTime.Value.AddHours(2) : packedTime.Value.AddHours(-1) }.AppendInsertAndReturnObject(BasicData.SQL);
					var overpackLastSecondScreening = new PkgPackageScreening(overpackPackage) { KPS_Passed = isLastSecondScreenPassed, KPS_Method = lastSecondScreeningMethod, KPS_FailureReason = "", KPS_Time = isLastSecondScreenTimeLaterThanLastPackingTime ? packedTime.Value.AddHours(1) : packedTime.Value.AddHours(-2) }.AppendInsertAndReturnObject(BasicData.SQL);

					var packageState1LastScreening = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = isLatestScreeningPassed, KPS_Method = lastScreeningMethod, KPS_FailureReason = "", }.AppendInsertAndReturnObject(BasicData.SQL);
					var packageState1LastSecondScreening = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = isLastSecondScreenPassed, KPS_Method = lastSecondScreeningMethod, KPS_FailureReason = "", }.AppendInsertAndReturnObject(BasicData.SQL);
					var packageState2LastScreening = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = isLatestScreeningPassed, KPS_Method = lastScreeningMethod, KPS_FailureReason = "", }.AppendInsertAndReturnObject(BasicData.SQL);
					var packageState2LastSecondScreening = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = isLastSecondScreenPassed, KPS_Method = lastSecondScreeningMethod, KPS_FailureReason = "", }.AppendInsertAndReturnObject(BasicData.SQL);
				}

				overpackPackageState.AppendInsertAndReturnObject(BasicData.SQL);
				subLeveloverpackPackageState.AppendInsertAndReturnObject(BasicData.SQL);
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);
				packageState3.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedOverpackPackageState = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == overpackPackageState.WPS_KP_Package).FirstOrDefault();
				var updatedSubLevelOverpackPackageState = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == subLeveloverpackPackageState.WPS_KP_Package).FirstOrDefault();
				AssertEquals(expectedSecurityStatus, updatedOverpackPackageState.WPS_SecurityStatus);
				AssertEquals(expectedSecurityStatus == "NOT" ? "NOT" : "REQ", updatedSubLevelOverpackPackageState.WPS_SecurityStatus);
				AssertEquals("TST", updatedOverpackPackageState.WPS_SystemLastEditUser);
				AssertEquals("TST", updatedSubLevelOverpackPackageState.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedOverpackPackageState.WPS_SystemLastEditTimeUtc);
				AssertNotEquals(BasicData.Time, updatedSubLevelOverpackPackageState.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateOverpackSecurityStatus_WhenItDoesNotHaveChildPackage()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
				BasicData.SetUpStandalonePackageTestData();

				var overpackPackage1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var overpackPackageState1 = new WhsItemPackageState(overpackPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_IsHandlingUnit = true, WPS_UnitType = "OVP", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_WL_LastLocation = BasicData.Location1.PK }.AppendInsertAndReturnObject(BasicData.SQL);

				var overpackPackage2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var overpackPackageState2 = new WhsItemPackageState(overpackPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV") { WPS_IsHandlingUnit = true, WPS_UnitType = "OVP", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_WL_LastLocation = BasicData.Location2.PK }.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedOverpackPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == overpackPackage1.PK).FirstOrDefault();
				AssertEquals("REQ", updatedOverpackPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedOverpackPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedOverpackPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedOverpackPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == overpackPackage2.PK).FirstOrDefault();
				AssertEquals("SEC", updatedOverpackPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedOverpackPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedOverpackPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidLevelHandlingUnitSecurityStatusToREQ()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHU2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState2 = new WhsItemPackageState(middleLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, middleLevelHU2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot4 = new PkgPackageHandlingUnitDivot(middleLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState1.WPS_IsSecure = false;
				packageState1.WPS_IsHandlingUnit = false;
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
				var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState2.WPS_IsSecure = false;
				packageState2.WPS_IsHandlingUnit = false;
				packageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedmiddleLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU1.PK).First();
				AssertEquals("REQ", updatedmiddleLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedmiddleLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedmiddleLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedmiddleLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU2.PK).First();
				AssertEquals("SCR", updatedmiddleLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedmiddleLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedmiddleLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidLevelHandlingUnitSecurityStatusToNOT()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHU2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState2 = new WhsItemPackageState(middleLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, middleLevelHU2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot4 = new PkgPackageHandlingUnitDivot(middleLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState1.WPS_IsSecure = false;
				packageState1.WPS_IsHandlingUnit = false;
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "SEA" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState2.WPS_IsSecure = false;
				packageState2.WPS_IsHandlingUnit = false;
				packageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedmiddleLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU1.PK).First();
				AssertEquals("NOT", updatedmiddleLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedmiddleLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedmiddleLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedmiddleLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU2.PK).First();
				AssertEquals("SCR", updatedmiddleLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedmiddleLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedmiddleLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidLevelHandlingUnitSecurityStatusToSEC()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);
				InsertRegistryWithFalseValue(BasicData.Whs1);
				InsertRegistryWithFalseValue(BasicData.Whs2);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHU2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState2 = new WhsItemPackageState(middleLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, middleLevelHU2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot4 = new PkgPackageHandlingUnitDivot(middleLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState1.WPS_IsSecure = true;
				packageState1.WPS_IsHandlingUnit = false;
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "STA", "SCR") { WPS_WDL_LoadList = dll2.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState2.WPS_IsSecure = true;
				packageState2.WPS_IsHandlingUnit = false;
				packageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedmiddleLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU1.PK).First();
				AssertEquals("SEC", updatedmiddleLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedmiddleLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedmiddleLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedmiddleLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU2.PK).First();
				AssertEquals("SCR", updatedmiddleLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedmiddleLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedmiddleLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidLevelHandlingUnitSecurityStatusToSCR()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState2 = new WhsItemPackageState(middleLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, middleLevelHU2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot4 = new PkgPackageHandlingUnitDivot(middleLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var scrPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var scrPackageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV", "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var childPkgPackageScreening2 = new PkgPackageScreening(BasicData.PkgPackage2) { KPS_Passed = true, KPS_FailureReason = "" }.AppendInsertAndReturnObject(BasicData.SQL);
				scrPackageState2.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedmiddleLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU1.PK).First();
				AssertEquals("SCR", updatedmiddleLevelHUPackageState1.WPS_SecurityStatus);
				AssertEquals("TST", updatedmiddleLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedmiddleLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				var updatedmiddleLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU2.PK).First();
				AssertEquals("NOT", updatedmiddleLevelHUPackageState2.WPS_SecurityStatus);
				AssertEquals("A", updatedmiddleLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedmiddleLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		#region PackageStateSecurityStatusForHighRiskPackage

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_Last2ScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "", lastScreeningPassed: false, lastSecondScreeningPassed: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_LastScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "", lastScreeningPassed: false, lastSecondScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_LastSecondScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "", lastScreeningPassed: true, lastSecondScreeningPassed: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_Last2ScreeningPassedWithSameScreeningMethod_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "", lastScreeningMethod: "ME1", lastScreeningPassed: true, lastSecondScreeningMethod: "ME1", lastSecondScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRN_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_Last2ScreeningPassed_HighRiskNotAuthorized_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRN", "TST", transitSecurityProcessingRequired: true, "", lastScreeningPassed: true, lastSecondScreeningPassed: true, isHighRiskAuthorized: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSCR_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_Last2ScreeningPassed_HighRiskAuthorized_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("SCR", "A", transitSecurityProcessingRequired: true, "", lastScreeningPassed: true, lastSecondScreeningPassed: true, isHighRiskAuthorized: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsAir_Last2ScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "AIR", lastScreeningPassed: false, lastSecondScreeningPassed: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsAir_LastScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "AIR", lastScreeningPassed: false, lastSecondScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsAir_LastSecondScreeningNotPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "AIR", lastScreeningPassed: true, lastSecondScreeningPassed: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsAir_Last2ScreeningPassedWithSameScreeningMethod_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "AIR", lastScreeningMethod: "ME1", lastScreeningPassed: true, lastSecondScreeningMethod: "ME1", lastSecondScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToHRN_WhenTransitSecurityProcessingRequired_TransportModeIsAir_Last2ScreeningPassed_HighRiskNotAuthorized_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRN", "TST", transitSecurityProcessingRequired: true, "AIR", lastScreeningPassed: true, lastSecondScreeningPassed: true, isHighRiskAuthorized: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToSCR_WhenTransitSecurityProcessingRequired_TransportModeIsAir_Last2ScreeningPassed_HighRiskAuthorized_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("SCR", "A", transitSecurityProcessingRequired: true, "AIR", lastScreeningPassed: true, lastSecondScreeningPassed: true, isHighRiskAuthorized: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingRequired_TransportModeIsSea_LastScreeningPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: true, "SEA", lastScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingRequired_TransportModeIsSea_LastScreeningNotPassed_IsHighRisk_IsSecure()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: true, "SEA", lastScreeningPassed: false, isSecure: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingRequired_TransportModeIsSea_LastScreeningNotPassed_IsHighRisk_IsSecureFalse()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: true, "SEA", lastScreeningPassed: false, isSecure: false);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_TransportModeIsAir_LastScreeningPassed_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: false, "Air", lastScreeningPassed: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_TransportModeIsAir_LastScreeningNotPassed_IsHighRisk_IsSecure()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: false, "Air", lastScreeningPassed: false, isSecure: true);
		}

		public void TestUpdateStandalonePackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_TransportModeIsAir_LastScreeningNotPassed_IsHighRisk_IsSecureFalse()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: false, "Air", lastScreeningPassed: false, isSecure: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsUnknown_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "", isUnloaded: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToHRS_WhenTransitSecurityProcessingRequired_TransportModeIsAir_IsHighRisk()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("HRS", "TST", transitSecurityProcessingRequired: true, "AIR", isUnloaded: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToNOT_WhenTransitSecurityProcessingRequired_TransportModeIsSea_IsHighRisk_IsSecure()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: true, "SEA", isSecure: true, isUnloaded: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToNOT_WhenTransitSecurityProcessingRequired_TransportModeIsSea_IsHighRisk_IsSecureFalse()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: true, "SEA", isSecure: false, isUnloaded: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_TransportModeIsAir_IsHighRisk_IsSecure()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: false, "Air", isSecure: true, isUnloaded: false);
		}

		public void TestUpdateStandaloneBookedPackageSecurityStatusToNOT_WhenTransitSecurityProcessingNotRequired_TransportModeIsAir_IsHighRisk_IsSecureFalse()
		{
			TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore("NOT", "TST", transitSecurityProcessingRequired: false, "Air", isSecure: false, isUnloaded: false);
		}

		void TestUpdateStandalonePackageSecurityStatus_IsHighRiskCore(string expectedSecurityStatus, string expectedSystemLastEditUser, bool transitSecurityProcessingRequired, string transportMode, bool isSecure = false, string lastScreeningMethod = "ME1", bool lastScreeningPassed = false, string lastSecondScreeningMethod = "ME2", bool lastSecondScreeningPassed = false, bool isHighRiskAuthorized = false, bool isUnloaded = true)
		{
			BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: transitSecurityProcessingRequired);
			BasicData.SetUpStandalonePackageTestData();

			InsertRegistryWithFalseValue(BasicData.Whs1);
			InsertRegistryWithFalseValue(BasicData.Whs2);

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = transportMode }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, isUnloaded ? "ARV" : "BKD", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = isUnloaded ? BasicData.RTU1.PK : null, WPS_WL_LastLocation = isUnloaded ? BasicData.Location1.PK : null, WPS_UnloadedTime = isUnloaded ? BasicData.UnloadedTime : null, WPS_UnloadedNotYetProcessedTime = isUnloaded ? BasicData.UnloadedTime : null, WPS_ReceivedAs = isUnloaded ? "SCN" : null, WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState.WPS_IsHighRisk = true;
			packageState.WPS_IsHighRiskAuthorized = isHighRiskAuthorized;
			packageState.WPS_IsSecure = isSecure;
			packageState.WPS_IsHandlingUnit = false;
			packageState.AppendInsertAndReturnObject(BasicData.SQL);

			var packageScreening1 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = lastSecondScreeningPassed, KPS_Method = lastScreeningMethod, KPS_FailureReason = lastSecondScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-3) }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageScreening2 = new PkgPackageScreening(BasicData.PkgPackage1) { KPS_Passed = lastScreeningPassed, KPS_Method = lastSecondScreeningMethod, KPS_FailureReason = lastScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var standalonePackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState.PK).FirstOrDefault();
			AssertEquals(expectedSecurityStatus, standalonePackageState1.WPS_SecurityStatus);
			AssertEquals(expectedSystemLastEditUser, standalonePackageState1.WPS_SystemLastEditUser);
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToHRS()
		{
			TestUpdateTopLevelHandlingUnitSecurityStatus_IsHighRiskCore("HRS", "TST");
		}

		public void TestUpdateTopHandlingUnitSecurityStatusToHRN()
		{
			TestUpdateTopLevelHandlingUnitSecurityStatus_IsHighRiskCore("HRN", "TST", lastScreeningPassed: true, lastSecondScreeningPassed: true);
		}

		void TestUpdateTopLevelHandlingUnitSecurityStatus_IsHighRiskCore(string expectedSecurityStatus, string expectedSystemLastEditUser, bool lastScreeningPassed = false, bool lastSecondScreeningPassed = false)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUpackageState = new WhsItemPackageState(topLevelHU.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_UnitType = "HU", WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU);

				var reqPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "REQ") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				reqPackageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPkgPackage = new PkgPackage(BasicData.PackageJob1, topLevelHU, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPackageState = new WhsItemPackageState(hrsPkgPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var hrsPkgPackageScreening1 = new PkgPackageScreening(hrsPkgPackage) { KPS_Passed = lastSecondScreeningPassed, KPS_Method = "ME1", KPS_FailureReason = lastSecondScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-3) }.AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPkgPackageScreening2 = new PkgPackageScreening(hrsPkgPackage) { KPS_Passed = lastScreeningPassed, KPS_Method = "ME2", KPS_FailureReason = lastScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);
				hrsPackageState.WPS_IsHighRisk = true;
				hrsPackageState.WPS_IsHandlingUnit = false;
				hrsPackageState.AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU.PK, hrsPkgPackage.PK).AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU.PK).FirstOrDefault();
				AssertEquals(expectedSecurityStatus, updatedTopLevelHUPackageState.WPS_SecurityStatus);
				AssertEquals(expectedSystemLastEditUser, updatedTopLevelHUPackageState.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidHandlingUnitSecurityStatusToHRS()
		{
			TestUpdateMidLevelHandlingUnitSecurityStatus_IsHighRiskCore("HRS", "TST");
		}

		public void TestUpdateMidHandlingUnitSecurityStatusToHRN()
		{
			TestUpdateMidLevelHandlingUnitSecurityStatus_IsHighRiskCore("HRN", "TST", lastScreeningPassed: true, lastSecondScreeningPassed: true);
		}

		void TestUpdateMidLevelHandlingUnitSecurityStatus_IsHighRiskCore(string expectedSecurityStatus, string expectedSystemLastEditUser, bool lastScreeningPassed = false, bool lastSecondScreeningPassed = false)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, transitSecurityProcessingRequired: true);

				var topLevelHU = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUpackageState = new WhsItemPackageState(topLevelHU.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU = new PkgPackage(BasicData.PackageJob1, topLevelHU, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var midLevelHUpackageState = new WhsItemPackageState(middleLevelHU.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "SCR") { WPS_IsHandlingUnit = true, WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_UnitType = "HU" }.AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU.PK, middleLevelHU.PK).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU);

				var reqPackageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV", "REQ") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				reqPackageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPkgPackage = new PkgPackage(BasicData.PackageJob1, topLevelHU, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPackageState = new WhsItemPackageState(hrsPkgPackage.PK, BasicData.Whs1, BasicData.RCN1.PK, "STA", "SCR") { WPS_WDL_LoadList = dll1.PK, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				var hrsPkgPackageScreening1 = new PkgPackageScreening(hrsPkgPackage) { KPS_Passed = lastSecondScreeningPassed, KPS_Method = "ME1", KPS_FailureReason = lastSecondScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-3) }.AppendInsertAndReturnObject(BasicData.SQL);
				var hrsPkgPackageScreening2 = new PkgPackageScreening(hrsPkgPackage) { KPS_Passed = lastScreeningPassed, KPS_Method = "ME2", KPS_FailureReason = lastScreeningPassed ? "" : "OTH", KPS_Time = DateTime.Now.AddMinutes(-2) }.AppendInsertAndReturnObject(BasicData.SQL);
				hrsPackageState.WPS_IsHighRisk = true;
				hrsPackageState.WPS_IsHandlingUnit = false;
				hrsPackageState.AppendInsertAndReturnObject(BasicData.SQL);

				var divot2 = new PkgPackageHandlingUnitDivot(middleLevelHU.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU.PK, hrsPkgPackage.PK).AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedMidLevelHUPackageState = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU.PK).FirstOrDefault();
				AssertEquals(expectedSecurityStatus, updatedMidLevelHUPackageState.WPS_SecurityStatus);
				AssertEquals(expectedSystemLastEditUser, updatedMidLevelHUPackageState.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedMidLevelHUPackageState.WPS_SystemLastEditTimeUtc);
			}
		}

		#endregion

		void RunCheckProcedure(GlbBranch branch, string systemLastEditUser, List<Guid> packageStatePKs = null)
		{
			const string sql = "EXEC dbo.UpdatePackageStateSecurityStatus @companyBranchPK, @systemLastEditUser, @RegistryValue, @WarehouseConfigurationValue, @PackageStatePKs";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddParameter("@companyBranchPK", SqlDbType.UniqueIdentifier, branch.PK);
				sqlCommand.AddParameter("@systemLastEditUser", SqlDbType.Char, systemLastEditUser);
				sqlCommand.AddParameter("@RegistryValue", SqlDbType.Bit, DBNull.Value);
				sqlCommand.AddParameter("@WarehouseConfigurationValue", SqlDbType.Bit, DBNull.Value);

				if (packageStatePKs == null)
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", TVPHelper.TVP_uniqueidentifier, Enumerable.Empty<Guid>());
				}
				else
				{
					sqlCommand.AddTableValuedParameter("@PackageStatePKs", TVPHelper.TVP_uniqueidentifier, packageStatePKs);
				}
				sqlCommand.ExecuteNonQuery();
			}
		}

		void InsertRegistryWithFalseValue(WhsWarehouse whs)
		{
			var sqlCommand = @"
IF NOT EXISTS (SELECT SD_PK FROM dbo.StmData WHERE SD_Name = @SD_Name AND SD_Owner = @SD_Owner)
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_Owner)
VALUES (newid(), @SD_Name, 0x460061006C0073006500, @SD_Owner)
ELSE
	UPDATE dbo.StmData
	SET SD_BinaryValue = 0x460061006C0073006500
	WHERE SD_Name = @SD_Name AND SD_Owner = @SD_Owner
";
			using (var command = Db.Connection.Command(sqlCommand))
			{
				command.AddParameter("@SD_Name", SqlDbType.VarChar, "MandatoryPackageScreeningForAirAndUnknownTransportMode");
				command.AddParameter("@SD_Owner", SqlDbType.UniqueIdentifier, whs.WW_GB_RelatedCompanyBranch);
				command.ExecuteNonQuery();
			}
		}

		#region SetUpBasicTestData

		BasicTestData BasicData => basicData ?? (basicData = new BasicTestData());
		BasicTestData basicData;

		class BasicTestData
		{
			public void SetUpBasicTestData(IDbConnectionWithSettings testConnection, bool transitSecurityProcessingRequired, string rcn1Direction = "", string dcn1Direction = "")
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(testConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(testConnection);
				var whs1 = transitSecurityProcessingRequired
					? new WhsWarehouse("WH1", branch1.PK) { WW_TransitSecurityProcessingRequired = true, WW_WarehouseType = "TRW" }.WithDockDoor(testConnection)
					: new WhsWarehouse("WH1", branch1.PK) { WW_WarehouseType = "TRW" }.WithDockDoor(testConnection);
				var whs2 = transitSecurityProcessingRequired
					? new WhsWarehouse("WH2", branch2.PK) { WW_TransitSecurityProcessingRequired = true, WW_WarehouseType = "TRW" }.WithDockDoor(testConnection)
					: new WhsWarehouse("WH2", branch2.PK) { WW_WarehouseType = "TRW" }.WithDockDoor(testConnection);
				var rcn1 = new WhsItemReceiveConsignment(whs1, "RC000001", "RC000001", "STD", "AUSYD");
				rcn1.WRC_Direction = rcn1Direction;
				rcn1.AppendInsertAndReturnObject(sql);
				var rcn2 = new WhsItemReceiveConsignment(whs2, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

				var dcn1 = new WhsItemDispatchConsignment(whs1, "DC000001", "DC000001", "STD");
				dcn1.WDC_Direction = dcn1Direction;
				dcn1.AppendInsertAndReturnObject(sql);
				var dcn2 = new WhsItemDispatchConsignment(whs2, "DC000002", "DC000002", "STD").AppendInsertAndReturnObject(sql);

				var packageJob1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var packageJob2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);

				var systemLastEditUser = "TST";
				var time = new DateTime(2023, 05, 24, 1, 1, 0);

				SQL = sql;
				Whs1 = whs1;
				Whs2 = whs2;
				RCN1 = rcn1;
				RCN2 = rcn2;
				DCN1 = dcn1;
				DCN2 = dcn2;
				Branch1 = branch1;
				Branch2 = branch2;
				PackageJob1 = packageJob1;
				PackageJob2 = packageJob2;
				SystemLastEditUser = systemLastEditUser;
				Time = time;
			}

			public void SetUpStandalonePackageTestData(PkgPackage topLevelHU1 = null, PkgPackage topLevelHU2 = null, PkgPackage topLevelHU3 = null)
			{
				var unloadedTime = new DateTimeOffset(2023, 05, 24, 13, 59, 0, new TimeSpan(10, 0, 0));
				var area1 = new WhsArea(Whs1.PK, "AREA1").AppendInsertAndReturnObject(SQL);
				var row1 = new WhsRow(Whs1, "A").AppendInsertAndReturnObject(SQL);
				var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK).AppendInsertAndReturnObject(SQL);
				var rtu1 = new WhsItemReceiveTransportationUnit(Whs1, "rtu1", location1, "rtu1").AppendInsertAndReturnObject(SQL);
				var pkgPackage1 = topLevelHU1 is null ? new PkgPackage(PackageJob1, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(SQL);
				var pkgPackage3 = topLevelHU3 is null ? new PkgPackage(PackageJob1, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob1, topLevelHU3, "PLT", 1).AppendInsertAndReturnObject(SQL);

				var area2 = new WhsArea(Whs2.PK, "AREA2").AppendInsertAndReturnObject(SQL);
				var row2 = new WhsRow(Whs2, "A").AppendInsertAndReturnObject(SQL);
				var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(SQL);
				var rtu2 = new WhsItemReceiveTransportationUnit(Whs2, "rtu2", location2, "rtu2").AppendInsertAndReturnObject(SQL);
				var pkgPackage2 = topLevelHU2 is null ? new PkgPackage(PackageJob2, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(SQL);

				UnloadedTime = unloadedTime;
				RTU1 = rtu1;
				RTU2 = rtu2;
				Location1 = location1;
				Location2 = location2;
				PkgPackage1 = pkgPackage1;
				PkgPackage2 = pkgPackage2;
				PkgPackage3 = pkgPackage3;
			}

			public SqlQueryBuilder SQL;
			public WhsWarehouse Whs1;
			public WhsWarehouse Whs2;
			public PkgPackageJob PackageJob1;
			public PkgPackageJob PackageJob2;
			public GlbBranch Branch1;
			public GlbBranch Branch2;
			public WhsItemReceiveConsignment RCN1;
			public WhsItemReceiveConsignment RCN2;
			public string SystemLastEditUser;
			public DateTime Time;

			public WhsItemReceiveTransportationUnit RTU1;
			public WhsItemReceiveTransportationUnit RTU2;
			public WhsItemDispatchConsignment DCN1;
			public WhsItemDispatchConsignment DCN2;
			public WhsLocation Location1;
			public WhsLocation Location2;
			public DateTimeOffset UnloadedTime;
			public PkgPackage PkgPackage1;
			public PkgPackage PkgPackage2;
			public PkgPackage PkgPackage3;
		}

		#endregion
	}
}
