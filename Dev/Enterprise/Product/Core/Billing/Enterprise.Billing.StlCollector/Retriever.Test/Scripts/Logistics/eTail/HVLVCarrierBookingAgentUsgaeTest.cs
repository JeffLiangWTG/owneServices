using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVCarrierBookingAgentUsage))]
	sealed class HVLVCarrierBookingAgentUsgaeTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 6);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2021, 6, 10));
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "GB1", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "TestHVI001", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "TestBarcode001", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference04", "TestShipperRef001", transaction1.Reference4);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2021, 6, 28));
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "GB2", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "TestHVI001", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", "TestBarcode001", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference04", "TestShipperRef001", transaction2.Reference4);
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ClusterKey int = 24;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = newid();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = newid();
				DECLARE @itemPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES (@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2019-03-29', '2019-03-29', '~BP', '~BP')
				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES (@ConsignmentPK, 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', '2019-03-29', '2019-03-29', '~BP', '~BP')
				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser) VALUES 
				(@itemPK, 'TestHVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, '2019-03-29', '2019-03-29', '~BP', '~BP'),
				(NEWID(), 'TestHVI002', 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK, '2019-03-29', '2019-03-29', '~BP', '~BP')
				INSERT dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GB_NKBranch) VALUES
				(NEWID(), @itemPK, 'HVLVItem', 'BKC', '2021-6-10', '2021-6-10', 'GB1'),
				(NEWID(), @itemPK, 'HVLVItem', 'BKC', '2021-6-28', '2021-6-28', 'GB2')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
