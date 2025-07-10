using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionCurrencySummary))]
	public class TransactionCurrencySummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertEquals(2, transactionCurrencySummary.TransactionCurrencySummaryRows.Count);

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, transactionCurrencySummary.LocalCurrency);

			AssertEquals(4, transactionCurrencySummary.TransactionsCountTotal);

			AssertEquals(new ZDate(2019, 1, 1), transactionCurrencySummary.EarliestDueDate);

			AssertEquals(new ZDate(2019, 3, 1), transactionCurrencySummary.LatestDueDate);

			AssertEquals(950m, transactionCurrencySummary.LocalAmountTotal);

			AssertEquals(950m, transactionCurrencySummary.OutStandingAmountTotal);

			AssertEquals(GlbCompany.CurrentCompany.GetLocalDecimals(), transactionCurrencySummary.LocalDecimals);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionCurrencySummary(transactionHeaders, Factory);
		}

		TransactionCurrencySummary transactionCurrencySummary;
		AccTransactionHeader[] transactionHeaders;
		protected override void SetUp()
		{
			base.SetUp();

			var creator = new TestObjectCreator(Factory);
			var invoice1 = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1, creator.ABIGAS);

			invoice1.AH_LocalTaxAmount = 100m;
			invoice1.AH_LocalOutstandingAmount = 100m;
			invoice1.AH_DueDate = new ZDate(2019, 1, 1);
			creator.CreateInvoiceLine(invoice1, 100m, setTaxes: false);

			var invoice2 = creator.CreateARInvoice<ARInvoice>("002", creator.AUD, 1, creator.ABIGAS);

			invoice2.AH_LocalOutstandingAmount = 200m;
			invoice2.AH_LocalTaxAmount = 200m;
			invoice2.AH_DueDate = new ZDate(2019, 2, 1);
			creator.CreateInvoiceLine(invoice2, 200m, setTaxes: false);

			var invoice3 = creator.CreateARInvoice<ARInvoice>("003", creator.USD, 2, creator.ABIGAS);

			invoice3.AH_LocalOutstandingAmount = 300m;
			invoice3.AH_LocalTaxAmount = 300m;
			invoice3.AH_DueDate = new ZDate(2019, 3, 1);
			creator.CreateInvoiceLine(invoice3, 300m, setTaxes: false);

			var receipt = creator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, creator.AUDBankAccount.PK);
			receipt.AH_OH = creator.AALSHI.PK;

			transactionHeaders = new AccTransactionHeader[] { invoice1, invoice2, invoice3, receipt };

			transactionCurrencySummary = new TransactionCurrencySummary(transactionHeaders, Factory);
		}
	}
}
