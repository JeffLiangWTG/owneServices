using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemStandardUsageV2))]
	sealed class HVLVItemStandardUsageV2Test : HVLVItemUsageTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSetCore(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			CombineAssertions(() =>
			{
				AssertRowContent(transactions, 0, "009", 2, 1, "GB1");
				AssertRowContent(transactions, 1, "010", 2, 1, "GB1");
				AssertRowContent(transactions, 2, "011", 2, 1, "GB2");
				AssertRowContent(transactions, 3, "012", 2, 1, "GB2");
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @GlbCompanyPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GlbCompanyPK2 UNIQUEIDENTIFIER = NEWID();

				DECLARE @ClusterKey          INT              = 24;
				DECLARE @PlusClusterKey      INT              = 36;
				DECLARE @BookingHeaderPK     UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentPK       UNIQUEIDENTIFIER = NEWID();

				DECLARE @Item1PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item2PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item3PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item4PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item5PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item6PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item7PK             UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item8PK             UNIQUEIDENTIFIER = NEWID();

				DECLARE @PlusItem1PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem2PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem3PK         UNIQUEIDENTIFIER = NEWID();
				DECLARE @PlusItem4PK         UNIQUEIDENTIFIER = NEWID();

				DECLARE @Shipment1           UNIQUEIDENTIFIER = newid();

				INSERT dbo.JobShipment
					(JS_PK, JS_UniqueConsignRef, JS_ShipmentType)
				VALUES
					(@Shipment1, 'StandardShipment1', 'HVL')

				INSERT dbo.HVLVBookingHeader
					(HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser)
				VALUES
					(@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2019-03-29', '2019-03-29', '~BP', '~BP')

				INSERT dbo.HVLVConsignment
					(HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser)
				VALUES
					(@ConsignmentPK, 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', '2019-03-29', '2019-03-29', '~BP', '~BP')

				INSERT dbo.HVLVConsignmentHeader
					(HCH_PK, HCH_AutoVersion, HCH_ClusterKey, HCH_JobNumber, HCH_IsArchived, HCH_JS_Shipment, HCH_UsageType, HCH_SystemCreateTimeUtc, HCH_SystemLastEditTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditUser)
				VALUES
					(@ConsignmentHeaderPK, '0', @PlusClusterKey, 'HCH_JobNumber01', '0', @Shipment1, 'S', '2019-02-01', sysutcdatetime(), '~BP', '~BP')

				INSERT dbo.HVLVItem
					(HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_ShipperFirstUsageTimeUtc, HVI_OriginFirstUsageTimeUtc, HVI_DestinationFirstUsageTimeUtc, HVI_SecurityFilingFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser)
				VALUES 
					(@Item1PK, 'TestHVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-10', '2019-02-28', '2019-02-28', '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item2PK, 'TestHVI002', 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-10', NULL, NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item3PK, 'TestHVI003', 'TestBarcode003', 'TestShipperRef003', @ClusterKey, @ConsignmentPK, '2019-02-01', NULL, NULL, '2019-02-28', '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item4PK, 'TestHVI004', 'TestBarcode004', 'TestShipperRef004', @ClusterKey, @ConsignmentPK, '2019-02-01', NULL, '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item5PK, 'TestHVI005', 'TestBarcode005', 'TestShipperRef005', @ClusterKey, @ConsignmentPK, NULL, NULL, NULL, '2019-02-28', '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item6PK, 'TestHVI006', 'TestBarcode006', 'TestShipperRef006', @ClusterKey, @ConsignmentPK, NULL, NULL, '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item7PK, 'TestHVI007', 'TestBarcode007', 'TestShipperRef007', @ClusterKey, @ConsignmentPK, NULL, '2019-02-10', NULL, '2019-02-28', '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item8PK, 'TestHVI008', 'TestBarcode008', 'TestShipperRef008', @ClusterKey, @ConsignmentPK, NULL, '2019-02-10', '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP')

				INSERT dbo.HVLVItem
					(HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_UsageType, HVI_ShipperFirstUsageTimeUtc, HVI_OriginFirstUsageTimeUtc, HVI_DestinationFirstUsageTimeUtc, HVI_SecurityFilingFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser)
				VALUES 
					(@PlusItem1PK, 'TestHVI009', 'TestBarcode009', 'TestShipperRef009', @PlusClusterKey, @ConsignmentPK, 'P', NULL, '2019-02-10', '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@PlusItem2PK, 'TestHVI010', 'TestBarcode010', 'TestShipperRef010', @PlusClusterKey, @ConsignmentPK, 'P', NULL, '2019-02-10', '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@PlusItem3PK, 'TestHVI011', 'TestBarcode011', 'TestShipperRef011', @PlusClusterKey, @ConsignmentPK, 'P', NULL, '2019-02-10', '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@PlusItem4PK, 'TestHVI012', 'TestBarcode012', 'TestShipperRef012', @PlusClusterKey, @ConsignmentPK, 'P', NULL, '2019-02-10', '2019-02-28', NULL, '2019-02-01', '2019-02-01', '~BP', '~BP')

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
				VALUES
					(@GlbCompanyPK1, 'GC1', 'AU company1', 'AU', 'AUD'),
					(@GlbCompanyPK2, 'GC2', 'AU company2', 'AU', 'AUD')

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
					(NEWID(), 'GB1', @GlbCompanyPK1),
					(NEWID(), 'GB2', @GlbCompanyPK2)

				INSERT INTO dbo.HVLVUsage
					(HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
				VALUES
					(NEWID(), @Item1PK, 'CW1', '~BP', 'GLS', 'GC1', 'GB1', '2019-02-01'),
					(NEWID(), @Item2PK, 'CW1', '~BP', 'GLS', 'GC1', 'GB1', '2019-02-01'),
					(NEWID(), @Item3PK, 'CW1', '~BP', 'GLS', 'GC1', 'GB1', '2019-02-01'),
					(NEWID(), @Item4PK, 'CW1', '~BP', 'GLS', 'GC1', 'GB1', '2019-02-01'),
					(NEWID(), @Item5PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01'),
					(NEWID(), @Item6PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01'),
					(NEWID(), @Item7PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01'),
					(NEWID(), @Item8PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01'),
					(NEWID(), @PlusItem1PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB1', '2019-02-01'),
					(NEWID(), @PlusItem2PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB1', '2019-02-01'),
					(NEWID(), @PlusItem3PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01'),
					(NEWID(), @PlusItem4PK, 'CW1', '~BP', 'GLS', 'GC2', 'GB2', '2019-02-01')

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_SE_NKEvent, SL_GB_NKBranch)
				VALUES
					(NEWID(), 'HVLVItem', @Item1PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @Item2PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @Item3PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @Item4PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @Item5PK, '2019-02-01', 'ADD', 'GB2'),
					(NEWID(), 'HVLVItem', @Item6PK, '2019-02-01', 'ADD', 'GB2'),
					(NEWID(), 'HVLVItem', @Item7PK, '2019-02-01', 'ADD', 'GB2'),
					(NEWID(), 'HVLVItem', @Item8PK, '2019-02-01', 'ADD', 'GB2'),
					(NEWID(), 'HVLVItem', @PlusItem1PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @PlusItem2PK, '2019-02-01', 'ADD', 'GB1'),
					(NEWID(), 'HVLVItem', @PlusItem3PK, '2019-02-01', 'ADD', 'GB2'),
					(NEWID(), 'HVLVItem', @PlusItem4PK, '2019-02-01', 'ADD', 'GB2')
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void TestVersions()
		{
			CombineAssertions("Test minimal and maximal CW1 version", () =>
			{
				AssertEquals("23.7.28.391", ScriptToTest.MinCW1Version);
				AssertEquals("", ScriptToTest.MaxCW1Version);
			});
		}
	}
}
