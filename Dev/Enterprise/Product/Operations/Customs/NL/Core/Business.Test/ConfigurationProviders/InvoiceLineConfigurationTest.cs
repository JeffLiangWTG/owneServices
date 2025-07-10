using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(InvoiceLineConfiguration))]
sealed class InvoiceLineConfigurationTest : EU.Business.Testing.InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
{
	public override void TestInflateItemPriceByValuationMarkup()
	{
		AssertEquals(false, configuration.InflateItemPriceByValuationMarkup(declaration));
	}

	public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
	{
		AssertEquals(false, configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));
	}

	public override void TestDefaultCountryOfSupplyFromSupplier()
	{
		AssertEquals(false, configuration.DefaultCountryOfSupplyFromSupplier(declaration));
	}

	public override void TestAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
	}

	public override void TestCountryOfDestinationVisibleOnImportControl()
	{
		AssertEquals(true, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
	}

	public override void TestCountryOfDestinationVisibleOnExportControl()
	{
		AssertEquals(true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
	}

	public override void TestFiscalReferencesSupport()
	{
		AssertEquals(false, configuration.FiscalReferencesSupport(declaration));
	}

	public override void TestMethodOfPaymentVisibleOnImportControl()
	{
		AssertEquals(false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
	}

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(declaration));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
	}

	public override void TestTaxSupport()
	{
		AssertEquals(false, configuration.TaxSupport(declaration));
	}

	public override void TestVehicleSupport()
	{
		AssertEquals(false, configuration.VehicleSupport(declaration));
	}

	public override void TestAuthorisationsForInvoiceLineSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupportForInvoiceLine(declaration));
	}

	public override void TestOrganizationsSupport()
	{
		AssertEquals(true, configuration.OrganizationsSupport(declaration));
	}

	public override void TestInvoiceLinePaymentSupport()
	{
		AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
	}

	public override void TestValueIndicatorsSupport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals(true, configuration.ValueIndicatorsSupport(declaration));
	}

	public override void TestAdditionalSupplyChainActorSupport() => CombineAssertions(() =>
	{
		AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(null));
	});

	public void TestInvoiceLineValidationDecider() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		AssertType<UCC6ImportInvoiceLineValidationDecider>(configuration.GetValidationDecider(invoiceLine));

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertType<UCC6ExportInvoiceLineValidationDecider>(configuration.GetValidationDecider(invoiceLine));
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
