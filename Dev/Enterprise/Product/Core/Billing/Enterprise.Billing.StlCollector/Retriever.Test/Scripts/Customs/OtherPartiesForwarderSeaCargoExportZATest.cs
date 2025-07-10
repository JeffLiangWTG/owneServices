using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderSeaCargoExportZA))]
	sealed class OtherPartiesForwarderSeaCargoExportZATest : RefStlScriptWithDefaultsTest
	{
		readonly Guid amaPk01 = Guid.NewGuid();
		readonly Guid amaPk02 = Guid.NewGuid();
		readonly Guid amaPk03 = Guid.NewGuid();

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DZA');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);

				DECLARE @amaPk01 UNIQUEIDENTIFIER = '{0}';
				DECLARE @amaPk02 UNIQUEIDENTIFIER = '{1}';
				DECLARE @amaPk03 UNIQUEIDENTIFIER = '{2}';

				INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x1, 'ZA', 'WZA', 'ZA company');
				INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x2, 'US', 'WUS', 'US company');
				INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x1, 0x1, 'BZA');
				INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x2, 0x2, 'BUS');

				INSERT dbo.AsycudaManifestHeader(AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_TransportMode, AMA_SystemCreateTimeUtc,AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) 
				VALUES
					(NEWID(), 'AMA 1', 'TY1', 'OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 1, 'ZA', 0x1),
					(NEWID(), 'AMA 2', 'TY2', 'OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 2, 'ZA', 0x1),
					(@amaPk01, 'AMA 3', 'TY3', 'OUT', 'SEA', '2019-01-01', '2019-01-01','BOB', 'BOB', 3, 'ZA', 0x1),
					(@amaPk02, 'AMA 4', 'TY4', 'OUT', 'ROA', '2019-01-01', '2019-01-01','BOB', 'BOB', 4, 'ZA', 0x1),
					(@amaPk03, 'AMA 5', 'TY5', 'OUT', '', '2019-01-01', '2019-01-01','BOB', 'BOB', 5, 'ZA', 0x01),

					(NEWID(), 'AMA 6', 'TY1','OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 6, 'US', 0x2),
					(NEWID(), 'AMA 7', 'TY2','OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 7, 'US', 0x2),
					(NEWID(), 'AMA 8', 'TY3','OUT', 'SEA', '2019-01-01', '2019-01-01','BOB', 'BOB', 8, 'US', 0x2),
					(NEWID(), 'AMA 9', 'TY4','OUT', 'ROA', '2019-01-01', '2019-01-01','BOB', 'BOB', 9, 'US', 0x2),
					(NEWID(), 'AMA 10', 'TY5','OUT', '', '2019-01-01', '2019-01-01','BOB', 'BOB', 10, 'US', 0x2);
				";

			var query = string.Format(sqlText, amaPk01, amaPk02, amaPk03);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "AMA 3");
			AssertEquals("[T1] CompanyCode", "WZA", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2019, 1, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "TY3", transaction1.Reference2);
			AssertEquals("[T1] TransactionGuidReference", amaPk01, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "BZA", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "BOB", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "AMA 4");
			AssertEquals("[T2] CompanyCode", "WZA", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2019, 1, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "TY4", transaction2.Reference2);
			AssertEquals("[T2] TransactionGuidReference", amaPk02, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "BZA", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "BOB", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "AMA 5");
			AssertEquals("[T3] CompanyCode", "WZA", transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2019, 1, 1), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionReference02", "TY5", transaction3.Reference2);
			AssertEquals("[T3] TransactionGuidReference", amaPk03, Guid.Parse(transaction3.Reference5));
			AssertEquals("[T3] BranchCode", "BZA", transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "BOB", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 01);
	}
}
