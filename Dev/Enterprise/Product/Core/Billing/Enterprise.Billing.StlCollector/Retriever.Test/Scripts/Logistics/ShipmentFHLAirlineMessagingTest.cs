using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ShipmentFHLAirlineMessaging))]
	sealed class ShipmentFHLAirlineMessagingTest : RefStlScriptWithDefaultsTest
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

				INSERT dbo.EDIInterchange (EI_PK, EI_InterchangeNum, EI_SessionGuid, EI_GB, EI_SystemCreateTimeUtc, EI_SystemCreateUser, EI_SystemLastEditTimeUtc, EI_SystemLastEditUser) VALUES
					(@EdiInterchangePK, '1', NEWID(), @GlbBranchPK, '2021-07-10', '~BP', '2021-07-11', '~BP')

				INSERT dbo.EDIMessage (EM_PK, EM_EI, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ApplicationCode, EM_ApplicationReference, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(NEWID(), @EdiInterchangePK, @GlbBranchPK, @GlbDepartmentPK, @ConsignmentPK, 'HVLVConsignment', '2021-07-13', 'CIM', 'HVC00000001', 'TRX', 'FHL', 'XXX', 'SNT', '~BP', '2021-07-13', '~BP'),
					(NEWID(), @EdiInterchangePK, @GlbBranchPK, @GlbDepartmentPK, @JobConsolPK, 'JobConsol', '2021-07-13', 'CIM', 'S00001000', 'TRX', 'FHL', 'XXX', 'SNT', '~BP', '2021-07-13', '~BP')
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of collected shipments", 1, transactions.Count());

			var transaction = transactions.Single();
			AssertEquals("BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 13), transaction.ServiceOccuredUTC);
			AssertEquals("TransactionReference01", "S00001000", transaction.Reference1);
			AssertEquals("TransactionReference02", "JS_HouseBill1", transaction.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 7);
	}
}
