using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureORNExportOutwardReportsCount))]
	sealed class CaptureORNExportOutwardReportsCountTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 10);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => CombineAssertions(() =>
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "C000TEST1");
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2022, 10, 13, 12, 0, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "ABC", transaction1.ClientStaffCode);
			AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] TransactionReference02", "00001", transaction1.Reference2);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] CompanyCode", "DNZ", transaction1.GetCompanyCode());
			AssertEquals("[T1] Branch", "AKL", transaction1.GetBranchCode());

			var transaction2 = FindRowByRef1(transactions, "C000TEST2");
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2022, 10, 13, 12, 0, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "ABC", transaction2.ClientStaffCode);
			AssertEquals("[T2] TransactionGuidReference", "00000004-0000-0000-0000-000000000000", transaction2.Reference5);
			AssertEquals("[T2] TransactionReference02", "00002", transaction2.Reference2);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] CompanyCode", "DNZ", transaction2.GetCompanyCode());
			AssertEquals("[T2] Branch", "CHC", transaction2.GetBranchCode());
		});

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @GePk UNIQUEIDENTIFIER = newid();

				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_MasterBillNum, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser) VALUES
					(@JkPk1, 'C000TEST1', '00001', '2022-10-10 12:00:00', 'ABC', '2022-10-10 12:00:00', 'ABC'),
					(@JkPk2, 'C000TEST2', '00002', '2022-10-10 13:00:00', 'ABC', '2022-10-10 13:00:00', 'ABC');

				INSERT dbo.CusEntryNum (CE_PK, CE_RN_NKCountryCode, CE_ParentTable, CE_ParentID, CE_EntryType, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(0x1, 'NZ', 'JobConsol', @JkPk1, 'ORN', '00000001', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC'),
					(0x2, 'AU', 'JobConsol', @JkPk1, 'ORN', '00000002', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC'),
					(0x3, 'NZ', 'JobDeclaration', @JkPk1, 'ORN', '00000003', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC'),
					(0x4, 'NZ', 'JobConsol', @JkPk2, 'ORN', '00000004', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC'),
					(0x5, 'NZ', 'JobConsol', @JkPk1, 'OCR', '00000005', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC'),
					(0x6, 'NZ', 'JobConsol', @JkPk1, 'OCR', '00000000', '2022-10-13 12:00:00', 'ABC', '2022-10-13 12:00:00', 'ABC');

				INSERT GlbCompany(GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES(@GcPk, 'DNZ', 'NZ company', 'NZD', 'NZ');
				INSERT GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@GbPk1, 'AKL', @GcPk);
				INSERT GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@GbPk2, 'CHC', @GcPk);
				INSERT GlbDepartment (GE_PK) VALUES ( @GePk );
				INSERT EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPk1, @GePk, 'NZC', @JkPk1, 'JobConsol', '2014-09-24', 'TRX', 'OCR', 'ORG', 'SNT', 'US1', '2014-09-24', 'US1');
				INSERT EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPk2, @GePk, 'NZC', @JkPk2, 'JobConsol', '2014-09-24', 'TRX', 'OCR', 'ORG', 'SNT', 'US1', '2014-09-24', 'US1');
				INSERT EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPk2, @GePk, 'NZC', @JkPk2, 'JobConsol', '2014-09-24', 'TRX', 'OCR', 'REP', 'SNT', 'US1', '2014-09-24', 'US1');
				INSERT EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPk2, @GePk, 'NZC', @JkPk2, 'JobConsol', '2014-09-24', 'TRX', 'ICR', 'ORG', 'SNT', 'US1', '2014-09-24', 'US1');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
