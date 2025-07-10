using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsMalasiaK4K5Manifest))]
	sealed class ExtensionsMalasiaK4K5ManifestTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JsPk UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@JsPk, 'SHP01');
				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @JsPk, 'JobShipment', 'CPG01', 'MY', 'MAN', 'CUS', '2014-01-01', 'US1', '2014-01-01', 'US1'),
					(newid(), @JsPk, 'JobShipment', 'CPG02', 'AU', 'MAN', 'CUS', '2014-09-01', 'US2', '2014-09-01', 'US2'),
					(newid(), @JsPk, 'JobShipment', 'CPG03', 'MY', 'MAN', 'CUS', '2014-09-21', 'US3', '2014-09-21', 'US3'),
					(newid(), @JsPk, 'JobShipment', 'CPG04', 'MY', 'MAN', 'XXX', '2014-09-21', 'US4', '2014-09-21', 'US4'),
					(newid(), @JsPk, 'JobShipment', 'CPG05', 'MY', 'MAN', 'CUS', '2014-10-01', 'US5', '2014-10-01', 'US5');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 21), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "CPG03", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "SHP01", transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 9);
			}
		}
	}
}
