using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitPackagesDispatched))]
	sealed class WarehouseTransitPackagesDispatchedTest : RefStlScriptWithDefaultsTest
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

				DECLARE @RTUPK UNIQUEIDENTIFIER = newid();
				DECLARE @LoadListPK UNIQUEIDENTIFIER = newid();
				DECLARE @DTU1PK UNIQUEIDENTIFIER = newid();
				DECLARE @DTU2PK UNIQUEIDENTIFIER = newid();

				DECLARE @ReceiveConsignmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @DispatchConsignmentPK UNIQUEIDENTIFIER = newid();

				DECLARE @PackageID1PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID2PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID3PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID4PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID5PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID6PK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageID7PK UNIQUEIDENTIFIER = newid();

				DECLARE @PackageJobPK UNIQUEIDENTIFIER = newid();

				DECLARE @PackageLoadedOutsideBillingCyclePK UNIQUEIDENTIFIER = newid();
				DECLARE @FLOPackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @DEPPackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @FINPackagePK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageOverTimePK UNIQUEIDENTIFIER = newid();
				DECLARE @PackageWithoutDTUPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.WhsWarehouse (WW_PK, WW_GB_RelatedCompanyBranch, WW_OA_WarehouseAddress, WW_WarehouseCode, WW_IsVirtualWarehouse, WW_WLT_DefaultLocationType, WW_SystemCreateTimeUtc, WW_SystemCreateUser, WW_SystemLastEditTimeUtc, WW_SystemLastEditUser) VALUES
					(@WarehousePK, @BranchPK, @AddressPK, 'W1', 1, '16C9FD62-730A-42ED-A20E-699606FFF360', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, WA_SystemCreateTimeUtc, WA_SystemCreateUser, WA_SystemLastEditTimeUtc, WA_SystemLastEditUser) VALUES
					(@AreaPK, @WarehousePK, '~!TransitWarehouse!~', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT dbo.WhsRow (WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_SystemLastEditTimeUtc, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditUser) VALUES
					(@RowPK, @WarehousePK, 'A', 1, 1, 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');
				INSERT dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, WL_SystemLastEditTimeUtc, WL_SystemCreateTimeUtc, WL_SystemCreateUser, WL_SystemLastEditUser) VALUES
					(@LocationPK, @AreaPK, @AreaPK, @RowPK, '16C9FD62-730A-42ED-A20E-699606FFF360', 1, GETUTCDATE(), GETUTCDATE(), '~BP', '~BP');

				INSERT dbo.WhsItemReceiveTransportationUnit (WRH_PK, WRH_WW_Warehouse, WRH_WL_StagingLocation, WRH_ReferenceNumber, WRH_SystemCreateTimeUtc, WRH_SystemLastEditTimeUtc, WRH_SystemCreateUser, WRH_SystemLastEditUser) VALUES
					(@RTUPK, @WarehousePK, @LocationPK, 'WRH01', SYSDATETIME(), SYSDATETIME(), '~BP', '~BP');

				INSERT dbo.WhsItemDispatchLoadList(WDL_PK, WDL_WW_Warehouse, WDL_JobID, WDL_ReferenceNumber, WDL_SystemCreateTimeUtc, WDL_SystemLastEditTimeUtc, WDL_SystemCreateUser, WDL_SystemLastEditUser) VALUES
					(@LoadListPK, @WarehousePK, 'WDL01', 'WDL01', SYSDATETIME(), SYSDATETIME(), '~BP', '~BP');

				INSERT dbo.WhsItemDispatchTransportationUnit (WDH_PK, WDH_WW_Warehouse, WDH_ReferenceNumber, WDH_SystemCreateTimeUtc, WDH_SystemLastEditTimeUtc, WDH_SystemCreateUser, WDH_SystemLastEditUser) VALUES
					(@DTU1PK, @WarehousePK, 'WDT01', SYSDATETIME(), SYSDATETIME(), 'US1', 'US1'),
					(@DTU2PK, @WarehousePK, 'WDT02', SYSDATETIME(), SYSDATETIME(), 'US2', 'US2');

				INSERT dbo.WhsItemReceiveConsignment (WRC_PK, WRC_WW_IntendedWarehouse, WRC_ConsignmentID, WRC_SystemCreateTimeUtc, WRC_SystemLastEditTimeUtc, WRC_JobID, WRC_SystemCreateUser, WRC_SystemLastEditUser) VALUES
					(@ReceiveConsignmentPK, @WarehousePK, 'WRC01', SYSDATETIME(), SYSDATETIME(), 'RC001', '~BP', '~BP');

				INSERT dbo.WhsItemDispatchConsignment (WDC_PK, WDC_ConsignmentID, WDC_WW_Warehouse, WDC_SystemCreateTimeUtc, WDC_SystemLastEditTimeUtc, WDC_JobID, WDC_SystemCreateUser, WDC_SystemLastEditUser) VALUES
					(@DispatchConsignmentPK, 'WDC01', @WarehousePK, SYSDATETIME(), SYSDATETIME(), 'DC001', '~BP', '~BP');

				INSERT dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
					(@PackageJobPK, @ReceiveConsignmentPK, 'WRC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.PkgPackageHeader(KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES
					(@PackageID1PK, 'KP01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID2PK, 'KP02', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID3PK, 'KP03', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID4PK, 'KP04', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID5PK, 'KP05', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID6PK, 'KP06', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageID7PK, 'KP07', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
					(@PackageLoadedOutsideBillingCyclePK, @PackageID1PK, @PackageJobPK, 1, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@FLOPackagePK, @PackageID2PK, @PackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@DEPPackagePK, @PackageID3PK, @PackageJobPK, 2, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@FINPackagePK, @PackageID4PK, @PackageJobPK, 3, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), @PackageID5PK, @PackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageWithoutDTUPK, @PackageID6PK, @PackageJobPK, 4, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@PackageOverTimePK, @PackageID7PK, @PackageJobPK, 5, 1, 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.WhsItemPackageState (WPS_PK, WPS_KP_Package, WPS_WW_Warehouse, WPS_Status, WPS_WRH_TransitReceiveHeader, WPS_WRC_TransitReceiveConsignment, WPS_WDC_TransitDispatchConsignment, WPS_WDH_TransitDispatchHeader, WPS_WDL_LoadList, WPS_IsSecure, WPS_SecurityStatus, WPS_WL_ReceiveLocation, WPS_WL_LastLocation, WPS_UnloadedTime, WPS_UnloadedNotYetProcessedTime, WPS_LoadedTime, WPS_SystemCreateTimeUtc, WPS_SystemCreateUser, WPS_SystemLastEditTimeUtc, WPS_SystemLastEditUser, WPS_ReceivedAs) VALUES
					(newid(), @PackageLoadedOutsideBillingCyclePK, @WarehousePK , 'DEP', @RTUPK, @ReceiveConsignmentPK, @DispatchConsignmentPK, @DTU1PK, @LoadListPK, 1, 'SEC', @LocationPK, @LocationPK, '2014-09-30 19:59:59 +08:00', '2014-09-30 19:59:59 +08:00', '2014-09-30 21:59:59 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @FLOPackagePK, @WarehousePK, 'FIN', @RTUPK, @ReceiveConsignmentPK, @DispatchConsignmentPK, @DTU1PK, @LoadListPK, 1, 'SEC', @LocationPK, @LocationPK, '2014-09-30 20:00:00 +08:00', '2014-09-30 20:00:00 +08:00', '2014-09-30 22:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @DEPPackagePK, @WarehousePK, 'DEP', @RTUPK, @ReceiveConsignmentPK, @DispatchConsignmentPK, @DTU1PK, @LoadListPK, 1, 'SEC', @LocationPK, @LocationPK, '2014-10-01 20:00:00 +08:00', '2014-10-01 20:00:00 +08:00', '2014-10-01 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @FINPackagePK, @WarehousePK, 'FIN', @RTUPK, @ReceiveConsignmentPK, @DispatchConsignmentPK, @DTU2PK , @LoadListPK, 1, 'SEC', @LocationPK, @LocationPK, '2014-10-01 09:09:09 +09:00', '2014-10-01 09:09:09 +09:00', '2014-10-02 21:00:00 +11:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @PackageOverTimePK, @WarehousePK, 'DEP', @RTUPK, @ReceiveConsignmentPK, @DispatchConsignmentPK, @DTU1PK , @LoadListPK, 1, 'SEC', @LocationPK, @LocationPK, '2014-10-01 09:09:09 +09:00', '2014-10-01 09:09:09 +09:00', '2014-10-31 21:00:00 +08:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN'),
					(newid(), @PackageWithoutDTUPK, @WarehousePK, 'PUT', @RTUPK, @ReceiveConsignmentPK, null, null, null, 1, 'SEC', @LocationPK, @LocationPK, '2014-10-03 05:03:03 +8:00', '2014-10-03 05:03:03 +8:00', null, GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SCN');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 0, "DEM", "DEM", "US1", 1, "KP02", "WDT01", new DateTimeOffset(2014, 9, 30, 22, 0, 0, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 1, "DEM", "DEM", "US1", 1, "KP03", "WDT01", new DateTimeOffset(2014, 10, 01, 21, 0, 0, TimeSpan.FromHours(8)));
			WarehouseTransitPackagesCommonTest.AssertRowSet(transactions, 2, "DEM", "DEM", "US2", 1, "KP04", "WDT02", new DateTimeOffset(2014, 10, 02, 21, 0, 0, TimeSpan.FromHours(11)));
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 10);
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
			WarehouseTransitPackagesCommonTest.AssertScriptShouldUseDateTimeOffsetParameters(new RefStlScriptRetriever(new WarehouseTransitPackagesDispatched()));
		}
	}
}
