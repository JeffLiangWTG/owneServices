using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesOrgLinkCompetitor))]
	sealed class SalesOrgLinkCompetitorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Forwarding");
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 12, 2, 32, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] AdditionalRefs", @"{""MainOrgCode"":""TESTORG1""}", transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef1(transactions, "Warehouse");
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 21, 4, 40, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] AdditionalRefs", @"{""MainOrgCode"":""TESTORG3""}", transaction2.AdditionalRefs);

			var transaction3 = FindRowByRef1(transactions, "Customs");
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 9, 22, 5, 50, 0), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US5", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] AdditionalRefs", @"{""MainOrgCode"":""TESTORG3""}", transaction3.AdditionalRefs);

			var transaction4 = FindRowByRef1(transactions, "Land Transport");
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2014, 9, 30, 2, 20, 0), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "US7", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T4] AdditionalRefs", @"{""MainOrgCode"":""TESTORG1""}", transaction4.AdditionalRefs);
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"
				DECLARE @Org1Pk UNIQUEIDENTIFIER = NEWID();
				DECLARE @Org2Pk UNIQUEIDENTIFIER = NEWID();
				DECLARE @Org3Pk UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsActive) VALUES
				(@Org1Pk, 'TESTORG1', 1),
				(@Org2Pk, 'TESTORG2', 0),
				(@Org3Pk, 'TESTORG3', 1)

				INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_OH_Parent, PR_OH_RelatedParty, PR_SystemCreateTimeUtc, PR_SystemCreateUser) VALUES
				(NEWID(), 'CMB', @Org1Pk, @Org2Pk, '2014-08-31 01:21:00', 'US1'),
				(NEWID(), 'CMF', @Org1Pk, @Org2Pk, '2014-09-12 02:32:00', 'US2'),
				(NEWID(), 'CMD', @Org2Pk, @Org1Pk, '2014-09-20 03:43:00', 'US3'),
				(NEWID(), 'CMW', @Org3Pk, @Org2Pk, '2014-09-21 04:40:00', 'US4'),
				(NEWID(), 'CMB', @Org3Pk, @Org1Pk, '2014-09-22 05:50:00', 'US5'),
				(NEWID(), 'CAG', @Org1Pk, @Org3Pk, '2014-09-23 03:30:00', 'US6'),
				(NEWID(), 'CMD', @Org1Pk, @Org3Pk, '2014-09-30 02:20:00', 'US7')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
