using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(InvoiceLineConfiguration))]
sealed class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
{
	public void TestGetSupportingDocumentValidationDecider()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invoiceLine));
	}

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
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(declaration.Factory, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("Import supported", true, configuration.FiscalReferencesSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertEquals("Export not supported", false, configuration.FiscalReferencesSupport(declaration));
			}
		});
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

	public override void TestOrganizationsSupport()
	{
		AssertEquals(true, configuration.OrganizationsSupport(declaration));
	}

	public override void TestVehicleSupport()
	{
		AssertEquals(false, configuration.VehicleSupport(declaration));
	}

	public override void TestAuthorisationsForInvoiceLineSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupportForInvoiceLine(declaration));
	}

	public override void TestInvoiceLinePaymentSupport()
	{
		AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
	}

	public override void TestValueIndicatorsSupport()
	{
		AssertEquals(true, configuration.ValueIndicatorsSupport(declaration));
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
