using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemUsageByCategory))]
	sealed class HVLVItemUsageByCategoryTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = $@"
				DECLARE @ClusterKey INT = 1;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemPK UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES
					(@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_WaybillNumber, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_RN_NKConsigneeCountryCode, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(@ConsignmentPK, 'HVC00000001', 'Waybill001', @BookingHeaderPK, @ClusterKey, 'CN', '2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemId, HVI_CurrentBarcode, HVI_ShipperReference, HVI_ClusterKey, HVI_HVC_Consignment, HVI_JS_LoadedOnShipment, HVI_DestinationFirstUsageTimeUtc, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditUser) VALUES 
					(@ItemPK, 'HVI001', 'TestBarcode001', 'TestShipperRef001', @ClusterKey, @ConsignmentPK, NULL, '2020-10-02', '2019-03-29', '2019-03-29', 'BP~', 'BP~')

				INSERT dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_GS_NKUser, HXU_UsageTimeUtc) VALUES
					(NEWID(), @ItemPK, 'LVD', 'USC', 'LMG', 'LTT', 'LS', '2021-07-10'),
					(NEWID(), @ItemPK, 'SEC', 'ACA', 'AMD', 'CPU', 'LSU', '2021-07-10')
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of collected items", 2, transactions.Count());

			var lmgTransaction = transactions.Single(x => x.GetCompanyCode() == "LMG");
			CombineAssertions(() =>
			{
				AssertEquals("BillingReference1", "LVD", lmgTransaction.Reference1);
				AssertEquals("BillingReference2", "USC", lmgTransaction.Reference2);
				AssertEquals("BillingReference3", "TestBarcode001", lmgTransaction.Reference3);
				AssertEquals("BillingReference4", "CN", lmgTransaction.Reference4);
				AssertEquals("BranchCode", "LTT", lmgTransaction.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 10), lmgTransaction.ServiceOccuredUTC);
			});

			var amdTransaction = transactions.Single(x => x.GetCompanyCode() == "AMD");
			CombineAssertions(() =>
			{
				AssertEquals("BillingReference1", "SEC", amdTransaction.Reference1);
				AssertEquals("BillingReference2", "ACA", amdTransaction.Reference2);
				AssertEquals("BillingReference3", "TestBarcode001", amdTransaction.Reference3);
				AssertEquals("BillingReference4", "CN", amdTransaction.Reference4);
				AssertEquals("BranchCode", "CPU", amdTransaction.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 10), amdTransaction.ServiceOccuredUTC);
			});
		}

		public void TestVersions()
		{
			CombineAssertions("Test minimal and maximal CW1 version", () =>
			{
				AssertEquals("23.7.28.391", ScriptToTest.MinCW1Version);
				AssertEquals("", ScriptToTest.MaxCW1Version);
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 7);

		protected override bool IsMandatoryForMilestones => false;
	}
}
