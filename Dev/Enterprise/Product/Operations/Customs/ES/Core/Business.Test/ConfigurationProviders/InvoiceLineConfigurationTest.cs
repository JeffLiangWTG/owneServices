using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(InvoiceLineConfiguration))]
public class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
{
	public void TestGetSupportingDocumentValidationDecider()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invocieLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invocieLine));
		}
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

	public override void TestMethodOfPaymentVisibleOnImportControl()
	{
		AssertEquals(true, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
	}

	public override void TestAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
	}

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(declaration));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(declaration));
	}

	public override void TestTaxSupport()
	{
		AssertEquals(false, configuration.TaxSupport(declaration));
	}

	public override void TestCountryOfDestinationVisibleOnImportControl()
	{
		AssertEquals(true, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
	}

	public override void TestCountryOfDestinationVisibleOnExportControl()
	{
		AssertEquals(false, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
	}

	public override void TestFiscalReferencesSupport()
	{
		AssertEquals(false, configuration.FiscalReferencesSupport(declaration));
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
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertEquals("Import", true, configuration.OrganizationsSupport(declaration));

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("Export", true, configuration.OrganizationsSupport(declaration));

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("MiscellaneousCustoms", false, configuration.OrganizationsSupport(declaration));
		});
	}

	public override void TestInvoiceLinePaymentSupport()
	{
		AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
	}

	public override void TestValueIndicatorsSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("UCC6 Import", expected: true, configuration.ValueIndicatorsSupport(declaration));
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				AssertEquals("not UCC6 Import", expected: false, configuration.ValueIndicatorsSupport(declaration));
			}
		});
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
