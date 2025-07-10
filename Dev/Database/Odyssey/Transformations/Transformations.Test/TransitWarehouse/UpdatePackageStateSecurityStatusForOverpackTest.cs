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
	[TestedType(typeof(UpdatePackageStateSecurityStatusForOverpack))]
	class UpdatePackageStateSecurityStatusForOverpackTest : DataTransformationTestCase
	{
		#region overpack

		public void TestOverpack_SecurityStatusShouldBeOVR()
		{
			RunOverpackTest(OVR, "TopOVP1", "SubOVP1", "Inner1", warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: OVR);
			RunOverpackTest(OVR, "TopOVP2", "SubOVP2", "Inner2", warehouseRequireSecurity: false, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: OVR);
			RunOverpackTest(OVR, "TopOVP3", "SubOVP3", "Inner3", warehouseRequireSecurity: false, mandatoryScreeningResigstryValue: false, transportMode: AIR, defaultSecurityStatus: OVR);

			RunOverpackTest(OVR, "TopOVP4", "SubOVP4", "Inner4", hasDTUFK: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: OVR);

			RunOverpackTest(OVR, "TopOVP5", "SubOVP5", "Inner5", latestScreeningResult: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, defaultSecurityStatus: OVR);
			RunOverpackTest(OVR, "TopOVP6", "SubOVP6", "Inner6", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, defaultSecurityStatus: OVR);
			RunOverpackTest(OVR, "TopOVP7", "SubOVP7", "Inner7", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: OVR);
		}

		public void TestOverpack_SecurityStatusShouldBeHRS()
		{
			RunOverpackTest(HRS, "TopOVP1", "SubOVP1", "Inner1", latestScreeningResult: false, lastSecondScreeningResult: false, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
			RunOverpackTest(HRS, "TopOVP2", "SubOVP2", "Inner2", latestScreeningResult: true, lastSecondScreeningResult: false, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
			RunOverpackTest(HRS, "TopOVP3", "SubOVP3", "Inner3", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: true, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
			RunOverpackTest(HRS, "TopOVP4", "SubOVP4", "Inner4", latestScreeningResult: false, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
			RunOverpackTest(HRS, "TopOVP5", "SubOVP5", "Inner5", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: false, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
		}

		public void TestOverpack_SecurityStatusShouldBeHRN()
		{
			RunOverpackTest(HRN, "TopOVP1", "SubOVP1", "Inner1", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: false, defaultSecurityStatus: SCR);
		}

		public void TestOverpack_SecurityStatusShouldBeREQ()
		{
			RunOverpackTest(REQ, "TopOVP1", "SubOVP1", "Inner1", isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP2", "SubOVP2", "Inner2", isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP3", "SubOVP3", "Inner3", isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: "", defaultSecurityStatus: SCR);

			RunOverpackTest(REQ, "TopOVP4", "SubOVP4", "Inner4", warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP5", "SubOVP5", "Inner5", warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP6", "SubOVP6", "Inner6", warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: "", defaultSecurityStatus: SCR);

			RunOverpackTest(REQ, "TopOVP7", "SubOVP7", "Inner7", warehouseRequireSecurity: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP8", "SubOVP8", "Inner8", warehouseRequireSecurity: true, defaultSecurityStatus: SCR);
			RunOverpackTest(REQ, "TopOVP9", "SubOVP9", "Inner9", warehouseRequireSecurity: true, transportMode: "", defaultSecurityStatus: SCR);

			RunOverpackTest(REQ, "TopOVP10", "SubOVP10", "Inner10", latestScreeningResult: true, isScreeningTimeLaterThanPackingTime: false, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
		}

		public void TestOverpack_SecurityStatusShouldBeNSCR()
		{
			RunOverpackTest(SCR, "TopOVP1", "SubOVP1", "Inner1", latestScreeningResult: true, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(SCR, "TopOVP2", "SubOVP2", "Inner2", latestScreeningResult: true, lastSecondScreeningResult: true, isLastScreeningMethodSameAsLastSecondScreeningMethod: false, isScreeningTimeLaterThanPackingTime: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: true, defaultSecurityStatus: SCR);
		}

		public void TestOverpack_SecurityStatusShouldBeNSEC()
		{
			RunOverpackTest(SEC, "TopOVP1", "SubOVP1", "Inner1", hasDTUFK: true, isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: AIR);
			RunOverpackTest(SEC, "TopOVP2", "SubOVP2", "Inner2", isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: false, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(SEC, "TopOVP3", "SubOVP3", "Inner3", isSecure: true, warehouseRequireSecurity: true, transportMode: AIR, defaultSecurityStatus: SCR);
		}

		public void TestOverpack_SecurityStatusShouldBeNOT()
		{
			RunOverpackTest(NOT, "TopOVP1", "SubOVP1", "Inner1", latestScreeningResult: false, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: ROA, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP2", "SubOVP2", "Inner2", latestScreeningResult: false, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP3", "SubOVP3", "Inner3", latestScreeningResult: false, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP4", "SubOVP4", "Inner4", latestScreeningResult: true, isScreeningTimeLaterThanPackingTime: true, transportMode: ROA, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP5", "SubOVP5", "Inner5", isSecure: true, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP6", "SubOVP6", "Inner6", isSecure: true, warehouseRequireSecurity: false, mandatoryScreeningResigstryValue: true, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP7", "SubOVP7", "Inner7", isSecure: true, warehouseRequireSecurity: false, mandatoryScreeningResigstryValue: false, transportMode: AIR, defaultSecurityStatus: SCR);
			RunOverpackTest(NOT, "TopOVP8", "SubOVP8", "Inner8", isSecure: true, warehouseRequireSecurity: true, mandatoryScreeningResigstryValue: true, transportMode: ROA, defaultSecurityStatus: SCR);
		}

		void RunOverpackTest(string expected, string topOverpackDescription, string subOverpackDescription, string innerDescription, bool? latestScreeningResult = null, bool? lastSecondScreeningResult = null, bool isLastScreeningMethodSameAsLastSecondScreeningMethod = false, bool isScreeningTimeLaterThanPackingTime = false, bool hasDTUFK = false, bool isSecure = false, bool warehouseRequireSecurity = false, bool mandatoryScreeningResigstryValue = false, string transportMode = null, bool isHighRisk = false, bool isHighRiskAuthorized = false, string defaultSecurityStatus = SEC)
		{
			SetWarehouseTransitSecurityProcessingRequired(warehouse, warehouseRequireSecurity);
			SetWarehouseRegistryValue(warehouse, mandatoryScreeningResigstryValue);

			var sql = new SqlQueryBuilder();

			var (topOverpackPackage, topOverpackPackageState) = CreatePackage(sql, topOverpackDescription, isHandlingUnit: true, isOverpack: true, isSecure: isSecure, hasDTUFK: hasDTUFK, transportMode: transportMode, isHighRisk: isHighRisk, isHighRiskAuthorized: isHighRiskAuthorized, defaultSecurityStatus: defaultSecurityStatus);
			var (subOverpackPackage, subOverpackPackageState) = CreatePackage(sql, subOverpackDescription, isHandlingUnit: true, isOverpack: true, isSecure: isSecure, hasDTUFK: hasDTUFK, transportMode: transportMode, isHighRisk: isHighRisk, isHighRiskAuthorized: isHighRiskAuthorized, defaultSecurityStatus: defaultSecurityStatus);

			var (innerPackage, innerPackageState) = CreatePackage(sql, innerDescription, transportMode: "AIR");

			if (latestScreeningResult.HasValue)
			{
				CreatePackageScreening(sql, topOverpackPackage, screeningTime: isScreeningTimeLaterThanPackingTime ? DateTime.Now.AddDays(2) : DateTime.Now.AddDays(-1), passed: latestScreeningResult.Value);
				CreatePackageScreening(sql, subOverpackPackage, screeningTime: isScreeningTimeLaterThanPackingTime ? DateTime.Now.AddDays(2) : DateTime.Now.AddDays(-1), passed: latestScreeningResult.Value);
				if (lastSecondScreeningResult.HasValue)
				{
					CreatePackageScreening(sql, topOverpackPackage, screeningTime: isScreeningTimeLaterThanPackingTime ? DateTime.Now.AddDays(1) : DateTime.Now.AddDays(-2), passed: lastSecondScreeningResult.Value, screenMethod: isLastScreeningMethodSameAsLastSecondScreeningMethod ? "ME1" : "ME2");
					CreatePackageScreening(sql, subOverpackPackage, screeningTime: isScreeningTimeLaterThanPackingTime ? DateTime.Now.AddDays(1) : DateTime.Now.AddDays(-2), passed: lastSecondScreeningResult.Value, screenMethod: isLastScreeningMethodSameAsLastSecondScreeningMethod ? "ME1" : "ME2");
				}
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			PackPackageIntoHandlingUnit(topOverpackPackage, subOverpackPackage);
			PackPackageIntoHandlingUnit(subOverpackPackage, innerPackage);

			RunTransformation();

			WhsItemPackageState.AssertFromDB(TestConnection, topOverpackPackageState.PK)
				.ExpectEquals($"WPS_SecurityStatus should be {expected}", p => p.WPS_SecurityStatus, expected)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, subOverpackPackageState.PK)
				.ExpectEquals($"WPS_SecurityStatus should be {expected}", p => p.WPS_SecurityStatus, expected)
				.VerifyAll();
		}

		#endregion

		#region Top Handling Unit
		public void TestTopHandlingUnit_SecurityStatusShouldBeHRS()
		{
			RunTopHandlingUnitTest(expected: HRS, childPackages: new string[] { HRS, HRN, REQ, NOT, SEC, SCR, OVR });
		}

		public void TestTopHandlingUnit_SecurityStatusShouldBeHRN()
		{
			RunTopHandlingUnitTest(expected: HRN, childPackages: new string[] { HRN, REQ, NOT, SEC, SCR, OVR });
		}

		public void TestTopHandlingUnit_SecurityStatusShouldBeREQ()
		{
			RunTopHandlingUnitTest(expected: REQ, childPackages: new string[] { REQ, NOT, SEC, SCR, OVR });
		}

		public void TestTopHandlingUnit_SecurityStatusShouldBeNOT()
		{
			RunTopHandlingUnitTest(expected: NOT, childPackages: new string[] { NOT, SEC, SCR, OVR });
		}

		public void TestTopHandlingUnit_SecurityStatusShouldBeSEC()
		{
			RunTopHandlingUnitTest(expected: SEC, childPackages: new string[] { SEC, SCR, OVR });
		}

		public void RunTopHandlingUnitTest(string expected, params string[] childPackages)
		{
			SetWarehouseTransitSecurityProcessingRequired(warehouse, true);
			SetWarehouseRegistryValue(warehouse, true);

			var sql = new SqlQueryBuilder();

			var handlingUnitDescription = "HU";
			var (hu, huState) = CreatePackage(sql, handlingUnitDescription, isHandlingUnit: true);
			var children = new List<PkgPackage>();
			foreach (var child in childPackages)
			{
				children.Add(CreateChildPackage(sql, handlingUnitDescription, child, isHandlingUnit: true, isOverpack: true).package);
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, children.ToArray());

			RunTransformation();

			WhsItemPackageState.AssertFromDB(TestConnection, huState.PK)
				.ExpectEquals($"WPS_SecurityStatus should be {expected}", p => p.WPS_SecurityStatus, expected)
				.VerifyAll();
		}

		#endregion

		#region Inner Handling Unit

		public void TestInnerHandlingUnit_SecurityStatusShouldSameAsTopHandlingUnit()
		{
			SetWarehouseTransitSecurityProcessingRequired(warehouse, true);
			SetWarehouseRegistryValue(warehouse, true);

			var sql = new SqlQueryBuilder();

			var (hu, _) = CreatePackage(sql, "TOPHU", isHandlingUnit: true);
			var (subHU, subHUState) = CreatePackage(sql, "SUBHU", isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);
			var (childPackage, childPackageState) = CreatePackage(sql, "Child", isSecure: true, transportMode: "AIR", isHandlingUnit: true, isOverpack: true, defaultSecurityStatus: SCR);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(hu, subHU);
			PackPackageIntoHandlingUnit(subHU, innerHU);
			PackPackageIntoHandlingUnit(innerHU, childPackage);

			RunTransformation();

			var errorMessage = $"WPS_SecurityStatus should be {REQ}";
			WhsItemPackageState.AssertFromDB(TestConnection, subHUState.PK)
				.ExpectEquals(errorMessage, p => p.WPS_SecurityStatus, REQ)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, innerHUState.PK)
				.ExpectEquals(errorMessage, p => p.WPS_SecurityStatus, REQ)
				.VerifyAll();
		}

		#endregion

		#region Implementation

		#region Override

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePackageStateSecurityStatusForOverpack();

		protected override void PrepareTestData()
		{
			SetWarehouseTransitSecurityProcessingRequired(warehouse, true);
			SetWarehouseRegistryValue(warehouse, true);

			var sql = new SqlQueryBuilder();

			var (topHU, topHUState) = CreatePackage(sql, "TOPHU", isHandlingUnit: true);
			var (childPackageREQ, childPackageREQState) = CreateChildPackage(sql, "TOPHU", REQ);
			var (childPackageSEC, childPackageSECState) = CreateChildPackage(sql, "TOPHU", SEC);
			var (childPackageSCR, childPackageSCRState) = CreateChildPackage(sql, "TOPHU", SCR);
			var (overpackPackage, overpackchildPackageState) = CreateChildPackage(sql, "OVP", SCR, isHandlingUnit: true, isOverpack: true);
			var (subHU, subHUState) = CreatePackage(sql, "SUBHU", isHandlingUnit: true);
			var (innerHU, innerHUState) = CreatePackage(sql, "InnerHU", isHandlingUnit: true);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			PackPackageIntoHandlingUnit(topHU, subHU);
			PackPackageIntoHandlingUnit(subHU, innerHU);
			PackPackageIntoHandlingUnit(innerHU, childPackageREQ);
			PackPackageIntoHandlingUnit(innerHU, overpackPackage);
			PackPackageIntoHandlingUnit(subHU, childPackageSEC);
			PackPackageIntoHandlingUnit(topHU, childPackageSCR);

			testData = new List<(WhsItemPackageState packageState, string expectedSecurityStatus)>
			{
				(childPackageREQState, REQ),
				(childPackageSECState, SEC),
				(childPackageSCRState, SCR),
				(overpackchildPackageState, SEC),
				(topHUState, REQ),
				(subHUState, REQ),
				(innerHUState, REQ),
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

		#endregion

		void SetWarehouseTransitSecurityProcessingRequired(WhsWarehouse warehouse, bool value)
		{
			TestConnection.ExecuteNonQuery($"UPDATE dbo.WhsWarehouse SET WW_TransitSecurityProcessingRequired={(value ? 1 : 0)}, WW_SystemLastEditTimeUtc=GETUTCDATE(), WW_SystemLastEditUser='ABC' WHERE WW_PK='{warehouse.PK}';");
		}

		void SetWarehouseRegistryValue(WhsWarehouse warehouse, bool value)
		{
			const string RegistryName = "MandatoryPackageScreeningForAirAndUnknownTransportMode";
			var branchID = warehouse.WW_GB_RelatedCompanyBranch;
			TestConnection.ExecuteNonQuery($@"DELETE FROM dbo.StmData WHERE SD_Name='{RegistryName}' AND SD_Owner='{branchID}';
									INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES
									(NEWID(), '{RegistryName}', '{branchID}', NULL, 'BOL', CONVERT(varbinary(max), N'{(value ? "True" : "False")}'), NULL)");
		}

		void PackPackageIntoHandlingUnit(PkgPackage handlingUnit, params PkgPackage[] children)
		{
			var sql = new SqlQueryBuilder();

			foreach (var child in children)
			{
				child.KP_KP_TopHandlingUnitPackage = handlingUnit.KP_KP_TopHandlingUnitPackage ?? handlingUnit;
				sql.AppendLine($"UPDATE dbo.PkgPackage SET KP_KP_TopHandlingUnitPackage = '{child.KP_KP_TopHandlingUnitPackage.FK}', KP_SystemLastEditTimeUtc=GETUTCDATE(), KP_SystemLastEditUser='ABC' WHERE KP_PK = '{child.PK}';");
				new PkgPackageHandlingUnitDivot(handlingUnit.PK, child.PK).AppendInsertAndReturnObject(sql);
			}

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, PkgPackageSchema.Constants.PK, TestWhsDataSetupHelper.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, PkgPackageHandlingUnitDivotSchema.Constants.PK, TestWhsDataSetupHelper.PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
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

		PkgPackageScreening CreatePackageScreening(SqlQueryBuilder sql, IPkgPackageSQL package, DateTime screeningTime = default, bool passed = false, string screenMethod = "ME1")
		{
			var packageScreening = new PkgPackageScreening(package);
			if (screeningTime != default)
			{
				packageScreening.KPS_Time = screeningTime;
			}
			if (passed)
			{
				packageScreening.KPS_Passed = true;
				packageScreening.KPS_FailureReason = string.Empty;
			}
			packageScreening.KPS_Method = screenMethod;

			packageScreening.AppendInsertAndReturnObject(sql);

			return packageScreening;
		}

		(PkgPackage package, WhsItemPackageState packageState) CreateChildPackage(SqlQueryBuilder sql, string handlingUnitDescription, string securityStatus, bool isHandlingUnit = false, bool isOverpack = false)
		{
			SetWarehouseTransitSecurityProcessingRequired(warehouse, true);
			SetWarehouseRegistryValue(warehouse, false);
			switch (securityStatus)
			{
				case SCR:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-SCR", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: SCR, isSecure: true, transportMode: AIR);
				case SEC:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-SEC", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: SEC, isSecure: true, transportMode: AIR);
				case REQ:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-REQ", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: REQ);
				case OVR:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-OVR", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: OVR);
				case NOT:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-NOT", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: NOT, transportMode: ROA);
				case HRS:
					return CreatePackage(sql, $"{handlingUnitDescription}-C-HRS", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: HRS, transportMode: AIR, isHighRisk: true);
				case HRN:
					var hrnResult = CreatePackage(sql, $"{handlingUnitDescription}-C-HRN", isHandlingUnit: isHandlingUnit, isOverpack: isOverpack, defaultSecurityStatus: HRN, transportMode: AIR, isHighRisk: true, isHighRiskAuthorized: false);
					CreatePackageScreening(sql, hrnResult.Item1, passed: true, screenMethod: "ME1");
					CreatePackageScreening(sql, hrnResult.Item1, passed: true, screenMethod: "ME2");
					return hrnResult;
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
		const string REQ = "REQ";
		const string NOT = "NOT";
		const string SEC = "SEC";
		const string OVR = "OVR";
		const string HRS = "HRS";
		const string HRN = "HRN";

		const string AIR = "AIR";
		const string ROA = "ROA";

		#endregion

		#endregion
	}
}
