using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesShippingReportExport))]
	sealed class OtherPartiesShippingReportExportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @EdPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @EdPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @EdPk03 UNIQUEIDENTIFIER = newid();
				INSERT dbo.ExportCustomsManifestHeader (ED_PK, ED_TransportMode, ED_ManifestType, ED_BGMReference, ED_SystemCreateTimeUtc, ED_SystemCreateUser) VALUES
					(@EdPk01, 'SEA', 'EMM', 'FR01', '2013-09-30', 'US4'),
					(@EdPk02, 'AIR', 'DEP', 'FR02', '2013-10-30', 'US5'),
					(@EdPk03, 'SEA', 'ESM', 'FR03', '2013-11-30', 'US3'),
					(newid(), 'SEA', 'SLT', 'FR04', '2013-12-30', 'US7');
				INSERT dbo.ExportCustomsManifestLines (EL_PK, EL_ED) VALUES
					(newid(), @EdPk01),
					(newid(), @EdPk02),
					(newid(), @EdPk03);
				INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser) VALUES
					(newid(), 'ExportCustomsManifestHeader', @EdPk01, 'ADD', '2014-01-01', '2014-01-01', 'US1'),
					(newid(), 'ExportCustomsManifestHeader', @EdPk02, 'ADD', '2013-11-01', '2013-11-01', 'US2'),
					(newid(), 'ExportCustomsManifestHeader', @EdPk03, 'ADD', '2013-11-29', '2013-11-28', 'US6');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 11, 30), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "REF: FR03", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "ESM", transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 11);
			}
		}
	}
}
