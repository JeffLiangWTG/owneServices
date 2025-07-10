using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs.Testing
{
	[TestedType(typeof(EUICS2ForwarderEntrySummaryDeclaration))]
	sealed class EUICS2ForwarderEntrySummaryDeclarationTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @GcPkDE UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkFR UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkDE UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkFR UNIQUEIDENTIFIER = newid();

				DECLARE @AmaPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk5 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk6 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk7 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk8 UNIQUEIDENTIFIER = newid();
				DECLARE @AmaPk9 UNIQUEIDENTIFIER = newid();

				DECLARE @GdPK01 UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkDE, 'DEC', 'DE company', 'DE'),
					(@GcPkFR, 'FRC', 'FR company', 'FR');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkDE, 'DEB', @GcPkDE),
					(@GbPkFR, 'FRB', @GcPkFR);

				INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
				(@AmaPk1, 'C0000ABC1', 'ENS', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1, 'DE', @GbPkDE),
				(@AmaPk2, 'C0000ABC2', 'ENS', 'VOC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 2, 'DE', @GbPkDE),
				(@AmaPk3, 'C0000ABC3', 'MAN', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3, 'DE', @GbPkDE),
				(@AmaPk4, 'C0000ABC4', 'IAM', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4, 'DE', @GbPkDE),
				(@AmaPk5, 'C0000ABC5', 'ENS', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5, 'FR', @GbPkFR),
				(@AmaPk6, 'C0000ABC6', 'ENS', 'VOC', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 6, 'DE', @GbPkDE),
				(@AmaPk7, 'C0000ABC7', 'ENS', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 7, 'DE', @GbPkDE),
				(@AmaPk8, 'C0000ABC8', 'ENS', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 8, 'DE', @GbPkDE),
				(@AmaPk9, 'C0000ABC9', 'ENS', 'NVC', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 9, 'DE', @GbPkDE);

				INSERT dbo.AsycudaBill (ABL_PK, ABL_AMA, ABL_BolType, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemLastEditTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditUser, ABL_ClusterKey) VALUES
				(0x1, @AmaPk1, 'CLD', 'BN001', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1),
				(0x2, @AmaPk1, 'STD', 'BN002', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1),
				(0x3, @AmaPk1, 'BOL', 'MAWB1', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 1),
				(0x4, @AmaPk2, 'STD', 'BN003', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 2),
				(0x5, @AmaPk3, 'CLD', 'BN004', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3),
				(0x6, @AmaPk3, 'STD', 'BN005', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3),
				(0x7, @AmaPk3, 'BOL', 'MAWB2', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 3),
				(0x8, @AmaPk4, 'STD', 'BN006', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4),
				(0x9, @AmaPk4, 'CLD', 'BN007', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4),
				(0x10, @AmaPk4, 'BOL', 'MAWB3', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 4),
				(0x11, @AmaPk5, 'STD', 'BN008', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5),
				(0x12, @AmaPk5, 'CLD', 'BN009', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5),
				(0x13, @AmaPk5, 'BOL', 'MAWB4', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 5),
				(0x14, @AmaPk6, 'CLD', 'BN010', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 6),
				(0x15, @AmaPk6, 'STD', 'BN011', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 6),
				(0x16, @AmaPk6, 'BOL', 'MAWB5', '2022-09-01', '2022-09-01', 'BOB', 'JOE', 6),
				(0x101, @AmaPk7, 'STD', 'BN012', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 7),
				(0x102, @AmaPk7, 'BOL', 'MAWB6', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 7),
				(0x103, @AmaPk8, 'STD', 'BN013', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 8),
				(0x104, @AmaPk8, 'BOL', 'MAWB7', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 8),
				(0x105, @AmaPk8, 'STD', 'BN014', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 9),
				(0x106, @AmaPk8, 'BOL', 'MAWB8', '2022-08-01', '2022-08-01', 'BOB', 'JOE', 9);

				INSERT dbo.CusEntryNum (CE_PK, CE_ParentId, CE_ParentTable, CE_EntryType, CE_SystemCreateTimeUtc, CE_SystemLastEditTimeUtc, CE_SystemCreateUser, CE_SystemLastEditUser) VALUES
				(newid(), @AmaPk1, 'AsycudaManifestHeader', 'ASY', '2022-08-01', '2022-08-09', 'BOB', 'JOE'),
				(newid(), @AmaPk2, 'AsycudaManifestHeader', 'ASY', '2022-08-02', '2022-08-10', 'BOB', 'JOE'),
				(newid(), @AmaPk3, 'AsycudaManifestHeader', 'ASY', '2022-08-03', '2022-08-11', 'BOB', 'JOE'),
				(newid(), @AmaPk4, 'AsycudaManifestHeader', 'ASY', '2022-08-04', '2022-08-12', 'BOB', 'JOE'),
				(newid(), @AmaPk5, 'AsycudaManifestHeader', 'ASY', '2022-08-05', '2022-08-13', 'BOB', 'JOE'),
				(newid(), @AmaPk6, 'AsycudaManifestHeader', 'ASY', '2022-08-07', '2022-08-15', 'BOB', 'JOE'),
				(newid(), @AmaPk7, 'AsycudaManifestHeader', 'LRN', '2022-08-08', '2022-08-16', 'BOB', 'JOE'),
				(newid(), @AmaPk8, 'AsycudaManifestHeader', 'ASY', '2022-07-01', '2022-08-17', 'BOB', 'JOE'),
				(newid(), @AmaPk9, 'AsycudaManifestHeader', 'ASY', '2022-09-01', '2022-08-17', 'BOB', 'JOE');
				";

			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("All elements", new[] { "Bill: BN001", "Bill: BN002", "Bill: BN008", "Bill: BN009" }, transactions.Select(x => x.Reference2));

				var transaction1 = transactions.Single(x => x.Reference2 == "Bill: BN001");
				AssertEquals("CompanyCode", "DEC", transaction1.GetCompanyCode());
				AssertEquals("BranchCode", "DEB", transaction1.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 1), transaction1.ServiceOccuredUTC);
				AssertEquals("TransactionReference01", "Manifest: C0000ABC1", transaction1.Reference1);
				AssertEquals("TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);

				var transaction2 = transactions.Single(x => x.Reference2 == "Bill: BN002");
				AssertEquals("CompanyCode", "DEC", transaction2.GetCompanyCode());
				AssertEquals("BranchCode", "DEB", transaction2.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 1), transaction2.ServiceOccuredUTC);
				AssertEquals("TransactionReference01", "Manifest: C0000ABC1", transaction2.Reference1);
				AssertEquals("TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction2.Reference5);

				var transaction3 = transactions.Single(x => x.Reference2 == "Bill: BN008");
				AssertEquals("CompanyCode", "FRC", transaction3.GetCompanyCode());
				AssertEquals("BranchCode", "FRB", transaction3.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 5), transaction3.ServiceOccuredUTC);
				AssertEquals("TransactionReference01", "Manifest: C0000ABC5", transaction3.Reference1);
				AssertEquals("TransactionGuidReference", "00000011-0000-0000-0000-000000000000", transaction3.Reference5);

				var transaction4 = transactions.Single(x => x.Reference2 == "Bill: BN009");
				AssertEquals("CompanyCode", "FRC", transaction4.GetCompanyCode());
				AssertEquals("BranchCode", "FRB", transaction4.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2022, 8, 5), transaction4.ServiceOccuredUTC);
				AssertEquals("TransactionReference01", "Manifest: C0000ABC5", transaction4.Reference1);
				AssertEquals("TransactionGuidReference", "00000012-0000-0000-0000-000000000000", transaction4.Reference5);
			});
		}
	}
}
