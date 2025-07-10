using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(AdditionalServicesSGAirlineMessaging))]
	sealed class AdditionalServicesSGAirlineMessagingTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringOnlyNoMaximumSizeRangeForTransactionalGrainScriptTesting(new DateTime(2017, 4, 30, 14, 0, 0), new DateTime(2017, 6, 30, 14, 0, 0));

		protected override void PrepareTestData()
		{
			string sqlText = @"
--Insert a company whose country code is not GB
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (0x1, 'AU', 'AUD', 'WTG', 'AU company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x1, 0x1, 'CBR')
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (0x1, 'DEP')
INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_HouseBill) VALUES (0x1, 'JS_UniqueConsignRef1', 'JS_HouseBill1')
INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_MasterBillNum, JK_RL_NKLoadPort) VALUES (0x1, 'JK_UniqueConsignRef1', 'JK_MasterBillNum1', 'SG123')
INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES (0x1, 0x1, 0x1)

--Insert another company whose country code is not GB
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (0x2, 'CN', 'CNY', 'WGT', 'CN company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x2, 0x2, 'SHA')

--Insert a company whose country code is GB
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES (0x3, 'GB', 'GBP', 'TWG', 'GB company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x3, 0x3, 'MAN')

INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_ApplicationReference, EM_Status, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
	(0x1, 0x1, 0x1, '2017-05-01', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table1', 'DAT', '2017-05-01', 'DAT'), -- All valid data, minimum allowed EM_SystemCreateTimeUtc
	(0x2, 0x1, 0x1, '2017-06-30', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table2', 'DAT', '2017-06-30', 'DAT'), -- All valid data, maximum allowed EM_SystemCreateTimeUtc
	(0x3, 0x1, 0x1, '2017-07-01', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table3', 'DAT', '2017-07-01', 'DAT'), -- EM_SystemCreateTimeUtc is just later than the allowed range
	(0x4, 0x1, 0x1, '2017-04-30', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table4', 'DAT', '2017-04-30', 'DAT'), -- EM_SystemCreateTimeUtc is just sooner than the allowed range
	(0x5, 0x1, 0x1, '2017-05-01', 'TXR', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table5', 'DAT', '2017-05-01', 'DAT'), -- EM_ReceiveTransmit is invalid
	(0x6, 0x1, 0x1, '2017-05-01', 'TRX', 'CDM', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table6', 'DAT', '2017-05-01', 'DAT'), -- EM_MessageType is invalid
	(0x7, 0x1, 0x1, '2017-05-01', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'STN', 0x1, 'Table7', 'DAT', '2017-05-01', 'DAT'), -- EM_Status is invalid
	(0X8, 0x1, 0x1, '2017-05-01', 'TRX', 'CMD', 'CDM', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table8', 'DAT', '2017-05-01', 'DAT'), -- EM_ApplicationCode is invalid
	(0X9, 0x1, 0x1, '2017-05-01', 'TRX', 'CMD', 'CMD', '', 'SNT', 0x1, 'Table9', 'DAT', '2017-05-01', 'DAT'), -- EM_ApplicationReference is not equal to JS_UniqueConsignRef
	(0x21, 0x2, 0x1, '2017-05-01', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table10', 'DAT', '2017-05-01', 'DAT'), -- All valid data
	(0x22, 0x3, 0x1, '2017-05-01', 'TRX', 'CMD', 'CMD', 'JK_UniqueConsignRef1', 'SNT', 0x1, 'Table11', 'DAT', '2017-05-01', 'DAT'); -- All valid data but its company country code
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = transactions.Single(t => t.GetBranchCode() == "SHA");
			AssertEquals("[T1] CompanyCode", "WGT", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2017, 5, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference01", "JK_UniqueConsignRef1", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "JS_UniqueConsignRef1", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "JS_HouseBill1", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "JK_MasterBillNum1", transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] UserCode", "DAT", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = transactions.Single(t => t.Reference4 == null);
			AssertEquals("[T2] CompanyCode", "WTG", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2017, 5, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "JS_UniqueConsignRef1", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "JS_HouseBill1", transaction2.Reference3);
			AssertEquals("[T2] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction2.Reference5);
			AssertEquals("[T2] BranchCode", "CBR", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "DAT", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = transactions.Single(t => t.Reference1 == "JK_UniqueConsignRef1" && t.GetBranchCode() == "CBR" && t.ServiceOccuredUTC == new DateTime(2017, 5, 1));
			AssertEquals("[T3] CompanyCode", "WTG", transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionReference02", "JS_UniqueConsignRef1", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "JS_HouseBill1", transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", "JK_MasterBillNum1", transaction3.Reference4);
			AssertEquals("[T3] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction3.Reference5);
			AssertEquals("[T3] UserCode", "DAT", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);

			var transaction4 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2017, 6, 30));
			AssertEquals("[T4] CompanyCode", "WTG", transaction4.GetCompanyCode());
			AssertEquals("[T4] TransactionReference01", "JK_UniqueConsignRef1", transaction4.Reference1);
			AssertEquals("[T4] TransactionReference02", "JS_UniqueConsignRef1", transaction4.Reference2);
			AssertEquals("[T4] TransactionReference03", "JS_HouseBill1", transaction4.Reference3);
			AssertEquals("[T4] TransactionReference04", "JK_MasterBillNum1", transaction4.Reference4);
			AssertEquals("[T4] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction4.Reference5);
			AssertEquals("[T4] BranchCode", "CBR", transaction4.GetBranchCode());
			AssertEquals("[T4] UserCode", "DAT", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
		}
	}
}
