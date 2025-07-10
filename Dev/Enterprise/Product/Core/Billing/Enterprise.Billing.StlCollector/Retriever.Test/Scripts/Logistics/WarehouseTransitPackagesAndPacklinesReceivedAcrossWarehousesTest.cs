using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitPackagesAndPacklinesReceivedAcrossWarehouses))]
	sealed class WarehouseTransitPackagesAndPacklinesReceivedAcrossWarehousesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DISABLE TRIGGER TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit ON PkgPackage;
				DISABLE TRIGGER TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit ON PkgPackageHandlingUnitDivot;
			";
			TestConnection.ExecuteNonQuery(sqlText);

			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "SV1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "DDL").InsertAndReturnObject(TestConnection);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			var whs2 = new WhsWarehouse("WH2", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 2", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "SV2").InsertAndReturnObject(TestConnection);
			var row2 = new WhsRow(whs2, "DDL").InsertAndReturnObject(TestConnection);
			var location2 = new WhsLocation(row2.PK, area2.PK, area2.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			var whs3 = new WhsWarehouse("WH3", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 3", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var area3 = new WhsArea(whs3.PK, "SV3").InsertAndReturnObject(TestConnection);
			var row3 = new WhsRow(whs3, "DDL").InsertAndReturnObject(TestConnection);
			var location3 = new WhsLocation(row3.PK, area3.PK, area3.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", location1, "VEH1").InsertAndReturnObject(TestConnection);
			var rtu2 = new WhsItemReceiveTransportationUnit(whs2, "RTU2", location2, "VEH2").InsertAndReturnObject(TestConnection);
			var rtu3 = new WhsItemReceiveTransportationUnit(whs3, "RTU3", location3, "VEH3").InsertAndReturnObject(TestConnection);

			// 1 -> PKG0
			// Normal unload -> Scan Package ID to receive in whs1
			var rcnForNormalUnloadInWarehouse1 = new WhsItemReceiveConsignment(whs1, "SYD001000", "RC001", "ServiceLevel", "NextDischargePort").InsertAndReturnObject(TestConnection);
			var pkgJobForNormalUnloadInWarehouse1 = new PkgPackageJob(Guid.NewGuid(), "PJ1", "WRC").InsertAndReturnObject(TestConnection);
			var pkgHeaderForNormalUnloadInWarehouse1 = new PkgPackageHeader("PKG0", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgForNormalUnloadInWarehouse1 = (new PkgPackage(pkgJobForNormalUnloadInWarehouse1, "PKG", 1) { KP_KPH_PackageHeader = pkgHeaderForNormalUnloadInWarehouse1 }).InsertAndReturnObject(TestConnection);
			var pkgStateForNormalUnloadInWarehouse1 = (new WhsItemPackageState(pkgForNormalUnloadInWarehouse1.PK, whs1, rcnForNormalUnloadInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTime(2014, 11, 1, 9, 0, 0), WPS_UnloadedNotYetProcessedTime = new DateTime(2014, 11, 1, 9, 0, 0) }).InsertAndReturnObject(TestConnection);

			// 3 -> PKG1_1, PKG1_2, OVP1
			// Scan Package ID onto OVP in whs1
			var rcnForUnloadToOVPInWarehouse1 = (new WhsItemReceiveConsignment(whs1, "SYD001001", "RC002", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 20) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToOVPInWarehouse1 = new PkgPackageJob(Guid.NewGuid(), "PJ2", "WRC").InsertAndReturnObject(TestConnection);
			var ovpHeader1 = new PkgPackageHeader("OVP1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader1 }).InsertAndReturnObject(TestConnection);
			var ovpState1 = (new WhsItemPackageState(ovp1.PK, whs1, rcnForUnloadToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "OVP", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 0, 0) }).InsertAndReturnObject(TestConnection);
			var pkgHeader1_1 = new PkgPackageHeader("PKG1_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader1_2 = new PkgPackageHeader("PKG1_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg1_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader1_1, KP_KP_ParentPackage = ovp1.PK, KP_KP_TopHandlingUnitPackage = ovp1 }).InsertAndReturnObject(TestConnection);
			var pkg1_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader1_2, KP_KP_ParentPackage = ovp1.PK, KP_KP_TopHandlingUnitPackage = ovp1 }).InsertAndReturnObject(TestConnection);
			var pkgState1_1 = (new WhsItemPackageState(pkg1_1.PK, whs1, rcnForUnloadToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 5, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 5, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState1_2 = (new WhsItemPackageState(pkg1_2.PK, whs1, rcnForUnloadToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 10, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 10, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp1.PK, pkg1_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp1.PK, pkg1_2.PK).InsertAndReturnObject(TestConnection);

			// 3 -> PKG2_1, PKG2_2, HU1
			// Scan Package ID onto HU in whs1
			var rcnForUnloadToHUInWarehouse1 = (new WhsItemReceiveConsignment(whs1, "SYD001002", "RC003", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 20) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToHUInWarehouse1 = new PkgPackageJob(Guid.NewGuid(), "PJ3", "WRC").InsertAndReturnObject(TestConnection);
			var huHeader1 = new PkgPackageHeader("HU1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PLT", 1) { KP_KPH_PackageHeader = huHeader1 }).InsertAndReturnObject(TestConnection);
			var huState1 = (new WhsItemPackageState(hu1.PK, whs1, rcnForUnloadToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "HU", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 11, 0) }).InsertAndReturnObject(TestConnection);
			var pkgHeader2_1 = new PkgPackageHeader("PKG2_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader2_2 = new PkgPackageHeader("PKG2_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg2_1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader2_1, KP_KP_TopHandlingUnitPackage = hu1 }).InsertAndReturnObject(TestConnection);
			var pkg2_2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader2_2, KP_KP_TopHandlingUnitPackage = hu1 }).InsertAndReturnObject(TestConnection);
			var pkgState2_1 = (new WhsItemPackageState(pkg2_1.PK, whs1, rcnForUnloadToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 12, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 12, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState2_2 = (new WhsItemPackageState(pkg2_2.PK, whs1, rcnForUnloadToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 13, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 13, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu1.PK, pkg2_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu1.PK, pkg2_2.PK).InsertAndReturnObject(TestConnection);

			// 3 -> 3 BOX, 7 PLT, OVP2
			// Scan Packline onto OVP in whs1
			var rcnForSkipScanModeToOVPInWarehouse1 = (new WhsItemReceiveConsignment(whs1, "SYD001003", "RC004", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 20) }).InsertAndReturnObject(TestConnection);
			var ovpHeader2 = new PkgPackageHeader("OVP2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader2 }).InsertAndReturnObject(TestConnection);
			var ovpState2 = (new WhsItemPackageState(ovp2.PK, whs1, rcnForSkipScanModeToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "OVP", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 0, 0) }).InsertAndReturnObject(TestConnection);
			var packline1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "BOX", 0) { KP_PackageQty = 3, KP_KP_ParentPackage = ovp2.PK, KP_KP_TopHandlingUnitPackage = ovp2 }).InsertAndReturnObject(TestConnection);
			var packline2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse1, "PLT", 0) { KP_PackageQty = 7, KP_KP_ParentPackage = ovp2.PK, KP_KP_TopHandlingUnitPackage = ovp2 }).InsertAndReturnObject(TestConnection);
			var packlineState1 = (new WhsItemPackageState(packline1.PK, whs1, rcnForSkipScanModeToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "SKP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 14, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 14, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState2 = (new WhsItemPackageState(packline2.PK, whs1, rcnForSkipScanModeToOVPInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "SKP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 15, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 15, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2.PK, packline1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2.PK, packline2.PK).InsertAndReturnObject(TestConnection);

			// 3 -> 2 BOX, 5 PLT, HU2
			// Scan Packline onto HU in whs1
			var rcnForSkipScanModeToHUInWarehouse1 = (new WhsItemReceiveConsignment(whs1, "SYD001004", "RC005", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 20) }).InsertAndReturnObject(TestConnection);
			var huHeader2 = new PkgPackageHeader("HU2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PLT", 1) { KP_KPH_PackageHeader = huHeader2 }).InsertAndReturnObject(TestConnection);
			var huState2 = (new WhsItemPackageState(hu2.PK, whs1, rcnForSkipScanModeToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "HU", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 0, 0) }).InsertAndReturnObject(TestConnection);
			var packline3 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "BOX", 1) { KP_PackageQty = 2, KP_KP_TopHandlingUnitPackage = hu2 }).InsertAndReturnObject(TestConnection);
			var packline4 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PLT", 1) { KP_PackageQty = 5, KP_KP_TopHandlingUnitPackage = hu2 }).InsertAndReturnObject(TestConnection);
			var packlineState3 = (new WhsItemPackageState(packline3.PK, whs1, rcnForSkipScanModeToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "SKP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 16, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 16, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState4 = (new WhsItemPackageState(packline4.PK, whs1, rcnForSkipScanModeToHUInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "SKP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 17, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 17, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2.PK, packline3.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2.PK, packline4.PK).InsertAndReturnObject(TestConnection);

			// 1 -> PKG0
			// Normal unload -> Scan Package ID to receive in whs2
			var rcnForNormalUnloadInWarehouse2 = new WhsItemReceiveConsignment(whs2, "SYD001000", "RC006", "ServiceLevel", "NextDischargePort").InsertAndReturnObject(TestConnection);
			var pkgJobForNormalUnloadInWarehouse2 = new PkgPackageJob(Guid.NewGuid(), "PJ4", "WRC").InsertAndReturnObject(TestConnection);
			var pkgHeaderForNormalUnloadInWarehouse2 = new PkgPackageHeader("PKG0", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgForNormalUnloadInWarehouse2 = (new PkgPackage(pkgJobForNormalUnloadInWarehouse2, "PKG", 1) { KP_KPH_PackageHeader = pkgHeaderForNormalUnloadInWarehouse2 }).InsertAndReturnObject(TestConnection);
			var pkgStateForNormalUnloadInWarehouse2 = (new WhsItemPackageState(pkgForNormalUnloadInWarehouse2.PK, whs2, rcnForNormalUnloadInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTime(2014, 11, 1, 12, 18, 0), WPS_UnloadedNotYetProcessedTime = new DateTime(2014, 11, 1, 12, 18, 0) }).InsertAndReturnObject(TestConnection);

			// 1 -> OVP1
			// Scan OVP with inner packages to receive in whs2
			var rcnForUnloadToOVPInWarehouse2 = (new WhsItemReceiveConsignment(whs2, "SYD001001", "RC007", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 21) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToOVPInWarehouse2 = new PkgPackageJob(Guid.NewGuid(), "PJ5", "WRC").InsertAndReturnObject(TestConnection);
			var ovpHeader2_1 = new PkgPackageHeader("OVP1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp2_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader2_1 }).InsertAndReturnObject(TestConnection);
			var ovpState2_1 = (new WhsItemPackageState(ovp2_1.PK, whs2, rcnForUnloadToOVPInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "OVP", WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 19, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 19, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgHeader2_1_1 = new PkgPackageHeader("PKG1_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader2_1_2 = new PkgPackageHeader("PKG1_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg2_1_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader2_1_1, KP_KP_ParentPackage = ovp2_1.PK, KP_KP_TopHandlingUnitPackage = ovp2_1 }).InsertAndReturnObject(TestConnection);
			var pkg2_1_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader2_1_2, KP_KP_ParentPackage = ovp2_1.PK, KP_KP_TopHandlingUnitPackage = ovp2_1 }).InsertAndReturnObject(TestConnection);
			var pkgState2_1_1 = (new WhsItemPackageState(pkg2_1_1.PK, whs2, rcnForUnloadToOVPInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 20, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 20, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState2_1_2 = (new WhsItemPackageState(pkg2_1_2.PK, whs2, rcnForUnloadToOVPInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 21, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 21, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2_1.PK, pkg2_1_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2_1.PK, pkg2_1_2.PK).InsertAndReturnObject(TestConnection);

			// 3 -> PKG2_1, PKG2_2, HU1
			// Scan HU with inner packages to receive in whs2
			var rcnForUnloadToHUInWarehouse2 = (new WhsItemReceiveConsignment(whs2, "SYD001002", "RC008", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 21) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToHUInWarehouse2 = new PkgPackageJob(Guid.NewGuid(), "PJ6", "WRC").InsertAndReturnObject(TestConnection);
			var huHeader2_1 = new PkgPackageHeader("HU1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu2_1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PLT", 1) { KP_KPH_PackageHeader = huHeader2_1 }).InsertAndReturnObject(TestConnection);
			var huState2_1 = (new WhsItemPackageState(hu2_1.PK, whs2, rcnForUnloadToHUInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "HU", WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 22, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 22, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgHeader2_2_1 = new PkgPackageHeader("PKG2_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader2_2_2 = new PkgPackageHeader("PKG2_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg2_2_1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader2_2_1, KP_KP_TopHandlingUnitPackage = hu2_1 }).InsertAndReturnObject(TestConnection);
			var pkg2_2_2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader2_2_2, KP_KP_TopHandlingUnitPackage = hu2_1 }).InsertAndReturnObject(TestConnection);
			var pkgState2_2_1 = (new WhsItemPackageState(pkg2_2_1.PK, whs2, rcnForUnloadToHUInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 23, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 23, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState2_2_2 = (new WhsItemPackageState(pkg2_2_2.PK, whs2, rcnForUnloadToHUInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 24, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 24, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2_1.PK, pkg2_2_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2_1.PK, pkg2_2_2.PK).InsertAndReturnObject(TestConnection);

			// 1 -> OVP2
			// Scan OVP with inner packlines to receive in whs2
			var rcnForSkipScanModeToOVPInWarehouse2 = (new WhsItemReceiveConsignment(whs2, "SYD001003", "RC009", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 21) }).InsertAndReturnObject(TestConnection);
			var ovpHeader2_2 = new PkgPackageHeader("OVP2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp2_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader2_2 }).InsertAndReturnObject(TestConnection);
			var ovpState2_2 = (new WhsItemPackageState(ovp2_2.PK, whs2, rcnForSkipScanModeToOVPInWarehouse2.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "OVP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 25, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 25, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packline2_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "BOX", 0) { KP_PackageQty = 3, KP_KP_ParentPackage = ovp2_2.PK, KP_KP_TopHandlingUnitPackage = ovp2_2 }).InsertAndReturnObject(TestConnection);
			var packline2_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse2, "PLT", 0) { KP_PackageQty = 7, KP_KP_ParentPackage = ovp2_2.PK, KP_KP_TopHandlingUnitPackage = ovp2_2 }).InsertAndReturnObject(TestConnection);
			var packlineState2_1 = (new WhsItemPackageState(packline2_1.PK, whs2, rcnForSkipScanModeToOVPInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 26, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 26, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState2_2 = (new WhsItemPackageState(packline2_2.PK, whs2, rcnForSkipScanModeToOVPInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 27, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 27, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2_2.PK, packline2_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp2_2.PK, packline2_2.PK).InsertAndReturnObject(TestConnection);

			// 3 -> 2 BOX, 5 PLT, HU2
			// Scan HU with inner packlines to receive in whs2
			var rcnForSkipScanModeToHUInWarehouse2 = (new WhsItemReceiveConsignment(whs2, "SYD001004", "RC010", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 21) }).InsertAndReturnObject(TestConnection);
			var huHeader2_2 = new PkgPackageHeader("HU2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu2_2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PLT", 1) { KP_KPH_PackageHeader = huHeader2_2 }).InsertAndReturnObject(TestConnection);
			var huState2_2 = (new WhsItemPackageState(hu2_2.PK, whs2, rcnForSkipScanModeToHUInWarehouse2.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "HU", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packline2_3 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "BOX", 1) { KP_PackageQty = 2, KP_KP_TopHandlingUnitPackage = hu2_2 }).InsertAndReturnObject(TestConnection);
			var packline2_4 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PLT", 1) { KP_PackageQty = 5, KP_KP_TopHandlingUnitPackage = hu2_2 }).InsertAndReturnObject(TestConnection);
			var packlineState2_3 = (new WhsItemPackageState(packline2_3.PK, whs2, rcnForSkipScanModeToHUInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState2_4 = (new WhsItemPackageState(packline2_4.PK, whs2, rcnForSkipScanModeToHUInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 30, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 30, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2_2.PK, packline2_3.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2_2.PK, packline2_4.PK).InsertAndReturnObject(TestConnection);

			// 1 + 2 + 1 = 4 -> OVP1, PKG1_1, PKG1_2, NEW_OVP1
			// Scan OVP with inner packages and repack inners to another OVP in whs3
			var rcnForUnloadToOVPInWarehouse3 = (new WhsItemReceiveConsignment(whs3, "SYD001001", "RC011", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 22) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToOVPInWarehouse3 = new PkgPackageJob(Guid.NewGuid(), "PJ7", "WRC").InsertAndReturnObject(TestConnection);
			var ovpHeader3_1 = new PkgPackageHeader("OVP1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp3_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader3_1 }).InsertAndReturnObject(TestConnection);
			var ovpState3_1 = (new WhsItemPackageState(ovp3_1.PK, whs3, rcnForUnloadToOVPInWarehouse3.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "OVP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 31, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 31, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var newOVPHeader1 = new PkgPackageHeader("NEW_OVP1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var newOVP1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = newOVPHeader1 }).InsertAndReturnObject(TestConnection);
			var newOVPState1 = (new WhsItemPackageState(newOVP1.PK, whs3, rcnForUnloadToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "OVP", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 34, 0) }).InsertAndReturnObject(TestConnection);
			var pkgHeader3_1_1 = new PkgPackageHeader("PKG1_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader3_1_2 = new PkgPackageHeader("PKG1_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg3_1_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader3_1_1, KP_KP_ParentPackage = newOVP1.PK, KP_KP_TopHandlingUnitPackage = newOVP1 }).InsertAndReturnObject(TestConnection);
			var pkg3_1_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PKG", 0) { KP_KPH_PackageHeader = pkgHeader3_1_2, KP_KP_ParentPackage = newOVP1.PK, KP_KP_TopHandlingUnitPackage = newOVP1 }).InsertAndReturnObject(TestConnection);
			var pkgState3_1_1 = (new WhsItemPackageState(pkg3_1_1.PK, whs3, rcnForUnloadToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 32, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 32, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState3_1_2 = (new WhsItemPackageState(pkg3_1_2.PK, whs3, rcnForUnloadToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 33, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 33, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newOVP1.PK, pkg3_1_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newOVP1.PK, pkg3_1_2.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp3_1.PK, pkg3_1_1.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp3_1.PK, pkg3_1_2.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);

			// 1 + 2 + 1 = 4 -> HU1, PKG2_1, PKG2_2, NEW_HU1
			// Scan HU with inner packages and repack inners to another HU in whs3
			var rcnForUnloadToHUInWarehouse3 = (new WhsItemReceiveConsignment(whs3, "SYD001002", "RC012", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 22) }).InsertAndReturnObject(TestConnection);
			var pkgJobForUnloadToHUInWarehouse3 = new PkgPackageJob(Guid.NewGuid(), "PJ8", "WRC").InsertAndReturnObject(TestConnection);
			var huHeader3_1 = new PkgPackageHeader("HU1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu3_1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = huHeader3_1 }).InsertAndReturnObject(TestConnection);
			var huState3_1 = (new WhsItemPackageState(hu3_1.PK, whs3, rcnForUnloadToHUInWarehouse3.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "HU", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 35, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 35, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var newHUHeader1 = new PkgPackageHeader("NEW_HU1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var newHU1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = newHUHeader1 }).InsertAndReturnObject(TestConnection);
			var newHUState1 = (new WhsItemPackageState(newHU1.PK, whs3, rcnForUnloadToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "HU", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 38, 0) }).InsertAndReturnObject(TestConnection);
			var pkgHeader3_2_1 = new PkgPackageHeader("PKG2_1", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgHeader3_2_2 = new PkgPackageHeader("PKG2_2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkg3_2_1 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader3_2_1, KP_KP_TopHandlingUnitPackage = newHU1 }).InsertAndReturnObject(TestConnection);
			var pkg3_2_2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PKG", 1) { KP_KPH_PackageHeader = pkgHeader3_2_2, KP_KP_TopHandlingUnitPackage = newHU1 }).InsertAndReturnObject(TestConnection);
			var pkgState3_2_1 = (new WhsItemPackageState(pkg3_2_1.PK, whs3, rcnForUnloadToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 36, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 36, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var pkgState3_2_2 = (new WhsItemPackageState(pkg3_2_2.PK, whs3, rcnForUnloadToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_ReceivedAs = "PKG", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 37, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 37, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2.PK, pkg3_2_1.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu2.PK, pkg3_2_2.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newHU1.PK, pkg3_2_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newHU1.PK, pkg3_2_2.PK).InsertAndReturnObject(TestConnection);

			// 1 + 2 + 1 = 4 -> OVP2, 3 BOX, 7 PLT, NEW_OVP2
			// Scan OVP with inner packlines and repack inners to another OVP in whs3
			var rcnForSkipScanModeToOVPInWarehouse3 = (new WhsItemReceiveConsignment(whs3, "SYD001003", "RC013", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 22) }).InsertAndReturnObject(TestConnection);
			var ovpHeader3_2 = new PkgPackageHeader("OVP2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var ovp3_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = ovpHeader3_2 }).InsertAndReturnObject(TestConnection);
			var ovpState3_2 = (new WhsItemPackageState(ovp3_2.PK, whs3, rcnForSkipScanModeToOVPInWarehouse3.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "OVP", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 39, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 39, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var newOVPHeader2 = new PkgPackageHeader("NEW_OVP2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var newOVP2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = newOVPHeader2 }).InsertAndReturnObject(TestConnection);
			var newOVPState2 = (new WhsItemPackageState(newOVP2.PK, whs3, rcnForSkipScanModeToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "OVP", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 42, 0) }).InsertAndReturnObject(TestConnection);
			var packline3_1 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "BOX", 0) { KP_PackageQty = 3, KP_KP_ParentPackage = newOVP2.PK, KP_KP_TopHandlingUnitPackage = newOVP2 }).InsertAndReturnObject(TestConnection);
			var packline3_2 = (new PkgPackage(pkgJobForUnloadToOVPInWarehouse3, "PLT", 0) { KP_PackageQty = 7, KP_KP_ParentPackage = newOVP2.PK, KP_KP_TopHandlingUnitPackage = newOVP2 }).InsertAndReturnObject(TestConnection);
			var packlineState3_1 = (new WhsItemPackageState(packline3_1.PK, whs3, rcnForSkipScanModeToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 40, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 40, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState3_2 = (new WhsItemPackageState(packline3_2.PK, whs3, rcnForSkipScanModeToOVPInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 41, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 41, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp3_2.PK, packline3_1.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(ovp3_2.PK, packline3_2.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newOVP2.PK, packline3_1.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newOVP2.PK, packline3_2.PK).InsertAndReturnObject(TestConnection);

			// 1 + 2 + 1 = 4 -> HU2, 2 BOX, 5 PLT, NEW_HU2
			// Scan HU with inner packlines and repack inners to another HU in whs3
			var rcnForSkipScanModeToHUInWarehouse3 = (new WhsItemReceiveConsignment(whs3, "SYD001004", "RC014", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 10, 22) }).InsertAndReturnObject(TestConnection);
			var huHeader3_2 = new PkgPackageHeader("HU2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var hu3_2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = huHeader3_2 }).InsertAndReturnObject(TestConnection);
			var huState3_2 = (new WhsItemPackageState(hu3_2.PK, whs3, rcnForSkipScanModeToHUInWarehouse3.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "HU", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 43, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 43, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var newHUHeader2 = new PkgPackageHeader("NEW_HU2", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var newHU2 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PLT", 1) { KP_KPH_PackageHeader = newHUHeader2 }).InsertAndReturnObject(TestConnection);
			var newHUState2 = (new WhsItemPackageState(newHU2.PK, whs3, rcnForSkipScanModeToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "HU", WPS_SystemCreateTimeUtc = new DateTime(2014, 11, 1, 12, 46, 0) }).InsertAndReturnObject(TestConnection);
			var packline3_3 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "BOX", 1) { KP_PackageQty = 2, KP_KP_TopHandlingUnitPackage = newHU2 }).InsertAndReturnObject(TestConnection);
			var packline3_4 = (new PkgPackage(pkgJobForUnloadToHUInWarehouse3, "PLT", 1) { KP_PackageQty = 5, KP_KP_TopHandlingUnitPackage = newHU2 }).InsertAndReturnObject(TestConnection);
			var packlineState3_3 = (new WhsItemPackageState(packline3_3.PK, whs3, rcnForSkipScanModeToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 44, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 44, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineState3_4 = (new WhsItemPackageState(packline3_4.PK, whs3, rcnForSkipScanModeToHUInWarehouse3.PK, "ARV")
			{ WPS_WL_LastLocation = location3.PK, WPS_WRH_TransitReceiveHeader = rtu3.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 45, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 45, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu3_2.PK, packline3_3.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(hu3_2.PK, packline3_4.PK, unpackTime: DateTime.Now).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newHU2.PK, packline3_3.PK).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(newHU2.PK, packline3_4.PK).InsertAndReturnObject(TestConnection);

			// 0 -> No data collected as it is out of the time range
			// A normal package in whs1 unloaded out of the time range
			var rcnForNormalUnloadOutRangeInWarehouse1 = new WhsItemReceiveConsignment(whs1, "SYD_PKG_OUT", "RC_PKG_OUT", "ServiceLevel", "NextDischargePort").InsertAndReturnObject(TestConnection);
			var pkgJobForNormalUnloadOutRangeInWarehouse1 = new PkgPackageJob(Guid.NewGuid(), "PJ_PKG_OUT", "WRC").InsertAndReturnObject(TestConnection);
			var pkgHeaderForNormalUnloadOutRangeInWarehouse1 = new PkgPackageHeader("PKG_OUT", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgForNormalUnloadOutRangeInWarehouse1 = (new PkgPackage(pkgJobForNormalUnloadOutRangeInWarehouse1, "PKG", 1) { KP_KPH_PackageHeader = pkgHeaderForNormalUnloadOutRangeInWarehouse1 }).InsertAndReturnObject(TestConnection);
			var pkgStateForNormalUnloadOutRangeInWarehouse1 = (new WhsItemPackageState(pkgForNormalUnloadOutRangeInWarehouse1.PK, whs1, rcnForNormalUnloadOutRangeInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTime(2014, 10, 1, 9, 0, 0), WPS_UnloadedNotYetProcessedTime = new DateTime(2014, 10, 1, 9, 0, 0) }).InsertAndReturnObject(TestConnection);

			// 1 -> PKG_OUT
			// A normal package (from whs1) in whs2 unloaded in the time range
			var rcnForNormalUnloadFromOutInWarehouse2 = new WhsItemReceiveConsignment(whs2, "SYD_PKG_OUT", "RC_PKG_IN", "ServiceLevel", "NextDischargePort").InsertAndReturnObject(TestConnection);
			var pkgJobForNormalUnloadFromOutInWarehouse2 = new PkgPackageJob(Guid.NewGuid(), "PJ_PKG_IN", "WRC").InsertAndReturnObject(TestConnection);
			var pkgHeaderForNormalUnloadFromOutInWarehouse2 = new PkgPackageHeader("PKG_OUT", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var pkgForNormalUnloadFromOutInWarehouse2 = (new PkgPackage(pkgJobForNormalUnloadFromOutInWarehouse2, "PKG", 1) { KP_KPH_PackageHeader = pkgHeaderForNormalUnloadFromOutInWarehouse2 }).InsertAndReturnObject(TestConnection);
			var pkgStateForNormalUnloadFromOutInWarehouse2 = (new WhsItemPackageState(pkgForNormalUnloadFromOutInWarehouse2.PK, whs2, rcnForNormalUnloadFromOutInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_ReceivedAs = "SCN", WPS_UnloadedTime = new DateTime(2014, 11, 1, 12, 18, 0), WPS_UnloadedNotYetProcessedTime = new DateTime(2014, 11, 1, 12, 18, 0) }).InsertAndReturnObject(TestConnection);

			// 0 -> No data collected as it is out of the time range
			// A packline in whs1 unloaded out of the time range
			var rcnForSkipScanModeToHUOutRangeInWarehouse1 = (new WhsItemReceiveConsignment(whs1, "SYD_PKL_OUT", "RC_PKL_OUT", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 9, 20) }).InsertAndReturnObject(TestConnection);
			var huOutRangeHeader = new PkgPackageHeader("HU_OUT", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var huOutRange = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "PLT", 1) { KP_KPH_PackageHeader = huOutRangeHeader }).InsertAndReturnObject(TestConnection);
			var huStateOutRange = (new WhsItemPackageState(huOutRange.PK, whs1, rcnForSkipScanModeToHUOutRangeInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "HU", WPS_SystemCreateTimeUtc = new DateTime(2014, 10, 1, 12, 0, 0) }).InsertAndReturnObject(TestConnection);
			var packlineOutRange = (new PkgPackage(pkgJobForUnloadToHUInWarehouse1, "BOX", 1) { KP_PackageQty = 5, KP_KP_TopHandlingUnitPackage = huOutRange }).InsertAndReturnObject(TestConnection);
			var packlineStateOutRange = (new WhsItemPackageState(packlineOutRange.PK, whs1, rcnForSkipScanModeToHUOutRangeInWarehouse1.PK, "ARV")
			{ WPS_WL_LastLocation = location1.PK, WPS_WRH_TransitReceiveHeader = rtu1.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "SKP", WPS_UnloadedTime = new DateTimeOffset(2014, 10, 1, 12, 16, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 10, 1, 12, 16, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(huOutRange.PK, packlineOutRange.PK).InsertAndReturnObject(TestConnection);

			// 1 -> HU_OUT
			// A packline (from whs1) in whs2 unloded in the time range
			var rcnForSkipScanModeToHUFromOutInWarehouse2 = (new WhsItemReceiveConsignment(whs2, "SYD_PKL_OUT", "RC_PKL_IN", "ServiceLevel", "NextDischargePort") { WRC_SystemCreateTimeUtc = new DateTime(2014, 9, 21) }).InsertAndReturnObject(TestConnection);
			var huHeaderFromOut = new PkgPackageHeader("HU_OUT", DateTime.Now, "~BP").InsertAndReturnObject(TestConnection);
			var huFromOut = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "PLT", 1) { KP_KPH_PackageHeader = huHeaderFromOut }).InsertAndReturnObject(TestConnection);
			var huStateFromOut = (new WhsItemPackageState(huFromOut.PK, whs2, rcnForSkipScanModeToHUFromOutInWarehouse2.PK, "ARV")
			{ WPS_ReceivedAs = "SCN", WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "HU", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			var packlineFromOut = (new PkgPackage(pkgJobForUnloadToHUInWarehouse2, "BOX", 1) { KP_PackageQty = 5, KP_KP_TopHandlingUnitPackage = huFromOut }).InsertAndReturnObject(TestConnection);
			var packlineStateFromOut = (new WhsItemPackageState(packlineFromOut.PK, whs2, rcnForSkipScanModeToHUFromOutInWarehouse2.PK, "ARV")
			{ WPS_WL_LastLocation = location2.PK, WPS_WRH_TransitReceiveHeader = rtu2.PK, WPS_UnitType = "PKL", WPS_ReceivedAs = "PKL", WPS_UnloadedTime = new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)), WPS_UnloadedNotYetProcessedTime = new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)) }).InsertAndReturnObject(TestConnection);
			new PkgPackageHandlingUnitDivot(huFromOut.PK, packlineFromOut.PK).InsertAndReturnObject(TestConnection);

			sqlText = @"
				ENABLE TRIGGER TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit ON PkgPackage;
				ENABLE TRIGGER TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit ON PkgPackageHandlingUnitDivot;
			";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 41, transactions.Count());

			AssertRow(transactions, 0, "DEM", "DEM", "A", 1, "PKG0", "SYD001000", new DateTimeOffset(2014, 11, 1, 9, 0, 0, TimeSpan.FromHours(11)), null, "1");
			AssertRow(transactions, 1, "DEM", "DEM", "A", 1, "PKG0", "SYD001000", new DateTimeOffset(2014, 11, 1, 12, 18, 0, TimeSpan.FromHours(11)), null, "2");

			AssertRow(transactions, 2, "DEM", "DEM", "A", 1, "PKG1_1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 5, 0, TimeSpan.FromHours(0)), "OVP", "1");
			AssertRow(transactions, 3, "DEM", "DEM", "A", 1, "PKG1_2", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 10, 0, TimeSpan.FromHours(0)), "OVP", "1");
			AssertRow(transactions, 4, "DEM", "DEM", "A", 1, "OVP1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 0, 0, TimeSpan.FromHours(0)), null, "1");
			AssertRow(transactions, 5, "DEM", "DEM", "A", 1, "OVP1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 19, 0, TimeSpan.FromHours(0)), null, "2");
			AssertRow(transactions, 6, "DEM", "DEM", "A", 1, "PKG1_1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 32, 0, TimeSpan.FromHours(0)), "OVP", "3");
			AssertRow(transactions, 7, "DEM", "DEM", "A", 1, "PKG1_2", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 33, 0, TimeSpan.FromHours(0)), "OVP", "3");
			AssertRow(transactions, 8, "DEM", "DEM", "A", 1, "OVP1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 31, 0, TimeSpan.FromHours(0)), null, "3");
			AssertRow(transactions, 9, "DEM", "DEM", "A", 1, "NEW_OVP1", "SYD001001", new DateTimeOffset(2014, 11, 1, 12, 34, 0, TimeSpan.FromHours(0)), null, "3");

			AssertRow(transactions, 10, "DEM", "DEM", "A", 1, "PKG2_1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 12, 0, TimeSpan.FromHours(0)), "HU", "1");
			AssertRow(transactions, 11, "DEM", "DEM", "A", 1, "PKG2_2", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 13, 0, TimeSpan.FromHours(0)), "HU", "1");
			AssertRow(transactions, 12, "DEM", "DEM", "A", 1, "HU1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 11, 0, TimeSpan.FromHours(0)), null, "1");
			AssertRow(transactions, 13, "DEM", "DEM", "A", 1, "PKG2_1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 23, 0, TimeSpan.FromHours(0)), "HU", "2");
			AssertRow(transactions, 14, "DEM", "DEM", "A", 1, "PKG2_2", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 24, 0, TimeSpan.FromHours(0)), "HU", "2");
			AssertRow(transactions, 15, "DEM", "DEM", "A", 1, "HU1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 22, 0, TimeSpan.FromHours(0)), null, "2");
			AssertRow(transactions, 16, "DEM", "DEM", "A", 1, "PKG2_1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 36, 0, TimeSpan.FromHours(0)), "HU", "3");
			AssertRow(transactions, 17, "DEM", "DEM", "A", 1, "PKG2_2", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 37, 0, TimeSpan.FromHours(0)), "HU", "3");
			AssertRow(transactions, 18, "DEM", "DEM", "A", 1, "HU1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 35, 0, TimeSpan.FromHours(0)), null, "3");
			AssertRow(transactions, 19, "DEM", "DEM", "A", 1, "NEW_HU1", "SYD001002", new DateTimeOffset(2014, 11, 1, 12, 38, 0, TimeSpan.FromHours(0)), null, "3");

			AssertRow(transactions, 20, "DEM", "DEM", "A", 3, "3 BOX", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 14, 0, TimeSpan.FromHours(0)), "OVP", "1");
			AssertRow(transactions, 21, "DEM", "DEM", "A", 7, "7 PLT", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 15, 0, TimeSpan.FromHours(0)), "OVP", "1");
			AssertRow(transactions, 22, "DEM", "DEM", "A", 1, "OVP2", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 0, 0, TimeSpan.FromHours(0)), null, "1");
			AssertRow(transactions, 23, "DEM", "DEM", "A", 1, "OVP2", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 25, 0, TimeSpan.FromHours(0)), null, "2");
			AssertRow(transactions, 24, "DEM", "DEM", "A", 3, "3 BOX", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 40, 0, TimeSpan.FromHours(0)), "OVP", "3");
			AssertRow(transactions, 25, "DEM", "DEM", "A", 7, "7 PLT", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 41, 0, TimeSpan.FromHours(0)), "OVP", "3");
			AssertRow(transactions, 26, "DEM", "DEM", "A", 1, "OVP2", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 39, 0, TimeSpan.FromHours(0)), null, "3");
			AssertRow(transactions, 27, "DEM", "DEM", "A", 1, "NEW_OVP2", "SYD001003", new DateTimeOffset(2014, 11, 1, 12, 42, 0, TimeSpan.FromHours(0)), null, "3");

			AssertRow(transactions, 28, "DEM", "DEM", "A", 2, "2 BOX", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 16, 0, TimeSpan.FromHours(0)), "HU", "1");
			AssertRow(transactions, 29, "DEM", "DEM", "A", 5, "5 PLT", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 17, 0, TimeSpan.FromHours(0)), "HU", "1");
			AssertRow(transactions, 30, "DEM", "DEM", "A", 1, "HU2", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 0, 0, TimeSpan.FromHours(0)), null, "1");
			AssertRow(transactions, 31, "DEM", "DEM", "A", 2, "2 BOX", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)), "HU", "2");
			AssertRow(transactions, 32, "DEM", "DEM", "A", 5, "5 PLT", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 30, 0, TimeSpan.FromHours(0)), "HU", "2");
			AssertRow(transactions, 33, "DEM", "DEM", "A", 1, "HU2", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)), null, "2");
			AssertRow(transactions, 34, "DEM", "DEM", "A", 2, "2 BOX", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 44, 0, TimeSpan.FromHours(0)), "HU", "3");
			AssertRow(transactions, 35, "DEM", "DEM", "A", 5, "5 PLT", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 45, 0, TimeSpan.FromHours(0)), "HU", "3");
			AssertRow(transactions, 36, "DEM", "DEM", "A", 1, "HU2", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 43, 0, TimeSpan.FromHours(0)), null, "3");
			AssertRow(transactions, 37, "DEM", "DEM", "A", 1, "NEW_HU2", "SYD001004", new DateTimeOffset(2014, 11, 1, 12, 46, 0, TimeSpan.FromHours(0)), null, "3");

			AssertRow(transactions, 38, "DEM", "DEM", "A", 1, "PKG_OUT", "SYD_PKG_OUT", new DateTimeOffset(2014, 11, 1, 12, 18, 0, TimeSpan.FromHours(11)), null, "2");
			AssertRow(transactions, 39, "DEM", "DEM", "A", 5, "5 BOX", "SYD_PKL_OUT", new DateTimeOffset(2014, 11, 1, 12, 29, 0, TimeSpan.FromHours(0)), "HU", "2");
			AssertRow(transactions, 40, "DEM", "DEM", "A", 1, "HU_OUT", "SYD_PKL_OUT", new DateTimeOffset(2014, 11, 1, 12, 28, 0, TimeSpan.FromHours(0)), null, "2");
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int line, string companyCode, string branchCode, string userCode, int itemCount, string reference01, string reference02, DateTimeOffset transactionDateUtc, string reference03 = "", string reference04 = "")
		{
			var transaction = transactions.Single(t => t.Reference1 == reference01 && t.Reference2 == reference02 && t.Reference4 == reference04);
			AssertEquals("[T" + line + "] CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals("[T" + line + "] BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals("[T" + line + "] UserCode", userCode, transaction.ClientStaffCode);
			AssertEquals("[T" + line + "] ItemCount", itemCount, transaction.BillableCount);
			AssertEquals("[T" + line + "] TransactionReference02", reference02, transaction.Reference2);
			AssertEquals("[T" + line + "] TransactionReference03", reference03, transaction.Reference3);
			AssertEquals("[T" + line + "] TransactionReference04", reference04, transaction.Reference4);

			AssertEquals("[T" + line + "] TransactionDateUtc", transactionDateUtc, new DateTimeOffset(transaction.ServiceOccuredUTC));
		}
	}
}
