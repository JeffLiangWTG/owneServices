using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoCtoOutturnZA))]
	sealed class OtherPartiesForwarderAirCargoCtoOutturnZATest : RefStlScriptWithDefaultsTest
	{
		readonly Guid amaPk01 = Guid.NewGuid();
		readonly Guid amaPk02 = Guid.NewGuid();

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DZA');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);

				DECLARE @amaPk01 UNIQUEIDENTIFIER = '{0}';
				DECLARE @amaPk02 UNIQUEIDENTIFIER = '{1}';

				INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x1, 'ZA', 'WZA', 'ZA company');
				INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x2, 'US', 'WUS', 'US company');
				INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x1, 0x1, 'BZA');
				INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x2, 0x2, 'BUS');

				INSERT dbo.AsycudaManifestHeader(AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_TransportMode, AMA_SystemCreateTimeUtc,AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) 
				VALUES
					(@amaPk01, 'AMA 1', 'TY1', 'OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 1, 'ZA', 0x1),
					(@amaPk02, 'AMA 2', 'TY2', 'OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 2, 'ZA', 0x1),
					(NEWID(), 'AMA 3', 'TY3', 'OUT', 'SEA', '2019-01-01', '2019-01-01','BOB', 'BOB', 3, 'ZA', 0x1),
					(NEWID(), 'AMA 4', 'TY4', 'OUT', 'ROA', '2019-01-01', '2019-01-01','BOB', 'BOB', 4, 'ZA', 0x1),
					(NEWID(), 'AMA 5', 'TY5', 'OUT', '', '2019-01-01', '2019-01-01','BOB', 'BOB', 5, 'ZA', 0x01),

					(NEWID(), 'AMA 6', 'TY1','OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 6, 'US', 0x2),
					(NEWID(), 'AMA 7', 'TY2','OUT', 'AIR', '2019-01-01', '2019-01-01','BOB', 'BOB', 7, 'US', 0x2),
					(NEWID(), 'AMA 8', 'TY3','OUT', 'SEA', '2019-01-01', '2019-01-01','BOB', 'BOB', 8, 'US', 0x2),
					(NEWID(), 'AMA 9', 'TY4','OUT', 'ROA', '2019-01-01', '2019-01-01','BOB', 'BOB', 9, 'US', 0x2),
					(NEWID(), 'AMA 10', 'TY5','OUT', '', '2019-01-01', '2019-01-01','BOB', 'BOB', 10, 'US', 0x2);
				";

			var query = string.Format(sqlText, amaPk01, amaPk02);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "AMA 1");
			AssertEquals("[T1] CompanyCode", "WZA", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2019, 1, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "TY1", transaction1.Reference2);
			AssertEquals("[T1] TransactionGuidReference", amaPk01, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "BZA", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "BOB", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "AMA 2");
			AssertEquals("[T2] CompanyCode", "WZA", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2019, 1, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "TY2", transaction2.Reference2);
			AssertEquals("[T2] TransactionGuidReference", amaPk02, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "BZA", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "BOB", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 01);
	}
}
