using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransitWarehouse
{
	[TestedType(typeof(ReleasePackageIfNoAnyOpenVariance))]
	public class ReleasePackageIfNoAnyOpenVarianceTest : DataTransformationTestCase
	{
		Guid packageShouldBeReleasePK;
		Guid packageHoldShouldBeRemovePK;
		Guid packageShouldNotBeReleasePK;
		Guid packageHoldShouldNotBeRemovePK;
		Guid innerPackageShouldBeReleasePK;
		Guid innerPackageHoldShouldBeRemovePK;
		Guid innerPackageShouldNotBeReleasePK;
		Guid innerPackageHoldShouldNotBeRemovePK;
		Guid notLCCPackageShouldNotBeReleasePK;
		Guid notLCCPackageHoldShouldNotBeRemovePK;

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouseOld_V01("WH1", "TRW", branch1.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(whs1, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(whs1.PK, "A1").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 2, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			var rcn1 = new WhsItemReceiveConsignment(whs1, "RCN1", "RCN1", "STD", "CNNJG").AppendInsertAndReturnObject(sql);
			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", location1, "RTU1").AppendInsertAndReturnObject(sql);
			var packageJobFromRCN1 = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "RCN1" }.AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(packageJobFromRCN1, "HU", 1) { KP_GoodsDescription = "P1", KP_IsHeld = true }.AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "PUT", whs1, rcn1, rtu1, lastLocation: location1);
			var packageHold1 = new PkgPackageHold(package1, "LCC").AppendInsertAndReturnObject(sql);
			packageHoldShouldBeRemovePK = packageHold1.PK;
			packageShouldBeReleasePK = package1.PK;

			var innerPackage1 = new PkgPackage(packageJobFromRCN1, package1, "BOX", 1) { KP_GoodsDescription = "InnerP1", KP_IsHeld = true }.AppendInsertAndReturnObject(sql);
			var innerPackageHold1 = new PkgPackageHold(innerPackage1, "LCC").AppendInsertAndReturnObject(sql);
			innerPackageHoldShouldBeRemovePK = innerPackageHold1.PK;
			innerPackageShouldBeReleasePK = innerPackage1.PK;

			var package2 = new PkgPackage(packageJobFromRCN1, "PLT", 1) { KP_GoodsDescription = "P2", KP_IsHeld = true }.AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "PUT", whs1, rcn1, rtu1, lastLocation: location1);
			var packageHold2 = new PkgPackageHold(package2, "LCC").AppendInsertAndReturnObject(sql);
			packageHoldShouldNotBeRemovePK = packageHold2.PK;
			packageShouldNotBeReleasePK = package2.PK;

			var innerPackage2 = new PkgPackage(packageJobFromRCN1, package2, "BOX", 1) { KP_GoodsDescription = "InnerP2", KP_IsHeld = true }.AppendInsertAndReturnObject(sql);
			var innerPackageHold2 = new PkgPackageHold(innerPackage2, "LCC").AppendInsertAndReturnObject(sql);
			innerPackageHoldShouldNotBeRemovePK = innerPackageHold2.PK;
			innerPackageShouldNotBeReleasePK = innerPackage2.PK;

			var package3 = new PkgPackage(packageJobFromRCN1, "HU", 1) { KP_GoodsDescription = "P3", KP_IsHeld = true }.AppendInsertAndReturnObject(sql);
			var packageState3 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package3.PK, "PUT", whs1, rcn1, rtu1, lastLocation: location1);
			var packageHold3 = new PkgPackageHold(package3, "HEL").AppendInsertAndReturnObject(sql);
			notLCCPackageHoldShouldNotBeRemovePK = packageHold3.PK;
			notLCCPackageShouldNotBeReleasePK = package3.PK;

			var now = DateTime.Now;
			var cycleCount1 = new WhsItemCycleCountLocation(location1.PK, 20, "CC1") { WIC_Status = "CMP", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now, WIC_EndTime = now, WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			new WhsItemCycleCountLocationVariance(cycleCount1, "APP", varianceQty: -1) { WIV_WPS_PackageState = packageState1.PK }.AppendInsertAndReturnObject(sql);
			new WhsItemCycleCountLocationVariance(cycleCount1, "APP", varianceQty: -1) { WIV_WPS_PackageState = packageState3.PK }.AppendInsertAndReturnObject(sql);

			var cycleCount2 = new WhsItemCycleCountLocation(location2.PK, 20, "CC2") { WIC_Status = "CMP", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now, WIC_EndTime = now, WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			new WhsItemCycleCountLocationVariance(cycleCount2, "APP", varianceQty: -1) { WIV_WPS_PackageState = packageState2.PK }.AppendInsertAndReturnObject(sql);

			var cycleCount3 = new WhsItemCycleCountLocation(location2.PK, 20, "CC3") { WIC_Status = "CMP", WIC_StartTime = now.AddMinutes(-10), WIC_ProcessingTime = now, WIC_EndTime = now, WIC_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			new WhsItemCycleCountLocationVariance(cycleCount3, "OPN", varianceQty: 0) { WIV_WPS_PackageState = packageState2.PK }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		protected override void AssertTransformationResults()
		{
			PkgPackageHold.AssertFromDB(TestConnection, packageHoldShouldBeRemovePK)
				.ExpectNotEquals("P1's hold should be removed", hold => hold.KHR_RemovedTime, null)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, packageShouldBeReleasePK)
				.ExpectEquals("P1 should be released", package => package.KP_IsHeld, false)
				.VerifyAll();

			PkgPackageHold.AssertFromDB(TestConnection, innerPackageHoldShouldBeRemovePK)
				.ExpectNotEquals("innerP1's hold should be removed", hold => hold.KHR_RemovedTime, null)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, innerPackageShouldBeReleasePK)
				.ExpectEquals("innerP1 should be released", package => package.KP_IsHeld, false)
				.VerifyAll();

			PkgPackageHold.AssertFromDB(TestConnection, packageHoldShouldNotBeRemovePK)
				.ExpectEquals("P2's hold should not be removed", hold => hold.KHR_RemovedTime, null)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, packageShouldNotBeReleasePK)
				.ExpectEquals("P2 should be released", package => package.KP_IsHeld, true)
				.VerifyAll();

			PkgPackageHold.AssertFromDB(TestConnection, innerPackageHoldShouldNotBeRemovePK)
				.ExpectEquals("Inner P2's hold should not be removed", hold => hold.KHR_RemovedTime, null)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, innerPackageShouldNotBeReleasePK)
				.ExpectEquals("Inner P2 should not be released", package => package.KP_IsHeld, true)
				.VerifyAll();

			PkgPackageHold.AssertFromDB(TestConnection, notLCCPackageHoldShouldNotBeRemovePK)
				.ExpectEquals("P3's hold should not be removed", hold => hold.KHR_RemovedTime, null)
				.VerifyAll();
			PkgPackage.AssertFromDB(TestConnection, notLCCPackageShouldNotBeReleasePK)
				.ExpectEquals("P3 should not be released", package => package.KP_IsHeld, true)
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ReleasePackageIfNoAnyOpenVariance();
	}
}
