using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralContainerTeu))]
	sealed class ForwarderGeneralContainerTeuTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk03 UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding) VALUES
					(@JkPk01, 'CON01', 1),
					(@JkPk02, 'CON02', 1),
					(@JkPk03, 'CON03', 0),
					(newid(), 'CON04', 1);
				INSERT dbo.RefContainer (RC_PK, RC_TEU, RC_Code) VALUES
					(@RcPk01,  3, '~RC@#$001~'),
					(@RcPk02,  5, '~RC@#$002~'),
					(@RcPk03, 10, '~RC@#$003~');
				INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RC, JC_ContainerNum, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
					(newid(), @JkPk01, @RcPk01, 'JC11', '2014-04-01', 'US1'),
					(newid(), @JkPk01, @RcPk03, 'JC13', '2014-03-01', 'US2'),
					(newid(), @JkPk02, @RcPk02, 'JC22', '2014-03-02', 'US3'),
					(newid(), @JkPk02, @RcPk03, 'JC23', '2014-05-02', 'US4'),
					(newid(), @JkPk03, @RcPk02, 'JC32', '2014-03-03', 'US5');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "CON01");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 3, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 10, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "JC13", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "CON02");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 3, 2), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 5, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "JC22", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 3);
			}
		}
	}
}
