using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InvoiceLineConfiguration))]
sealed class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
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
		AssertEquals(false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
	}

	public override void TestAdditionalInfosSupport()
	{
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(declaration));
	}

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(declaration));
	}

	public override void TestTaxSupport()
	{
		CombineAssertions("Check TaxSupport", () =>
		{
			AssertEquals("TaxSupport for JobDeclaration", false, configuration.TaxSupport(declaration));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.IMP;

			AssertEquals("TaxSupport for Products", true, configuration.TaxSupport(pivot));
		});
	}

	public override void TestCountryOfDestinationVisibleOnImportControl()
	{
		AssertEquals(false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
	}

	public override void TestCountryOfDestinationVisibleOnExportControl()
	{
		AssertEquals(true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
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
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("UCC6", true, configuration.AuthorisationsSupportForInvoiceLine(declaration));

				declaration.JE_MessageType = "IMP";
				AssertEquals("For IMP", false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("Not UCC6", false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
			}
		});
	}

	public override void TestOrganizationsSupport()
	{
		AssertEquals(false, configuration.OrganizationsSupport(declaration));
	}

	public override void TestInvoiceLinePaymentSupport()
	{
		AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
	}

	public override void TestValueIndicatorsSupport()
	{
		AssertEquals(false, configuration.ValueIndicatorsSupport(declaration));
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(declaration));
	}

	public void TestGetValidationDecider()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration
				.Invoices.AddNew()
				.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("UCC6 EXP", configuration.GetValidationDecider(invoiceLine));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportInvoiceLineValidationDecider>("UCC6 IMP", configuration.GetValidationDecider(invoiceLine));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("Not UCC6 EXP", configuration.GetValidationDecider(invoiceLine));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull("Not UCC6 IMP", configuration.GetValidationDecider(invoiceLine));
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
