using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitOuterPackagesReceivedForStatistics))]
	sealed class WarehouseTransitOuterPackagesReceivedForStatisticsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CompanyPK UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @BranchPK UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @CompanyPK);
				DECLARE @AddressPK UNIQUEIDENTIFIER = (SELECT TOP (1) OA_PK FROM dbo.OrgAddress);
				DECLARE @WarehousePK UNIQUEIDENTIFIER = newid();
				DECLARE @AreaPK UNIQUEIDENTIFIER = newid();
				DECLARE @RowPK UNIQUEIDENTIFIER = newid();
				DECLARE @LocationPK UNIQUEIDENTIFIER = newid();

				DECLARE @RTUWithPackagesAssignedToRCNPK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN3PK UNIQUEIDENTIFIER = newid();
				DECLARE @DCN1PK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN3PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @DCN1PackageJobPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01OVP1IDInRCN1PK UNIQUEIDENTIFIER = '7BF8DD71-B639-4BD7-83CD-6DE26601C11C';
				DECLARE @KP02PKG1IDInOVP1InRCN1PK UNIQUEIDENTIFIER = '1B943B0A-86BD-46E5-9C8A-8A1B267E6163';
				DECLARE @KP03PKG2IDInOVP1InRCN1PK UNIQUEIDENTIFIER = '871BEE3E-9D2C-4D9C-B161-6D9C4CB734F0';
				DECLARE @KP04PKG3IDInRCN1PK UNIQUEIDENTIFIER = 'E998A08B-3468-4ADC-A3AE-8C7669D6A23F';
				DECLARE @KP05HU1IDInRCN1PK UNIQUEIDENTIFIER = '01B92168-73EE-4073-8EE4-C42BFD95BCE3';
				DECLARE @KP06PKG4IDInHU1InRCN1PK UNIQUEIDENTIFIER = 'A366A7DF-2643-4CC5-8855-C2587086C580';
				DECLARE @KP07PKG5IDInHU1InRCN1PK UNIQUEIDENTIFIER = '52C0E327-C2BC-4069-8186-00CCAF8A97A5';
				DECLARE @KP08OVP2IDInRCN2PK UNIQUEIDENTIFIER = '71CB48FA-DBC1-44BA-B05D-C48008B283C5';
				DECLARE @KP09PKG1IDInOVP2InRCN2PK UNIQUEIDENTIFIER = '8B25E8BE-C746-4030-ACBB-5B546D617D7B';
				DECLARE @KP10PKG2IDInOVP2InRCN2PK UNIQUEIDENTIFIER = '8945CC15-AE7C-494E-B124-AEB61E673561';
				DECLARE @KP11PKG3IDInRCN2PK UNIQUEIDENTIFIER = '830173FD-B9A5-4709-8256-8AF8A749DCF7';
				DECLARE @KP12PKG1IDInRCN3PK UNIQUEIDENTIFIER = 'A03CB435-6530-423B-9D56-E78F0A552717';
				DECLARE @KP13PKG1IDInDCN1PK UNIQUEIDENTIFIER = 'F025A2EF-E862-4177-9B70-AB6474D92912';

				DECLARE @KP01PackagePK UNIQUEIDENTIFIER = '7FEDD11C-613C-4909-A5C1-4476EF91701E';
				DECLARE @KP02PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04PackagePK UNIQUEIDENTIFIER = '289BF2F7-BB0C-40C2-A76B-F102B480A80D';
				DECLARE @KP05PackagePK UNIQUEIDENTIFIER = '193DD003-8B49-4F4A-87C4-A8384C576BD9';
				DECLARE @KP06PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP07PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP08PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP09PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP10PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP11PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP12PackagePK UNIQUEIDENTIFIER = '0BC26464-54A6-4835-A942-40D9AF6C5104';
				DECLARE @KP13PackagePK UNIQUEIDENTIFIER = '5C29D2B6-3E4B-4214-9723-8B258704B3C1';
				DECLARE @KP14PackagePK UNIQUEIDENTIFIER = 'C959DBA9-B29E-4420-A07A-929C7A78D077';

				INSERT dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WarehousePK, @BranchPK, @AddressPK, 'W1', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES
					(@AreaPK, @WarehousePK, '~!TransitWarehouse!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsRow (WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
					(@RowPK, @WarehousePK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');
				INSERT dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, WL_SystemCreateTimeUtc, WL_SystemLastEditTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
					(@LocationPK, @AreaPK, @AreaPK, @RowPK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

				INSERT dbo.WhsItemReceiveTransportationUnit (WRH_PK, WRH_WW_Warehouse, WRH_WL_StagingLocation, WRH_UnitType, WRH_ReferenceNumber, WRH_SystemCreateTimeUtc, WRH_SystemLastEditTimeUtc, WRH_SystemCreateUser, WRH_SystemLastEditUser) VALUES
					(@RTUWithPackagesAssignedToRCNPK, @WarehousePK, @LocationPK, 'VEH', 'WRH01', SYSDATETIME(), SYSDATETIME(), 'US1', 'US1');

				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_Direction, WRC_TransportMode, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_JobID, WRC_SystemCreateUser, WRC_SystemLastEditUser) VALUES
					(@RCN1PK, @WarehousePK, 'WRC01', 'IMP', 'SEA', SYSDATETIME(), SYSDATETIME(), 'RC001', '~BP', '~BP'),
					(@RCN2PK, @WarehousePK, 'WRC02', 'EXP', 'AIR', SYSDATETIME(), SYSDATETIME(), 'RC002', '~BP', '~BP'),
					(@RCN3PK, @WarehousePK, 'WRC03', 'EXP', 'AIR', SYSDATETIME(), SYSDATETIME(), 'RC003', '~BP', '~BP');

				INSERT dbo.WhsItemDispatchConsignment (WDC_PK, WDC_WW_Warehouse, WDC_ConsignmentID, WDC_Direction, WDC_TransportMode, WDC_SystemCreateTimeUtc, WDC_SystemLastEditTimeUtc, WDC_JobID, WDC_SystemCreateUser, WDC_SystemLastEditUser) VALUES
					(@DCN1PK, @WarehousePK, 'WDC01', 'IMP', 'SEA', SYSDATETIME(), SYSDATETIME(), 'DC001', '~BP', '~BP');

				INSERT dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
					(@RCN1PackageJobPK, @RCN1PK, 'WRC', 'PJ1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@RCN2PackageJobPK, @RCN2PK, 'WRC', 'PJ2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@RCN3PackageJobPK, @RCN3PK, 'WRC', 'PJ3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@DCN1PackageJobPK, @DCN1PK, 'WDC', 'PJ4', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.PkgPackageHeader(KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES
					(@KP01OVP1IDInRCN1PK, 'OVP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PKG1IDInOVP1InRCN1PK, 'OVP01-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PKG2IDInOVP1InRCN1PK, 'OVP01-KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PKG3IDInRCN1PK, 'KP03-RCN1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05HU1IDInRCN1PK, 'HU01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06PKG4IDInHU1InRCN1PK, 'HU01-KP04', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07PKG5IDInHU1InRCN1PK, 'HU01-KP05', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08OVP2IDInRCN2PK, 'OVP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP09PKG1IDInOVP2InRCN2PK, 'OVP02-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP10PKG2IDInOVP2InRCN2PK, 'OVP02-KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP11PKG3IDInRCN2PK, 'KP03-RCN2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP12PKG1IDInRCN3PK, 'KP01-RCN3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP13PKG1IDInDCN1PK, 'KP01-DCN1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_KP_ParentPackage, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_Weight, KP_WeightUQ, KP_Volume, KP_VolumeUQ, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KP01PackagePK, @KP01OVP1IDInRCN1PK, @RCN1PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PackagePK, @KP02PKG1IDInOVP1InRCN1PK, @RCN1PackageJobPK, @KP01PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PackagePK, @KP03PKG2IDInOVP1InRCN1PK, @RCN1PackageJobPK, @KP01PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PackagePK, @KP04PKG3IDInRCN1PK, @RCN1PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackagePK, @KP05HU1IDInRCN1PK, @RCN1PackageJobPK, NULL, 1, 1, 'PKG', 1100, 'G', 1200, 'L', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06PackagePK, @KP06PKG4IDInHU1InRCN1PK, @RCN1PackageJobPK, @KP05PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07PackagePK, @KP07PKG5IDInHU1InRCN1PK, @RCN1PackageJobPK, @KP05PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08PackagePK, @KP08OVP2IDInRCN2PK, @RCN2PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP09PackagePK, @KP09PKG1IDInOVP2InRCN2PK, @RCN2PackageJobPK, @KP08PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP10PackagePK, @KP10PKG2IDInOVP2InRCN2PK, @RCN2PackageJobPK, @KP08PackagePK, 0, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP11PackagePK, @KP11PKG3IDInRCN2PK, @RCN2PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP12PackagePK, @KP12PKG1IDInRCN3PK, @RCN3PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP13PackagePK, @KP13PKG1IDInDCN1PK, @DCN1PackageJobPK, NULL, 1, 1, 'PKG', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP14PackagePK, NULL, @RCN1PackageJobPK, NULL, 1, 10, 'PLT', 1, 'KG', 1, 'M3', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_WRC_TransitReceiveConsignment, WPS_WDC_TransitDispatchConsignment, WPS_Status, WPS_WRH_TransitReceiveHeader, WPS_WL_LastLocation, WPS_UnloadedTime, WPS_UnloadedNotYetProcessedTime, WPS_AdjustedOut, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser, WPS_ReceivedAs, WPS_IsHandlingUnit, WPS_UnitType) VALUES
					(newid(), @KP01PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:00:00 +00:00', '2014-10-31 13:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 1, 'OVP'),
					(newid(), @KP02PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP03PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP04PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 15:00:00 +00:00', '2014-10-31 15:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP05PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 15:00:00 +00:00', '2014-10-31 15:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 1, 'HU'),
					(newid(), @KP06PackagePK, @WarehousePK, @RCN1PK, NULL,'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:00:00 +00:00', '2014-10-31 16:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP07PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP08PackagePK, @WarehousePK, @RCN2PK, NULL, 'BKD', NULL, NULL, NULL, NULL, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', '', 1, 'OVP'),
					(newid(), @KP09PackagePK, @WarehousePK, @RCN2PK, NULL, 'BKD', NULL, NULL, NULL, NULL, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', '', 0, 'PKG'),
					(newid(), @KP10PackagePK, @WarehousePK, @RCN2PK, NULL, 'BKD', NULL, NULL, NULL, NULL, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', '', 0, 'PKG'),
					(newid(), @KP11PackagePK, @WarehousePK, @RCN2PK, NULL, 'BKD', NULL, NULL, NULL, NULL, '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', '', 0, 'PKG'),
					(newid(), @KP12PackagePK, @WarehousePK, @RCN3PK, @DCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP13PackagePK, @WarehousePK, NULL, @DCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP14PackagePK, @WarehousePK, @RCN1PK, NULL, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:00:00 +00:00', '2014-10-31 13:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 1, 'PKL');

				INSERT dbo.PkgPackageHandlingUnitDivot (KPD_PK, KPD_KP_Package, KPD_KP_HandlingUnit, KPD_PackedTime, KPD_UnpackedTime, KPD_SystemCreateTimeUtc, KPD_SystemCreateUser, KPD_SystemLastEditTimeUtc, KPD_SystemLastEditUser) VALUES
					(newid(), @KP02PackagePK, @KP01PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP03PackagePK, @KP01PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP06PackagePK, @KP05PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP07PackagePK, @KP05PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP09PackagePK, @KP08PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP10PackagePK, @KP08PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @KP01PackagePK, 'PkgPackage', 111, 'CUS', 'CEN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @RCN3PK, 'WhsItemReceiveConsignment', 222, 'CUS', 'CEN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @DCN1PK, 'WhsItemDispatchConsignment', 333, 'CUS', 'CEN', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction>  transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)), "W1", "7FEDD11C-613C-4909-A5C1-4476EF91701E", "OVP01", 1, "PKG", "OVP", "IMP", "SEA", "VEH", 1.00, 1.00, 1);
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 23, 0, 0, TimeSpan.FromHours(8)), "W1", "193DD003-8B49-4F4A-87C4-A8384C576BD9", "HU01", 1, "PKG", "HU", "IMP", "SEA", "VEH", 1.10, 1.20, 0);
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 23, 0, 0, TimeSpan.FromHours(8)), "W1", "289BF2F7-BB0C-40C2-A76B-F102B480A80D", "KP03-RCN1", 1, "PKG", "PKG", "IMP", "SEA", "VEH", 1.00, 1.00, 0);
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 22, 0, 0, TimeSpan.FromHours(8)), "W1", "0BC26464-54A6-4835-A942-40D9AF6C5104", "KP01-RCN3", 1, "PKG", "PKG", "EXP", "SEA", "VEH", 1.00, 1.00, 0);
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 22, 0, 0, TimeSpan.FromHours(8)), "W1", "5C29D2B6-3E4B-4214-9723-8B258704B3C1", "KP01-DCN1", 1, "PKG", "PKG", null, "SEA", "VEH", 1.00, 1.00, 0);
			AssertRow(transactions, 1, "DEM", "DEM", 1, new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)), "W1", "C959DBA9-B29E-4420-A07A-929C7A78D077", "PLT*10", 10, "PLT", "PKL", "IMP", "SEA", "VEH", 1.00, 1.00, 0);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, int expectedCount, DateTimeOffset transactionDateUtc, string reference01, string reference02, string reference03, int pkgQty, string packageType, string unitType, string direction, string transportMode, string transportUnitType, double weightInKG, double volumeInM3, int hasCEN)
		{
			string assertPrefix = $"[T{rowNumber}";
			var transaction = transactions.Single(t => t.Reference1 == reference01 && t.Reference2 == reference02);
			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
			AssertEquals(assertPrefix + "TransactionDateUtc", transactionDateUtc, new DateTimeOffset(transaction.ServiceOccuredUTC));
#if NETFRAMEWORK
			var expectedAdditionalRefs = direction != null ? "{" + $"\"PkgQty\":{pkgQty},\"PackageType\":\"{packageType}\",\"UnitType\":\"{unitType}\",\"Direction\":\"{direction}\",\"TransportMode\":\"{transportMode}\",\"TransportUnitType\":\"{transportUnitType}\",\"WeightInKG\":{string.Format("{0:F}", weightInKG)},\"VolumeInM3\":{string.Format("{0:F}", volumeInM3)},\"HasCEN\":{hasCEN}" + "}"
				: "{" + $"\"PkgQty\":{pkgQty},\"PackageType\":\"{packageType}\",\"UnitType\":\"{unitType}\",\"TransportMode\":\"{transportMode}\",\"TransportUnitType\":\"{transportUnitType}\",\"WeightInKG\":{string.Format("{0:F}", weightInKG)},\"VolumeInM3\":{string.Format("{0:F}", volumeInM3)},\"HasCEN\":{hasCEN}" + "}";
#elif NET
			var expectedAdditionalRefs = direction != null ? "{" + $"\"PkgQty\":{pkgQty},\"PackageType\":\"{packageType}\",\"UnitType\":\"{unitType}\",\"Direction\":\"{direction}\",\"TransportMode\":\"{transportMode}\",\"TransportUnitType\":\"{transportUnitType}\",\"WeightInKG\":{string.Format("{0:F2}", weightInKG)},\"VolumeInM3\":{string.Format("{0:F2}", volumeInM3)},\"HasCEN\":{hasCEN}" + "}"
				: "{" + $"\"PkgQty\":{pkgQty},\"PackageType\":\"{packageType}\",\"UnitType\":\"{unitType}\",\"TransportMode\":\"{transportMode}\",\"TransportUnitType\":\"{transportUnitType}\",\"WeightInKG\":{string.Format("{0:F2}", weightInKG)},\"VolumeInM3\":{string.Format("{0:F2}", volumeInM3)},\"HasCEN\":{hasCEN}" + "}";
#endif
			AssertEquals(assertPrefix + "AdditionalRefs", expectedAdditionalRefs, transaction.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 11);
			}
		}
	}
}
