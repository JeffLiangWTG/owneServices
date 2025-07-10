using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(InvoiceLineConfiguration))]
	class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
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

		public void TestGetPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invocieLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				AssertType<UCC6ImportPreviousDocumentValidationDecider>("IsUCC6 IMP", configuration.GetPreviousDocumentValidationDecider(invocieLine));
			}
		}

		public override void TestInflateItemPriceByValuationMarkup()
		{
			AssertEquals(true, configuration.InflateItemPriceByValuationMarkup(declaration));
		}

		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			AssertEquals(true, configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));
		}

		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			AssertEquals(true, configuration.DefaultCountryOfSupplyFromSupplier(declaration));
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
			AssertEquals(false, configuration.TaxSupport(declaration));
		}

		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				AssertEquals("Country Of Destination is invisible if it is not UCC6 import declaration.", false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertEquals("Country Of Destination is visible if it is UCC6 import declaration.", true, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
			}
		}

		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			AssertEquals(true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
		}

		public override void TestFiscalReferencesSupport()
		{
			AssertEquals(true, configuration.FiscalReferencesSupport(declaration));
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
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				AssertEquals("Prerequisite", false, declaration.IsUCC6AndIsImport);
				AssertEquals("Organizations are supported in UCC6 import declarations.", false, configuration.OrganizationsSupport(declaration));
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertEquals("Prerequisite", true, declaration.IsUCC6AndIsImport);
				AssertEquals("Organizations should be supported in UCC6 import declarations.", true, configuration.OrganizationsSupport(declaration));
			}
		}

		public override void TestInvoiceLinePaymentSupport()
		{
			AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
		}

		public override void TestValueIndicatorsSupport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Valuation Indicators are supported in UCC6 import declarations only.", false, configuration.ValueIndicatorsSupport(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Valuation Indicators are supported in UCC6 import declarations only.", false, configuration.ValueIndicatorsSupport(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Valuation Indicators should be supported in UCC6 import.", true, configuration.ValueIndicatorsSupport(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Valuation Indicators are supported in UCC6 import declarations only.", false, configuration.ValueIndicatorsSupport(declaration));
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
}
