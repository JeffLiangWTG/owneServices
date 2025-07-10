using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(UpdatePackageJobToMatchRCN))]
	public class UpdatePackageJobToMatchRCNTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Package Job to Match RCN._1] ON [dbo].[WhsItemPackageState] ([WPS_SystemLastEditTimeUtc]) INCLUDE ([WPS_KP_Package], [WPS_WRC_TransitReceiveConsignment]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Package Job to Match RCN._2] ON [dbo].[PkgPackage] ([KP_PK]) INCLUDE ([KP_AutoVersion], [KP_KPH_PackageHeader]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Package Job to Match RCN._3] ON [dbo].[PkgPackageJob] ([KJ_ParentTableCode], [KJ_ParentID]) WHERE ([KJ_ParentTableCode]='WRC') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		public void TestReassignedPackages_Duplicate_PackageID_RCNPackageJob()
		{
			var sql = SetupBasicTestData();
			var yesterday = DateTime.Today.AddDays(-1);

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			var header1 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rcn2 = new PkgPackage(packageJobFromRCN2, "PLT", 1) { KP_GoodsDescription = "P1", KP_KPH_PackageHeader = header1 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rcn2.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			var header2 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rcn1 = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P2", KP_KPH_PackageHeader = header2 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rcn1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
			RunTransformationTwice();

			PkgPackage.AssertFromDB(TestConnection, package_rcn1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN1.PK)
				.VerifyAll();
		}

		public void TestReassignedPackages_Duplicate_PackageID_RTUPackageJob()
		{
			var sql = SetupBasicTestData();
			var yesterday = DateTime.Today.AddDays(-1);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRTU1 = new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			var header1 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rcn2 = new PkgPackage(packageJobFromRCN2, "PLT", 1) { KP_GoodsDescription = "P1", KP_KPH_PackageHeader = header1 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rcn2.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			var header2 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rtu1 = new PkgPackage(packageJobFromRTU1, "PLT", 1) { KP_GoodsDescription = "P2", KP_KPH_PackageHeader = header2 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rtu1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
			RunTransformationTwice();

			PkgPackage.AssertFromDB(TestConnection, package_rtu1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRTU1.PK)
				.VerifyAll();
		}

		public void TestReassignedPackages_Duplicate_PackageID_MultiplePackageJobTypes()
		{
			var sql = SetupBasicTestData();
			var yesterday = DateTime.Today.AddDays(-1);

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRTU1 = new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			var header1 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rcn1 = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P1", KP_KPH_PackageHeader = header1 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rcn1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			var header2 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rtu1 = new PkgPackage(packageJobFromRTU1, "PLT", 1) { KP_GoodsDescription = "P2", KP_KPH_PackageHeader = header2 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rtu1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
			RunTransformationTwice();

			PkgPackage.AssertFromDB(TestConnection, package_rcn1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN1.PK)
				.VerifyAll();

			PkgPackage.AssertFromDB(TestConnection, package_rtu1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRTU1.PK)
				.VerifyAll();
		}

		public void TestReassignedPackages_Duplicate_LooseID_RCNPackageJob()
		{
			var sql = SetupBasicTestData();
			var yesterday = DateTime.Today.AddDays(-1);

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			var header1 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var pivot_rcn2 = new PkgPackageJobPackageHeaderPivot(packageJobFromRCN2, header1).AppendInsertAndReturnObject(sql);

			var header2 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rcn1 = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P2", KP_KPH_PackageHeader = header2 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rcn1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
			RunTransformationTwice();

			PkgPackage.AssertFromDB(TestConnection, package_rcn1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN1.PK)
				.VerifyAll();
		}

		public void TestReassignedPackages_Duplicate_LooseID_RTUPackageJob()
		{
			var sql = SetupBasicTestData();
			var yesterday = DateTime.Today.AddDays(-1);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRTU1 = new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			var header1 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var pivot_rcn2 = new PkgPackageJobPackageHeaderPivot(packageJobFromRCN2, header1).AppendInsertAndReturnObject(sql);

			var header2 = new PkgPackageHeader("Duplicate", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			var package_rtu1 = new PkgPackage(packageJobFromRTU1, "PLT", 1) { KP_GoodsDescription = "P2", KP_KPH_PackageHeader = header2 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package_rtu1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
			RunTransformationTwice();

			PkgPackage.AssertFromDB(TestConnection, package_rtu1.PK)
				.ExpectEquals("do not change packageJob if it will result in duplicate", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRTU1.PK)
				.VerifyAll();
		}

		StringBuilder SetupBasicTestData()
		{
			var sql = new StringBuilder();

			branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			whs1 = new WhsWarehouseOld_V01("WH1", "TRW", branch1.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(whs1, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "A1").AppendInsertAndReturnObject(sql);
			stagingLocation1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);
			var row12 = new WhsRow(whs1, "R12").AppendInsertAndReturnObject(sql);
			var area12 = new WhsArea(whs1.PK, "A12").AppendInsertAndReturnObject(sql);
			stagingLocation12 = new WhsLocation(row12.PK, area12.PK, area12.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			return sql;
		}

		void ExecuteQuery(StringBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, PkgPackageSchema.Constants.PK, TestWhsDataSetupHelper.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, PkgPackageHandlingUnitDivotSchema.Constants.PK, TestWhsDataSetupHelper.PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		#region Override

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePackageJobToMatchRCN();

		protected override void AssertTransformationResults()
		{
			PkgPackage.AssertFromDB(TestConnection, ovp1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package1_ovp1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package2_ovp1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();

			PkgPackage.AssertFromDB(TestConnection, hu1.PK)
				.ExpectEquals("no change to handling unit", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromKPU1.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package3_HU1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package4_HU1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();

			PkgPackage.AssertFromDB(TestConnection, package5_matched.PK)
				.ExpectEquals("no change when package job already matches", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN1.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package6_mismatched.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, package7_rtu1.PK)
				.ExpectEquals("package job should match package state RCN", p => p.KP_KJ_ParentPackageJob.FK, packageJobFromRCN2.PK)
				.VerifyAll();
		}

		protected override void PrepareTestData()
		{
			var sql = SetupBasicTestData();

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignment(whs1, "RCN2", "RCN2", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			packageJobFromRCN2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN2" }.AppendInsertAndReturnObject(sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", stagingLocation1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRTU1 = new PkgPackageJob(rtu1.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RTU1" }.AppendInsertAndReturnObject(sql);

			// Overpack with pkg 1 & 2
			var yesterday = DateTime.Today.AddDays(-1);
			var header_ovp1 = new PkgPackageHeader("Overpack1", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			ovp1 = new PkgPackage(packageJobFromRCN1, "PLT", 0) { KP_GoodsDescription = "O1", KP_KPH_PackageHeader = header_ovp1 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, ovp1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1, receiveLocation: stagingLocation12, isHandlingUnit: true, unitType: "OVP");

			package1_ovp1 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 1) { KP_GoodsDescription = "P1" }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1_ovp1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);
			package2_ovp1 = new PkgPackage(packageJobFromRCN1, ovp1, "PLT", 0) { KP_GoodsDescription = "P2" }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2_ovp1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);
			new PkgPackageHandlingUnitDivot(ovp1.PK, package1_ovp1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(ovp1.PK, package2_ovp1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);

			var kpu1 = new PkgHandlingUnit(branch1.PK);
			packageJobFromKPU1 = new PkgPackageJob(kpu1.PK) { KJ_ParentTableCode = "KPU", KJ_JobID = "KPU1" }.AppendInsertAndReturnObject(sql);
			hu1 = new PkgPackage(packageJobFromKPU1, "PLT", 0) { KP_GoodsDescription = "HU1" }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, hu1.PK, "PUT", whs1, null, rtu1, lastLocation: stagingLocation1, receiveLocation: stagingLocation12, isHandlingUnit: true, unitType: "HU");

			package3_HU1 = new PkgPackage(packageJobFromRCN1, hu1, "PLT", 1) { KP_GoodsDescription = "P3" }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3_HU1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);
			package4_HU1 = new PkgPackage(packageJobFromRCN1, hu1, "PLT", 1) { KP_GoodsDescription = "P4" }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package4_HU1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			new PkgPackageHandlingUnitDivot(hu1.PK, package3_HU1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(hu1.PK, package4_HU1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "AAA" }.AppendInsertAndReturnObject(sql);

			var header_p5 = new PkgPackageHeader("Package5", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			package5_matched = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P5", KP_KPH_PackageHeader = header_p5 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package5_matched.PK, "PUT", whs1, rcn1, rtu1, lastLocation: stagingLocation1);

			var header_p6 = new PkgPackageHeader("Package6", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			package6_mismatched = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P6", KP_KPH_PackageHeader = header_p6 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package6_mismatched.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			var header_p7 = new PkgPackageHeader("Package7", yesterday, "~BP").AppendInsertAndReturnObject(sql);
			package7_rtu1 = new PkgPackage(packageJobFromRTU1, "PLT", 1) { KP_GoodsDescription = "P7", KP_KPH_PackageHeader = header_p7 }.AppendInsertAndReturnObject(sql);
			TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package7_rtu1.PK, "PUT", whs1, rcn2, rtu1, lastLocation: stagingLocation1);

			ExecuteQuery(sql);
		}

		#endregion

		#region Setup

		void RunTransformationTwice()
		{
			GetNewTestTransformationInstance().Run();
			GetNewTestTransformationInstance().Run();
		}

		GlbBranch branch1;
		WhsWarehouseOld_V01 whs1;
		WhsLocation stagingLocation1;
		WhsLocation stagingLocation12;

		PkgPackage ovp1;
		PkgPackage package1_ovp1;
		PkgPackage package2_ovp1;
		PkgPackage hu1;
		PkgPackage package3_HU1;
		PkgPackage package4_HU1;
		PkgPackage package5_matched;
		PkgPackage package6_mismatched;
		PkgPackage package7_rtu1;

		PkgPackageJob packageJobFromKPU1;
		PkgPackageJob packageJobFromRCN1;
		PkgPackageJob packageJobFromRCN2;

		#endregion
	}
}
