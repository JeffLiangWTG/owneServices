using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(JPAdvancedCargoInformationFilling))]
	sealed class JPAdvancedCargoInformationFillingTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction = transactions.First();
			AssertEquals("[T1] CompanyCode", "WTG", transaction.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2023, 11, 9, 03, 12, 32, 747), transaction.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference01", "AFR10000001", transaction.Reference1);
			AssertEquals("[T1] TransactionReference02", "SPQA", transaction.Reference2);
			AssertEquals("[T1] TransactionReference03", "TEST00000001", transaction.Reference3);
			AssertEquals("[T1] TransactionReference04", "SAPC1000000001", transaction.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals("[T1] BranchCode", "JPC", transaction.GetBranchCode());
			AssertEquals("[T1] UserCode", "~BP", transaction.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction.BillableCount);
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES
					(0x1, 'AU', 'AUD', 'WTG', 'AU company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES
					(0x1, 0x1, 'JPC')
INSERT dbo.JPAFRHeader (JPH_PK, JPH_GB_Branch, JPH_SystemCreateTimeUtc, JPH_SystemLastEditTimeUtc, JPH_JobReference, JPH_MasterBillNumber, JPH_CarrierCode, JPH_SystemCreateUser, JPH_SystemLastEditUser) VALUES
					(0x1, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR10000001', 'TEST00000001', 'SPQA', '~BP', '~BP'),
					(0x2, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR20000002', 'TEST00000002', 'SPQA', '~BP', '~BP'),
					(0x3, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR30000003', 'TEST00000003', 'SPQA', '~BP', '~BP'),
					(0x4, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR40000004', 'TEST00000004', 'SPQA', '~BP', '~BP'),
					(0x5, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR50000005', 'TEST00000005', 'SPQA', '~BP', '~BP'),
					(0x6, 0x1, '2023-10-15 03:12:32', '2023-10-15 03:12:32', 'AFR60000006', 'TEST00000006', 'SPQA', '~BP', '~BP'),
					(0x7, 0x1, '2023-11-09 01:00:00', '2023-11-09 01:00:01', 'AFR70000007', 'TEST00000007', 'SPQA', '~BP', '~BP')
INSERT dbo.JPAFRBills (JPB_PK, JPB_JPH_Header, JPB_BillNumber, JPB_IsMaterBill, JPB_SystemCreateTimeUtc, JPB_SystemLastEditTimeUtc, JPB_SystemCreateUser, JPB_SystemLastEditUser) VALUES
					(0x1, 0x1, 'SAPC1000000001', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- valid
					(0x2, 0x2, 'SAPC2000000002', 1, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- invalid JPB_IsMasterBill
					(0x3, 0x3, 'SAPC3000000000', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- valid
					(0x4, 0x4, 'SAPC4000000004', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- valid
					(0x5, 0x5, 'SAPC5000000005', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- valid
					(0x6, 0x6, 'SAPC6000000006', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP'),	-- valid
					(0x7, 0x7, 'SAPC7000000007', 0, '2023-11-09 01:00:01', '2023-11-09 01:00:01', '~BP', '~BP')		-- valid
INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsEstimate, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow, SL_DataSource) VALUES
				(0x1, 'JPAFRHeader', 0x1, 'N', 'N', 'AHR-ACCEPTED', '2023-11-09 03:12:32.747', '2023-11-09 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),		-- all valid
				(0x2, 'JPAFRHeader', 0x2, 'N', 'N', 'AHR-ACCEPTED', '2023-11-09 03:12:32.747', '2023-11-09 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),		-- bind to an invalid bill
				(0x3, 'JobComInvoiceLine', 0x3, 'N', 'N', 'AHR-ACCEPTED', '2023-11-09 03:12:32.747', '2023-11-09 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),	-- invalid SL_Table
				(0x4, 'JPAFRHeader', 0x4, 'N', 'N', 'AHR-REJECTED', '2023-11-09 03:12:32.747', '2023-11-09 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),		-- invalid SL_Reference
				(0x5, 'JPAFRHeader', 0x5, 'N', 'N', 'AHR-ACCEPTED', '2023-11-09 03:12:32.747', '2023-11-09 03:12:32.747', '~BP', 'CMP', '', '', 0, 'C'),		-- invalid SL_SE_NKEvent
				(0x6, 'JPAFRHeader', 0x6, 'N', 'N', 'AHR-ACCEPTED', '2023-10-15 03:12:32.747', '2023-10-15 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),		-- invalid SL_PostedTimeUtc (too early)
				(0x7, 'JPAFRHeader', 0x7, 'N', 'N', 'AHR-ACCEPTED', '2023-12-15 03:12:32.747', '2023-12-15 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C'),		-- invalid SL_PostedTimeUtc (too late)
				(0x8, 'JPAFRHeader', 0x6, 'N', 'N', 'AHR-ACCEPTED', '2023-11-15 03:12:32.747', '2023-11-15 03:12:32.747', '~BP', 'MSC', '', '', 0, 'C')			-- all valid, but should be grouped with 0x6 and not be selected due to the MIN(sl2.SL_PostedTimeUtc) method
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
