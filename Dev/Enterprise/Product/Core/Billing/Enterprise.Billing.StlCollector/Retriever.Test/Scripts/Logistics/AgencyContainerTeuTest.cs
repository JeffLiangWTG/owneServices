using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AgencyContainerTeu))]
	sealed class AgencyContainerTeuTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk03 UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_IsForwardRegistered, JS_IsCFSRegistered) VALUES
					(@JsPk01, 'SHP01', 1, 0, 0),
					(@JsPk02, 'SHP02', 0, 0, 0),
					(@JsPk03, 'SHP03', 1, 0, 1),
					(@JsPk04, 'SHP04', 1, 0, 0),
					(@JsPk05, 'SHP05', 1, 0, 0),
					(newid(), 'SHP06', 1, 0, 0);
				INSERT dbo.RefContainer (RC_PK, RC_TEU, RC_Code) VALUES
					(@RcPk01,  1, '~RC@#$001~'),
					(@RcPk02,  3, '~RC@#$002~'),
					(@RcPk03,  7, '~RC@#$003~');
				INSERT dbo.JobContainer (JC_PK, JC_JS_FCLBookingOnlyLink, JC_RC, JC_ContainerNum, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
					(newid(), @JsPk01, @RcPk01, 'JC11', '2014-04-01', 'US1'),
					(newid(), @JsPk01, @RcPk03, 'JC13', '2014-04-01', 'US2'),
					(newid(), @JsPk02, @RcPk01, 'JC21', '2014-04-02', 'US3'),
					(newid(), @JsPk02, @RcPk02, 'JC22', '2014-04-02', 'US4'),
					(newid(), @JsPk03, @RcPk02, 'JC32', '2014-04-03', 'US5'),
					(newid(), @JsPk04, @RcPk02, 'JC42', '2014-04-04', 'US6'),
					(newid(), @JsPk05, @RcPk01, 'JC51', '2014-05-05', 'US7');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "JC11");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "SHP01", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "JC13");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 4, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 7, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "SHP01", transaction2.Reference1);

			var transaction3 = FindRowByRef2(transactions, "JC42");
			AssertEquals("[T3] CompanyCode", null, transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 4, 4), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US6", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 3, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "SHP04", transaction3.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 4);
			}
		}
	}
}
