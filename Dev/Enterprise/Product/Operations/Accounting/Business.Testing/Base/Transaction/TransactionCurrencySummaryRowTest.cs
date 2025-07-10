using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionCurrencySummaryRow))]
	public class TransactionCurrencySummaryRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertEquals(200m, transactionCurrencySummaryRow1.Amount);
			AssertEquals(300m, transactionCurrencySummaryRow2.Amount);

			AssertEquals(723m, transactionCurrencySummaryRow1.LocalAmount);
			AssertEquals(791m, transactionCurrencySummaryRow2.LocalAmount);

			AssertEquals(250.0m, transactionCurrencySummaryRow1.OutStandingOSAmount);
			AssertEquals(300m, transactionCurrencySummaryRow2.OutStandingOSAmount);

			AssertEquals(823m, transactionCurrencySummaryRow1.OutStandingLocalAmount);
			AssertEquals(791m, transactionCurrencySummaryRow2.OutStandingLocalAmount);

			AssertEquals(2, transactionCurrencySummaryRow1.TransactionCount);
			AssertEquals(1, transactionCurrencySummaryRow2.TransactionCount);

			AssertEquals(0.303767m, transactionCurrencySummaryRow1.AverageExRate);
			AssertEquals(0.379267m, transactionCurrencySummaryRow2.AverageExRate);

			AssertEquals("AUD", transactionCurrencySummaryRow1.Currency);
			AssertEquals("USD", transactionCurrencySummaryRow2.Currency);

			AssertEquals(2, transactionCurrencySummaryRow1.CurrencyDecimals);
			AssertEquals(2, transactionCurrencySummaryRow2.CurrencyDecimals);

			AssertEquals(GlbCompany.CurrentCompany.GetLocalDecimals(), transactionCurrencySummaryRow1.LocalDecimals);
			AssertEquals(GlbCompany.CurrentCompany.GetLocalDecimals(), transactionCurrencySummaryRow2.LocalDecimals);

			AssertEquals(2, transactionCurrencySummaryRow1.OSDecimals);
			AssertEquals(2, transactionCurrencySummaryRow2.OSDecimals);

			AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, transactionCurrencySummaryRow1.ExchangeRateDecimals);
			AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, transactionCurrencySummaryRow2.ExchangeRateDecimals);
		}

		#region Implementation

		protected TransactionCurrencySummaryRow SummaryRow
		{
			get { return (TransactionCurrencySummaryRow)CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionCurrencySummaryRow(Factory);
		}

		#endregion

		TransactionCurrencySummaryRow transactionCurrencySummaryRow1;
		TransactionCurrencySummaryRow transactionCurrencySummaryRow2;

		protected override void SetUp()
		{
			base.SetUp();

			var creator = new TestObjectCreator(Factory);
			ARInvoice invoice1 = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1, creator.ABIGAS);
			creator.CreateInvoiceLine(invoice1, 100m, setTaxes: false);

			invoice1.AH_OSTotalAmount = 100m;
			invoice1.AH_LocalTaxAmount = 100m;
			invoice1.AH_LocalOutstandingAmount = 300m;
			invoice1.AH_OSTotal = 100m;
			invoice1.AH_DueDate = new ZDate(2019, 1, 1);

			ARInvoice invoice2 = creator.CreateARInvoice<ARInvoice>("002", creator.AUD, 1, creator.ABIGAS);
			creator.CreateInvoiceLine(invoice2, 200m, setTaxes: false);
			invoice2.AH_OSTotalAmount = 100m;
			invoice2.AH_LocalOutstandingAmount = 400m;
			invoice2.AH_LocalTaxAmount = 300m;
			invoice2.AH_LocalTaxAmountOtherTaxes = 23m;
			invoice2.AH_OSTotal = 100m;
			invoice2.AH_DueDate = new ZDate(2019, 2, 1);

			ARInvoice invoice3 = creator.CreateARInvoice<ARInvoice>("003", creator.USD, 2, creator.ABIGAS);
			creator.CreateInvoiceLine(invoice3, 300m, setTaxes: false);
			invoice3.AH_OSTotalAmount = 300m;
			invoice3.AH_LocalOutstandingAmount = 800m;
			invoice3.AH_LocalTaxAmount = 600m;
			invoice3.AH_LocalTaxAmountOtherTaxes = 41m;
			invoice3.AH_OSTotal = 300m;
			invoice3.AH_DueDate = new ZDate(2019, 3, 1);

			var transactionCurrencySummary = new TransactionCurrencySummary(new AccTransactionHeader[] { invoice1, invoice2, invoice3 }, Factory);

			transactionCurrencySummaryRow1 = transactionCurrencySummary.TransactionCurrencySummaryRows[0];
			transactionCurrencySummaryRow2 = transactionCurrencySummary.TransactionCurrencySummaryRows[1];
		}
	}
}
