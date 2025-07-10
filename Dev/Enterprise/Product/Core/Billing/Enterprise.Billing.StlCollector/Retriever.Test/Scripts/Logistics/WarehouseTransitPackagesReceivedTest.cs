using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitPackagesReceived))]
	sealed class WarehouseTransitPackagesReceivedTest : RefStlScriptWithDefaultsTest
	{
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
				DECLARE @StandaloneRTUPK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @StandaloneRTUPackageJobPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01PackageIDInRCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02PackageIDInRCN2PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03PackageIDInRCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04PackageIDInRCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP05PackageIDPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP05PackageWithoutRCNPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WarehousePK, @BranchPK, @AddressPK, 'W1', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES
					(@AreaPK, @WarehousePK, '~!TransitWarehouse!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsRow (WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
					(@RowPK, @WarehousePK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');
				INSERT dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, WL_SystemCreateTimeUtc, WL_SystemLastEditTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
					(@LocationPK, @AreaPK, @AreaPK, @RowPK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

				INSERT dbo.WhsItemReceiveTransportationUnit (WRH_PK, WRH_WW_Warehouse, WRH_WL_StagingLocation, WRH_ReferenceNumber, WRH_SystemCreateTimeUtc, WRH_SystemLastEditTimeUtc, WRH_SystemCreateUser, WRH_SystemLastEditUser) VALUES
					(@RTUWithPackagesAssignedToRCNPK, @WarehousePK, @LocationPK, 'WRH01', SYSDATETIME(), SYSDATETIME(), 'US1', 'US1'),
					(@StandaloneRTUPK, @WarehousePK, @LocationPK, 'WRH02', SYSDATETIME(), SYSDATETIME(), 'US2', 'US2');

				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_JobID, WRC_SystemCreateUser, WRC_SystemLastEditUser) VALUES
					(@RCN1PK, @WarehousePK, 'WRC01', SYSDATETIME(), SYSDATETIME(), 'RC001', '~BP', '~BP'),
					(@RCN2PK, @WarehousePK, 'WRC02', SYSDATETIME(), SYSDATETIME(), 'RC002', '~BP', '~BP');

				INSERT dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_JobID, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
					(@RCN1PackageJobPK, @RCN1PK, 'WRC', 'PJ1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@RCN2PackageJobPK, @RCN2PK, 'WRC', 'PJ2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@StandaloneRTUPackageJobPK, @StandaloneRTUPK, 'WRH', 'PJ3', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.PkgPackageHeader(KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES
					(@KP01PackageIDInRCN1PK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PackageIDInRCN2PK, 'KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PackageIDInRCN1PK, 'KP03', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PackageIDInRCN1PK, 'KP04', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackageIDPK, 'KP05', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KP01PackagePK, @KP01PackageIDInRCN1PK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02PackagePK, @KP02PackageIDInRCN2PK, @RCN2PackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03PackagePK, @KP03PackageIDInRCN1PK, @RCN1PackageJobPK, 3, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04PackagePK, @KP04PackageIDInRCN1PK, @RCN1PackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackageWithoutRCNPK, @KP05PackageIDPK, @StandaloneRTUPackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_WRC_TransitReceiveConsignment, WPS_Status, WPS_WRH_TransitReceiveHeader, WPS_WL_LastLocation, WPS_UnloadedTime, WPS_UnloadedNotYetProcessedTime, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser, WPS_ReceivedAs) VALUES
					(newid(), @KP01PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 20:59:59 +08:00', '2014-10-31 20:59:59 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP02PackagePK, @WarehousePK, @RCN2PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-10-31 21:00:00 +08:00', '2014-10-31 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP03PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-11-01 00:00:00 +11:00', '2014-11-01 00:00:00 +11:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP04PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-11-30 21:00:00 +08:00', '2014-11-30 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP05PackageWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-11-09 09:09:09 +09:00', '2014-11-09 09:09:09 +09:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 0, "DEM", "DEM", "US1", 1, "KP02", "RC002", new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 1, "DEM", "DEM", "US1", 1, "KP03", "RC001", new DateTimeOffset(2014, 11, 01, 0, 0, 0, TimeSpan.FromHours(11)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 2, "DEM", "DEM", "US2", 1, "KP05", "WRH02", new DateTimeOffset(2014, 11, 09, 9, 9, 9, TimeSpan.FromHours(9)));
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
			WarehouseTransitPackagesCommonTest.AssertScriptShouldUseDateTimeOffsetParameters(new RefStlScriptRetriever(new WarehouseTransitPackagesReceived()));
		}
	}
}
