using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(PrintServerScript))]
	sealed class PrintServerScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				return testDateRange ?? (testDateRange = AusydMonthRange.New(twoMonthsAgo.Year, twoMonthsAgo.Month));
			}
		}
		IDateTimeRange testDateRange;

		DateTime RangeMonthAsDate
		{
			get
			{
				if (rangeMonthAsDate == null)
				{
					rangeMonthAsDate = new DateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 1);
				}
				return rangeMonthAsDate.Value;
			}
		}
		DateTime? rangeMonthAsDate;

		readonly DateTime twoMonthsAgo = DateTime.UtcNow.Date.AddMonths(-2);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Server1");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", null, transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000001-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "Server2");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", RangeMonthAsDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", null, transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction2.Reference5);
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x1, 'Server1')
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x2, 'Server2')
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x3, 'Server3')
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x4, 'Server4')
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting) VALUES (0x1, 0x1, 'Disp1', 'Queue1', 1)
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting) VALUES (0x2, 0x2, 'Disp2', 'Queue2', 1)
INSERT INTO dbo.StmPrintQueue(SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting) VALUES(0x3, 0x3, 'Disp3', 'Queue3', 0)";
			TestConnection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
