using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	[TestedType(typeof(UpdatePkgStateAndRCNCustomsStatus))]
	public class UpdatePkgStateAndRCNCustomsStatusTest : DataTransformationTestCase
	{
		#region TestsForCustomsControlledWhs

		public void TestPackagesAndRCN_CusWhs() => RunPackagesAndRCNTestCore(null, null, null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_RCNCEN() => RunPackagesAndRCNTestCore(null, null, "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_RCNCRN() => RunPackagesAndRCNTestCore(null, null, "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_RCNPAN() => RunPackagesAndRCNTestCore(null, null, "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN() => RunPackagesAndRCNTestCore(null, "CEN", null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_RCNCEN() => RunPackagesAndRCNTestCore(null, "CEN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_RCNCRN() => RunPackagesAndRCNTestCore(null, "CEN", "CRN", 1, "CLR", defaultCustomStatus, "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CEN_RCNPAN() => RunPackagesAndRCNTestCore(null, "CEN", "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CRN() => RunPackagesAndRCNTestCore(null, "CRN", null, 1, defaultCustomStatus, "CLR", "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_RCNCEN() => RunPackagesAndRCNTestCore(null, "CRN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CRN_RCNCRN() => RunPackagesAndRCNTestCore(null, "CRN", "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_RCNPAN() => RunPackagesAndRCNTestCore(null, "CRN", "PAN", 1, defaultCustomStatus, "CLR", "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_PAN() => RunPackagesAndRCNTestCore(null, "PAN", null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_PAN_RCNCEN() => RunPackagesAndRCNTestCore(null, "PAN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_PAN_RCNCRN() => RunPackagesAndRCNTestCore(null, "PAN", "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_PAN_RCNPAN() => RunPackagesAndRCNTestCore(null, "PAN", "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CEN() => RunPackagesAndRCNTestCore("CEN", "CEN", null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CEN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "CEN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CEN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "CEN", "CRN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CEN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "CEN", "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CRN() => RunPackagesAndRCNTestCore("CEN", "CRN", null, 1, defaultCustomStatus, "CLR", "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_CEN_CRN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "CRN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_CRN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "CRN", "CRN", 1, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CEN_CRN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "CRN", "PAN", 1, defaultCustomStatus, "CLR", "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_CEN_PAN() => RunPackagesAndRCNTestCore("CEN", "PAN", null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "PAN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CEN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "PAN", "CRN", 1, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CEN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "PAN", "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CRN_CRN() => RunPackagesAndRCNTestCore("CRN", "CRN", null, 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_CRN_RCNCEN() => RunPackagesAndRCNTestCore("CRN", "CRN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CRN_CRN_RCNCRN() => RunPackagesAndRCNTestCore("CRN", "CRN", "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_CRN_RCNPAN() => RunPackagesAndRCNTestCore("CRN", "CRN", "PAN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_PAN() => RunPackagesAndRCNTestCore("CRN", "PAN", null, 1, "CLR", defaultCustomStatus, "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("CRN", "PAN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_CRN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("CRN", "PAN", "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_CRN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("CRN", "PAN", "PAN", 1, "CLR", defaultCustomStatus, "CUS", "CLR");
		public void TestPackagesAndRCN_CusWhs_PAN_PAN() => RunPackagesAndRCNTestCore("PAN", "PAN", null, 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_PAN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("PAN", "PAN", "CEN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CusWhs_PAN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("PAN", "PAN", "CRN", 1, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CusWhs_PAN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("PAN", "PAN", "PAN", 1, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);

		#endregion

		#region TestsForNonCustomsControlledWhs

		public void TestPackagesAndRCN() => RunPackagesAndRCNTestCore(null, null, null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_RCNCEN() => RunPackagesAndRCNTestCore(null, null, "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_RCNCRN() => RunPackagesAndRCNTestCore(null, null, "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_RCNPAN() => RunPackagesAndRCNTestCore(null, null, "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN() => RunPackagesAndRCNTestCore(null, "CEN", null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_RCNCEN() => RunPackagesAndRCNTestCore(null, "CEN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_RCNCRN() => RunPackagesAndRCNTestCore(null, "CEN", "CRN", 0, "CLR", defaultCustomStatus, "CLR", "CLR");
		public void TestPackagesAndRCN_CEN_RCNPAN() => RunPackagesAndRCNTestCore(null, "CEN", "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CRN() => RunPackagesAndRCNTestCore(null, "CRN", null, 0, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_RCNCEN() => RunPackagesAndRCNTestCore(null, "CRN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CRN_RCNCRN() => RunPackagesAndRCNTestCore(null, "CRN", "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_RCNPAN() => RunPackagesAndRCNTestCore(null, "CRN", "PAN", 0, defaultCustomStatus, "CLR", "PAN", "CLR");
		public void TestPackagesAndRCN_PAN() => RunPackagesAndRCNTestCore(null, "PAN", null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_PAN_RCNCEN() => RunPackagesAndRCNTestCore(null, "PAN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_PAN_RCNCRN() => RunPackagesAndRCNTestCore(null, "PAN", "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_PAN_RCNPAN() => RunPackagesAndRCNTestCore(null, "PAN", "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CEN() => RunPackagesAndRCNTestCore("CEN", "CEN", null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CEN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "CEN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CEN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "CEN", "CRN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CEN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "CEN", "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CRN() => RunPackagesAndRCNTestCore("CEN", "CRN", null, 0, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CEN_CRN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "CRN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_CRN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "CRN", "CRN", 0, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CEN_CRN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "CRN", "PAN", 0, defaultCustomStatus, "CLR", "PAN", "CLR");
		public void TestPackagesAndRCN_CEN_PAN() => RunPackagesAndRCNTestCore("CEN", "PAN", null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("CEN", "PAN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CEN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("CEN", "PAN", "CRN", 0, defaultCustomStatus, "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CEN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("CEN", "PAN", "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CRN_CRN() => RunPackagesAndRCNTestCore("CRN", "CRN", null, 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_CRN_RCNCEN() => RunPackagesAndRCNTestCore("CRN", "CRN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CRN_CRN_RCNCRN() => RunPackagesAndRCNTestCore("CRN", "CRN", "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_CRN_RCNPAN() => RunPackagesAndRCNTestCore("CRN", "CRN", "PAN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_PAN() => RunPackagesAndRCNTestCore("CRN", "PAN", null, 0, "CLR", defaultCustomStatus, "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("CRN", "PAN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_CRN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("CRN", "PAN", "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_CRN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("CRN", "PAN", "PAN", 0, "CLR", defaultCustomStatus, "PAN", "CLR");
		public void TestPackagesAndRCN_PAN_PAN() => RunPackagesAndRCNTestCore("PAN", "PAN", null, 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_PAN_PAN_RCNCEN() => RunPackagesAndRCNTestCore("PAN", "PAN", "CEN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);
		public void TestPackagesAndRCN_PAN_PAN_RCNCRN() => RunPackagesAndRCNTestCore("PAN", "PAN", "CRN", 0, "CLR", "CLR", "CLR", "CLR");
		public void TestPackagesAndRCN_PAN_PAN_RCNPAN() => RunPackagesAndRCNTestCore("PAN", "PAN", "PAN", 0, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);

		#endregion

		#region RunPackagesAndRCNTestCore

		void RunPackagesAndRCNTestCore(string pkg1CusEntryType, string pkg2CusEntryType, string rcnCusEntryType, int whsCustomsFlag, string pkg1Status, string pkg2Status, string rcnStatus, string topHUStatus)
		{
			UpdatePackageCountingRegistry(registryValue: trueRegistryValue, branchPK: branch.PK);
			UpdateWhsCustomsFlag(isCustomsControlled: whsCustomsFlag, whsPK: warehouse.PK);

			var (pkg1Pk, pkg2Pk, topHUPk, midHUPk) = CreateWhsPkgData(pkg1CusEntryType, pkg2CusEntryType, rcnCusEntryType);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topHUPk, midHUPk, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus, defaultCustomStatus);

			UpdatePackageCountingRegistry(registryValue: falseRegistryValue, branchPK: branch.PK);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topHUPk, midHUPk, pkg1Status, pkg2Status, rcnStatus, topHUStatus);
		}

		#endregion

		#region TestUtils

		void AssertAllFromDB(Guid pkg1Pk, Guid pkg2Pk, Guid topHUPk, Guid midHUPk, string pkg1Status, string pkg2Status, string rcnStatus, string topHUStatus)
		{
			WhsItemPackageState.AssertFromDB(TestConnection, pkg1Pk)
				.ExpectEquals($"WPS_CustomsStatus should be {pkg1Status}", p => p.WPS_CustomsStatus, pkg1Status)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, pkg2Pk)
				.ExpectEquals($"WPS_CustomsStatus should be {pkg2Status}", p => p.WPS_CustomsStatus, pkg2Status)
				.VerifyAll();
			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals($"WRC_CustomsStatus should be {rcnStatus}", rcn => rcn.WRC_CustomsStatus, rcnStatus)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, topHUPk)
				.ExpectEquals($"Top HU WPS_CustomsStatus should be {topHUStatus}", p => p.WPS_CustomsStatus, topHUStatus)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, midHUPk)
				.ExpectEquals($"Mid HU WPS_CustomsStatus should be {topHUStatus}", p => p.WPS_CustomsStatus, topHUStatus)
				.VerifyAll();
		}

		(Guid pkg1Pk, Guid pkg2Pk, Guid topHuPk, Guid midHUPk) CreateWhsPkgData(string pkg1CusEntryType, string pkg2CusEntryType, string rcnCusEntryType)
		{
			var sql = new StringBuilder();

			var packageJobFromRTU = new PkgPackageJob(rtu.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RCN" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status: "ARV", warehouse, rcn: rcn, rtu: rtu, lastLocation: stagingLocation1, customStatus: defaultCustomStatus);
			var package2 = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status: "ARV", warehouse, rcn: rcn, rtu: rtu, lastLocation: stagingLocation1, customStatus: defaultCustomStatus);
			var topHUPkg = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var topHUPkgState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, topHUPkg.PK, status: "ARV", warehouse, rcn: rcn, rtu: rtu, lastLocation: stagingLocation1, isHandlingUnit: true, unitType: "HU", customStatus: defaultCustomStatus);
			var midHUPkg = new PkgPackage(packageJobFromRTU, "PLT", 0) { KP_KP_TopHandlingUnitPackage = topHUPkg }.AppendInsertAndReturnObject(sql);
			var midHUPkgState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, midHUPkg.PK, status: "ARV", warehouse, rcn: rcn, rtu: rtu, lastLocation: stagingLocation1, isHandlingUnit: true, unitType: "HU", customStatus: defaultCustomStatus);

			package1.KP_KP_TopHandlingUnitPackage = topHUPkg;
			package2.KP_KP_TopHandlingUnitPackage = topHUPkg;
			midHUPkg.KP_KP_TopHandlingUnitPackage = topHUPkg;
			sql.AppendLine($"UPDATE dbo.PkgPackage SET KP_KP_TopHandlingUnitPackage = '{topHUPkg.PK}', KP_SystemLastEditTimeUtc = GETDATE(), KP_SystemLastEditUser = 'GDS'" +
				$" WHERE KP_PK IN ('{midHUPkg.PK}', '{package1.PK}', '{package2.PK}');");
			var divot1 = new PkgPackageHandlingUnitDivot(topHUPkg.PK, midHUPkg.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
			var divot2 = new PkgPackageHandlingUnitDivot(midHUPkg.PK, package1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
			var divot3 = new PkgPackageHandlingUnitDivot(midHUPkg.PK, package2.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);

			if (pkg1CusEntryType != null)
			{
				var cusEntryPkg1 = new CusEntryNum(parentID: package1.PK, parentTable: "PkgPackage", entryNum: "JapanPortNaritaJul2024", entryType: pkg1CusEntryType, entryStatus: "")
				{ CE_Category = pkg1CusEntryType == "PAN" ? "PRT" : "CUS" }.AppendInsertAndReturnObject(sql);
			}
			if (pkg2CusEntryType != null)
			{
				var cusEntryPkg1 = new CusEntryNum(parentID: package2.PK, parentTable: "PkgPackage", entryNum: "JapanPortNaritaJul2024", entryType: pkg2CusEntryType, entryStatus: "")
				{ CE_Category = pkg2CusEntryType == "PAN" ? "PRT" : "CUS" }.AppendInsertAndReturnObject(sql);
			}
			if (rcnCusEntryType != null)
			{
				var cusEntryRcn = new CusEntryNum(parentID: rcn.PK, parentTable: "WhsItemReceiveConsignment", entryNum: "JapanPortNaritaJul2024", entryType: rcnCusEntryType, entryStatus: "")
				{ CE_Category = rcnCusEntryType == "PAN" ? "PRT" : "CUS" }.AppendInsertAndReturnObject(sql);
			}

			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			return (packageState1.PK, packageState2.PK, topHUPkgState.PK, midHUPkgState.PK);
		}

		void UpdateWhsCustomsFlag(int isCustomsControlled, Guid whsPK)
		{
			using (var cmd = Db.Connection.Command($@"
				UPDATE
					dbo.WhsWarehouse
				SET
					WW_IsCustomsControlled = {isCustomsControlled},
					WW_SystemLastEditTimeUtc = GETDATE(),
					WW_SystemLastEditUser = 'GDS'
				WHERE
					WW_PK = '{whsPK}'
			"))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void UpdatePackageCountingRegistry(string registryValue, Guid branchPK)
		{
			using (var cmd = Db.Connection.Command($@"
				DELETE FROM dbo.StmData WHERE SD_Name = 'ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation';
				INSERT INTO dbo.StmData
					(SD_PK, SD_Name, SD_Owner, SD_Type, SD_IsLogged, SD_BinaryValue, SD_IsCancelled, SD_PreserveTestValue, SD_SystemCreateUser, SD_SystemLastEditUser)
				VALUES
					(NEWID(), 'ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation', '{branchPK}', 'BOL', 1, {registryValue}, 0, 0, 'G', 'G');
			"))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Setup and Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var sql = new StringBuilder();

			branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			warehouse = new WhsWarehouse("WH1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(warehouse, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(warehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			stagingLocation1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			rcn = new WhsItemReceiveConsignment(warehouse, "RCN", "RCN", "STD", "CNNJG") { WRC_CustomsStatus = defaultCustomStatus }.AppendInsertAndReturnObject(sql);

			rtu = new WhsItemReceiveTransportationUnit(warehouse, "RTU", stagingLocation1, "RTU").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		WhsWarehouse warehouse;
		GlbBranch branch;
		WhsLocation stagingLocation1;

		WhsItemReceiveConsignment rcn;
		WhsItemReceiveTransportationUnit rtu;

		readonly string trueRegistryValue = "0x5400720075006500";
		readonly string falseRegistryValue = "0x460061006C0073006500";
		readonly string defaultCustomStatus = "NON";

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePkgStateAndRCNCustomsStatus();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update customs status for packages and rcns_1] ON [dbo].[CusEntryNum] ([CE_ParentTable], [CE_Category]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update customs status for packages and rcns_2] ON [dbo].[WhsItemPackageState] ([WPS_CustomsStatus]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion
	}
}
