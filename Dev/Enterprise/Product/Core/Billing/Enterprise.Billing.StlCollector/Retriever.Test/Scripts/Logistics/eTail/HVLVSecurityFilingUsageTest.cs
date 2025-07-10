using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVSecurityFilingUsage))]
	sealed class HVLVSecurityFilingUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 2);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 4, transactions.Count());

				AssertRowMatchingRef1(transactions, "Basic usage", null, "B1", new DateTime(2019, 2, 1, 12, 1, 0), "", 1, "TestHVI001", "TestBarcode001", "UAM", "TestShipperRef001");
				AssertEquals("No transactions are created if category not SEC", 0, FindRowsByRef1(transactions, "TestHVI002").Count());
				var testHVI003MatchingTransactions = FindRowsByRef1(transactions, "TestHVI003");
				AssertEquals("Select all SEC per item", 2, testHVI003MatchingTransactions.Count());
				AssertRowMatchingOccured(testHVI003MatchingTransactions, "Select all SEC per item, match 1/2", null, "B2", new DateTime(2019, 2, 2, 13, 2, 0), "", 1, "TestHVI003", "TestBarcode003", "UAM", "TestShipperRef003");
				AssertRowMatchingOccured(testHVI003MatchingTransactions, "Select all SEC per item, match 2/2", null, "B2", new DateTime(2019, 2, 3, 1, 4, 0), "", 1, "TestHVI003", "TestBarcode003", "USM", "TestShipperRef003");
				AssertEquals("Transactions don't include those created before collection DateTimeRange", 1, FindRowsByRef1(transactions, "TestHVI004").Count());
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ClusterKey INT = 24;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item1PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item2PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item3PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item4PK UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES
					(@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from OrgAddress), '2019-03-29', '2019-03-29', '~BP', '~BP')
				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(@ConsignmentPK, 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', '2019-03-29', '2019-03-29', '~BP', '~BP')
				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser) VALUES 
					(@Item1PK, 'TestHVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item2PK, 'TestHVI002', 'TestBarcode002', 'TestShipperRef002', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item3PK, 'TestHVI003', 'TestBarcode003', 'TestShipperRef003', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-01', '~BP', '~BP'),
					(@Item4PK, 'TestHVI004', 'TestBarcode004', 'TestShipperRef004', @ClusterKey, @ConsignmentPK, '2019-02-01', '2019-02-01', '~BP', '~BP')

				INSERT dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc) VALUES
					(NEWID(), @Item1PK, 'SEC', 'U1', 'UAM', 'C1', 'B1', '2019-02-01 12:01:10'),
					(NEWID(), @Item2PK, 'LVD', 'U1', 'ACR', 'C1', 'B1', '2019-02-01 12:01:10'),
					(NEWID(), @Item3PK, 'SEC', 'U1', 'UAM', 'C2', 'B2', '2019-02-02 13:02:20'),
					(NEWID(), @Item3PK, 'SEC', 'U1', 'USM', 'C2', 'B2', '2019-02-03 01:03:34'),
					(NEWID(), @Item4PK, 'SEC', 'U1', 'UAM', 'C2', 'B2', '2019-01-03 12:01:01'),
					(NEWID(), @Item4PK, 'SEC', 'U1', 'USM', 'C2', 'B2', '2019-02-03 01:03:05')
			";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
