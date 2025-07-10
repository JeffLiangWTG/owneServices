using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesOrgCreation))]
	sealed class SalesOrgCreationTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionGuidReference", "00000000-0000-0000-0000-000000201409", transaction1.Reference5);
			AssertEquals("[T1] TransactionReference01", "4", transaction1.Reference1);
			AssertEquals("[T1] AdditionalRefs", @"{""CompetitorCount"":1,""SalesCount"":2,""SalesCompetitorCount"":1}", transaction1.AdditionalRefs);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsSalesLead, OH_IsCompetitor, OH_IsActive, OH_SystemCreateTimeUtc, OH_SystemCreateUser) VALUES
				(NEWID(), 'TESTORG1', 1, 1, 1, '2014-08-31 01:21:00', 'US1'),
				(NEWID(), 'TESTORG2', 1, 1, 1, '2014-09-12 02:32:00', 'US2'),
				(NEWID(), 'TESTORG3', 1, 1, 0, '2014-09-20 03:43:00', 'US3'),
				(NEWID(), 'TESTORG4', 1, 0, 1, '2014-09-21 03:30:00', 'US4'),
				(NEWID(), 'TESTORG5', 1, 0, 1, '2014-09-22 04:40:00', 'US5'),
				(NEWID(), 'TESTORG6', 0, 0, 1, '2014-09-23 05:50:00', 'US6'),
				(NEWID(), 'TESTORG7', 0, 1, 1, '2014-09-30 03:30:00', 'US7')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
