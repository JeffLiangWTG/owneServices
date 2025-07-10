using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class APReconciliationCreditorInvoicedExchangeRateProviderTest : TestCaseWithFactory
	{
		public void TestTrySetExchangeRateAndAmounts()
		{
			AssertEquals("Precondition", 2m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);

			var result = calculator.TrySetExchangeRateAndAmounts(reconciliationLine, draftInvoice.Company.GetExchangeRate());

			AssertEquals(true, result);
			AssertEquals(5m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(40m, reconciliationLine.LocalExTaxAmount);
		}

		public void TestTrySetExchangeRateAndAmounts_ReconciliationLineIsInLocalCurrency()
		{
			reconciliationLine.OSCurrency = CurrencyCodes.Australia;
			reconciliationLine.OSExTaxAmount = 100m;
			reconciliationLine.ExchangeRate = 1m;

			AssertEquals("Precondition", 1m, reconciliationLine.ExchangeRate);
			AssertEquals(100m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);

			var result = calculator.TrySetExchangeRateAndAmounts(reconciliationLine, draftInvoice.Company.GetExchangeRate());

			AssertEquals(false, result);
			AssertEquals(1m, reconciliationLine.ExchangeRate);
			AssertEquals(100m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);
		}

		public void TestTrySetExchangeRateAndAmounts_DraftInvoiceIsInForeignCurrency()
		{
			draftInvoice.AIH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;

			AssertEquals("Precondition", 2m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);

			var result = calculator.TrySetExchangeRateAndAmounts(reconciliationLine, draftInvoice.Company.GetExchangeRate());

			AssertEquals(false, result);
			AssertEquals(2m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);
		}

		public void TestTrySetExchangeRateAndAmounts_NoMatchingCreditorInvoicedExchangeRate()
		{
			draftInvoice.AIH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			draftInvoice.ExchangeRates.Single(x => ((AccDraftInvoiceExRate)x).AIE_RX_NKRateCurrency == CurrencyCodes.UnitedStates).Delete();

			AssertEquals("Precondition", 2m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);

			var result = calculator.TrySetExchangeRateAndAmounts(reconciliationLine, draftInvoice.Company.GetExchangeRate());

			AssertEquals(false, result);
			AssertEquals(2m, reconciliationLine.ExchangeRate);
			AssertEquals(200m, reconciliationLine.OSExTaxAmount);
			AssertEquals(100m, reconciliationLine.LocalExTaxAmount);
		}

		protected override void SetUp()
		{
			draftInvoice = TestObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", TestObjectCreator.Creditor1.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);

			Factory.Save();

			reconciliationLine = new APReconciliationLine();
			reconciliationLine.ExchangeRate = 2m;
			reconciliationLine.LocalExTaxAmount = 100m;
			reconciliationLine.OSExTaxAmount = 200m;
			reconciliationLine.LocalCurrency = CurrencyCodes.Australia;
			reconciliationLine.OSCurrency = CurrencyCodes.UnitedStates;
			calculator = new APReconciliationCreditorInvoicedExchangeRateProvider(draftInvoice);

			AssertEquals("Precondition", true, draftInvoice.IsInLocalCurrency);
		}

		AccDraftInvoiceHeader draftInvoice;
		APReconciliationLine reconciliationLine;
		APReconciliationCreditorInvoicedExchangeRateProvider calculator;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
