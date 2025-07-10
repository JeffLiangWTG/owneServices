using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLineFreightAdjustmentsCalculatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("invoiceLine is required", () => new InvoiceLineFreightAdjustmentsCalculator(invoiceLine: null));
		AssertNoExceptionThrown(() => new InvoiceLineFreightAdjustmentsCalculator(invoiceLine: Factory.New<JobComInvoiceLine>()));
	}

	public void TestEvaluateAmount()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var charge1 = invoiceLine.Charges.AddNewWithTestValues("OFT", 1000.14m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine.Charges.AddNewWithTestValues("ADD", 111m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine.Charges.AddNewWithTestValues("OFT", 222m, isStatisticalValueApplicable: true, isIncludedInLines: true);
		var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew().SetTestValues("OFT", 43.33m, isStatisticalValueApplicable: true, isIncludedInLines: false);

		var charge4 = invoiceLine.Charges.AddNewWithTestValues("OFT", 555.12m, isStatisticalValueApplicable: false, isIncludedInLines: true);
		invoiceLine.Charges.AddNewWithTestValues("ADD", 333m, isStatisticalValueApplicable: false, isIncludedInLines: true);
		invoiceLine.Charges.AddNewWithTestValues("OFT", 444m, isStatisticalValueApplicable: true, isIncludedInLines: true);
		var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew().SetTestValues("OFT", 78.76m, isStatisticalValueApplicable: false, isIncludedInLines: true);

		CombineAssertions(() =>
		{
			var expectedAmount = charge1.J7_Amount + apportionedCharge1.J7_Amount - charge4.J7_Amount - apportionedCharge2.J7_Amount;
			var freightAdjustment = new InvoiceLineFreightAdjustmentsCalculator(invoiceLine).EvaluateAmount();
			AssertEquals("Amount", expectedAmount, freightAdjustment.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Italy, freightAdjustment.Currency.Code);
		});
	}

	public void TestEvaluateAmountWithCurrencyConversion()
	{
		var foreignCurrencyWithValidCustomsRate = RefCurrency.New(Factory);
		foreignCurrencyWithValidCustomsRate.RX_Code = "VAL";
		foreignCurrencyWithValidCustomsRate.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 2m);

		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.CEI_DateForDuty = ZDateTime.Today;
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var charge1 = invoiceLine.Charges.AddNewWithTestValues("OFT", 1000.14m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		var charge2 = invoiceLine.Charges.AddNewWithTestValues("OFT", 555.12m, "VAL", isStatisticalValueApplicable: false, isIncludedInLines: true);

		CombineAssertions(() =>
		{
			var expectedAmount = charge1.J7_Amount - (charge2.J7_Amount / 2);

			var freightAdjustment = new InvoiceLineFreightAdjustmentsCalculator(invoiceLine).EvaluateAmount();
			AssertEquals("Amount", expectedAmount, freightAdjustment.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Italy, freightAdjustment.Currency.Code);
		});
	}
}
