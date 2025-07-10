using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_TransactionsBalancesTest : ScriptTest
	{
		#region AH_BalanceInLocal

		#region ByPeriodEnd

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PartlyPaid_ByPeriodEnd()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -30, 20);
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_BalanceInLocal" }, new object[][] { new object[] { -80m }, new object[] { 80m } });
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PaidBefore_ByPeriodEnd()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -30);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PaidAfter_ByPeriodEnd()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(-30);
			Factory.Save();
			PayInvoice(invoice, -10);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201902");

			AssertDataTableAllRows("", resultTable, new[] { "AH_BalanceInLocal" }, new object[][] { new object[] { -100m }, new object[] { 100m } });
		}

		[TestDate(2020, 12, 30)]
		public void TestShouldHitIndexNR_RX__AH_GC_AH_Ledger_AH_FullyPaidDate_AH_PostDate_AH_TransactionType()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31));

			PrepareDate();

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader(GetQuery("202012"), cmd => { });

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("csfn_TransactionsBalances"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				Assert(queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_GC_AH_Ledger_AH_FullyPaidDate_AH_PostDate_AH_TransactionType));
				Assert("Should not contains any TableScan", queryPlanAnalyzer.TableScans.All(x => x.TableName != AccTransactionHeaderSchema.Constants.TableName));
			}

			void PrepareDate()
			{
				TestConnection.ExecuteNonQuery($@"
DECLARE @TotalCount int = 0;
WHILE @TotalCount < 20
BEGIN
-- Should be returned(AH_PostDate <= Period.PeriodEnd)
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV' + CAST(@TotalCount AS varchar(10)), '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should be returned(AH_FullyPaidDate > Period.PeriodEnd)
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_FullyPaidDate, AH_OutstandingAmount, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV3' + CAST(@TotalCount AS varchar(10)), '2021-01-01 15:00:00', 0.000, '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should not be returned(Post Date bigger than '2020-12-30')
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV5' + CAST(@TotalCount AS varchar(10)), '2021-01-01 15:00:00', '2021-01-01 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2021-01-01 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should not be returned(Leger not equal to 'AR' or 'AP')
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'UA', 'UAI', 'UAI' + CAST(@TotalCount AS varchar(10)), '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	SET @TotalCount = @TotalCount + 1
END");
			}
		}

		[TestDate(2020, 02, 10)]
		public void TestTransactionMatchedInFutureDate()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202001, new ZDateTime(2020, 01, 01), new ZDateTime(2020, 01, 31));
			period.SetupSinglePeriod(202002, new ZDateTime(2020, 02, 01), new ZDateTime(2020, 02, 28));
			period.SetupSinglePeriod(202003, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 03, 31));
			period.SetupSinglePeriod(202004, new ZDateTime(2020, 04, 01), new ZDateTime(2020, 04, 30));

			var invHeader = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 6000m, 0m, 6000m, 0m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK
				, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), false);

			var recHeader1 = TestObjectCreator.CreateARReceipt(1m, 1000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var recHeader2 = TestObjectCreator.CreateARReceipt(1m, 2000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var recHeader3 = TestObjectCreator.CreateARReceipt(1m, 3000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var recHeaderFuture = TestObjectCreator.CreateARReceipt(1m, 1234m, new DateTime(2020, 03, 01), new DateTime(2020, 03, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var matchingBase1 = new ARMatchingBase(Factory);
			matchingBase1.MatchDate = new DateTime(2020, 02, 01);
			matchingBase1.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase1.AddIMatching(invHeader);
			matchingBase1.AddIMatching(recHeader1);
			matchingBase1.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader1 });
			((IMatching)invHeader).OSPartialPaymentAmount = 1000;
			((IMatching)recHeader1).OSPartialPaymentAmount = -1000;
			matchingBase1.Match_ForTestOnly();

			var matchingBase2 = new ARMatchingBase(Factory);
			matchingBase2.MatchDate = new DateTime(2020, 03, 01);
			matchingBase2.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase2.AddIMatching(invHeader);
			matchingBase2.AddIMatching(recHeader2);
			matchingBase2.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader2 });
			((IMatching)invHeader).OSPartialPaymentAmount = 2000;
			((IMatching)recHeader2).OSPartialPaymentAmount = -2000;
			matchingBase2.Match_ForTestOnly();

			var matchingBase3 = new ARMatchingBase(Factory);
			matchingBase3.MatchDate = new DateTime(2020, 04, 01);
			matchingBase3.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase3.AddIMatching(invHeader);
			matchingBase3.AddIMatching(recHeader3);
			matchingBase3.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader3 });
			((IMatching)invHeader).OSPartialPaymentAmount = 3000;
			((IMatching)recHeader3).OSPartialPaymentAmount = -3000;
			matchingBase3.Match_ForTestOnly();

			Factory.Save();

			var resultMonth01 = RunScript("202001").Rows.Cast<DataRow>();
			AssertEquals("before matched , showing all transactions", 4, resultMonth01.Count());
			AssertEquals("before matched , showing all transactions", 0m, resultMonth01.Sum(row => (decimal)row["AH_BalanceInLocal"]));
			AssertEquals(true, resultMonth01.Any(row => FindLine(row, invHeader , 6000m)));
			AssertEquals(true, resultMonth01.Any(row => FindLine(row, recHeader1, -1000m)));
			AssertEquals(true, resultMonth01.Any(row => FindLine(row, recHeader2, -2000m)));
			AssertEquals(true, resultMonth01.Any(row => FindLine(row, recHeader3, -3000m)));

			var resultMonth02 = RunScript("202002").Rows.Cast<DataRow>();
			AssertEquals("after matched 1 transaction , showing 3 transactions", 3, resultMonth02.Count());
			AssertEquals("before matched , showing all transactions", 0m, resultMonth02.Sum(row => (decimal)row["AH_BalanceInLocal"]));
			AssertEquals(true, resultMonth02.Any(row => FindLine(row, invHeader, 5000m)));
			AssertEquals(false, resultMonth02.Any(row => FindLine(row, recHeader1, -1000m)));
			AssertEquals(true, resultMonth02.Any(row => FindLine(row, recHeader2, -2000m)));
			AssertEquals(true, resultMonth02.Any(row => FindLine(row, recHeader3, -3000m)));

			var resultMonth03 = RunScript("202003").Rows.Cast<DataRow>();
			AssertEquals("after matched 2 transaction , showing 2 transactions plus the future one", 3, resultMonth03.Count());
			AssertEquals("before matched , showing all transactions", -1234m, resultMonth03.Sum(row => (decimal)row["AH_BalanceInLocal"]));
			AssertEquals(true, resultMonth03.Any(row => FindLine(row, invHeader, 3000m)));
			AssertEquals(false, resultMonth03.Any(row => FindLine(row, recHeader1, -1000m)));
			AssertEquals(false, resultMonth03.Any(row => FindLine(row, recHeader2, -2000m)));
			AssertEquals(true, resultMonth03.Any(row => FindLine(row, recHeader3, -3000m)));
			AssertEquals(true, resultMonth03.Any(row => FindLine(row, recHeaderFuture, -1234m)));

			var resultMonth04 = RunScript("202004").Rows.Cast<DataRow>();
			AssertEquals("after matched all transaction , showing the future one only", 1, resultMonth04.Count());
			AssertEquals(true, resultMonth04.Any(row => FindLine(row, recHeaderFuture, -1234m)));

			bool FindLine(DataRow row, AccTransactionHeader header,decimal expectedOutstanding)
			{
				return (Guid)row["AH_OH"] == header.AH_OH.ToGuid()
				&& (decimal)row["AH_BalanceInLocal"] == expectedOutstanding;
			}
		}

		#endregion

		#region ByCurrentDate

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PartlyPaid_ByCurrentDate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0);
			Factory.Save();
			PayInvoice(invoice, 0, 20);

			var resultTable = RunScript("201903");

			AssertDataTableAllRows("", resultTable, new[] { "AH_BalanceInLocal" }, new object[][] { new object[] { 80m } });
		}

		[TestDate(2019, 3, 14)]
		public void TestAH_BalanceInLocal_PaidBefore_ByCurrentDate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 01, 01));
			var invoice = CreateInvoice(0);
			Factory.Save();
			PayInvoice(invoice, -1);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			var resultTable = RunScript("201903");

			AssertEquals("Rows.Count", 0, resultTable.Rows.Count);
		}

		#endregion

		#endregion

		InvoicingBase CreateInvoice(int daysPosted)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", organisation: TestObjectCreator.Debtor);
			invoice.AH_PostDate = ZDateTime.Today.AddDays(daysPosted);
			TestObjectCreator.CreateInvoiceLine(invoice, 100, setTaxes: false);

			return invoice;
		}

		void PayInvoice(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null)
		{
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid);
			invoice.Factory.Save();
		}

		static DataTable RunScript(string period)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, GetQuery(period));
		}

		static string GetQuery(string period) => Invariant($@"
SELECT *
FROM csfn_TransactionsBalances({period},'{GlbCompany.CurrentCompany.PK}', 'AR')
ORDER BY AH_BalanceInLocal");
	}
}


