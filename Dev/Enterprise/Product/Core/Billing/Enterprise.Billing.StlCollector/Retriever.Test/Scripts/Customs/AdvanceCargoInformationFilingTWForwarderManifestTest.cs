using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(AdvanceCargoInformationFilingTWForwarderManifest))]
	sealed class AdvanceCargoInformationFilingTWForwarderManifestTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 7);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Job Reference:MAN00000001");
			CombineAssertions(() =>
			{
				AssertEquals("[T1] CompanyCode", "~C1", transaction1.GetCompanyCode());
				AssertEquals("[T1] TransactionReference02", "Master Bill:ABL1234567", transaction1.Reference2);
				AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
				AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
				AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);
				AssertEquals("[T1] BranchCode", "~B1", transaction1.GetBranchCode());
				AssertEquals("[T1] UserCode", "E", transaction1.ClientStaffCode);
				AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			});

			var transaction2 = FindRowByRef1(transactions, "Job Reference:MAN00000002");
			CombineAssertions(() =>
			{
				AssertEquals("[T2] CompanyCode", "~C1", transaction2.GetCompanyCode());
				AssertEquals("[T2] TransactionReference02", "Master Bill:ABL8901234", transaction2.Reference2);
				AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
				AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
				AssertEquals("[T2] TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction2.Reference5);
				AssertEquals("[T2] BranchCode", "~B1", transaction2.GetBranchCode());
				AssertEquals("[T2] UserCode", "A", transaction2.ClientStaffCode);
				AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			});

			var transaction3 = FindRowByRef1(transactions, "Job Reference:MAN00000003");
			CombineAssertions(() =>
			{
				AssertEquals("[T3] CompanyCode", "~C1", transaction3.GetCompanyCode());
				AssertEquals("[T3] TransactionReference02", null, transaction3.Reference2);
				AssertEquals("[T3] TransactionReference03", null, transaction3.Reference3);
				AssertEquals("[T3] TransactionReference04", null, transaction3.Reference4);
				AssertEquals("[T3] TransactionGuidReference", "00000003-0000-0000-0000-000000000000", transaction3.Reference5);
				AssertEquals("[T3] BranchCode", "~B1", transaction3.GetBranchCode());
				AssertEquals("[T3] UserCode", "B", transaction3.ClientStaffCode);
				AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
			DECLARE @TWCompanyPk UNIQUEIDENTIFIER = newid();
			DECLARE @TWBranchPk UNIQUEIDENTIFIER = newid();
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@TWCompanyPk, 'TW', '~C1', 'TW company');
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES (@TWBranchPk, @TWCompanyPk, '~B1');
			INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_ApplicationCode, AMA_ManifestType, AMA_ClusterKey, AMA_GB, AMA_IsActive, AMA_JobReference, AMA_RN_NKCountry, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser) VALUES
			('00000001-0000-0000-0000-000000000000', 'NVC', 'MAN', 1, @TWBranchPk, 1, 'MAN00000001', 'TW', '2023-07-01 00:00:00.000', 'E', '2023-07-02 00:00:00.000', 'F'),
			('00000002-0000-0000-0000-000000000000', 'NVC', 'MAN', 2, @TWBranchPk, 1, 'MAN00000002', 'TW', '2023-07-02 00:00:00.000', 'A', '2023-07-03 00:00:00.000', 'G'),
			('00000003-0000-0000-0000-000000000000', 'NVC', 'MAN', 3, @TWBranchPk, 1, 'MAN00000003', 'TW', '2023-07-02 00:00:00.000', 'B', '2023-07-03 00:00:00.000', 'H'),
			('00000004-0000-0000-0000-000000000000', 'BCD', 'MAN', 4, @TWBranchPk, 1, 'MAN00000004', 'TW', '2023-07-02 00:00:00.000', 'C', '2023-07-03 00:00:00.000', 'I'),
			('00000005-0000-0000-0000-000000000000', 'NVC', 'ASY', 5, @TWBranchPk, 1, 'MAN00000005', 'TW', '2023-07-02 00:00:00.000', 'D', '2023-07-03 00:00:00.000', 'J'),
			('00000006-0000-0000-0000-000000000000', 'NVC', 'MAN', 6, @TWBranchPk, 1, 'MAN00000006', 'US', '2023-07-02 00:00:00.000', 'E', '2023-07-03 00:00:00.000', 'K');
			

			INSERT INTO dbo.AsycudaBill(ABL_PK, ABL_AMA, ABL_ClusterKey, ABL_BillNumber, ABL_BolType, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser) VALUES
			(newid(), '00000001-0000-0000-0000-000000000000', 1, 'ABL1234567', 'BOL', '2023-07-04 00:00:00.000', 'M5V', '2023-07-05 00:00:00.000', 'B'),
			(newid(), '00000002-0000-0000-0000-000000000000', 2, 'ABL8901234', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			(newid(), '00000003-0000-0000-0000-000000000000', 3, 'ABL9012345', '', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			(newid(), '00000004-0000-0000-0000-000000000000', 4, 'ABL0123456', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			(newid(), '00000005-0000-0000-0000-000000000000', 5, 'ABL2345678', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			(newid(), '00000006-0000-0000-0000-000000000000', 6, 'ABL3456789', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C')";
			Db.Connection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
