using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemPlusUsage))]
	sealed class HVLVItemPlusUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 2);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected three billable transaction to be returned", 3, transactions.Count());
				AssertRow(transactions.Single(t => t.Reference1 == "TestHVI009" && t.ServiceOccuredUTC == new DateTime(2019, 2, 10)), "0", "GC2", "GB1", new DateTime(2019, 2, 10), string.Empty, 1, "TestHVI009", "TestBarcode009", "V2", "TestShipperRef009");
				AssertRow(transactions.Single(t => t.Reference1 == "TestHVI010" && t.ServiceOccuredUTC == new DateTime(2019, 2, 12)), "1", "GC2", "GB1", new DateTime(2019, 2, 12), string.Empty, 1, "TestHVI010", "TestBarcode010", "V2", "TestShipperRef010");
				AssertRow(transactions.Single(t => t.Reference1 == "TestHVI011" && t.ServiceOccuredUTC == new DateTime(2019, 2, 14)), "2", "GC2", "GB2", new DateTime(2019, 2, 14), string.Empty, 1, "TestHVI011", "TestBarcode011", "V2", "TestShipperRef011");
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @GlbCompanyPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GlbCompanyPK2 UNIQUEIDENTIFIER = NEWID();

				DECLARE @ClusterKey           INT              = 24;
				DECLARE @PlusClusterKey       INT              = 36;

				DECLARE @ConsignmentHeaderPK  UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentHeader2PK UNIQUEIDENTIFIER = NEWID();

				DECLARE @ConsignmentPK        UNIQUEIDENTIFIER = NEWID();
				DECLARE @Consignment2PK       UNIQUEIDENTIFIER = NEWID();

				DECLARE @StandardItem1PK      UNIQUEIDENTIFIER = NEWID();
				DECLARE @StandardItem2PK      UNIQUEIDENTIFIER = NEWID();
				DECLARE @StandardItem3PK      UNIQUEIDENTIFIER = NEWID();

				DECLARE @PlusItem1PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem2PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem3PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem4PK         UNIQUEIDENTIFIER = NEWID();

				DECLARE @Shipment1           UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment2           UNIQUEIDENTIFIER = newid();

				INSERT dbo.JobShipment
					(JS_PK, JS_UniqueConsignRef, JS_ShipmentType)
				VALUES
					(@Shipment1, 'StandardShipment', 'HVL'),
					(@Shipment2, 'PlusShipment', 'HVL')

				INSERT dbo.HVLVConsignmentHeader
					(HCH_PK, HCH_AutoVersion, HCH_ClusterKey, HCH_JobNumber, HCH_IsArchived, HCH_JS_Shipment, HCH_UsageType, HCH_SystemCreateTimeUtc, HCH_SystemLastEditTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditUser)
				VALUES
					(@ConsignmentHeaderPK, '0', @ClusterKey, 'HCH_JobNumber01', '0', @Shipment1, 'S', '2019-02-01', sysutcdatetime(), '~BP', '~BP'),
					(@ConsignmentHeader2PK, '0', @PlusClusterKey, 'HCH_JobNumber02', '0', @Shipment2, 'P', '2019-02-01', sysutcdatetime(), '~BP', '~BP')

				INSERT dbo.HVLVConsignment
					(HVC_PK, HVC_ConsignmentId, HVC_HCH_Header, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser)
				VALUES
					(@ConsignmentPK, 'TESTHVC001', @ConsignmentHeaderPK, @ClusterKey, 'CLR', '2019-03-29', '2019-03-29', '~BP', '~BP'),
					(@Consignment2PK, 'TESTHVC002', @ConsignmentHeader2PK, @PlusClusterKey, 'CLR', '2019-03-29', '2019-03-29', '~BP', '~BP')

				INSERT dbo.HVLVItem
					(HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_UsageType, HVI_ShipperFirstUsageTimeUtc, HVI_OriginFirstUsageTimeUtc, HVI_DestinationFirstUsageTimeUtc, HVI_SecurityFilingFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser)
				VALUES 
					(@StandardItem1PK, 'TestHVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, 'S', '2019-02-01', '2019-02-01', '2019-02-01', '2019-02-01', '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@StandardItem2PK, 'TestHVI002', 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK, 'S', '2019-02-04', '2019-02-04', NULL, NULL, '2019-02-04', '2019-02-04', '~BP', '~BP'),
					(@StandardItem3PK, 'TestHVI003', 'TestBarcode003', 'TestShipperRef003', @ClusterKey, @ConsignmentPK, 'S', '2019-02-07', NULL, NULL, '2019-02-07', '2019-02-07', '2019-02-07', '~BP', '~BP'),
					(@PlusItem1PK, 'TestHVI009', 'TestBarcode009', 'TestShipperRef009', @PlusClusterKey, @Consignment2PK, 'P', NULL, '2019-02-10', '2019-02-10', NULL, '2019-02-10', '2019-02-10', '~BP', '~BP'),
					(@PlusItem2PK, 'TestHVI010', 'TestBarcode010', 'TestShipperRef010', @PlusClusterKey, @Consignment2PK, 'P', NULL, '2019-02-12', '2019-02-12', NULL, '2019-02-12', '2019-02-12', '~BP', '~BP'),
					(@PlusItem3PK, 'TestHVI011', 'TestBarcode011', 'TestShipperRef011', @PlusClusterKey, @Consignment2PK, 'P', NULL, '2019-02-14', '2019-02-14', NULL, '2019-02-14', '2019-02-14', '~BP', '~BP'),
					(@PlusItem4PK, 'TestHVI012', 'TestBarcode012', 'TestShipperRef012', @PlusClusterKey, @Consignment2PK, 'P', NULL, '2019-02-16', '2019-02-16', NULL, '2019-02-16', '2019-02-16', '~BP', '~BP')

				INSERT INTO dbo.HVLVUsage
					(HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
				VALUES
					(NEWID(), @StandardItem1PK, 'CW1', '~BP', 'GLS', 'GC1', 'GB1', '2019-02-01'),
					(NEWID(), @StandardItem2PK, 'CW1', '~BP', 'GLC', 'GC1', 'GB1', '2019-02-04'),
					(NEWID(), @StandardItem3PK, 'CW1', '~BP', 'CWU', 'GC1', 'GB1', '2019-02-07'),
					(NEWID(), @PlusItem1PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB1', '2019-02-10'),
					(NEWID(), @PlusItem2PK, 'CW1', '~BP', 'GLC', 'GC2', 'GB1', '2019-02-12'),
					(NEWID(), @PlusItem3PK, 'CW1', '~BP', 'CWU', 'GC2', 'GB2', '2019-02-14'),
					(NEWID(), @PlusItem4PK, 'WEB', '~BP', 'CWU', 'GC2', 'GB2', '2019-02-16')
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
