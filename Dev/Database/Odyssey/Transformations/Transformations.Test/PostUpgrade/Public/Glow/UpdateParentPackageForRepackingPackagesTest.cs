using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	[TestedType(typeof(UpdateParentPackageForRepackingPackages))]
	public class UpdateParentPackageForRepackingPackagesTest : DataTransformationTestCase
	{
		Guid ovp1PK;
		Guid ovp2PK;
		Guid pkg1PK;
		Guid pkg2PK;
		Guid pkg3PK;
		Guid hu1PK;
		Guid pkg4PK;

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update KP_KP_ParentPackage for Repacking Packages_1] ON [dbo].[WhsItemPackageState] ([WPS_UnitType]) INCLUDE ([WPS_KP_Package]) WHERE ([WPS_UnitType]='OVP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update KP_KP_ParentPackage for Repacking Packages_2] ON [dbo].[PkgPackageHandlingUnitDivot] ([KPD_KP_HandlingUnit], [KPD_UnpackedTime]) INCLUDE ([KPD_KP_Package], [KPD_PackedTime]) WHERE ([KPD_UnpackedTime] IS NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update KP_KP_ParentPackage for Repacking Packages_3] ON [dbo].[PkgPackage] ([KP_KP_ParentPackage]) INCLUDE ([KP_Sequence], [KP_SystemLastEditTimeUtc], [KP_SystemLastEditUser]) WHERE ([KP_KP_ParentPackage] IS NULL) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouseOld_V01("WH1", "TRW", branch1.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(whs1, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "A1").AppendInsertAndReturnObject(sql);
			var stagingLocation1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);
			var row12 = new WhsRow(whs1, "R12").AppendInsertAndReturnObject(sql);
			var area12 = new WhsArea(whs1.PK, "A12").AppendInsertAndReturnObject(sql);
			var stagingLocation12 = new WhsLocation(row12.PK, area12.PK, area12.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);
			var ovp1 = new PkgPackage(packageJobFromRCN1, "PLT", 0) { KP_GoodsDescription = "OVP1" }.AppendInsertAndReturnObject(sql);
			var ovpState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, ovp1.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation1, receiveLocation: stagingLocation12, isHandlingUnit: true, unitType: "OVP");
			var package1 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 1) { KP_GoodsDescription = "P1" }.AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation1);
			var package2 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 0) { KP_GoodsDescription = "P2" }.AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation1);

			var divot1 = new PkgPackageHandlingUnitDivot(ovp1.PK, package1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);
			var divot2 = new PkgPackageHandlingUnitDivot(ovp1.PK, package2.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);

			var row2 = new WhsRow(whs1, "R2").AppendInsertAndReturnObject(sql);
			var area2 = new WhsArea(whs1.PK, "A2").AppendInsertAndReturnObject(sql);
			var stagingLocation2 = new WhsLocation(row2.PK, area2.PK, area2.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);
			var row22 = new WhsRow(whs1, "R22").AppendInsertAndReturnObject(sql);
			var area22 = new WhsArea(whs1.PK, "A22").AppendInsertAndReturnObject(sql);
			var stagingLocation22 = new WhsLocation(row22.PK, area22.PK, area22.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			var ovp2 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 0) { KP_GoodsDescription = "OVP2" }.AppendInsertAndReturnObject(sql);
			var ovpState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, ovp2.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation2, receiveLocation: stagingLocation22, isHandlingUnit: true, unitType: "OVP");
			var package3 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 0) { KP_GoodsDescription = "P3" }.AppendInsertAndReturnObject(sql);
			var packageState3 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation2);

			var divot3 = new PkgPackageHandlingUnitDivot(ovp2.PK, package3.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);
			var divot4 = new PkgPackageHandlingUnitDivot(ovp1.PK, ovp2.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);

			var hu1 = new PkgPackage(packageJobFromRCN1, "PLT", 0) { KP_GoodsDescription = "HU1" }.AppendInsertAndReturnObject(sql);
			var huState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, hu1.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation2, receiveLocation: stagingLocation22, isHandlingUnit: true, unitType: "HU");
			var package4 = new PkgPackage(packageJobFromRCN1, hu1, "PLT", 1) { KP_GoodsDescription = "P4" }.AppendInsertAndReturnObject(sql);
			var packageState4 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation2);

			var divot5 = new PkgPackageHandlingUnitDivot(hu1.PK, package4.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, PkgPackageSchema.Constants.PK, TestWhsDataSetupHelper.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, PkgPackageHandlingUnitDivotSchema.Constants.PK, TestWhsDataSetupHelper.PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			ovp1PK = ovp1.PK;
			ovp2PK = ovp2.PK;
			pkg1PK = package1.PK;
			pkg2PK = package2.PK;
			pkg3PK = package3.PK;
			hu1PK = hu1.PK;
			pkg4PK = package4.PK;
		}

		protected override void AssertTransformationResults()
		{
			var instance = GetNewTestTransformationInstance();
			instance.Run();
			PkgPackage.AssertFromDB(TestConnection, ovp1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg2PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, ovp2PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg3PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp2PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, hu1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg4PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 1)
				.VerifyAll();

			var instance2 = GetNewTestTransformationInstance();
			instance2.Initialise();
			instance2.Run();
			PkgPackage.AssertFromDB(TestConnection, ovp1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg2PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, ovp2PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp1PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg3PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, ovp2PK)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, hu1PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 0)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, pkg4PK)
				.ExpectEquals("only OVP child pkg's parent at all level will be updated.", p => p.KP_KP_ParentPackage, null)
				.ExpectEquals("only OVP child pkg's sequence at all level will be updated.", p => p.KP_Sequence, 1)
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateParentPackageForRepackingPackages();
	}
}
