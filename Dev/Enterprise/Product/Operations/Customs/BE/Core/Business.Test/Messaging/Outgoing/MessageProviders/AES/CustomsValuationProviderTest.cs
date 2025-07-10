using System.Linq;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CustomsValuationProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsValuationProvider>
{
	public void TestValuationMethod()
	{
		invoiceLine.JI_ValuationCode = "A";

		AssertEquals("ValuationMethod", "A", GetProvider().ValuationMethod);
	}

	public void TestAdditionsAndDeductions()
	{
		var invoiceHeader = invoiceLine.InvoiceHeader;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		var charge = invoiceLine.Charges.AddNew();
		charge.J7_ChargeType = "CT1";
		charge.J7_Amount = 10m;

		var charge2 = invoiceLine.Charges.AddNew();
		charge2.J7_ChargeType = "CT1";
		charge2.J7_Amount = 20m;

		var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
		apportionedCharge1.J7_ChargeType = "CT1";
		apportionedCharge1.J7_Amount = 1m;
		apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

		var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
		apportionedCharge2.J7_ChargeType = "CT2";
		apportionedCharge2.J7_Amount = 2m;
		apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

		CombineAssertions(() =>
		{
			AssertEquals("AdditionsAndDeductions", 2, Provider.AdditionsAndDeductions.Count);
			AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsAndDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "CT1|31", "CT2|2" }, Provider.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString()));
		});
	}

	protected override CustomsValuationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();

		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		provider = new CustomsValuationProvider(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	CustomsValuationProvider provider;
}
