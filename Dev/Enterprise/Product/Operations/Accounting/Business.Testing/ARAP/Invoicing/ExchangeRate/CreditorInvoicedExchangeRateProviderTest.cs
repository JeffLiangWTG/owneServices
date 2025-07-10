using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class CreditorInvoicedExchangeRateProviderTest : TestCaseWithFactory
	{
		public void TestTrySetCreditorInvoicedExchangeRate_Line()
		{
			AssertEquals("Precondition", 2m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(100m, line.AL_LocalExTaxAmount);

			var result = provider.TrySetCreditorInvoicedExchangeRate(line);

			AssertEquals(true, result);
			AssertEquals(5m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(40m, line.AL_LocalExTaxAmount);
		}

		public void TestTrySetCreditorInvoicedExchangeRate_Line_IsInLocalCurrency()
		{
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;

			AssertEquals("Precondition", 1m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(200m, line.AL_LocalExTaxAmount);

			var result = provider.TrySetCreditorInvoicedExchangeRate(line);

			AssertEquals(false, result);
			AssertEquals(1m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(200m, line.AL_LocalExTaxAmount);
		}

		public void TestTrySetCreditorInvoicedExchangeRate_Line_DraftInvoiceIsInForeignCurrency()
		{
			draftInvoice.AIH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;

			AssertEquals("Precondition", 2m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(100m, line.AL_LocalExTaxAmount);

			var result = provider.TrySetCreditorInvoicedExchangeRate(line);

			AssertEquals(false, result);
			AssertEquals(2m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(100m, line.AL_LocalExTaxAmount);
		}

		public void TestTrySetCreditorInvoicedExchangeRate_Line_NoMatchingCreditorInvoicedExchangeRate()
		{
			draftInvoice.AIH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			draftInvoice.ExchangeRates.Single(x => ((AccDraftInvoiceExRate)x).AIE_RX_NKRateCurrency == CurrencyCodes.UnitedStates).Delete();

			AssertEquals("Precondition", 2m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(100m, line.AL_LocalExTaxAmount);

			var result = provider.TrySetCreditorInvoicedExchangeRate(line);

			AssertEquals(false, result);
			AssertEquals(2m, line.AL_ExchangeRate);
			AssertEquals(200m, line.AL_OSExTaxAmount);
			AssertEquals(100m, line.AL_LocalExTaxAmount);
		}

		public void TestIsCreditorInvoicedExchangeRateApplicable()
		{
			var header = Factory.NewWithValidTestData<APInvoice>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(true, CreditorInvoicedExchangeRateProvider.IsCreditorInvoicedExchangeRateApplicable(header));

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals(true, CreditorInvoicedExchangeRateProvider.IsCreditorInvoicedExchangeRateApplicable(header));

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.AdjustmentNote;
			AssertEquals(false, CreditorInvoicedExchangeRateProvider.IsCreditorInvoicedExchangeRateApplicable(header));

			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(false, CreditorInvoicedExchangeRateProvider.IsCreditorInvoicedExchangeRateApplicable(header));

			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.UAInvoice;
			AssertEquals(false, CreditorInvoicedExchangeRateProvider.IsCreditorInvoicedExchangeRateApplicable(header));
		}

		protected override void SetUp()
		{
			draftInvoice = TestObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", TestObjectCreator.Creditor1.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);

			Factory.Save();

			var header = Factory.NewWithValidTestData<APInvoice>();

			line = Factory.NewWithValidTestData<APInvoiceLine>();
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			line.AL_LocalExTaxAmount = 100m;
			line.AL_OSExTaxAmount = 200m;
			line.AL_ExchangeRate = 2m;
			line.AL_AH = header.PK;

			consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			consolCost.E6_RX_NKCurrency = CurrencyCodes.UnitedStates;
			consolCost.E6_LocalCostAmount = 100m;
			consolCost.E6_OSCostAmount = 200m;
			consolCost.E6_ExchangeRate = 2m;

			provider = new CreditorInvoicedExchangeRateProvider(draftInvoice);

			AssertEquals("Precondition", true, draftInvoice.IsInLocalCurrency);
		}

		AccDraftInvoiceHeader draftInvoice;
		APInvoiceLine line;
		JobConsolCost consolCost; 
		CreditorInvoicedExchangeRateProvider provider;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
