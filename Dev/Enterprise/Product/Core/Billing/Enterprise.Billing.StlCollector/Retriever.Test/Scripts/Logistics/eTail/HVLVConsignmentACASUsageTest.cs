using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVConsignmentACASUsage))]
	sealed class HVLVConsignmentACASUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 7);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of collected consignments", 1, transactions.Count());

			var transaction = transactions.Single();
			AssertEquals("BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 16), transaction.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "Consign", transaction.Reference1);
			AssertEquals("TransactionReference02", "Waybill004", transaction.Reference2);
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ClusterKey INT = 1;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentWithoutACASMessageStatusPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentWithACASMessageStatusPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentWithNonACAUsageCode UNIQUEIDENTIFIER = NEWID();

				DECLARE @ConsignmentBeforeCollectorDateRange UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentAfterCollectorDateRange UNIQUEIDENTIFIER = NEWID();

				DECLARE @ItemWithoutACASMessageStatusPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemWithACASMessageStatusPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemBeforeCollectorDateRange UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemAfterCollectorDateRange UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemWithNonACAUsageCode UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES (@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2021-07-10', '2021-07-11', '~BP', '~BP')
				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_WaybillNumber, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_ACASMessageStatus, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(@ConsignmentWithoutACASMessageStatusPK, 'ConsignNoACASMessageStatus', 'Waybill002', @BookingHeaderPK, @ClusterKey, 'CLR', '', '2021-07-10', '2021-07-11', '~BP', '~BP'),
					(@ConsignmentWithACASMessageStatusPK, 'Consign', 'Waybill004', @BookingHeaderPK, @ClusterKey, 'CLR', 'ARQ', '2021-07-10', '2021-07-16', '~BP', '~BP'),
					(@ConsignmentBeforeCollectorDateRange, 'ConsignBefore', 'Waybill005', @BookingHeaderPK, @ClusterKey, 'CLR', 'ARQ', '2021-06-10', '2021-06-11', '~BP', '~BP'),
					(@ConsignmentAfterCollectorDateRange, 'ConsignAfter', 'Waybill006', @BookingHeaderPK, @ClusterKey, 'CLR', 'ARQ', '2021-08-10', '2021-08-11', '~BP', '~BP'),
					(@ConsignmentWithNonACAUsageCode, 'ConsignNonACA', 'Waybill007', @BookingHeaderPK, @ClusterKey, 'CLR', 'ARQ', '2021-08-10', '2021-08-11', '~BP', '~BP')

				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemID, HVI_HVC_Consignment, HVI_ClusterKey, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser) VALUES
					(@ItemWithoutACASMessageStatusPK, 'Item2', @ConsignmentWithoutACASMessageStatusPK, @ClusterKey, '2021-07-10', '2021-07-13', '~BP', '~BP'),
					(@ItemWithACASMessageStatusPK, 'Item4', @ConsignmentWithACASMessageStatusPK, @ClusterKey, '2021-07-10', '2021-07-13', '~BP', '~BP'),
					(@ItemBeforeCollectorDateRange, 'Item5', @ConsignmentBeforeCollectorDateRange, @ClusterKey, '2021-07-10', '2021-07-13', '~BP', '~BP'),
					(@ItemAfterCollectorDateRange, 'Item6', @ConsignmentAfterCollectorDateRange, @ClusterKey, '2021-07-10', '2021-07-13', '~BP', '~BP'),
					(@ItemWithNonACAUsageCode, 'Item7', @ConsignmentAfterCollectorDateRange, @ClusterKey, '2021-07-14', '2021-07-14', '~BP', '~BP')


				INSERT dbo.HVLVUsage (HXU_PK, HXU_UsageTimeUtc, HXU_GC_NKCompany, HXU_BranchCode, HXU_HVI_ParentItem, HXU_Category, HXU_Code, HXU_GS_NKUser) VALUES
					(NEWID(), '2021-07-14', 'WTG', 'DEM', @ItemWithoutACASMessageStatusPK, 'SEC', 'ACA', '~BP'),
					(NEWID(), '2021-07-16', 'WTG', 'DEM', @ItemWithACASMessageStatusPK, 'SEC', 'ACA', '~BP'),
					(NEWID(), '2021-06-11', 'WTG', 'DEM', @ItemBeforeCollectorDateRange, 'SEC', 'ACA', '~BP'),
					(NEWID(), '2021-08-11', 'WTG', 'DEM', @ItemAfterCollectorDateRange, 'SEC', 'ACA', '~BP'),
					(NEWID(), '2021-07-11', 'WTG', 'DEM', @ItemWithNonACAUsageCode, 'SEC', 'ABC', '~BP')";

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
