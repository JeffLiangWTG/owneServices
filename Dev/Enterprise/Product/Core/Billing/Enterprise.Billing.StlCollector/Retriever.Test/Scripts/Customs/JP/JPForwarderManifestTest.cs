using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(JPForwarderManifest))]
	sealed class JPForwarderManifestTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 7);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef1AndRef2(transactions, "Job Reference:MAN00000001", "House Bill:ABL1234568");
			CombineAssertions(() =>
			{
				AssertEquals("[T1] CompanyCode", "~C1", transaction1.GetCompanyCode());
				AssertNull("[T1] TransactionReference03", transaction1.Reference3);
				AssertNull("[T1] TransactionReference04", transaction1.Reference4);
				AssertEquals("[T1] TransactionGuidReference", "00000001-2000-0000-0000-000000000000", transaction1.Reference5);
				AssertEquals("[T1] BranchCode", "~B1", transaction1.GetBranchCode());
				AssertEquals("[T1] UserCode", "M5V", transaction1.ClientStaffCode);
				AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			});

			var transaction2 = FindRowByRef1AndRef2(transactions, "Job Reference:MAN00000001", "House Bill:ABL1234569");
			CombineAssertions(() =>
			{
				AssertEquals("[T2] CompanyCode", "~C1", transaction2.GetCompanyCode());
				AssertNull("[T2] TransactionReference03", transaction2.Reference3);
				AssertNull("[T2] TransactionReference04", transaction2.Reference4);
				AssertEquals("[T2] TransactionGuidReference", "00000001-3000-0000-0000-000000000000", transaction2.Reference5);
				AssertEquals("[T2] BranchCode", "~B1", transaction2.GetBranchCode());
				AssertEquals("[T2] UserCode", "M5V", transaction2.ClientStaffCode);
				AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			});

			var transaction3 = FindRowByRef1AndRef2(transactions, "Job Reference:MAN00000002", "House Bill:ABL8901235");
			CombineAssertions(() =>
			{
				AssertEquals("[T3] CompanyCode", "~C1", transaction3.GetCompanyCode());
				AssertNull("[T3] TransactionReference03", transaction3.Reference3);
				AssertNull("[T3] TransactionReference04", transaction3.Reference4);
				AssertEquals("[T3] TransactionGuidReference", "00000002-2000-0000-0000-000000000000", transaction3.Reference5);
				AssertEquals("[T3] BranchCode", "~B1", transaction3.GetBranchCode());
				AssertEquals("[T3] UserCode", "M5V", transaction3.ClientStaffCode);
				AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			});

			var transaction4 = FindRowByRef1AndRef2(transactions, "Job Reference:MAN00000003", "House Bill:ABL9012346");
			CombineAssertions(() =>
			{
				AssertEquals("[T4] CompanyCode", "~C1", transaction4.GetCompanyCode());
				AssertNull("[T4] TransactionReference03", transaction4.Reference3);
				AssertNull("[T4] TransactionReference04", transaction4.Reference4);
				AssertEquals("[T4] TransactionGuidReference", "00000003-2000-0000-0000-000000000000", transaction4.Reference5);
				AssertEquals("[T4] BranchCode", "~B1", transaction4.GetBranchCode());
				AssertEquals("[T4] UserCode", "M5V", transaction4.ClientStaffCode);
				AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
			DECLARE @JPCompanyPk UNIQUEIDENTIFIER = newid();
			DECLARE @JPBranchPk UNIQUEIDENTIFIER = newid();
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@JPCompanyPk, 'JP', '~C1', 'JP company');
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES (@JPBranchPk, @JPCompanyPk, '~B1');
			INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_ApplicationCode, AMA_ManifestType, AMA_ClusterKey, AMA_GB, AMA_IsActive, AMA_JobReference, AMA_RN_NKCountry, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser) VALUES
			('00000001-0000-0000-0000-000000000000', 'NVC', 'ASY', 1, @JPBranchPk, 1, 'MAN00000001', 'JP', '2023-07-01 00:00:00.000', 'E', '2023-07-02 00:00:00.000', 'F'),
			('00000002-0000-0000-0000-000000000000', 'NVC', 'ASY', 2, @JPBranchPk, 1, 'MAN00000002', 'JP', '2023-07-02 00:00:00.000', 'A', '2023-07-03 00:00:00.000', 'G'),
			('00000003-0000-0000-0000-000000000000', 'NVC', 'ASY', 3, @JPBranchPk, 1, 'MAN00000003', 'JP', '2023-07-02 00:00:00.000', 'B', '2023-07-03 00:00:00.000', 'H'),
			('00000004-0000-0000-0000-000000000000', 'BCD', 'ASY', 4, @JPBranchPk, 1, 'MAN00000004', 'JP', '2023-07-02 00:00:00.000', 'C', '2023-07-03 00:00:00.000', 'I'),
			('00000005-0000-0000-0000-000000000000', 'NVC', 'ASY', 5, @JPBranchPk, 1, 'MAN00000005', 'US', '2023-07-02 00:00:00.000', 'D', '2023-07-03 00:00:00.000', 'J');
			

			INSERT INTO dbo.AsycudaBill(ABL_PK, ABL_AMA, ABL_ClusterKey, ABL_BillNumber, ABL_BolType, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser) VALUES
			('00000001-1000-0000-0000-000000000000', '00000001-0000-0000-0000-000000000000', 1, 'ABL1234567', 'BOL', '2023-07-04 00:00:00.000', 'M5V', '2023-07-05 00:00:00.000', 'B'),
			('00000001-2000-0000-0000-000000000000', '00000001-0000-0000-0000-000000000000', 1, 'ABL1234568', '', '2023-07-04 00:00:00.000', 'M5V', '2023-07-05 00:00:00.000', 'B'),
			('00000001-3000-0000-0000-000000000000', '00000001-0000-0000-0000-000000000000', 1, 'ABL1234569', '', '2023-07-04 00:00:00.000', 'M5V', '2023-07-05 00:00:00.000', 'B'),
			('00000002-1000-0000-0000-000000000000', '00000002-0000-0000-0000-000000000000', 2, 'ABL8901234', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000002-2000-0000-0000-000000000000', '00000002-0000-0000-0000-000000000000', 2, 'ABL8901235', '', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000003-1000-0000-0000-000000000000', '00000003-0000-0000-0000-000000000000', 3, 'ABL9012345', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000003-2000-0000-0000-000000000000', '00000003-0000-0000-0000-000000000000', 3, 'ABL9012346', '', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000004-1000-0000-0000-000000000000', '00000004-0000-0000-0000-000000000000', 4, 'ABL0123456', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000004-2000-0000-0000-000000000000', '00000004-0000-0000-0000-000000000000', 4, 'ABL0123457', '', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000005-1000-0000-0000-000000000000', '00000005-0000-0000-0000-000000000000', 5, 'ABL2345678', 'BOL', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C'),
			('00000005-2000-0000-0000-000000000000', '00000005-0000-0000-0000-000000000000', 5, 'ABL2345679', '', '2023-07-06 00:00:00.000', 'M5V', '2023-07-07 00:00:00.000', 'C');";
			Db.Connection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
