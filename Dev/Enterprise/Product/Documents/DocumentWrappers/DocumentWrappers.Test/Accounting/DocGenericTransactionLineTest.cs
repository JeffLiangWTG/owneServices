using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocGenericTransactionLineTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(Factory.NewWithValidTestData<DirectPaymentLine>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(Factory.NewWithValidTestData<AccCashAdvanceRequestLine>(), Factory).GetType());
			var line1 = Enterprise.DocumentWrappers.DocAPPayment.FlattenedLine.New(DocTransactionHeader.New(Factory.New<APInvoice>(), Factory), Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line1, Factory).GetType());
			var line2 = Enterprise.DocumentWrappers.DocPaymentApproval.FlattenedLine.New(DocPaymentApprovalItem.New(Factory.New<PaymentApprovalItem>(), Factory), Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line2, Factory).GetType());
			var line3 = Enterprise.DocumentWrappers.DocTransactionHeader.FlattenedLine.New(DocTransactionHeader.New(Factory.New<APInvoice>(), Factory), Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line3, Factory).GetType());
			var line4 = new DocStatementSummaryLine(DocStatement.New(new PrintStatement(Factory, GlbBranch.CurrentBranch), Factory), DocTransactionHeader.New(Factory.NewWithValidTestData<ARInvoice>(), Factory));
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line4, Factory).GetType());
			var line5 = DocAccountingJournalLine.New(new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory()).Lines.First(), Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line5, Factory).GetType());
			var line6 = DocARInvoiceLine.New(Factory.NewWithValidTestData<ARInvoiceLine>(), Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line6, Factory).GetType());
			var line7 = DocARInvoiceLineForRollUp.New(Factory);
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionLine), DocGenericTransactionLine.New(line7, Factory).GetType());
		}

		public void TestCashAdvanceRequestRelatedProperties()
		{
			var cashAdvanceRequestLine = Factory.New<AccCashAdvanceRequestLine>();
			cashAdvanceRequestLine.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			cashAdvanceRequestLine.CAL_OSAmount = 100m;
			cashAdvanceRequestLine.CAL_LocalAmount = 75m;
			cashAdvanceRequestLine.CAL_OSPaidAmount = 60m;
			cashAdvanceRequestLine.CAL_LocalPaidAmount = 45m;
			var docLine = DocGenericTransactionLine.New(cashAdvanceRequestLine, Factory);
			AssertEquals("PAI", docLine.Status);
			AssertEquals("OSAmount", 100m, docLine.OsAmount);
			AssertEquals("OSPaidAmount", 60m, docLine.OsPaidAmount);
			AssertEquals("LocalAmount", 75m, docLine.LocalTotalAmount);
			AssertEquals("LocalPaidAmount", 45m, docLine.LocalPaidAmount);
		}

		public void TestAlternateGLAccountAndParentAccountNum()
		{
			var reportingBook = CreateReportingBook();
			var table = CreateGeneralLedgerTransactionData();
			var row = table.NewRow();
			row["AlternateGLAccount"] = "101";
			row["GLAccountNum"] = "1010.11.11";
			var accountingJOurnal = new AccountingJournalLineForReportingBook(row, reportingBook, Factory.New<AccTransactionLines>());
			var docAccountingJOurnal = DocAccountingJournalLine.New(accountingJOurnal, Factory);
			var docGenericTransactionLine = DocGenericTransactionLine.New(docAccountingJOurnal, Factory);
			AssertEquals("101", docGenericTransactionLine.AlternateGLAccount);
			AssertEquals("1010.11.11", docGenericTransactionLine.ParentAccountNum);
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_Code = "TRR";
			reportingBook.ARB_Description = "Description";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			Factory.Save();
			return reportingBook;
		}

		DataTable CreateGeneralLedgerTransactionData()
		{
			var dataTable = new DataTable("GeneralLedgerTransactionData");
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add("AlternateGLAccountDescription", typeof(string));
			dataTable.Columns.Add("AlternateGLAccount", typeof(string));
			dataTable.Columns.Add("GLAccountNum", typeof(string));
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("PostDate", typeof(ZDateTime));
			dataTable.Columns.Add("PostPeriod", typeof(ZInt));
			dataTable.Columns.Add("GLAmountLocalDebit", typeof(ZDecimal));
			dataTable.Columns.Add("GLAmountLocalCredit", typeof(ZDecimal));
			dataTable.Columns.Add("GLAmountOSDebit", typeof(ZDecimal));
			dataTable.Columns.Add("GLAmountOSCredit", typeof(ZDecimal));
			dataTable.Columns.Add("Currency", typeof(ZString));
			return dataTable;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			DirectPaymentLine transactionLine = Factory.New<DirectPaymentLine>();
			return DocGenericTransactionLine.New(transactionLine, Factory);
		}
	}
}
