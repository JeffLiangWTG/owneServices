using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(InvoiceLineConfiguration))]
	public class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
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
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("CDS", true, configuration.MethodOfPaymentVisibleOnImportControl(declaration));

				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				AssertEquals("Chief", false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
			});
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
			AssertEquals(false, configuration.TaxSupport(declaration));
		}

		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("CDS", true, configuration.CountryOfDestinationVisibleOnImportControl(declaration));

				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				AssertEquals("Chief", false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
			});
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
			AssertEquals(false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
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
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
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
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
		}

		JobDeclaration declaration;
	}
}
