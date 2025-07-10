using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineHarbourRateProviderTest : TestCaseWithFactory
{
	public void TestHarbourRate_WhenDeclarationIsNull()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var harbourRateProvider = (IHarbourRateProvider)new JobComInvoiceLineHarbourRateProvider(invoiceLine);
		AssertNull(nameof(harbourRateProvider.HarbourRate), harbourRateProvider.HarbourRate);
	}

	public void TestHarbourRate_WhenDeclarationIsImport()
	{
		new ITUniversalReferenceTestDataHelper(Factory)
			.SetupHarbourRates();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.ZG_PortTaxRate = "A3";

		var harbourRateProvider = (IHarbourRateProvider)new JobComInvoiceLineHarbourRateProvider(invoiceLine);
		var harbourRate = harbourRateProvider.HarbourRate;
		AssertNotNull(nameof(harbourRate), harbourRate);
		AssertEquals(nameof(harbourRate.ZXF_PortTaxType), "9AA", harbourRate.ZXF_PortTaxType);
	}

	public void TestHarbourRate_WhenDeclarationIsExport()
	{
		new ITUniversalReferenceTestDataHelper(Factory)
			.SetupHarbourRates();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_RL_NKPortOfLoading = "USLAX";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.ZG_PortTaxRate = "A4";

		var harbourRateProvider = (IHarbourRateProvider)new JobComInvoiceLineHarbourRateProvider(invoiceLine);
		var harbourRate = harbourRateProvider.HarbourRate;
		AssertNotNull(nameof(harbourRate), harbourRate);
		AssertEquals(nameof(harbourRate.ZXF_PortTaxType), "9AB", harbourRate.ZXF_PortTaxType);
	}

	public void TestHarbourRate_WhenHarbourRateNotFound()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var harbourRateProvider = (IHarbourRateProvider)new JobComInvoiceLineHarbourRateProvider(invoiceLine);
		AssertNull(nameof(harbourRateProvider.HarbourRate), harbourRateProvider.HarbourRate);
	}
}
