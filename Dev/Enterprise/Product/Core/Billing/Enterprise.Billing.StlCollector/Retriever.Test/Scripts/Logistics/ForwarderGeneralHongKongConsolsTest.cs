using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralHongKongConsols))]
	sealed class ForwarderGeneralHongKongConsolsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES
					(@JkPk01, 'JS01'),
					(@JkPk02, 'JS02');
				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryType, CE_RN_NKCountryCode, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @JkPk01, 'JobShipment', 'ZZZ', 'HK', 'CE01', '2014-04-01', 'US1', '2014-04-01', 'US1'),
					(newid(), @JkPk01, 'JobShipment', 'INS', 'HK', 'CE02', '2014-04-02', 'US2', '2014-04-02', 'US2'),
					(newid(), @JkPk01, 'JobShipment', 'INS', 'HK', 'CE03', '2014-04-03', 'US3', '2014-04-03', 'US3'),
					(newid(), @JkPk02, 'JobShipment', 'INS', 'AU', 'CE04', '2014-04-04', 'US4', '2014-04-04', 'US4'),
					(newid(), @JkPk02, 'JobShipment', 'INS', 'HK', 'CE05', '2014-04-05', 'US5', '2014-04-05', 'US5');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "CE02");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 4, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "JS01", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "CE03");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 4, 3), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "JS01", transaction2.Reference1);

			var transaction3 = FindRowByRef2(transactions, "CE05");
			AssertEquals("[T3] CompanyCode", null, transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 4, 5), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US5", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "JS02", transaction3.Reference1);
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
