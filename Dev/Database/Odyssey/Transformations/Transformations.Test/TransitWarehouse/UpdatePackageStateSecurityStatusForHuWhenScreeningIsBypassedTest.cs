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
	[TestedType(typeof(UpdatePackageStateSecurityStatusForHuWhenScreeningIsBypassed))]
	class UpdatePackageStateSecurityStatusForHuWhenScreeningIsBypassedTest : DataTransformationTestCase
	{
		#region Top Handling Unit

		public void TestTopHandlingUnit_SecurityStatusShouldBeOVR() => RunTopHandlingUnitTest(expected: OVR, childPackages: new string[] { OVR, OVR, OVR });

		public void TestTopHandlingUnit_SecurityStatusShouldBeSCR() => RunTopHandlingUnitTest(expected: SCR, childPackages: new string[] { SCR, OVR, OVR });

		public void RunTopHandlingUnitTest(string expected, params string[] childPackages)
		{
			var sql = new SqlQueryBuilder();

			var handlingUnitDescription = "HU";
			var (hu, huState) = CreatePackage(sql, handlingUnitDescription, isHandlingUnit: true);
			var children = new List<PkgPackage>();
			foreach (var child in childPackages)
			{
				children.Add(CreateChildPackage(sql, handlingUnitDescription,  child, isHandlingUnit: false, isOverpack: false).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, children.ToArray());

			RunTransformation();

			WhsItemPackageState.AssertFromDB(TestConnection, huState.PK)
				.ExpectEquals($"WPS_SecurityStatus should be {expected}", p => p.WPS_SecurityStatus, expected)
				.VerifyAll();
		}

		#endregion

		#region Inner Handling Unit - one mid level hu

		public void TestInnerHandlingUnit_SecurityStatusShouldBeOVR() => RunInnerHandlingUnitTest(expected: OVR, childPackages: new string[] { OVR, OVR, OVR });

		public void TestInnerHandlingUnit_SecurityStatusShouldBeSCR() => RunInnerHandlingUnitTest(expected: SCR, childPackages: new string[] { SCR, OVR, OVR });

		public void RunInnerHandlingUnitTest(string expected, params string[] childPackages)
		{
			var sql = new SqlQueryBuilder();

			var handlingUnitDescription = "HU";
			var (hu, huState) = CreatePackage(sql, handlingUnitDescription, isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			var children = new List<PkgPackage>();
			foreach (var child in childPackages)
			{
				children.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: false, isOverpack: false).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, innerHU);
			PackPackageIntoHandlingUnit(innerHU, children.ToArray());

			RunTransformation();

			var packageStates = WhsItemPackageState.ShallowLoadFromDB(TestConnection, ps => ps.WPS_SecurityStatus == expected && (ps.PK == huState.PK || ps.PK == innerHUState.PK));
			AssertEquals(2, packageStates.Length);
		}

		#endregion

		#region Inner Handling Unit - many mid level hu

		public void TestManyInnerHandlingUnit_SecurityStatusShouldBeOVR() => RunManyInnerHandlingUnitTest(expected: OVR, childPackages: new string[] { OVR, OVR, OVR });

		public void TestManyInnerHandlingUnit_SecurityStatusShouldBeSCR() => RunManyInnerHandlingUnitTest(expected: SCR, childPackages: new string[] { SCR, OVR, OVR });

		public void RunManyInnerHandlingUnitTest(string expected, params string[] childPackages)
		{
			var sql = new SqlQueryBuilder();

			var (hu, huState) = CreatePackage(sql, "TOPHU", isHandlingUnit: true);
			var (subHU, subHUState) = CreatePackage(sql, "SUBHU", isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			var children = new List<PkgPackage>();
			foreach (var child in childPackages)
			{
				children.Add(CreateChildPackage(sql, "HU", child, isHandlingUnit: false, isOverpack: false).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, subHU);
			PackPackageIntoHandlingUnit(subHU, innerHU);
			PackPackageIntoHandlingUnit(innerHU, children.ToArray());

			RunTransformation();

			var packageStates = WhsItemPackageState.ShallowLoadFromDB(TestConnection, ps => ps.WPS_SecurityStatus == expected && (ps.PK == huState.PK || ps.PK == subHUState.PK || ps.PK == innerHUState.PK));
			AssertEquals(3, packageStates.Length);
		}

		#endregion

		#region Inner Handling Unit - with mid level hu

		public void TestMidHandlingUnit_SecurityStatusShouldBeOVR() => RunMidHandlingUnitTest(expected: OVR, childPackages1: new string[] { OVR, OVR, OVR }, childPackages2: new string[] { OVR, OVR, OVR });

		public void TestMidHandlingUnit_SecurityStatusShouldBeOVR2() => RunMidHandlingUnitTest(expected: SCR, childPackages1: new string[] { OVR, OVR, OVR }, childPackages2: new string[] { SCR, OVR, OVR });

		public void RunMidHandlingUnitTest(string expected, string[] childPackages1, string[] childPackages2)
		{
			var sql = new SqlQueryBuilder();

			var handlingUnitDescription = "HU";
			var (hu, huState) = CreatePackage(sql, handlingUnitDescription, isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			var children1 = new List<PkgPackage>();
			foreach (var child in childPackages1)
			{
				children1.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: false, isOverpack: false).package);
			}

			var children2 = new List<PkgPackage>();
			foreach (var child in childPackages2)
			{
				children2.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: false, isOverpack: false).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, innerHU);
			PackPackageIntoHandlingUnit(innerHU, children1.ToArray());
			PackPackageIntoHandlingUnit(hu, children2.ToArray());

			RunTransformation();

			var packageStates = WhsItemPackageState.ShallowLoadFromDB(TestConnection, ps => ps.WPS_SecurityStatus == expected && (ps.PK == huState.PK || ps.PK == innerHUState.PK));
			AssertEquals(2, packageStates.Length);
		}

		#endregion
		
		#region Inner Handling Unit - with two mid level hu

		public void TestWithTwoMidHandlingUnit_SecurityStatusShouldBeOVR() => RunWithTwoMidHandlingUnitTest(expected: OVR, childPackages1: new string[] { OVR, OVR, OVR }, childPackages2: new string[] { OVR, OVR, OVR });

		public void TestWithTwoMidHandlingUnit_SecurityStatusShouldBeSCR2() => RunWithTwoMidHandlingUnitTest(expected: SCR, childPackages1: new string[] { SCR, OVR, OVR }, childPackages2: new string[] { OVR, OVR, OVR });

		public void RunWithTwoMidHandlingUnitTest(string expected, string[] childPackages1, string[] childPackages2)
		{
			var sql = new SqlQueryBuilder();

			var handlingUnitDescription = "HU";
			var (hu, huState) = CreatePackage(sql, handlingUnitDescription, isHandlingUnit: true);
			var (innerHU1, innerHUState1) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);
			var (innerHU2, innerHUState2) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			var children1 = new List<PkgPackage>();
			foreach (var child in childPackages1)
			{
				children1.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: false, isOverpack: false).package);
			}

			var children2 = new List<PkgPackage>();
			foreach (var child in childPackages2)
			{
				children2.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: false, isOverpack: false).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, innerHU1);
			PackPackageIntoHandlingUnit(hu, innerHU2);
			PackPackageIntoHandlingUnit(innerHU1, children1.ToArray());
			PackPackageIntoHandlingUnit(innerHU2, children2.ToArray());

			RunTransformation();

			var packageStates = WhsItemPackageState.ShallowLoadFromDB(TestConnection, ps => ps.WPS_SecurityStatus == expected && (ps.PK == huState.PK || ps.PK == innerHUState1.PK || ps.PK == innerHUState2.PK));
			AssertEquals(3, packageStates.Length);
		}

		#endregion

		#region Implementation

		#region Override

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePackageStateSecurityStatusForHuWhenScreeningIsBypassed();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();

			var (topHU, topHUState) = CreatePackage(sql, "TOPHU", isHandlingUnit: true);
			var (subHU, subHUState) = CreatePackage(sql, "SUBHU", isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			var (childPackageOVR1, childPackageOVRState1) = CreateChildPackage(sql, "TOPHU", OVR);
			var (childPackageOVR2, childPackageOVRState2) = CreateChildPackage(sql, "TOPHU", OVR);
			
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(topHU, subHU);
			PackPackageIntoHandlingUnit(subHU, innerHU);
			PackPackageIntoHandlingUnit(innerHU, childPackageOVR1);
			PackPackageIntoHandlingUnit(innerHU, childPackageOVR2);

			testData = new List<(WhsItemPackageState packageState, string expectedSecurityStatus)>
			{
				(childPackageOVRState1, OVR),
				(childPackageOVRState2, OVR),
				(topHUState, OVR),
				(subHUState, OVR),
				(innerHUState, OVR),
			};
		}

		protected override void AssertTransformationResults()
		{
			foreach (var (packageState, expectedSecurityStatus) in testData)
			{
				WhsItemPackageState.AssertFromDB(TestConnection, packageState.PK)
					.ExpectEquals($"WPS_SecurityStatus should be {expectedSecurityStatus}", p => p.WPS_SecurityStatus, expectedSecurityStatus)
					.VerifyAll();
			}
		}

		List<(WhsItemPackageState packageState, string expectedSecurityStatus)> testData;

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Update Security Status for HU when screening is bypassed from dll_1] ON [dbo].[PkgPackage] ([KP_PK]) INCLUDE ([KP_KP_TopHandlingUnitPackage]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status for HU when screening is bypassed from dll_2] ON [dbo].[WhsItemPackageState] ([WPS_KP_Package], [WPS_UnitType]) INCLUDE ([WPS_SecurityStatus], [WPS_WDH_TransitDispatchHeader]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Security Status for HU when screening is bypassed from dll_3] ON [dbo].[WhsItemPackageState] ([WPS_UnitType]) INCLUDE ([WPS_KP_Package], [WPS_SecurityStatus], [WPS_WDH_TransitDispatchHeader]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
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

		(PkgPackage, WhsItemPackageState) CreatePackage(SqlQueryBuilder sql, string description, bool isHandlingUnit = false, bool isOverpack = false, bool isSecure = false, string transportMode = null, bool hasDTUFK = false, bool isHighRisk = false, bool isHighRiskAuthorized = false, string defaultSecurityStatus = SCR)
		{
			var package = new PkgPackage(packageJob, "PLT", 1)
			{
				KP_GoodsDescription = description
			}.AppendInsertAndReturnObject(sql);

			var packageState = new WhsItemPackageState(package.PK, warehouse, rcn.PK, "PUT")
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

		(PkgPackage package, WhsItemPackageState packageState) CreateChildPackage(SqlQueryBuilder sql, string handlingUnitDescription, string securityStatus, bool isHandlingUnit = false, bool isOverpack = false)
		{
			switch (securityStatus)
			{
				case SCR:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-SCR", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: SCR, isSecure: true, transportMode: AIR);
				case OVR:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-OVR", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: OVR);
				default:
					throw new NotImplementedException();
			}
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
		const string OVR = "OVR";
		const string AIR = "AIR";

		#endregion

		#endregion
	}
}
