using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(PrintQueueScript))]
	sealed class PrintQueueScriptTest : RefStlScriptWithDefaultsTest
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
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Disp1");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", null, transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", "00000002-0000-0000-0000-000000000000", transaction1.Reference5);
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "Disp3");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", RangeMonthAsDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", null, transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", "00000004-0000-0000-0000-000000000000", transaction2.Reference5);
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "Queue2");
			AssertEquals("[T3] CompanyCode", null, transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionDateUtc", RangeMonthAsDate, transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionReference02", null, transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", null, transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", null, transaction3.Reference4);
			AssertEquals("[T3] TransactionGuidReference", "00000003-0000-0000-0000-000000000000", transaction3.Reference5);
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x1, 'Server1')
INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (0x2, 'Server2')
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_AllowPrinting) VALUES (0x1, 0x1, '', 1)
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting, SQ_SystemCreateTimeUtc) VALUES (0x2, 0x1, 'Disp1', 'Queue1', 1, '2021-10-15')
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_AllowPrinting) VALUES (0x3, 0x2, 'Queue2', 1)
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_AllowPrinting) VALUES (0x4, 0x2, 'Disp3', 1)
INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting, SQ_SystemCreateTimeUtc) VALUES (0x5, 0x1, 'Disp1', 'Queue4', 1, '2021-10-20')
INSERT INTO dbo.StmPrintQueue(SQ_PK, SQ_SPS_Server, SQ_DisplayName, SQ_QueueName, SQ_AllowPrinting) VALUES(0x6, 0x2, 'Disp4', 'Queue5', 0)";
			TestConnection.Command(sqlText).ExecuteNonQuery();
		}
	}
}
