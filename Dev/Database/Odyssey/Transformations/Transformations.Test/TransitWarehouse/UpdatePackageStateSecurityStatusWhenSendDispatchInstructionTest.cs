using System;
using System.Collections.Generic;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(UpdatePackageStateSecurityStatusWhenSendDispatchInstruction))]
	class UpdatePackageStateSecurityStatusWhenSendDispatchInstructionTest : DataTransformationTestCase
	{
		#region Standalone Package

		public void TestStandalonePackage_ShouldUpdateFromREQToNOT_TransportMode() => RunStandalonePackageTest("P1", securityStatus: REQ, expectedSecurityStatus: NOT, transportMode: COU);

		public void TestStandalonePackage_ShouldUpdateFromHRSToNOT_TransportMode() => RunStandalonePackageTest("P2", securityStatus: HRS, expectedSecurityStatus: NOT, transportMode: ROA);

		public void TestStandalonePackage_ShouldUpdateFromHRNToNOT_TransportMode() => RunStandalonePackageTest("P3", securityStatus: HRN, expectedSecurityStatus: NOT, transportMode: RAI);

		public void TestStandalonePackage_ShouldUpdateFromSECToNOT_TransportMode() => RunStandalonePackageTest("P4", securityStatus: SEC, expectedSecurityStatus: NOT, transportMode: SEA);

		public void TestStandalonePackage_ShouldUpdateFromSCRToNOT_TransportMode()  => RunStandalonePackageTest("P5", securityStatus: SEC, expectedSecurityStatus: NOT, transportMode: SEA);

		public void TestStandalonePackage_ShouldUpdateFromREQToNOT_Direction() => RunStandalonePackageTest("P6", securityStatus: REQ, expectedSecurityStatus: NOT, rcnDirection: EXP, dcnDirection: IMP);

		public void TestStandalonePackage_ShouldUpdateFromHRSToNOT_Direction() => RunStandalonePackageTest("P7", securityStatus: HRS, expectedSecurityStatus: NOT, rcnDirection: EXP, dcnDirection: DOM);

		public void TestStandalonePackage_ShouldUpdateFromHRNToNOT_Direction() => RunStandalonePackageTest("P8", securityStatus: HRN, expectedSecurityStatus: NOT, rcnDirection: string.Empty, dcnDirection: IMP);

		public void TestStandalonePackage_ShouldUpdateFromSECToNOT_Direction() => RunStandalonePackageTest("P9", securityStatus: SEC, expectedSecurityStatus: NOT, rcnDirection: string.Empty, dcnDirection: DOM);

		public void TestStandalonePackage_ShouldUpdateFromSCRToNOT_Direction() => RunStandalonePackageTest("P10", securityStatus: SEC, expectedSecurityStatus: NOT, rcnDirection: EXP, dcnDirection: IMP);

		public void TestStandalonePackage_ShouldUpdateOVPPackage() => RunStandalonePackageTest("P11", securityStatus: SEC, expectedSecurityStatus: NOT, isOVP: true, transportMode: SEA);

		public void TestStandalonePackage_ShouldNotUpdate_NOT() => RunStandalonePackageTest("P12", securityStatus: NOT, expectedSecurityStatus: NOT, transportMode: SEA);

		public void TestStandalonePackage_ShouldNotUpdate_OVR() => RunStandalonePackageTest("P13", securityStatus: OVR, expectedSecurityStatus: OVR, transportMode: SEA);

		public void TestStandalonePackage_ShouldNotUpdate_TransportMode_AIR() => RunStandalonePackageTest("P14", securityStatus: REQ, expectedSecurityStatus: REQ, transportMode: AIR);

		public void TestStandalonePackage_ShouldNotUpdate_TransportMode_Empty() => RunStandalonePackageTest("P15", securityStatus: REQ, expectedSecurityStatus: REQ, transportMode: "");

		public void TestStandalonePackage_ShouldNotUpdate_RCN_IMP_DCN_IMP() => RunStandalonePackageTest("P16", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: IMP, dcnDirection: IMP);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_IMP_DCN_DOM() => RunStandalonePackageTest("P17", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: IMP, dcnDirection: DOM);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_DOM_DCN_DOM() => RunStandalonePackageTest("P18", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: DOM, dcnDirection: DOM);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_DOM_DCN_IMP() => RunStandalonePackageTest("P19", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: DOM, dcnDirection: IMP);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_IMP_DCN_EXP() => RunStandalonePackageTest("P20", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: IMP, dcnDirection: EXP);
		
		public void TestStandalonePackage_ShouldNotUpdate_RCN_DOM_DCN_EXP() => RunStandalonePackageTest("P21", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: DOM, dcnDirection: EXP);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_IMP_DCN_EMPTY() => RunStandalonePackageTest("P22", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: IMP, dcnDirection: string.Empty);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_DOM_DCN_EMPTY() => RunStandalonePackageTest("P23", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: DOM, dcnDirection: string.Empty);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_EXP_DCN_EXP() => RunStandalonePackageTest("P24", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: EXP, dcnDirection: EXP);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_EXP_DCN_EMPTY() => RunStandalonePackageTest("P25", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: EXP, dcnDirection: string.Empty);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_EMPTY_DCN_EXP() => RunStandalonePackageTest("P26", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: string.Empty, dcnDirection: EXP);

		public void TestStandalonePackage_ShouldNotUpdate_RCN_EMPTY_DCN_EMPTY() => RunStandalonePackageTest("P27", securityStatus: REQ, expectedSecurityStatus: REQ, rcnDirection: string.Empty, dcnDirection: string.Empty);

		public void TestStandalonePackage_ShouldNotUpdate_HasDTU() => RunStandalonePackageTest("P28", securityStatus: SCR, expectedSecurityStatus: SCR, transportMode: SEA, hasDTU: true);

		public void TestStandalonePackage_ShouldNotUpdate_AdjustedOutPackage() => RunStandalonePackageTest("P29", securityStatus: SCR, expectedSecurityStatus: SCR, transportMode: SEA, isAdjustedOut: true);

		void RunStandalonePackageTest(string description, string securityStatus, string expectedSecurityStatus, string transportMode = AIR, string rcnDirection = IMP, string dcnDirection = IMP, bool isOVP = false, bool hasDTU = false, bool isAdjustedOut = false)
		{
			var sql = new SqlQueryBuilder();
			SetWarehouseTransitSecurityProcessingRequired(warehouse, value: true);
			var (_, packageState) = CreatePackage(sql, description, isHandlingUnit: isOVP, isOverpack: isOVP, transportMode: transportMode, hasDTUFK: hasDTU, defaultSecurityStatus: securityStatus, isAdjustedOut: isAdjustedOut);

			sql.AppendLine($"UPDATE dbo.WhsItemReceiveConsignment SET WRC_Direction = '{rcnDirection}', WRC_SystemLastEditTimeUtc=GETUTCDATE(), WRC_SystemLastEditUser='ABC' WHERE WRC_PK = '{rcn.PK}';");
			sql.AppendLine($"UPDATE dbo.WhsItemDispatchConsignment SET WDC_Direction = '{dcnDirection}', WDC_SystemLastEditTimeUtc=GETUTCDATE(), WDC_SystemLastEditUser='ABC' WHERE WDC_PK = '{dcn.PK}';");

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			var lastEditUser = securityStatus == expectedSecurityStatus ? "A" : "~BP";

			assertTestResult(packageState.PK, expectedSecurityStatus, lastEditUser);
		}
		#endregion

		#region Handling Unit

		public void TestHandlingUnit_WithOneChildPackage() => RunHandlingUnitTest(expectedSecurityStatus: NOT);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs1() => RunHandlingUnitTest(expectedSecurityStatus: NOT, extraChildPackagesShouldNotBeUpdated: [OVR]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs2() => RunHandlingUnitTest(expectedSecurityStatus: NOT, extraChildPackagesShouldNotBeUpdated: [OVR, SCR]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs3() => RunHandlingUnitTest(expectedSecurityStatus: NOT, extraChildPackagesShouldNotBeUpdated: [OVR, SCR, SEC]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs4() => RunHandlingUnitTest(expectedSecurityStatus: NOT, extraChildPackagesShouldNotBeUpdated: [OVR, SCR, SEC, NOT]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs5() => RunHandlingUnitTest(expectedSecurityStatus: REQ, extraChildPackagesShouldNotBeUpdated: [OVR, SCR, SEC, NOT, REQ]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs6() => RunHandlingUnitTest(expectedSecurityStatus: HRN, extraChildPackagesShouldNotBeUpdated: [OVR, SCR, SEC, NOT, REQ, HRN]);

		public void TestHandlingUnit_WithMultipleChildPackagesFromDifferentDLLs7() => RunHandlingUnitTest(expectedSecurityStatus: HRS, extraChildPackagesShouldNotBeUpdated: [OVR, SCR, SEC, NOT, REQ, HRN, HRS]);

		public void RunHandlingUnitTest(string expectedSecurityStatus, params string[] extraChildPackagesShouldNotBeUpdated)
		{
			var sql = new SqlQueryBuilder();
			SetWarehouseTransitSecurityProcessingRequired(warehouse, value: true);

			var (topHUPackage, topHUPackageState) = CreatePackage(sql, "TopHU", isHandlingUnit: true);
			var (midHUPackage, midHUPackageState) = CreatePackage(sql, "MidHU", isHandlingUnit: true);

			var childPackageSHouldBeUpdatedToNOT = CreatePackage(sql, "child", defaultSecurityStatus: REQ, transportMode: SEA).package;
			var childPackages = new List<PkgPackage> { childPackageSHouldBeUpdatedToNOT };

			foreach (var child in extraChildPackagesShouldNotBeUpdated)
			{
				childPackages.Add(CreatePackage(sql, child, defaultSecurityStatus: child, transportMode: AIR).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(topHUPackage, midHUPackage);
			PackPackageIntoHandlingUnit(midHUPackage, childPackages.ToArray());

			RunTransformation();

			assertTestResult(topHUPackageState.PK, expectedSecurityStatus, "~BP");
			assertTestResult(midHUPackageState.PK, expectedSecurityStatus, "~BP");
		}

		void assertTestResult(Guid packageStatePK, string expectedSecurityStatus, string lastEditUser)
		{
			WhsItemPackageState.AssertFromDB(TestConnection, packageStatePK)
				.ExpectEquals($"WPS_SecurityStatus should be {expectedSecurityStatus}", p => p.WPS_SecurityStatus, expectedSecurityStatus)
				.ExpectEquals($"WPS_SystemLastEditUser should be {lastEditUser}", p => p.WPS_SystemLastEditUser, lastEditUser)
				.VerifyAll();
		}

		#endregion

		#region Implementation

		#region Override

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePackageStateSecurityStatusWhenSendDispatchInstruction();

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_1] ON [dbo].[WhsItemPackageState] ([WPS_KP_Package]) INCLUDE ([WPS_SecurityStatus], [WPS_UnitType]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_2] ON [dbo].[WhsItemDispatchLoadList] ([WDL_PK], [WDL_TransportMode]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_3] ON [dbo].[WhsItemPackageState] ([WPS_UnitType], [WPS_SecurityStatus], [WPS_WDH_TransitDispatchHeader], [WPS_AdjustedOut]) INCLUDE ([WPS_KP_Package], [WPS_WDC_TransitDispatchConsignment], [WPS_WDL_LoadList], [WPS_WRC_TransitReceiveConsignment]) WHERE (([WPS_UnitType] IN ('OVP', 'PKL', 'PKG')) AND [WPS_AdjustedOut]='' AND [WPS_WDH_TransitDispatchHeader] IS NULL AND [WPS_SecurityStatus]<>'NOT' AND [WPS_SecurityStatus]<>'OVR') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_4] ON [dbo].[WhsItemReceiveConsignment] ([WRC_PK], [WRC_Direction]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_5] ON [dbo].[WhsItemDispatchConsignment] ([WDC_PK], [WDC_Direction]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status when send dispatch instruction and fix for DCN direction is Import/Domestic_6] ON [dbo].[PkgPackage] ([KP_PK]) INCLUDE ([KP_KP_TopHandlingUnitPackage]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];
		#endregion

		void PackPackageIntoHandlingUnit(PkgPackage handlingUnit, params PkgPackage[] children)
		{
			var sql = new SqlQueryBuilder();

			foreach (var child in children)
			{
				child.KP_KP_TopHandlingUnitPackage = handlingUnit.KP_KP_TopHandlingUnitPackage ?? handlingUnit;
				sql.AppendLine($"UPDATE dbo.PkgPackage SET KP_KP_TopHandlingUnitPackage = '{child.KP_KP_TopHandlingUnitPackage.FK}', KP_SystemLastEditTimeUtc=GETUTCDATE(), KP_SystemLastEditUser='ABC' WHERE KP_PK = '{child.PK}';");
				new PkgPackageHandlingUnitDivot(handlingUnit.PK, child.PK).AppendInsertAndReturnObject(sql);
			}

			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		#region Create Methods

		(PkgPackage package, WhsItemPackageState packageState) CreatePackage(SqlQueryBuilder sql, string description, bool isHandlingUnit = false, bool isOverpack = false, bool isSecure = false, string transportMode = null, bool hasDTUFK = false, bool isHighRisk = false, bool isHighRiskAuthorized = false, string defaultSecurityStatus = SCR, bool isAdjustedOut = false)
		{
			var package = new PkgPackage(packageJob, "PLT", 1)
			{
				KP_GoodsDescription = description
			}.AppendInsertAndReturnObject(sql);

			var packageState = new WhsItemPackageState(package.PK, warehouse, rcn.PK, isAdjustedOut ? "ADJ" : "PUT")
			{
				WPS_WL_LastLocation = location.PK,
				WPS_WL_ReceiveLocation = location.PK,
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_IsSecure = isSecure,
				WPS_IsHandlingUnit = isHandlingUnit,
				WPS_SecurityStatus = defaultSecurityStatus,
				WPS_IsHighRisk = isHighRisk,
				WPS_IsHighRiskAuthorized = isHighRiskAuthorized,
				WPS_UnitType = !isHandlingUnit ? "PKG" : isOverpack ? "OVP" : "HU",
				WPS_AdjustedOut = isAdjustedOut ? "LCC" : string.Empty,
			};

			if (transportMode != null)
			{
				var dll = new WhsItemDispatchLoadList($"DLL-{description}", warehouse)
				{
					WDL_WL_StagingLocation = location.PK,
					WDL_TransportMode = transportMode
				}.AppendInsertAndReturnObject(sql);

				packageState.WPS_WDL_LoadList = dll.PK;
			}

			if (hasDTUFK)
			{
				packageState.WPS_WDH_TransitDispatchHeader = dtu.PK;
				packageState.WPS_Status = "FLO";
				packageState.WPS_LoadedTime = new DateTimeOffset(2022, 12, 12, 12, 0, 0, TimeSpan.FromMinutes(480));
			}

			packageState.AppendInsertAndReturnObject(sql);

			return (package, packageState);
		}

		void SetWarehouseTransitSecurityProcessingRequired(WhsWarehouse warehouse, bool value)
		{
			TestConnection.ExecuteNonQuery($"UPDATE dbo.WhsWarehouse SET WW_TransitSecurityProcessingRequired={(value ? 1 : 0)}, WW_SystemLastEditTimeUtc=GETUTCDATE(), WW_SystemLastEditUser='ABC' WHERE WW_PK='{warehouse.PK}';");
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			var sql = new SqlQueryBuilder();

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			warehouse = new WhsWarehouse("WH1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(warehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			rcn = new WhsItemReceiveConsignment(warehouse, "RC1", "RC1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			rtu = new WhsItemReceiveTransportationUnit(warehouse, "rtu", location, "rtu")
			{
				WRH_GateInTime = new DateTimeOffset(2021, 7, 6, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WRH_UnloadCompleteTime = new DateTimeOffset(2021, 7, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WRH_UnloadCompleteNotYetProcessedTime = new DateTimeOffset(2021, 7, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WRH_GateOutTime = new DateTimeOffset(2021, 7, 8, 11, 0, 0, TimeSpan.FromMinutes(480)),
			}.AppendInsertAndReturnObject(sql);
			packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			dcn = new WhsItemDispatchConsignment(warehouse, "DCN1", "DCN1", string.Empty).AppendInsertAndReturnObject(sql);
			dtu = new WhsItemDispatchTransportationUnit(warehouse, "DTU1")
			{
				WDH_VehicleReference = "DTU1",
				WDH_GateInTime = new DateTimeOffset(2021, 7, 7, 10, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_LoadCompleteTime = new DateTimeOffset(2021, 7, 7, 11, 0, 0, TimeSpan.FromMinutes(480)),
				WDH_GateOutTime = new DateTimeOffset(2021, 7, 7, 12, 0, 0, TimeSpan.FromMinutes(480))
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		#endregion

		#region TestData

		WhsWarehouse warehouse;
		WhsLocation location;

		WhsItemReceiveConsignment rcn;
		WhsItemReceiveTransportationUnit rtu;

		WhsItemDispatchConsignment dcn;
		WhsItemDispatchTransportationUnit dtu;

		PkgPackageJob packageJob;

		#endregion

		#region consts

		const string SCR = "SCR";
		const string REQ = "REQ";
		const string NOT = "NOT";
		const string SEC = "SEC";
		const string OVR = "OVR";
		const string HRS = "HRS";
		const string HRN = "HRN";

		const string AIR = "AIR";
		const string COU = "COU";
		const string ROA = "ROA";
		const string RAI = "RAI";
		const string SEA = "SEA";

		const string IMP = "IMP";
		const string EXP = "EXP";
		const string DOM = "DOM";

		#endregion

		#endregion
	}
}
