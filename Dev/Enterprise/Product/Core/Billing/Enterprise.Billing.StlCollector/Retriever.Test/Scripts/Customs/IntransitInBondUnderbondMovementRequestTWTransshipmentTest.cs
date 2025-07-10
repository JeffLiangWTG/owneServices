using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(IntransitInBondUnderbondMovementRequestTWTransshipment))]
	sealed class IntransitInBondUnderbondMovementRequestTWTransshipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Job Reference:BH123");
			CombineAssertions(() =>
			{
				AssertEquals("[T1] CompanyCode", "~C1", transaction1.GetCompanyCode());
				AssertEquals("[T1] TransactionReference02", "Entry Number:BWAB1288800087", transaction1.Reference2);
				AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
				AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
				AssertEquals("[T1] TransactionGuidReference", "00000001-0001-0000-0000-000000000000", transaction1.Reference5);
				AssertEquals("[T1] BranchCode", "~B1", transaction1.GetBranchCode());
				AssertEquals("[T1] UserCode", "A", transaction1.ClientStaffCode);
				AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			});

			var transaction2 = FindRowByRef1(transactions, "Job Reference:BH456");
			CombineAssertions(() =>
			{
				AssertEquals("[T2] CompanyCode", "~C1", transaction2.GetCompanyCode());
				AssertEquals("[T2] TransactionReference02", "Entry Number:CXBC2399911198", transaction2.Reference2);
				AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
				AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
				AssertEquals("[T2] TransactionGuidReference", "00000001-0002-0000-0000-000000000000", transaction2.Reference5);
				AssertEquals("[T2] BranchCode", "~B1", transaction2.GetBranchCode());
				AssertEquals("[T2] UserCode", "B", transaction2.ClientStaffCode);
				AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			});

			var transaction3 = FindRowByRef1(transactions, "Job Reference:BH789");
			CombineAssertions(() =>
			{
				AssertEquals("[T3] CompanyCode", "~C1", transaction3.GetCompanyCode());
				AssertEquals("[T3] TransactionReference02", "Entry Number:DYCD3400022209", transaction3.Reference2);
				AssertEquals("[T3] TransactionReference03", null, transaction3.Reference3);
				AssertEquals("[T3] TransactionReference04", null, transaction3.Reference4);
				AssertEquals("[T3] TransactionGuidReference", "00000001-0003-0000-0000-000000000000", transaction3.Reference5);
				AssertEquals("[T3] BranchCode", "~B1", transaction3.GetBranchCode());
				AssertEquals("[T3] UserCode", "C", transaction3.ClientStaffCode);
				AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
			DECLARE @TWCompanyPk UNIQUEIDENTIFIER = newid();
			DECLARE @TWBranchPk UNIQUEIDENTIFIER = newid();
			DECLARE @USCompanyPk UNIQUEIDENTIFIER = newid();
			DECLARE @USBranchPk UNIQUEIDENTIFIER = newid();
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@TWCompanyPk, 'TW', '~C1', 'TW company');
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES (@TWBranchPk, @TWCompanyPk, '~B1');
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@USCompanyPk, 'US', '~C2', 'US company');
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES (@USBranchPk, @USCompanyPk, '~B2');

			INSERT INTO dbo.CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_SystemCreateUser, BH_SystemLastEditUser, BH_ImportTransportMode, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc) VALUES
			('00000001-0001-0000-0000-000000000000', @TWBranchPk, 'BH123', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0002-0000-0000-000000000000', @TWBranchPk, 'BH456', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0003-0000-0000-000000000000', @TWBranchPk, 'BH789', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0004-0000-0000-000000000000', @TWBranchPk, 'BH790', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0005-0000-0000-000000000000', @TWBranchPk, 'BH791', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0006-0000-0000-000000000000', @TWBranchPk, 'BH792', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0007-0000-0000-000000000000', @TWBranchPk, 'BH793', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0008-0000-0000-000000000000', @USBranchPk, 'BH794', '~BP', '~BP', '01', 'TW', '2022-11-09 01:00:00', '2022-11-09 01:00:00'),
			('00000001-0009-0000-0000-000000000000', @TWBranchPk, 'BH795', '~BP', '~BP', '01', 'US', '2022-11-09 01:00:00', '2022-11-09 01:00:00');

			INSERT INTO dbo.CusEntryNum (CE_PK, CE_EntryNum, CE_ParentID, CE_ParentTable, CE_Category, CE_EntryType, CE_EntryStatus, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
			(NEWID(), 'BWAB1288800087', '00000001-0001-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'A', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'CXBC2399911198', '00000001-0002-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'B', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022209', '00000001-0003-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'C', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022210', '00000001-0004-0000-0000-000000000000', 'CusEntryHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'D', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022211', '00000001-0005-0000-0000-000000000000', 'CusInBondHeader', 'OTH', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'E', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022212', '00000001-0006-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'IMP', 'C', 'TW', '2022-11-09 01:00:00', 'F', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022213', '00000001-0007-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'US', '2022-11-09 01:00:00', 'G', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022214', '00000001-0008-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'H', '2022-11-09 01:00:00', '~BP'),
			(NEWID(), 'DYCD3400022215', '00000001-0009-0000-0000-000000000000', 'CusInBondHeader', 'CUS', 'TRS', 'C', 'TW', '2022-11-09 01:00:00', 'I', '2022-11-09 01:00:00', '~BP');";
			Db.Connection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
