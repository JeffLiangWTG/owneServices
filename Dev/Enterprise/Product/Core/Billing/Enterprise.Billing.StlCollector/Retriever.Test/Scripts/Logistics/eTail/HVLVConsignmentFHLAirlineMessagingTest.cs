using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVConsignmentFHLAirlineMessaging))]
	sealed class HVLVConsignmentFHLAirlineMessagingTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = $@"
				DECLARE @GlbCompanyPK UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GlbBranchPK UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GlbCompanyPK);
				DECLARE @GlbDepartmentPK UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);

				DECLARE @EdiInterchangePK UNIQUEIDENTIFIER = NEWID();

				DECLARE @ClusterKey INT = 1;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ConsignmentPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Consignment2PK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @Item2PK UNIQUEIDENTIFIER = NEWID();


				DECLARE @JobShipmentPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @JobConsolPK UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_HouseBill)
					VALUES (@JobShipmentPK, 'S00001000', 'JS_HouseBill1')

				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_MasterBillNum)
					VALUES (@JobConsolPK, 'C00001000', 'JK_MasterBillNum1')

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES
					(@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_WaybillNumber, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(@ConsignmentPK, 'HVC00000001', 'Waybill001', @BookingHeaderPK, @ClusterKey, '2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemID, HVI_HVC_Consignment, HVI_ClusterKey, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SYstemCreateUser, HVI_SystemLastEditUser) VALUES
					(@ItemPK, 'Item1', @ConsignmentPK, @ClusterKey,'2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVUsage (HXU_PK, HXU_UsageTimeUtc, HXU_GC_NKCompany, HXU_BranchCode, HXU_HVI_ParentItem, HXU_Category, HXU_Code, HXU_GS_NKUser) VALUES
					(NEWID(), '2021-07-13', 'WTG', 'DEM', @ItemPK, 'SEC', 'FHL', '~BP')

				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_WaybillNumber, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(@Consignment2PK, 'HVC00000002', 'Waybill002', @BookingHeaderPK, @ClusterKey, '2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVItem (HVI_PK, HVI_ItemID, HVI_HVC_Consignment, HVI_ClusterKey, HVI_SystemCreateTimeUtc, HVI_SystemLastEditTimeUtc, HVI_SYstemCreateUser, HVI_SystemLastEditUser) VALUES
					(@Item2PK, 'Item2', @Consignment2PK, @ClusterKey,'2021-07-10', '2021-07-11', '~BP', '~BP')

				INSERT dbo.HVLVUsage (HXU_PK, HXU_UsageTimeUtc, HXU_GC_NKCompany, HXU_BranchCode, HXU_HVI_ParentItem, HXU_Category, HXU_Code, HXU_GS_NKUser) VALUES
					(NEWID(), '2021-07-13', 'WTG', 'DEM', @Item2PK, 'SEC', 'ACA', '~BP')
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of collected consignments", 1, transactions.Count());

			var transaction = transactions.Single();
			AssertEquals("BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 13), transaction.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "HVC00000001", transaction.Reference1);
			AssertEquals("TransactionReference02", "Waybill001", transaction.Reference2);
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
