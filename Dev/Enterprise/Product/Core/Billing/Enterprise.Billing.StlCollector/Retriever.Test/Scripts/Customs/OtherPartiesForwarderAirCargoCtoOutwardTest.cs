using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoCtoOutward))]
	sealed class OtherPartiesForwarderAirCargoCtoOutwardTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				INSERT dbo.CusEntryNum (CE_PK, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_ParentID, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), '00000000', 'NZ', 'ORN', 'CUS', newid(), 'JobShipment', '2014-02-01', 'US0', getutcdate(), '~BP'),
					(newid(), 'CPG01'   , 'AU', 'ORN', 'CUS', newid(), 'JobShipment', '2014-02-01', 'US1', getutcdate(), '~BP'),
					(newid(), 'CPG02'   , 'NZ', 'ORN', 'CUS', newid(), 'JobShipment', '2014-02-02', 'US2', getutcdate(), '~BP'),
					(newid(), 'CPG03'   , 'NZ', 'ORN', 'CUS', newid(), 'JobShipment', '2014-01-03', 'US3', getutcdate(), '~BP'),
					(newid(), 'CPG04'   , 'NZ', 'XXX', 'CUS', newid(), 'JobShipment', '2014-02-04', 'US4', getutcdate(), '~BP'),
					(newid(), 'CPG05'   , 'NZ', 'ORN', 'CUS', newid(), 'JobShipment', '2014-03-01', 'US5', getutcdate(), '~BP'),
					(newid(), ''        , 'NZ', 'ORN', 'CUS', newid(), 'JobShipment', '2014-02-06', 'US6', getutcdate(), '~BP');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			CombineAssertions("First Transaction ", () =>
			{
				var transaction1 = FindRowByRef1(transactions, "Entry No: CPG02");
				AssertEquals("CompanyCode", null, transaction1.GetCompanyCode());
				AssertEquals("BranchCode", null, transaction1.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2014, 2, 2), transaction1.ServiceOccuredUTC);
				AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
				AssertEquals("ItemCount", 1, transaction1.BillableCount);
				AssertEquals("TransactionReference02", null, transaction1.Reference2);
			});

			CombineAssertions("Second Transaction ", () =>
			{
				var transaction2 = FindRowByRef1(transactions, "Entry No:");
				AssertEquals("CompanyCode", null, transaction2.GetCompanyCode());
				AssertEquals("BranchCode", null, transaction2.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2014, 2, 6), transaction2.ServiceOccuredUTC);
				AssertEquals("UserCode", "US6", transaction2.ClientStaffCode);
				AssertEquals("ItemCount", 1, transaction2.BillableCount);
				AssertEquals("TransactionReference02", null, transaction2.Reference2);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 2);
			}
		}
	}
}
