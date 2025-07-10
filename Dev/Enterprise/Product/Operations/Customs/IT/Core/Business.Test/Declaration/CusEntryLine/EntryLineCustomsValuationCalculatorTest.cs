using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryLineCustomsValuationCalculatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("entryLine is required", () => new EntryLineCustomsValuationCalculator(entryLine: null));
		AssertNoExceptionThrown(() => new EntryLineCustomsValuationCalculator(entryLine: Factory.New<CusEntryLine>()));
	}

	public void TestEvaluateAmount()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Italy;
		var entryLine = Factory.New<CusEntryLine>();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine1.JI_LinePrice = 100.12m;
		var charge1 = invoiceLine1.Charges.AddNewWithTestValues("OFT", 1000.14m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("ADD", 111m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("OFT", 222m, isStatisticalValueApplicable: true, isIncludedInLines: true);
		var apportionedCharge1 = invoiceLine1.ApportionedCharges.AddNew().SetTestValues("OFT", 43.33m, isStatisticalValueApplicable: true, isIncludedInLines: false);

		var charge4 = invoiceLine1.Charges.AddNewWithTestValues("OFT", 555.12m, isStatisticalValueApplicable: false, isIncludedInLines: true);
		invoiceLine1.Charges.AddNewWithTestValues("ADD", 333m, isStatisticalValueApplicable: false, isIncludedInLines: true);
		invoiceLine1.Charges.AddNewWithTestValues("OFT", 444m, isStatisticalValueApplicable: true, isIncludedInLines: true);
		var apportionedCharge2 = invoiceLine1.ApportionedCharges.AddNew().SetTestValues("OFT", 78.76m, isStatisticalValueApplicable: false, isIncludedInLines: true);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		invoiceLine2.JI_LinePrice = 43.87m;
		var charge5 = invoiceLine2.Charges.AddNewWithTestValues("OFT", 122.41m, isStatisticalValueApplicable: true, isIncludedInLines: false);

		CombineAssertions(() =>
		{
			var customsValuation = new EntryLineCustomsValuationCalculator(entryLine).EvaluateAmount();

			var expectedFreightAdjustmentsAmount = charge1.J7_Amount + apportionedCharge1.J7_Amount - charge4.J7_Amount - apportionedCharge2.J7_Amount + charge5.J7_Amount;
			AssertEquals("FreightAdjustments.Amount", expectedFreightAdjustmentsAmount, customsValuation.FreightAdjustments.Amount);
			AssertEquals("FreightAdjustments.Currency", Core.Constants.CurrencyCodes.Italy, customsValuation.FreightAdjustments.Currency.Code);

			var expectedLineValueAmount = invoiceLine1.JI_LinePrice + invoiceLine2.JI_LinePrice;
			AssertEquals("LineValue.Amount", expectedLineValueAmount, customsValuation.LineValue.Amount);
			AssertEquals("LineValue.Currency", Core.Constants.CurrencyCodes.Italy, customsValuation.LineValue.Currency.Code);
		});
	}

	public void TestEvaluateAmountWithCurrencyConversion()
	{
		var foreignCurrencyWithValidCustomsRate = RefCurrency.New(Factory);
		foreignCurrencyWithValidCustomsRate.RX_Code = "VAL";
		foreignCurrencyWithValidCustomsRate.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 2m);

		var entryLine = Factory.New<CusEntryHeader>().MergedLines.AddNew();

		var invoice1 = Factory.New<JobComInvoiceHeader>();
		invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
		invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Italy;
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine1.JI_LinePrice = 100.12m;

		var invoice2 = Factory.New<JobComInvoiceHeader>();
		invoice2.JZ_ValuationDateOverride = ZDateTime.Today;
		invoice2.JZ_RX_NKInvoice_Currency = "VAL";
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		invoiceLine2.JI_LinePrice = 44.88m;

		CombineAssertions(() =>
		{
			var customsValuation = new EntryLineCustomsValuationCalculator(entryLine).EvaluateAmount();

			var expectedLineValueAmount = invoiceLine1.JI_LinePrice + (invoiceLine2.JI_LinePrice / 2);
			AssertEquals("LineValue.Amount", expectedLineValueAmount, customsValuation.LineValue.Amount);
			AssertEquals("LineValue.Currency", Core.Constants.CurrencyCodes.Italy, customsValuation.LineValue.Currency.Code);
		});
	}
}
