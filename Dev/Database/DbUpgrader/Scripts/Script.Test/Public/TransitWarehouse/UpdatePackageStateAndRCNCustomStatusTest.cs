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
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse.UpdatePackageStateAndRCNCustomStatus))]

	class UpdatePackageStateAndRCNCustomStatusTest : DbCreateScriptTest
	{
		const string TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);

		public void TestUpdateRCNAndPackageCustomStatusIsNON_WhenPackageAndRCNHasNoReferences_WhenWareHouseIsNotCustomsControlled_IsNotPortAuthorityControlled()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenPackageAndRCNHasNoReference_WarehouseIsCustomOrPortAuthorityControlled(false, false, "NON");
		}

		public void TestUpdateRCNAndPackageCustomStatusIsCUS_WhenPackageAndRCNHasNoReferences_WhenWareHouseIsCustomsControlled_IsNotPortAuthorityControlled()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenPackageAndRCNHasNoReference_WarehouseIsCustomOrPortAuthorityControlled(true, false, "CUS");
		}

		public void TestUpdateRCNAndPackageCustomStatusIsPAN_WhenPackageAndRCNHasNoReferences_WhenWareHouseIsCustomsControlled_IsPortAuthorityControlled()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenPackageAndRCNHasNoReference_WarehouseIsCustomOrPortAuthorityControlled(true, true, "PAN");
		}

		public void TestCore_UpdateRCNAndPackageCustomsStatus_WhenPackageAndRCNHasNoReference_WarehouseIsCustomOrPortAuthorityControlled(bool requiredCustomsClear, bool requiredPortAuthorityClear, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection, requiredCustomsClear, requiredPortAuthorityClear);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			if (requiredPortAuthorityClear)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
			}

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedRcn1.WRC_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("~BP", updatedRcn1.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedPackageState1.WPS_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("A", updatedPackageState1.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdatePackageCustomsStatusIsNON_WhenPackageHasNoCEN_HasNoCRN_RCNHasNoCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, false, false, false, "NON");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenPackageHasCEN_HasNoCRN_RCNHasNoCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, false, false, false, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasCRN_RCNHasNoCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, true, false, false, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasCRN_RCNHasNoCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, true, false, false, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenPackageHasNoCEN_HasNoCRN_RCNHasCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, false, true, false, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenPackageHasCEN_HasNoCRN_RCNHasCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, false, true, false, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasCRN_RCNHasCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, true, true, false, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasCRN_RCNHasCEN_HasNoCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, true, true, false, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasNoCRN_RCNHasNoCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, false, false, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasNoCRN_RCNHasNoCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, false, false, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasCRN_RCNHasNoCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, true, false, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasCRN_RCNHasNoCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, true, false, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasNoCRN_RCNHasCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, false, true, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasNoCRN_RCNHasCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, false, true, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoCEN_HasCRN_RCNHasCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(false, true, true, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasCEN_HasCRN_RCNHasCEN_HasCRNReference()
		{
			TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(true, true, true, true, "CLR");
		}

		void TestCore_UpdatePackageCustomsStatus_WhenPAckageAndRCNHasSomeCENAndSomeCRNReferences(bool hasPackageCEN, bool hasPackageCRN, bool hasRCNCEN, bool hasRCNCRN, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			if (hasPackageCEN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CEN", "CUS");
			}
			if (hasPackageCRN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CRN", "CUS");
			}
			if (hasRCNCEN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
			}
			if (hasRCNCRN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedPackageState1.WPS_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("A", updatedPackageState1.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdatePackageCustomsStatusIsNON_WhenPackageHasNoPAN_RCNHasNoPANReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(false, false, false, false, "NON");
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageHasPAN_RCNHasNoPANReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(true, false, false, false, "PAN");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasPANCleared_RCNHasNoPANReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(true, true, false, false, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageHasPANCleared_RCNHasPANClearedReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(false, false, true, false, "PAN");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasNoPAN_RCNHasPANClearReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(false, false, true, true, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenPackageHasPANCleared_RCNHasPANClearReference()
		{
			TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(true, true, true, true, "CLR");
		}

		void TestCore_UpdatePackageCustomsStatus_PackageAndRCNHasSomePAN_SomePANClearedReference(bool hasPackagePAN, bool hasPackagePANCleared, bool hasRCNPAN, bool hasRCNPANCleared, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			if (hasPackagePAN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CEN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CRN", "CUS");

				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "PAN", "PRT", hasPackagePANCleared ? "CLR" : "");
			}
			if (hasRCNPAN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");

				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT", hasRCNPANCleared ? "CLR" : "");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedPackageState1.WPS_CustomsStatus);

			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("A", updatedPackageState1.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdatePackageCustomsStatusIsNON_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageAndRCN_HasNoCEN_HasNoCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(0, 0, 0, 0, "NON");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageHasNoCEN_HasNoCRN_RCNHasNoCEN_HasCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(0, 0, 0, 1, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageHasNoCEN_HasCRN_RCNHasNoCEN_HasNoCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(0, 1, 0, 0, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageHasMoreCEN_ThanCRNCount_RCNHasNoCEN_HasNoCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(3, 2, 0, 0, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageHasNoCEN_HasNoCRN_RCNHasMoreCEN_ThanCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(0, 0, 3, 1, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageHasNoCEN_HasCRN_RCNHasCENMoreThanCRNReferences()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(0, 1, 3, 1, "CUS");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageAndRCNHasCEN_PackageCENCountLessThanCRNCount()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(2, 3, 2, 1, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageAndRCNHasCEN_RCNCENCountLessThanCRNCount()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(1, 0, 2, 3, "CLR");
		}

		public void TestUpdatePackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_PackageAndRCNHasCEN_SumCENCountLessThanSumCRNCount()
		{
			TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(2, 3, 2, 2, "CLR");
		}

		void TestCore_UpdatePackageCustomStatus_WithPackageOrRCNIsCustomCleared(int numberOfPackageCEN, int numberOfPackageCRN, int numberOfRCNCEN, int numberOfRCNCRN, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			for (var i = 0; i < numberOfPackageCEN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CEN", "CUS");
			}
			for (var i = 0; i < numberOfPackageCRN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CRN", "CUS");
			}
			for (var i = 0; i < numberOfRCNCEN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
			}
			for (var i = 0; i < numberOfRCNCRN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedPackageState1.WPS_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("A", updatedPackageState1.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
			}

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageHasSomePAN_SomePANNotCleared()
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "PAN", "PRT");
			CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "PAN", "PRT", "CLR");

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("PAN", updatedPackageState1.WPS_CustomsStatus);
			AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateRCNCustomsStatusIsNON_WhenRCNHasNoCEN_AllPackageHasNONCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "NON", "NON", "NON");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasNoCEN_SomePackageHasPAN_SomePackageHasNONCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "NON", "PAN", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasNoCEN_SomePackageHasNON_SomePAckageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "NON", "CLR", "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasNoCEN_SomePackageHasPAN_SomePackageHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "PAN", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasNoCEN_AllPackageHasPANCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "PAN", "PAN", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasNoCEN_SomePackageHasPAN_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "PAN", "CLR", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasNoCENPAN_AllPackagesHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "CUS", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasNoCEN_SomePackageHasCUS_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "CUS", "CLR", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenRCNHasNoCEN_AllPackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(false, false, "CLR", "CLR", "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasCEN_SomePackageHasCUS_SomePackageHasPANCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "PAN", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasCEN_SomePackageHasCUS_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "CUS", "CLR", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasCEN_AllPackageHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "CUS", "CUS", "CUS");
		}
		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasCEN_SomePackageHasPAN_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "PAN", "CLR", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasCEN_AllPackagesHasPANCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "PAN", "PAN", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenRCNHasCEN_AllPackagesHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, false, "CLR", "CLR", "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasCEN_HasCRN_AllPackageHasPANCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, true, "PAN", "PAN", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasCEN_HasCRN_SomePackageHasPAN_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, true, "PAN", "CLR", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenRCNHasCEN_HasCRN_AllPackagesCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(true, true, "CLR", "CLR", "CLR");
		}

		void TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomeCEN_HasSomeCRNReferences(bool hasRCNCEN, bool hasRCNCRN, string package1CustomsStatus, string package2CustomsStatus, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			if (hasRCNCEN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
			}

			if (hasRCNCRN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);
			CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, package1CustomsStatus);

			var packageState3 = new WhsItemPackageState(BasicData.PkgPackage3.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState3.AppendInsertAndReturnObject(BasicData.SQL);
			CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage3, package2CustomsStatus);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedRcn1.WRC_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("~BP", updatedRcn1.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasPAN_SomePackageHasPAN_SomePackageHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "PAN", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasPAN_AllPackagesHasPANCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "PAN", "PAN", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHasPAN_SomePackageHasPAN_SomePackageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "PAN", "CLR", "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasPAN_SomePackageHasCUS_SomePAckageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "CUS", "CLR", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasPAN_AllPackageHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "CUS", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenRCNHasPAN_AllPackagesHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, false, "CLR", "CLR", "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_RCNHasPANCleared_AllPackagesHasCUSCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, true, "CUS", "CUS", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenRCNHasPANCleared_SomePackageHasCUS_SomePckageHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, true, "CUS", "CLR", "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenRCNHasPANCleared_AllPackagesHasCLRCustomStatus()
		{
			TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(true, true, "CLR", "CLR", "CLR");
		}

		void TestCore_UpdateRCNCustomsStatus_WhenRCNHasSomePAN_HasSomePANClearedReferences(bool hasRCNPAN, bool isRCNPANCleared, string package1CustomsStatus, string package2CustomsStatus, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			if (hasRCNPAN)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT", isRCNPANCleared ? "CLR" : "");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);
			CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, package1CustomsStatus);

			var packageState3 = new WhsItemPackageState(BasicData.PkgPackage3.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState3.AppendInsertAndReturnObject(BasicData.SQL);
			CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage3, package2CustomsStatus);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedRcn1.WRC_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("~BP", updatedRcn1.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);
		}

		CusEntryNum CreateCustomEntries_ForPackageOrRCNOrDCN(Guid id, string parentTable, string entryType, string entryCategory, string entryStatus = "")
		{
			var cusEntryNum = new CusEntryNum(id, parentTable, "123", entryType, entryStatus);
			cusEntryNum.CE_Category = entryCategory;
			return cusEntryNum.AppendInsertAndReturnObject(BasicData.SQL);
		}

		void CreateAddOnValue_ForCusEntryNum(Guid parentPK, string name, string type, string data)
		{
			string createDataSQL = string.Format(@"
				insert into dbo.GenCustomAddOnValue(XV_PK, XV_ParentID, XV_Name, XV_Type, XV_Data)
				values (NEWID(), '{0}', '{1}', '{2}', '{3}')",
				parentPK, name, type, data);
			TestConnection.ExecuteNonQuery(createDataSQL);
		}

		void CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(PkgPackage package, string packageCustomsStatus)
		{
			if (packageCustomsStatus == "CLR")
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "CEN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "CRN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "PAN", "PRT", "CLR");
			}
			else if (packageCustomsStatus == "CUS" || packageCustomsStatus == "PAN")
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "CEN", "CUS");
				if (packageCustomsStatus == "PAN")
				{
					CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "CRN", "CUS");
					CreateCustomEntries_ForPackageOrRCNOrDCN(package.PK, "PkgPackage", "PAN", "PRT");
				}
			}
		}

		public void TestUpdateRCNCustomStatusIsNON_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_RCNHasNoCEN_HasNoCRNReferences()
		{
			TestCore_UpdateRCNCustomStatus_WithRCNHasSomeCEN_HasSomeCRNReferences(0, 0, "NON");
		}

		public void TestUpdateRCNCustomStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_RCNHasCEN_CENCountMoreThanCRNCount()
		{
			TestCore_UpdateRCNCustomStatus_WithRCNHasSomeCEN_HasSomeCRNReferences(5, 2, "CUS");
		}

		public void TestUpdateRCNCustomStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_WhenRCNHasNoCEN_ButHasCRNReferences()
		{
			TestCore_UpdateRCNCustomStatus_WithRCNHasSomeCEN_HasSomeCRNReferences(0, 5, "CLR");
		}

		public void TestUpdateRCNCustomStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_WhenRCNHasCEN_RCNCENCountEqualToCRNCount()
		{
			TestCore_UpdateRCNCustomStatus_WithRCNHasSomeCEN_HasSomeCRNReferences(5, 5, "CLR");
		}

		void TestCore_UpdateRCNCustomStatus_WithRCNHasSomeCEN_HasSomeCRNReferences(int numberOfRCNCEN, int numberOfRCNCRN, string expectedCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			for (var i = 0; i < numberOfRCNCEN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
			}

			for (var i = 0; i < numberOfRCNCRN; i++)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
			}

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState2.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals(expectedCustomsStatus, updatedRcn1.WRC_CustomsStatus);
			if (expectedCustomsStatus == CustomsStatusNon)
			{
				AssertEquals("~BP", updatedRcn1.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}
			else
			{
				AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);
			}

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenRCNHaSomesPAN_SomePANNotCleared()
		{
			BasicData.SetUpBasicTestData(TestConnection);
			BasicData.SetUpStandalonePackageTestData();

			CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT");
			CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT", "CLR");

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals("PAN", updatedRcn1.WRC_CustomsStatus);
			AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsDOM_NoPANPENReference()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Domestic, null, null, null, null, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsNotPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsDOM_PANIsNotCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(false, TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Domestic, false, false, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsNotPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsDOM_PENIsNotCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(false, TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Domestic, true, true, false, false, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsDOM_NoPANPENReference()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Domestic, null, null, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsIMP_NoPANPENReference()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Codes.Import, null, null, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsIMP_PackagePANIsCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Import, true, null, null, null, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsIMP_RCNPANIsCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Import, null, true, null, null, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsEXP_RCNPENIsCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Export, null, null, true, null, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsEXP_DCNPENIsCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Export, null, null, null, true, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsEmpty_RCNPENIsCleared()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, string.Empty, null, null, true, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEmpty_DCNDirectionIsEXP_OnlyHasPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, string.Empty, null, null, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsEXP_OnlyHasPAN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Export, true, true, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsIMP_OnlyHasPAN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Import, true, true, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEmpty_DCNDirectionIsDOM_OnlyHasPAN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, "", TransitWarehouseConsignmentDirections.Codes.Domestic, true, true, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsEmpty_OnlyHasPAN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Domestic, "", true, true, null, null, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsEXP_OnlyHasPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Export, null, null, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsIMP_OnlyHasPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Import, null, null, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEmpty_DCNDirectionIsDOM_OnlyHasPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, "", TransitWarehouseConsignmentDirections.Codes.Domestic, null, null, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsPAN_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsDOM_DCNDirectionIsEmpty_OnlyHasPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Domestic, "", null, null, true, true, "PAN");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsIMP_DCNDirectionIsEXP_HasBothPANAndPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Codes.Export, true, null, true, null, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEXP_DCNDirectionIsIMP_HasBothPANAndPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Codes.Import, null, true, null, true, "CLR");
		}

		public void TestUpdatePackageCustomStatusIsCLR_WhenWarehouseIsPortAuthorityControlled_RCNDirectionIsEmpty_DCNDirectionIsEmpty_HasBothPANAndPEN()
		{
			TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(true, "", "", true, true, true, true, "CLR");
		}

		void TestCore_UpdatePackagePANCustomsStatus_ConsignmentDirection(bool isPortAuthorityControlled, string rcnDirection, string dcnDirection, bool? isPackgePANCleared, bool? isRCNPANCleared, bool? isRCNPENCleared, bool? isDCNPENCleared, string expectedCustomStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection, isPortAuthorityControlled: isPortAuthorityControlled, rcnDirection: rcnDirection, dcnDirection: dcnDirection);
			BasicData.SetUpStandalonePackageTestData();

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var dll2 = new WhsItemDispatchLoadList("dll2", BasicData.Whs2).AppendInsertAndReturnObject(BasicData.SQL);

			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WDC_TransitDispatchConsignment = BasicData.DCN1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
			var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WDL_LoadList = dll2.PK, WPS_WDC_TransitDispatchConsignment = BasicData.DCN2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

			if (isPackgePANCleared != null)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "PAN", "PRT", isPackgePANCleared.Value ? "CLR" : "");
			}

			if (isRCNPANCleared != null)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT", isRCNPANCleared.Value ? "CLR" : "");
			}

			if (isRCNPENCleared != null)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PEN", "PRT", isRCNPENCleared.Value ? "CLR" : "");
			}

			if (isDCNPENCleared != null)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.DCN1.PK, "WhsItemDispatchConsignment", "PEN", "PRT", isDCNPENCleared.Value ? "CLR" : "");
			}

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());
			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals(expectedCustomStatus, updatedPackageState1.WPS_CustomsStatus);
			AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);

			var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
			AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
		}

		public void TestUpdateRCNAndPackageCustomsStatusIsCLR_WhenWarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled_AllPackagesCleared()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenWarehouse_IsCustomControlledOrIsPortAuthorityControlled_AllPackagesCleared(false, false);
		}

		public void TestUpdateRCNAndPackageCustomsStatusIsCLR_WhenWarehouseIsCustomsControlled_IsNotPortAuthorityControlled_AllPackagesCleared()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenWarehouse_IsCustomControlledOrIsPortAuthorityControlled_AllPackagesCleared(true, false);
		}

		public void TestUpdateRCNAndPackageCustomsStatusIsCLR_WhenWarehouseIsCustomsControlled_IsPortAuthorityControlled_AllPackagesCleared()
		{
			TestCore_UpdateRCNAndPackageCustomsStatus_WhenWarehouse_IsCustomControlledOrIsPortAuthorityControlled_AllPackagesCleared(true, true);
		}

		void TestCore_UpdateRCNAndPackageCustomsStatus_WhenWarehouse_IsCustomControlledOrIsPortAuthorityControlled_AllPackagesCleared(bool requiredCustomsClear, bool requiredPortAuthorityClear)
		{
			BasicData.SetUpBasicTestData(TestConnection, requiredCustomsClear, requiredPortAuthorityClear);
			BasicData.SetUpStandalonePackageTestData();

			if (requiredPortAuthorityClear)
			{
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", entryType: "CEN", "CUS");
				CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", entryType: "CRN", "CUS");
			}

			CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, "CLR");

			var dll1 = new WhsItemDispatchLoadList("dll1", BasicData.Whs1).AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WDL_LoadList = dll1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			AssertEquals("CLR", updatedRcn1.WRC_CustomsStatus);
			AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, updatedRcn1.WRC_SystemLastEditTimeUtc);

			var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
			AssertEquals("CLR", updatedPackageState1.WPS_CustomsStatus);
			AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
			AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);

			var updatedRcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
			AssertEquals(CustomsStatusNon, updatedRcn2.WRC_CustomsStatus);
			AssertEquals("~BP", updatedRcn2.WRC_SystemLastEditUser);
			AssertEquals(BasicData.Time, updatedRcn2.WRC_SystemLastEditTimeUtc);
		}

		public void TestUpdateTopHUCustomStatusIsNON_WhenAllChildPackagesCustomStatusHasNONCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("NON", "NON", "NON");
		}

		public void TestUpdateTopHUCustomStatusIsCUS_WhenSomeChildPackagesCustomStatusHasCUSCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("NON", "CUS", "CUS");
		}

		public void TestUpdateTopHUCustomStatusIsPAN_WhenSomeChildPackagesCustomStatusHasPANCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("NON", "PAN", "PAN");
		}

		public void TestUpdateTopHUCustomStatusIsCLR_WhenSomeChildPackagesHasNON_SomeChildPAckagesHasCLRCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("NON", "CLR", "CLR");
		}

		public void TestUpdateTopHUCustomStatusIsCUS_AllChildPackagesCustomStatusHasCUSCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("CUS", "CUS", "CUS");
		}

		public void TestUpdateTopHUCustomStatusIsPAN_AllChildPackagesCustomStatusHasPANCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("PAN", "PAN", "PAN");
		}

		public void TestUpdateTopHUCustomStatusIsCUS_SomeChildPackagesCustomStatusHasCUS_SomeChildPackagesHasPANCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("PAN", "CUS", "CUS");
		}

		public void TestUpdateTopHUCustomStatusIsCUS_SomeChildPackagesHasCUS_SomeChildPackagesHasCLRCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("CLR", "CUS", "CUS");
		}

		public void TestUpdateTopHUCustomStatusIsPAN_SomeChildPackagesHasPAN_SomeChildPackagesHasCLRCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("CLR", "PAN", "PAN");
		}

		public void TestUpdateTopHUCustomStatusIsCLR_AllChildPackagesCustomStatusHasCLRCustomStatus()
		{
			TestCore_UpdateTopHUCustomsStatus("CLR", "CLR", "CLR");
		}

		void TestCore_UpdateTopHUCustomsStatus(string package1CustomsStatus, string package2CustomsStatus, string expectedCustomsStatus)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_IsHandlingUnit = true, WPS_UnitType = "HU", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_IsHandlingUnit = true, WPS_UnitType = "HU", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var pkgPackage3 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, pkgPackage3.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState3 = new WhsItemPackageState(pkgPackage3.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState3.AppendInsertAndReturnObject(BasicData.SQL);

				var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "BKD") { WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState2.AppendInsertAndReturnObject(BasicData.SQL);

				CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, package1CustomsStatus);
				CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(pkgPackage3, package2CustomsStatus);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals(expectedCustomsStatus, updatedTopLevelHUPackageState1.WPS_CustomsStatus);
				if (expectedCustomsStatus == CustomsStatusNon)
				{
					AssertEquals("A", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}
				else
				{
					AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}

				var updatedTopLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU2.PK).FirstOrDefault();
				AssertEquals(CustomsStatusNon, updatedTopLevelHUPackageState2.WPS_CustomsStatus);
				AssertEquals("A", updatedTopLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedTopLevelHUPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		public void TestUpdateMidHUCustomStatusIsNON_WhenTopHUCustomStatusIsNON()
		{
			TestUpdateTopHUAndMidLevelHUCustomStatus("NON", "NON");
		}

		public void TestUpdateMidHUCustomStatusIsCUS_WhenTopHUCustomStatusIsCUS()
		{
			TestUpdateTopHUAndMidLevelHUCustomStatus("CUS", "CUS");
		}

		public void TestUpdateMidHUCustomStatusIsPAN_WhenTopHUCustomStatusIsPAN()
		{
			TestUpdateTopHUAndMidLevelHUCustomStatus("PAN", "PAN");
		}

		public void TestUpdateMidHUCustomStatusIsCLR_WhenTopHUCustomStatusIsCLR()
		{
			TestUpdateTopHUAndMidLevelHUCustomStatus("CLR", "CLR");
		}

		void TestUpdateTopHUAndMidLevelHUCustomStatus(string topHUCustomStatus, string expectedCustomStatus)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_IsHandlingUnit = true, WPS_UnitType = "HU", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_IsHandlingUnit = true, WPS_UnitType = "HU", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var pkgPackage2 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, pkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState1.AppendInsertAndReturnObject(BasicData.SQL);

				var packageState2 = new WhsItemPackageState(pkgPackage2.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
				packageState2.AppendInsertAndReturnObject(BasicData.SQL);

				if (topHUCustomStatus == "CUS")
				{
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, "PAN");
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(pkgPackage2, "CUS");
				}
				else if (topHUCustomStatus == "PAN")
				{
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, "PAN");
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(pkgPackage2, "CLR");
				}
				else if (topHUCustomStatus == "CLR")
				{
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(BasicData.PkgPackage1, "CLR");
					CreateCustomsEntriesForPackage_BasedOnPackageCustomStstus(pkgPackage2, "CLR");
				}

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

				var updatedTopLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == topLevelHU1.PK).FirstOrDefault();
				AssertEquals(expectedCustomStatus, updatedTopLevelHUPackageState1.WPS_CustomsStatus);
				if (expectedCustomStatus == CustomsStatusNon)
				{
					AssertEquals("A", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}
				else
				{
					AssertEquals("TST", updatedTopLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertNotEquals(BasicData.Time, updatedTopLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}

				var updatedMidLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.WPS_KP_Package == middleLevelHU1.PK).FirstOrDefault();
				AssertEquals(expectedCustomStatus, updatedMidLevelHUPackageState1.WPS_CustomsStatus);
				if (expectedCustomStatus == CustomsStatusNon)
				{
					AssertEquals("A", updatedMidLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertEquals(BasicData.Time, updatedMidLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}
				else
				{
					AssertEquals("TST", updatedMidLevelHUPackageState1.WPS_SystemLastEditUser);
					AssertNotEquals(BasicData.Time, updatedMidLevelHUPackageState1.WPS_SystemLastEditTimeUtc);
				}
			}
		}

		#region Package Quantity Counting Algorithm

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_CRNHasNoAddOnValue()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, hasRCNCRNWithoutAddOnValue: true, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_CRNOuterIsGreaterThanRCNOuters_CRNInnerIsNULL()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, outerQtyInCRN1: 2, outerQtyInCRN2: 3, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_CRNOuterIsEqualToRCNOuters_CRNInnerIsNULL()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, outerQtyInCRN1: 2, outerQtyInCRN2: 2, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_CRNOuterIsNULL_CRNInnerIsGreaterThanRCNInners()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, innerQtyInCRN1: 2, innerQtyInCRN2: 2, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_CRNOuterIsNULL_CRNInnerIsEqualToRCNInners()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, innerQtyInCRN1: 1, innerQtyInCRN2: 2, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_HasCRNOuter_CRNOuterIsGreaterThanRCNOuters()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, outerQtyInCRN1: 5, innerQtyInCRN1: 2, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsCLR_WhenPackageQuantityCountingAlgorithmEnabled_PANCleared_HasCEN_HasCRNInner_CRNInnerIsGreaterThanRCNInners()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: true, hasRCNCEN: true, outerQtyInCRN1: 3, innerQtyInCRN1: 4, expectedStatus: "CLR");
		}

		public void TestUpdateRCNCustomsStatusIsNON_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasNoPAN_RCNHasNoCEN_WarehouseIsNotCustomsControlled_IsNotPortAuthorityControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: false, hasRCNCEN: false, isCustomsControlled: false, isPortAuthorityControlled: false, expectedStatus: "NON");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasNoCEN_WarehouseIsCustomsControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: false, isCustomsControlled: true, expectedStatus: "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasCEN_RCNHasNoCRN()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: true, expectedStatus: "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasCEN_CRNOuterIsLessThanRCNOuters()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: true, outerQtyInCRN1: 3, expectedStatus: "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasCEN_CRNInnerIsLessThanRCNInners()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: true, innerQtyInCRN1: 2, expectedStatus: "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasCEN_CRNOuterIsLessThanRCNOuters_CRNInnerIsLessThanRCNInners()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: true, outerQtyInCRN1: 3, innerQtyInCRN1: 2, expectedStatus: "CUS");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasNoPAN_WarehouseIsPortAuthorityControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: false, isPortAuthorityControlled: true, expectedStatus: "PAN");
		}

		public void TestUpdateRCNCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasPAN_RCNPANNotCleared_WarehouseIsPortAuthorityControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: false, expectedStatus: "PAN");
		}

		public void TestUpdatePackageCustomsStatusIsNON_WhenPackageQuantityCountingAlgorithmEnabled_PKGHasNoPAN_RCNCustomsStatusIsNON_WarehouseIsNotCustomsControlled_WarehouseIsNotPortAuthorityControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasPKGPAN: false, hasRCNPAN: false, hasRCNCEN: false, isCustomsControlled: false, isPortAuthorityControlled: false, expectedStatus: "NON", assertPKG: true);
		}

		public void TestUpdatePackageCustomsStatusIsCUS_WhenPackageQuantityCountingAlgorithmEnabled_RCNCustomsStatusIsCUS()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNCEN: true, expectedStatus: "CUS", assertPKG: true);
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasNoPAN_PackageHasNoPAN_WarehouseIsPortAuthorityControlled()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: false, hasPKGPAN: false, isPortAuthorityControlled: true, expectedStatus: "PAN", assertPKG: true);
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasNoPAN_PKGHasPAN_PKGPANNotCleared()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasPKGPAN: true, isPKGPANCleared: false, hasRCNPAN: false, expectedStatus: "PAN", assertPKG: true);
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasPAN_PKGHasNoPAN_RCNPANNotCleared()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: false, hasPKGPAN: false, expectedStatus: "PAN", assertPKG: true);
		}

		public void TestUpdatePackageCustomsStatusIsPAN_WhenPackageQuantityCountingAlgorithmEnabled_RCNHasPAN_PKGHasNoPAN_RCNPANNotCleared_PKGPANNotCleared()
		{
			UpdatePackageAndRCNCustomsStatusCore(hasRCNPAN: true, isRCNPANCleared: false, hasPKGPAN: true, isPKGPANCleared: false, expectedStatus: "PAN", assertPKG: true);
		}

		void UpdatePackageAndRCNCustomsStatusCore(bool hasRCNPAN = false, bool isRCNPANCleared = false, bool hasRCNCEN = false, bool hasRCNCRNWithoutAddOnValue = false, bool hasPKGPAN = false, bool isPKGPANCleared = false, bool hasPKGCEN = false, int? outerQtyInCRN1 = null, int? outerQtyInCRN2 = null, int? innerQtyInCRN1 = null, int? innerQtyInCRN2 = null, bool isCustomsControlled = false, bool isPortAuthorityControlled = false, string expectedStatus = "", bool assertPKG = false)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				// RCN1: 4 outers and 3 inners
				// - HU1
				// - - PKG1, PKG3
				// - OVP1
				// - - PKG4, PKG5, PKG6
				// - PKG7

				// RCN2: 1 outers and 0 inners
				// - HU2
				// - - PKG2

				BasicData.SetUpBasicTestData(TestConnection, isCustomsControlled, isPortAuthorityControlled);

				var hu1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var hu2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var ovp = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				BasicData.SetUpStandalonePackageTestData(hu1, hu2);

				new WhsItemPackageState(hu1.PK, BasicData.Whs1, null, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				new WhsItemPackageState(hu2.PK, BasicData.Whs2, null, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				new WhsItemPackageState(ovp.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_UnitType = "OVP", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				var pkgPackage4 = new PkgPackage(BasicData.PackageJob1, ovp, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var pkgPackage5 = new PkgPackage(BasicData.PackageJob1, ovp, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var pkgPackage6 = new PkgPackage(BasicData.PackageJob1, ovp, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var pkgPackage7 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(hu1.PK, BasicData.PkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(hu2.PK, BasicData.PkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(hu1.PK, BasicData.PkgPackage3.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot4 = new PkgPackageHandlingUnitDivot(ovp.PK, pkgPackage4.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot5 = new PkgPackageHandlingUnitDivot(ovp.PK, pkgPackage5.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot6 = new PkgPackageHandlingUnitDivot(ovp.PK, pkgPackage6.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState2 = new WhsItemPackageState(BasicData.PkgPackage2.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState3 = new WhsItemPackageState(BasicData.PkgPackage3.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState4 = new WhsItemPackageState(pkgPackage4.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState5 = new WhsItemPackageState(pkgPackage5.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState6 = new WhsItemPackageState(pkgPackage6.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState7 = new WhsItemPackageState(pkgPackage7.PK, BasicData.Whs1, BasicData.RCN1.PK, "BKD") { WPS_SystemLastEditTimeUtc = BasicData.Time }.AppendInsertAndReturnObject(BasicData.SQL);

				if (hasRCNPAN)
				{
					CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "PAN", "PRT", isRCNPANCleared ? "CLR" : "");
				}

				if (hasRCNCEN)
				{
					CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CEN", "CUS");
				}

				if (hasPKGPAN)
				{
					CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "PAN", "PRT", isPKGPANCleared ? "CLR" : "");
				}

				if (hasPKGCEN)
				{
					CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.PkgPackage1.PK, "PkgPackage", "CEN", "CUS");
				}

				if (hasRCNCRNWithoutAddOnValue)
				{
					var cusEntyNum = CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
				}

				if (outerQtyInCRN1 != null)
				{
					var cusEntyNum = CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
					CreateAddOnValue_ForCusEntryNum(cusEntyNum.PK, "TWOuterPackQty", "INT", outerQtyInCRN1.ToString());
				}

				if (outerQtyInCRN2 != null)
				{
					var cusEntyNum = CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
					CreateAddOnValue_ForCusEntryNum(cusEntyNum.PK, "TWOuterPackQty", "INT", outerQtyInCRN2.ToString());
				}

				if (innerQtyInCRN1 != null)
				{
					var cusEntyNum = CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
					CreateAddOnValue_ForCusEntryNum(cusEntyNum.PK, "TWInnerPackQty", "INT", innerQtyInCRN1.ToString());
				}

				if (innerQtyInCRN2 != null)
				{
					var cusEntyNum = CreateCustomEntries_ForPackageOrRCNOrDCN(BasicData.RCN1.PK, "WhsItemReceiveConsignment", "CRN", "CUS");
					CreateAddOnValue_ForCusEntryNum(cusEntyNum.PK, "TWInnerPackQty", "INT", innerQtyInCRN2.ToString());
				}

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser, isApplyPackageQuantityCountingAlgorithm: true);

				if (!assertPKG)
				{
					var updatedRCN1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
					AssertEquals(expectedStatus, updatedRCN1.WRC_CustomsStatus);
					if (expectedStatus == CustomsStatusNon)
					{
						AssertEquals("~BP", updatedRCN1.WRC_SystemLastEditUser);
						AssertEquals(BasicData.Time, updatedRCN1.WRC_SystemLastEditTimeUtc);
					}
					else
					{
						AssertEquals("TST", updatedRCN1.WRC_SystemLastEditUser);
						AssertNotEquals(BasicData.Time, updatedRCN1.WRC_SystemLastEditTimeUtc);
					}
				}
				else
				{
					var updatedPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
					AssertEquals(expectedStatus, updatedPackageState1.WPS_CustomsStatus);
					if (expectedStatus == CustomsStatusNon)
					{
						AssertEquals("A", updatedPackageState1.WPS_SystemLastEditUser);
						AssertEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
					}
					else
					{
						AssertEquals("TST", updatedPackageState1.WPS_SystemLastEditUser);
						AssertNotEquals(BasicData.Time, updatedPackageState1.WPS_SystemLastEditTimeUtc);
					}
				}

				var updatedRCN2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
				AssertEquals(CustomsStatusNon, updatedRCN2.WRC_CustomsStatus);
				AssertEquals("~BP", updatedRCN2.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedRCN2.WRC_SystemLastEditTimeUtc);

				var updatedPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
				AssertEquals(CustomsStatusNon, updatedPackageState2.WPS_CustomsStatus);
				AssertEquals("A", updatedPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, updatedPackageState2.WPS_SystemLastEditTimeUtc);
			}
		}

		#endregion

		public void TestUpdatePackageCustomsStatus_OnlyUpdateHandlingUnitAndPackageFromTargetedPackages_WhenPackageQuantityCountingAlgorithmEnabled()
		{
			UpdatePackageCustomsStatus_OnlyUpdateHandlingUnitAndPackageFromTargetedRCNCore(packageQuantityCountingAlgorithmEnabled: true);
		}

		public void TestUpdatePackageCustomsStatus_OnlyUpdateHandlingUnitAndPackageFromTargetedPackages_WhenPackageQuantityCountingAlgorithmDisabled()
		{
			UpdatePackageCustomsStatus_OnlyUpdateHandlingUnitAndPackageFromTargetedRCNCore(packageQuantityCountingAlgorithmEnabled: false);
		}

		void UpdatePackageCustomsStatus_OnlyUpdateHandlingUnitAndPackageFromTargetedRCNCore(bool packageQuantityCountingAlgorithmEnabled)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, Db.Connection))
			{
				BasicData.SetUpBasicTestData(TestConnection, isCustomsControlled: true);

				var topLevelHU1 = new PkgPackage(BasicData.PackageJob1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHU2 = new PkgPackage(BasicData.PackageJob2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				BasicData.SetUpStandalonePackageTestData(topLevelHU1, topLevelHU2);

				var topLevelHUPackageState1 = new WhsItemPackageState(topLevelHU1.PK, BasicData.Whs1, null, "ARV") { WPS_IsHandlingUnit = true, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var topLevelHUPackageState2 = new WhsItemPackageState(topLevelHU2.PK, BasicData.Whs2, null, "ARV") { WPS_IsHandlingUnit = true, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);

				var middleLevelHU1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHU2 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState1 = new WhsItemPackageState(middleLevelHU1.PK, BasicData.Whs1, null, "ARV") { WPS_IsHandlingUnit = true, WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var middleLevelHUPackageState2 = new WhsItemPackageState(middleLevelHU2.PK, BasicData.Whs2, null, "ARV") { WPS_IsHandlingUnit = true, WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_UnitType = "HU", WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);

				var pkgPackage1 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var pkgPackage2 = new PkgPackage(BasicData.PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var pkgPackage3 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);
				var pkgPackage4 = new PkgPackage(BasicData.PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(BasicData.SQL);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, middleLevelHU1.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot2 = new PkgPackageHandlingUnitDivot(topLevelHU1.PK, pkgPackage2.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot3 = new PkgPackageHandlingUnitDivot(middleLevelHU1.PK, pkgPackage1.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var divot4 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, middleLevelHU2.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot5 = new PkgPackageHandlingUnitDivot(topLevelHU2.PK, pkgPackage4.PK).AppendInsertAndReturnObject(BasicData.SQL);
				var divot6 = new PkgPackageHandlingUnitDivot(middleLevelHU2.PK, pkgPackage3.PK).AppendInsertAndReturnObject(BasicData.SQL);

				var packageState1 = new WhsItemPackageState(pkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState2 = new WhsItemPackageState(pkgPackage2.PK, BasicData.Whs1, BasicData.RCN1.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState3 = new WhsItemPackageState(pkgPackage3.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);
				var packageState4 = new WhsItemPackageState(pkgPackage4.PK, BasicData.Whs2, BasicData.RCN2.PK, "ARV") { WPS_WRH_TransitReceiveHeader = BasicData.RTU2.PK, WPS_WL_LastLocation = BasicData.Location2.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time, WPS_CustomsStatus = "CLR" }.AppendInsertAndReturnObject(BasicData.SQL);

				TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

				RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser, isApplyPackageQuantityCountingAlgorithm: true, new List<Guid>() { packageState1.PK, packageState2.PK });

				var rcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
				AssertEquals("CUS", rcn1.WRC_CustomsStatus);
				AssertEquals("TST", rcn1.WRC_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, rcn1.WRC_SystemLastEditTimeUtc);

				topLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == topLevelHUPackageState1.PK).FirstOrDefault();
				AssertEquals("CUS", topLevelHUPackageState1.WPS_CustomsStatus);
				AssertEquals("TST", topLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, topLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				middleLevelHUPackageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == middleLevelHUPackageState1.PK).FirstOrDefault();
				AssertEquals("CUS", middleLevelHUPackageState1.WPS_CustomsStatus);
				AssertEquals("TST", middleLevelHUPackageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, middleLevelHUPackageState1.WPS_SystemLastEditTimeUtc);

				packageState1 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState1.PK).FirstOrDefault();
				AssertEquals("CUS", packageState1.WPS_CustomsStatus);
				AssertEquals("TST", packageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, packageState1.WPS_SystemLastEditTimeUtc);

				packageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState2.PK).FirstOrDefault();
				AssertEquals("CUS", packageState1.WPS_CustomsStatus);
				AssertEquals("TST", packageState1.WPS_SystemLastEditUser);
				AssertNotEquals(BasicData.Time, packageState1.WPS_SystemLastEditTimeUtc);

				var rcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN2.PK).FirstOrDefault();
				AssertEquals(CustomsStatusNon, rcn2.WRC_CustomsStatus);
				AssertEquals("~BP", rcn2.WRC_SystemLastEditUser);
				AssertEquals(BasicData.Time, rcn2.WRC_SystemLastEditTimeUtc);

				topLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == topLevelHUPackageState2.PK).FirstOrDefault();
				AssertEquals("CLR", topLevelHUPackageState2.WPS_CustomsStatus);
				AssertEquals("A", topLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, topLevelHUPackageState2.WPS_SystemLastEditTimeUtc);

				middleLevelHUPackageState2 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == middleLevelHUPackageState2.PK).FirstOrDefault();
				AssertEquals("CLR", middleLevelHUPackageState2.WPS_CustomsStatus);
				AssertEquals("A", middleLevelHUPackageState2.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, middleLevelHUPackageState2.WPS_SystemLastEditTimeUtc);

				packageState3 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState3.PK).FirstOrDefault();
				AssertEquals("CLR", packageState3.WPS_CustomsStatus);
				AssertEquals("A", packageState3.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, packageState3.WPS_SystemLastEditTimeUtc);

				packageState4 = WhsItemPackageState.ShallowLoadFromDB(TestConnection, p => p.PK == packageState4.PK).FirstOrDefault();
				AssertEquals("CLR", packageState4.WPS_CustomsStatus);
				AssertEquals("A", packageState4.WPS_SystemLastEditUser);
				AssertEquals(BasicData.Time, packageState4.WPS_SystemLastEditTimeUtc);
			}
		}

		#region PackageStateWithDTU

		public void TestUpdateStandAlonePackageCustomsStatus_PackageIsFLO() => TestUpdateStandAlonePackageCustomsStatus_PackageHasDTUCore("FLO", true);
		public void TestUpdateStandAlonePackageCustomsStatus_PackageIsDEP() => TestUpdateStandAlonePackageCustomsStatus_PackageHasDTUCore("DEP", false);
		public void TestUpdateStandAlonePackageCustomsStatus_PackageIsFIN() => TestUpdateStandAlonePackageCustomsStatus_PackageHasDTUCore("FIN", false);

		void TestUpdateStandAlonePackageCustomsStatus_PackageHasDTUCore(string packageStatus, bool shouldUpdateCustomsStatus)
		{
			BasicData.SetUpBasicTestData(TestConnection, true, false);
			BasicData.SetUpStandalonePackageTestData();

			var dcn = new WhsItemDispatchConsignment(BasicData.Whs1, "DCN1", "DCN1", "STD").AppendInsertAndReturnObject(BasicData.SQL);
			var dll = new WhsItemDispatchLoadList("dll1", BasicData.Whs1) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(BasicData.SQL);
			var dtu = new WhsItemDispatchTransportationUnit(BasicData.Whs1, "DTU1").AppendInsertAndReturnObject(BasicData.SQL);
			var packageState1 = new WhsItemPackageState(BasicData.PkgPackage1.PK, BasicData.Whs1, BasicData.RCN1.PK, packageStatus, "NOT") { WPS_WRH_TransitReceiveHeader = BasicData.RTU1.PK, WPS_WDL_LoadList = dll.PK, WPS_WDH_TransitDispatchHeader = dtu.PK, WPS_WDC_TransitDispatchConsignment = dcn.PK, WPS_WL_LastLocation = BasicData.Location1.PK, WPS_UnloadedTime = BasicData.UnloadedTime, WPS_LoadedTime = BasicData.UnloadedTime.AddMinutes(1), WPS_UnloadedNotYetProcessedTime = BasicData.UnloadedTime, WPS_ReceivedAs = "SCN", WPS_SystemLastEditTimeUtc = BasicData.Time };
			packageState1.AppendInsertAndReturnObject(BasicData.SQL);

			TestConnection.ExecuteNonQuery(BasicData.SQL.ToStringWithNewLineBetweenAppends());

			RunCheckProcedure(BasicData.Branch1, BasicData.SystemLastEditUser);

			var updatedRcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, p => p.PK == basicData.RCN1.PK).FirstOrDefault();
			if (shouldUpdateCustomsStatus)
			{
				AssertEquals("CUS", updatedRcn1.WRC_CustomsStatus);
				AssertEquals("TST", updatedRcn1.WRC_SystemLastEditUser);
			}
			else
			{
				AssertEquals("NON", updatedRcn1.WRC_CustomsStatus);
				AssertEquals("~BP", updatedRcn1.WRC_SystemLastEditUser);
			}
		}

		#endregion

		void RunCheckProcedure(GlbBranch branch, string systemLastEditUser, bool isApplyPackageQuantityCountingAlgorithm = false, List<Guid> packageStatePKs = null)
		{
			var sql = "EXEC dbo.UpdatePackageStateAndRCNCustomStatus @CompanyBranchPK, @SystemLastEditUser, @CurrentUTC, @WarehouseConfigCustomControlled, @WarehouseConfigPortControlled, @IsApplyPackageQuantityCountingAlgorithm, @PackageStatePKs";
			using var sqlCommand = Db.Connection.Command(sql);
			sqlCommand.AddParameter("@CompanyBranchPK", SqlDbType.UniqueIdentifier, branch.PK);
			sqlCommand.AddParameter("@SystemLastEditUser", SqlDbType.Char, systemLastEditUser);
			sqlCommand.AddParameter("@CurrentUTC", SqlDbType.DateTime, DateTime.UtcNow);
			sqlCommand.AddParameter("@WarehouseConfigCustomControlled", SqlDbType.Bit, DBNull.Value);
			sqlCommand.AddParameter("@WarehouseConfigPortControlled", SqlDbType.Bit, DBNull.Value);
			sqlCommand.AddParameter("@IsApplyPackageQuantityCountingAlgorithm", SqlDbType.Bit, isApplyPackageQuantityCountingAlgorithm);
			sqlCommand.AddTableValuedParameter("@PackageStatePKs", TVPHelper.TVP_uniqueidentifier, packageStatePKs ?? Enumerable.Empty<Guid>());
			sqlCommand.ExecuteNonQuery();
		}

		BasicTestData BasicData => basicData ??= new BasicTestData();
		BasicTestData basicData;
		readonly string CustomsStatusNon = "NON";

		class BasicTestData
		{
			public void SetUpBasicTestData(IDbConnectionWithSettings testConnection, bool isCustomsControlled = false, bool isPortAuthorityControlled = false, string rcnDirection = TransitWarehouseConsignmentDirections.Codes.Import, string dcnDirection = TransitWarehouseConsignmentDirections.Codes.Import)
			{
				SQL = new SqlQueryBuilder();

				Time = new DateTime(2023, 05, 24, 1, 1, 0);
				SystemLastEditUser = "TST";

				Branch1 = new GlbBranch("BR1").InsertAndReturnObject(testConnection);
				Branch2 = new GlbBranch("BR2").InsertAndReturnObject(testConnection);
				Whs1 = new WhsWarehouse("WH1", Branch1.PK) { WW_IsCustomsControlled = isCustomsControlled, WW_IsPortAuthorityControlled = isPortAuthorityControlled, WW_WarehouseType = "TRW" }.WithDockDoor(testConnection);
				Whs2 = new WhsWarehouse("WH2", Branch2.PK) { WW_IsCustomsControlled = isCustomsControlled, WW_IsPortAuthorityControlled = isPortAuthorityControlled, WW_WarehouseType = "TRW" }.WithDockDoor(testConnection);
				RCN1 = new WhsItemReceiveConsignment(Whs1, "RC000001", "RC000001", "STD", "AUSYD") { WRC_SystemLastEditTimeUtc = Time, WRC_Direction = rcnDirection }.AppendInsertAndReturnObject(SQL);
				RCN2 = new WhsItemReceiveConsignment(Whs2, "RC000002", "RC000002", "STD", "AUSYD") { WRC_SystemLastEditTimeUtc = Time, WRC_Direction = rcnDirection }.AppendInsertAndReturnObject(SQL);

				DCN1 = new WhsItemDispatchConsignment(Whs1, "DC000001", "DC000001", "STD") { WDC_Direction = dcnDirection }.AppendInsertAndReturnObject(SQL);
				DCN2 = new WhsItemDispatchConsignment(Whs2, "DC000002", "DC000002", "STD") { WDC_Direction = dcnDirection }.AppendInsertAndReturnObject(SQL);

				PackageJob1 = new PkgPackageJob(RCN1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(SQL);
				PackageJob2 = new PkgPackageJob(RCN2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(SQL);
			}

			public void SetUpStandalonePackageTestData(PkgPackage topLevelHU1 = null, PkgPackage topLevelHU2 = null)
			{
				UnloadedTime = new DateTimeOffset(2023, 05, 24, 13, 59, 0, new TimeSpan(10, 0, 0));
				var area1 = new WhsArea(Whs1.PK, "AREA1").AppendInsertAndReturnObject(SQL);
				var row1 = new WhsRow(Whs1, "A").AppendInsertAndReturnObject(SQL);
				Location1 = new WhsLocation(row1.PK, area1.PK, area1.PK).AppendInsertAndReturnObject(SQL);
				RTU1 = new WhsItemReceiveTransportationUnit(Whs1, "rtu1", Location1, "rtu1").AppendInsertAndReturnObject(SQL);
				PkgPackage1 = topLevelHU1 is null ? new PkgPackage(PackageJob1, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(SQL);
				PkgPackage3 = topLevelHU1 is null ? new PkgPackage(PackageJob1, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob1, topLevelHU1, "PLT", 1).AppendInsertAndReturnObject(SQL);

				var area2 = new WhsArea(Whs2.PK, "AREA2").AppendInsertAndReturnObject(SQL);
				var row2 = new WhsRow(Whs2, "A").AppendInsertAndReturnObject(SQL);
				Location2 = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(SQL);
				RTU2 = new WhsItemReceiveTransportationUnit(Whs2, "rtu2", Location2, "rtu2").AppendInsertAndReturnObject(SQL);
				PkgPackage2 = topLevelHU2 is null ? new PkgPackage(PackageJob2, "PLT", 1).AppendInsertAndReturnObject(SQL) : new PkgPackage(PackageJob2, topLevelHU2, "PLT", 1).AppendInsertAndReturnObject(SQL);
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
	}
}
