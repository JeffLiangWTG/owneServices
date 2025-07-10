using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitUnlabelledPacklinesReceived))]
	sealed class WarehouseTransitUnlabelledPacklinesReceivedTest : RefStlScriptWithDefaultsTest
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
				DECLARE @StandaloneRTUPK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PK UNIQUEIDENTIFIER = newid();

				DECLARE @RCN1PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @RCN2PackageJobPK UNIQUEIDENTIFIER = newid();
				DECLARE @StandaloneRTUPackageJobPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01OuterInRCN1IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP05PackageIDInRCN1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP06OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP07OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP08OuterInRCN2IDPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01OuterPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02OuterWithoutRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03OuterNestedInnersPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04OuterWithoutPackageStatePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP05PackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @KP06OuterWithoutRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP07OuterWithoutRCNPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP08OuterWithoutRCNPK UNIQUEIDENTIFIER = newid();

				DECLARE @KP01Inner1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP01Inner2PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP02Inner1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03Inner1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP03Inner1_1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP04InnerPK UNIQUEIDENTIFIER = newid();
				DECLARE @KP06Inner1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP07Inner1PK UNIQUEIDENTIFIER = newid();
				DECLARE @KP08Inner1PK UNIQUEIDENTIFIER = newid();

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
					(@KP01OuterInRCN1IDPK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02OuterInRCN2IDPK, 'KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03OuterInRCN2IDPK, 'KP03', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04OuterInRCN2IDPK, 'KP04', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackageIDInRCN1PK, 'KP05', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06OuterInRCN2IDPK, 'KP06', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07OuterInRCN2IDPK, 'KP07', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08OuterInRCN2IDPK, 'KP08', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KP01OuterPK, @KP01OuterInRCN1IDPK, @RCN1PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02OuterWithoutRCNPK, @KP02OuterInRCN2IDPK, @StandaloneRTUPackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03OuterNestedInnersPK, @KP03OuterInRCN2IDPK, @RCN2PackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04OuterWithoutPackageStatePK, @KP04OuterInRCN2IDPK, @RCN2PackageJobPK, 5, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP05PackagePK, @KP05PackageIDInRCN1PK, @RCN1PackageJobPK, 7, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06OuterWithoutRCNPK, @KP06OuterInRCN2IDPK, @StandaloneRTUPackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07OuterWithoutRCNPK, @KP07OuterInRCN2IDPK, @StandaloneRTUPackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08OuterWithoutRCNPK, @KP08OuterInRCN2IDPK, @StandaloneRTUPackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KP_ParentPackage, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@KP01Inner1PK, @KP01OuterPK, @RCN1PackageJobPK, 0, 2, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP01Inner2PK, @KP01OuterPK, @RCN1PackageJobPK, 0, 3, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP02Inner1PK, @KP02OuterWithoutRCNPK, @StandaloneRTUPackageJobPK, 0, 3, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03Inner1PK, @KP03OuterNestedInnersPK, @RCN2PackageJobPK, 0, 8, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP03Inner1_1PK, @KP03Inner1PK, @RCN2PackageJobPK, 0, 16, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP04InnerPK, @KP04OuterWithoutPackageStatePK, @RCN2PackageJobPK, 0, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP06Inner1PK, @KP06OuterWithoutRCNPK, @StandaloneRTUPackageJobPK, 0, 3, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP07Inner1PK, @KP07OuterWithoutRCNPK, @StandaloneRTUPackageJobPK, 0, 3, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@KP08Inner1PK, @KP08OuterWithoutRCNPK, @StandaloneRTUPackageJobPK, 0, 3, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_WRC_TransitReceiveConsignment, WPS_Status, WPS_WRH_TransitReceiveHeader, WPS_WL_LastLocation, WPS_UnloadedTime, WPS_UnloadedNotYetProcessedTime, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser, WPS_ReceivedAs) VALUES
					(newid(), @KP01OuterPK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-11-01 01:01:01 +08:00', '2014-11-01 01:01:01 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP02OuterWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-11-02 02:02:02 +08:00', '2014-11-02 02:02:02 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP03OuterNestedInnersPK, @WarehousePK, @RCN2PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-11-03 03:03:03 +11:00', '2014-11-03 03:03:03 +11:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP05PackagePK, @WarehousePK, @RCN1PK, 'ARV', @RTUWithPackagesAssignedToRCNPK, @LocationPK, '2014-11-03 03:03:03 +11:00', '2014-11-03 03:03:03 +11:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP06OuterWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-10-31 20:59:59 +08:00', '2014-10-31 20:59:59 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP07OuterWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-10-31 21:00:00 +08:00', '2014-10-31 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @KP08OuterWithoutRCNPK, @WarehousePK, NULL, 'ARV', @StandaloneRTUPK, @LocationPK, '2014-11-30 21:00:00 +08:00', '2014-11-30 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 0, "DEM", "DEM", "US2", 1, "KP07", "WRH02", new DateTimeOffset(2014, 10, 31, 21, 0, 0, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 1, "DEM", "DEM", "US1", 2, "KP01", "RC001", new DateTimeOffset(2014, 11, 01, 1, 1, 1, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 2, "DEM", "DEM", "US2", 1, "KP02", "WRH02", new DateTimeOffset(2014, 11, 02, 2, 2, 2, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 3, "DEM", "DEM", "US1", 1, "KP03", "RC002", new DateTimeOffset(2014, 11, 03, 3, 3, 3, TimeSpan.FromHours(11)));
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 11);
			}
		}

		public void TestScriptShouldUseDateTimeOffsetParameters()
		{
			WarehouseTransitPackagesCommonTest.AssertScriptShouldUseDateTimeOffsetParameters(new RefStlScriptRetriever(new WarehouseTransitUnlabelledPacklinesReceived()));
		}
	}
}
