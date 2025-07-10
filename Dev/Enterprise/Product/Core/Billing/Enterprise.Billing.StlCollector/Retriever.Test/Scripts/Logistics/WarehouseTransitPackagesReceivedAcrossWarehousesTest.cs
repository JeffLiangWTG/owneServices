using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitPackagesReceivedAcrossWarehouses))]
	sealed class WarehouseTransitPackagesReceivedAcrossWarehousesTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @CompanyPK UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @Company2PK UNIQUEIDENTIFIER = newid();
				INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@Company2PK, 'US', 'DE2', 'US company')
				DECLARE @BranchPK UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @CompanyPK);
				DECLARE @Branch2PK UNIQUEIDENTIFIER = newid();
				INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@Branch2PK, @Company2PK, 'DE2')
				DECLARE @AddressPK UNIQUEIDENTIFIER = (SELECT TOP (1) OA_PK FROM dbo.OrgAddress);
				DECLARE @WarehousePK UNIQUEIDENTIFIER = newid();
				DECLARE @AreaPK UNIQUEIDENTIFIER = newid();
				DECLARE @RowPK UNIQUEIDENTIFIER = newid();
				DECLARE @LocationPK UNIQUEIDENTIFIER = newid();
				DECLARE @Warehouse2PK UNIQUEIDENTIFIER = newid();
				DECLARE @Area2PK UNIQUEIDENTIFIER = newid();
				DECLARE @Row2PK UNIQUEIDENTIFIER = newid();
				DECLARE @Location2PK UNIQUEIDENTIFIER = newid();

				DECLARE @RTUWithPackagesAssignedToRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @RTU2WithPackagesAssignedToRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @StandaloneRTUPK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN3PK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN3PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @StandaloneRTUPackageJobPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01PackageID1InRCN1PK UNIQUEIDENTIFIER = 'CD041B5B-B192-42D6-B727-33068A6DDE24';
				DECLARE @KP02PackageID1InRCN2PK UNIQUEIDENTIFIER = '1B57E6A1-BD0D-49B4-AC44-2D179F9FABAD';
				DECLARE @KP03PackageID1InRCN3PK UNIQUEIDENTIFIER = 'D4E2641B-377A-483B-B664-C914BE12F952';
				DECLARE @KP04PackageID2InRCN1PK UNIQUEIDENTIFIER = '2FFC2803-2596-48E6-A324-F2995C8C5D48';
				DECLARE @KP05PackageID1PK UNIQUEIDENTIFIER = '83649B1E-2614-4D66-B495-FD22C9340515';
				DECLARE @KP06PackageID2ADJInRCN2PK UNIQUEIDENTIFIER = 'CA8B8535-CC20-41A4-9A5F-C0686C8C4F03';
				DECLARE @KP07PackageID2InRCN3PK UNIQUEIDENTIFIER = '9645A266-3196-4530-B5BA-F9B3DA6C075E';
				DECLARE @KP08OVPID1InRCN1PK UNIQUEIDENTIFIER = 'B42F2AC3-9BD7-4120-AA39-F4868FEA2845';
				DECLARE @KP09PKG1InOVPID1InRCN1PK UNIQUEIDENTIFIER = '07509B2E-4B7C-44FF-B465-C01129C892EB';
				DECLARE @KP10PKG2InOVPID1InRCN1PK UNIQUEIDENTIFIER = '4331AB03-7B53-464B-AE20-B48DC8FB7BDB';
				DECLARE @KP11OVPID1InRCN2PK UNIQUEIDENTIFIER = '01BC967D-F07A-441D-9F76-059E72FD8EE1';
				DECLARE @KP12PKG1InOVPID1InRCN2PK UNIQUEIDENTIFIER = '7EAE497D-DC1F-4193-9F9A-5786C612FFC6';
				DECLARE @KP13PKG2InOVPID1InRCN2PK UNIQUEIDENTIFIER = 'B5E12D21-46A2-4111-9F12-5FF18B89092C';
				DECLARE @KP14HUID1InRCN1PK UNIQUEIDENTIFIER = '8F8D99A8-B41E-4682-A9C2-16BAC57BAB2C';
				DECLARE @KP15PKG1InHUID1InRCN1PK UNIQUEIDENTIFIER = '2A168206-7633-4DA4-B524-80C0E27324CB';
				DECLARE @KP16HUID1InRCN2PK UNIQUEIDENTIFIER = 'F13A5720-D4F9-474E-9D43-C04E343DF6C3';
				DECLARE @KP17PKG1InHUID1InRCN2PK UNIQUEIDENTIFIER = '712CB680-6F1B-4967-943C-FD4698656067';
				DECLARE @KP18OVPOUTSIDE UNIQUEIDENTIFIER = '6049D5C9-2554-4A65-BD1B-8F552B773CF8';
				DECLARE @KP19PKGOUTSIDE UNIQUEIDENTIFIER = '1CB5CA2B-4BC4-449F-9A5F-01F03672E347';

				DECLARE @KP01PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP05PackageWithoutRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP06PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP07PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP08PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP09PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP10PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP11PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP12PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP13PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP14PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP15PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP16PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP17PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP18PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP19PackagePK UNIQUEIDENTIFIER = newid();

				INSERT dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WarehousePK, @BranchPK, @AddressPK, 'W1', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@Warehouse2PK, @Branch2PK, @AddressPK, 'W2', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES
					(@AreaPK, @WarehousePK, '~!TransitWarehouse!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@Area2PK, @Warehouse2PK, '~!TransitWarehouse2!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsRow (WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
					(@RowPK, @WarehousePK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'),
					(@Row2PK, @Warehouse2PK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');
				INSERT dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, WL_SystemCreateTimeUtc, WL_SystemLastEditTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
					(@LocationPK, @AreaPK, @AreaPK, @RowPK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'),
					(@Location2PK, @Area2PK, @Area2PK, @Row2PK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

				INSERT dbo.WhsItemReceiveTransportationUnit (WRH_PK, WRH_WW_Warehouse, WRH_WL_StagingLocation, WRH_ReferenceNumber, WRH_SystemCreateTimeUtc, WRH_SystemLastEditTimeUtc, WRH_SystemCreateUser, WRH_SystemLastEditUser) VALUES
					(@RTUWithPackagesAssignedToRCNPK, @WarehousePK, @LocationPK, 'WRH01', SYSDATETIME(), SYSDATETIME(), 'US1', 'US1'),
					(@RTU2WithPackagesAssignedToRCNPK, @Warehouse2PK, @Location2PK, 'WRH02', SYSDATETIME(), SYSDATETIME(), 'US1', 'US1'),
					(@StandaloneRTUPK, @WarehousePK, @LocationPK, 'WRH03', SYSDATETIME(), SYSDATETIME(), 'US2', 'US2');

				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_JobID, WRC_SystemCreateUser, WRC_SystemLastEditUser) VALUES
					(@RCN1PK, @WarehousePK, 'WRC01', SYSDATETIME(), SYSDATETIME(), 'RC001', '~BP', '~BP'),
					(@RCN2PK, @Warehouse2PK, 'WRC02', SYSDATETIME(), SYSDATETIME(), 'RC002', '~BP', '~BP'),
					(@RCN3PK, @Warehouse2PK, 'WRC03', SYSDATETIME(), SYSDATETIME(), 'RC003', '~BP', '~BP');

				INSERT dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
					(@RCN1PackageJobPK, @RCN1PK, 'WRC', 'PJ1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@RCN2PackageJobPK, @RCN2PK, 'WRC', 'PJ2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@RCN3PackageJobPK, @RCN3PK, 'WRC', 'PJ3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@StandaloneRTUPackageJobPK, @StandaloneRTUPK, 'WRH', 'PJ4', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.PkgPackageHeader(KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES
					(@KP01PackageID1InRCN1PK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PackageID1InRCN2PK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PackageID1InRCN3PK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PackageID2InRCN1PK, 'KP0123456789012345678901234567890134', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackageID1PK, 'KP03', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06PackageID2ADJInRCN2PK, 'KP0123456789012345678901234567890134', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07PackageID2InRCN3PK, 'KP0123456789012345678901234567890134', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08OVPID1InRCN1PK, 'OVP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP09PKG1InOVPID1InRCN1PK, 'OVP01-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP10PKG2InOVPID1InRCN1PK, 'OVP01-KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP11OVPID1InRCN2PK, 'OVP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP12PKG1InOVPID1InRCN2PK, 'OVP01-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP13PKG2InOVPID1InRCN2PK, 'OVP01-KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP14HUID1InRCN1PK, 'HU01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP15PKG1InHUID1InRCN1PK, 'HU01-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP16HUID1InRCN2PK, 'HU01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP17PKG1InHUID1InRCN2PK, 'HU01-KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP18OVPOUTSIDE, 'OVP-OUT', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP19PKGOUTSIDE, 'PKG-OUT', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KP01PackagePK, @KP01PackageID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PackagePK, @KP02PackageID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PackagePK, @KP03PackageID1InRCN3PK, @RCN3PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PackagePK, @KP04PackageID2InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackageWithoutRCNPK, @KP05PackageID1PK, @StandaloneRTUPackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06PackagePK, @KP06PackageID2ADJInRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07PackagePK, @KP07PackageID2InRCN3PK, @RCN3PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08PackagePK, @KP08OVPID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP09PackagePK, @KP09PKG1InOVPID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP10PackagePK, @KP10PKG2InOVPID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP11PackagePK, @KP11OVPID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP12PackagePK, @KP12PKG1InOVPID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP13PackagePK, @KP13PKG2InOVPID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP14PackagePK, @KP14HUID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP15PackagePK, @KP15PKG1InHUID1InRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP16PackagePK, @KP16HUID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP17PackagePK, @KP17PKG1InHUID1InRCN2PK, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP18PackagePK, @KP18OVPOUTSIDE, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP19PackagePK, @KP19PKGOUTSIDE, @RCN2PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_WRC_TransitReceiveConsignment, WPS_Status, WPS_WRH_TransitReceiveHeader, WPS_WL_LastLocation, WPS_UnloadedTime, WPS_UnloadedNotYetProcessedTime, WPS_AdjustedOut, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser, WPS_ReceivedAs, WPS_IsHandlingUnit, WPS_UnitType) VALUES
					(newid(), @KP01PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:00:00 +00:00', '2014-10-31 13:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP02PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 14:00:00 +00:00', '2014-10-31 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP03PackagePK, @Warehouse2PK, @RCN3PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-11-30 14:00:00 +00:00', '2014-11-30 14:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP04PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 15:00:00 +00:00', '2014-10-31 15:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP05PackageWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-10-31 15:00:00 +00:00', '2014-10-31 15:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP06PackagePK, @Warehouse2PK, @RCN2PK, 'ADJ', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:00:00 +00:00', '2014-10-31 16:00:00 +00:00', 'DMG', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP07PackagePK, @Warehouse2PK, @RCN3PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-09-30 15:00:00 +00:00', '2014-09-30 15:00:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP08PackagePK, @WarehousePK, @RCN1PK, 'ARV', NULL, @LocationPK, NULL, NULL, '', '2014-10-31 13:00:00', '~BP', GetUtcDate(), '~BP', '', 1, 'OVP'),
					(newid(), @KP09PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:30:00 +00:00', '2014-10-31 13:30:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP10PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:40:00 +00:00', '2014-10-31 13:40:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP11PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:30:00 +00:00', '2014-10-31 16:30:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 1, 'OVP'),
					(newid(), @KP12PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:40:00 +00:00', '2014-10-31 16:40:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP13PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:50:00 +00:00', '2014-10-31 16:50:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP14PackagePK, @WarehousePK, @RCN1PK, 'ARV', NULL, @LocationPK, NULL, NULL, '', '2014-10-31 13:00:00', '~BP', GetUtcDate(), '~BP', '', 1 ,'HU'),
					(newid(), @KP15PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 13:30:00 +00:00', '2014-10-31 13:30:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP16PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', NULL, @LocationPK, NULL, NULL, '', '2014-10-31 16:00:00', '~BP', GetUtcDate(), '~BP', '', 1, 'HU'),
					(newid(), @KP17PackagePK, @Warehouse2PK, @RCN2PK, 'ARV', @RTU2WithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 16:30:00 +00:00', '2014-10-31 16:30:00 +00:00', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN', 0, 'PKG'),
					(newid(), @KP18PackagePK, @Warehouse2PK, @RCN2PK, 'BKD', NULL, NULL, NULL, NULL, '', '2014-10-31 16:00:00', '~BP', GetUtcDate(), '~BP', '', 1, 'OVP'),
					(newid(), @KP19PackagePK, @Warehouse2PK, @RCN2PK, 'BKD', NULL, NULL, NULL, NULL, '', '2014-10-31 16:00:00', '~BP', GetUtcDate(), '~BP', '', 0, 'PKG');

				INSERT dbo.PkgPackageHandlingUnitDivot (KPD_PK, KPD_KP_Package, KPD_KP_HandlingUnit, KPD_PackedTime, KPD_UnpackedTime, KPD_SystemCreateTimeUtc, KPD_SystemCreateUser, KPD_SystemLastEditTimeUtc, KPD_SystemLastEditUser) VALUES
					(newid(), @KP09PackagePK, @KP08PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP10PackagePK, @KP08PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP15PackagePK, @KP14PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @KP19PackagePK, @KP18PackagePK, NULL, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 15, transactions.Count());

			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 0, "DE2", "DE2", "~BP", 1, "KP01", "RC002", new DateTimeOffset(2014, 10, 31, 22, 0, 0, TimeSpan.FromHours(8)), "CD041B5B-B192-42D6-B727-33068A6DDE24", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 1, "DE2", "DE2", "~BP", 1, "KP0123456789012345678901234567890134", "RC002", new DateTimeOffset(2014, 11, 1, 0, 0, 0, TimeSpan.FromHours(8)), "9645A266-3196-4530-B5BA-F9B3DA6C075E", "3");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 2, "DEM", "DEM", "~BP", 1, "KP01", "RC001", new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)), "CD041B5B-B192-42D6-B727-33068A6DDE24", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 3, "DEM", "DEM", "~BP", 1, "KP0123456789012345678901234567890134", "RC001", new DateTimeOffset(2014, 10, 31, 23, 0, 0, TimeSpan.FromHours(8)), "9645A266-3196-4530-B5BA-F9B3DA6C075E", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 4, "DEM", "DEM", "~BP", 1, "KP03", "WRH03", new DateTimeOffset(2014, 10, 31, 23, 0, 0, TimeSpan.FromHours(8)), "83649B1E-2614-4D66-B495-FD22C9340515", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 5, "DE2", "DE2", "~BP", 1, "OVP01", "RC002", new DateTimeOffset(2014, 11, 1, 0, 30, 0, TimeSpan.FromHours(8)), "B42F2AC3-9BD7-4120-AA39-F4868FEA2845", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 6, "DE2", "DE2", "~BP", 1, "OVP01-KP01", "RC002", new DateTimeOffset(2014, 11, 1, 0, 40, 0, TimeSpan.FromHours(8)), "07509B2E-4B7C-44FF-B465-C01129C892EB", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 7, "DE2", "DE2", "~BP", 1, "OVP01-KP02", "RC002", new DateTimeOffset(2014, 11, 1, 0, 50, 0, TimeSpan.FromHours(8)), "4331AB03-7B53-464B-AE20-B48DC8FB7BDB", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 8, "DEM", "DEM", "~BP", 1, "OVP01", "RC001", new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)), "B42F2AC3-9BD7-4120-AA39-F4868FEA2845", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 9, "DEM", "DEM", "~BP", 1, "OVP01-KP01", "RC001", new DateTimeOffset(2014, 10, 31, 21, 30, 0, TimeSpan.FromHours(8)), "07509B2E-4B7C-44FF-B465-C01129C892EB", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 10, "DEM", "DEM", "~BP", 1, "OVP01-KP02", "RC001", new DateTimeOffset(2014, 10, 31, 21, 40, 0, TimeSpan.FromHours(8)), "4331AB03-7B53-464B-AE20-B48DC8FB7BDB", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 11, "DE2", "DE2", "~BP", 1, "HU01", "RC002", new DateTimeOffset(2014, 11, 1, 0, 0, 0, TimeSpan.FromHours(8)), "8F8D99A8-B41E-4682-A9C2-16BAC57BAB2C", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 12, "DE2", "DE2", "~BP", 1, "HU01-KP01", "RC002", new DateTimeOffset(2014, 11, 1, 0, 30, 0, TimeSpan.FromHours(8)), "2A168206-7633-4DA4-B524-80C0E27324CB", "2");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 13, "DEM", "DEM", "~BP", 1, "HU01", "RC001", new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)), "8F8D99A8-B41E-4682-A9C2-16BAC57BAB2C", "1");
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 14, "DEM", "DEM", "~BP", 1, "HU01-KP01", "RC001", new DateTimeOffset(2014, 10, 31, 21, 30, 0, TimeSpan.FromHours(8)), "2A168206-7633-4DA4-B524-80C0E27324CB", "1");
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 11);
			}
		}

		/// <summary>
		/// If [WhsItemPackageState].[WPS_KP_Package] is not unique, packages will be counted more than once.
		/// </summary>
		public void TestEnusureTransitPackageStateFkToPackageIsUnique()
		{
			WarehouseTransitPackagesCommonTest.AssertTransitPackageStateFkToPackageIsUnique(TestConnection);
		}

		public void TestScriptShouldUseDateTimeOffsetParameters()
		{
			WarehouseTransitPackagesCommonTest.AssertScriptShouldUseDateTimeOffsetParameters(new RefStlScriptRetriever(new WarehouseTransitPackagesReceivedAcrossWarehouses()));
		}
	}
}
