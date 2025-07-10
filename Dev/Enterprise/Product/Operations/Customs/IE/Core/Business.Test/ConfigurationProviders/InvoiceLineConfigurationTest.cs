using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(InvoiceLineConfiguration))]
	public class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
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
			AssertEquals(true, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
		}

		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			AssertEquals(true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
		}

		public override void TestFiscalReferencesSupport()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
				{
					AssertEquals("UCC5 Import", true, configuration.FiscalReferencesSupport(declaration));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("UCC6 Import", true, configuration.FiscalReferencesSupport(declaration));

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					AssertEquals("UCC6 Export", false, configuration.FiscalReferencesSupport(declaration));
				}
			});
		}

		public override void TestVehicleSupport()
		{
			AssertEquals(false, configuration.VehicleSupport(declaration));
		}

		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, false))
				{
					AssertEquals("UCC5 Import", false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("UCC6 Import", true, configuration.AuthorisationsSupportForInvoiceLine(declaration));

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					AssertEquals("UCC6 Export", true, configuration.AuthorisationsSupportForInvoiceLine(declaration));
				}
			});
		}

		public override void TestOrganizationsSupport()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
				{
					AssertEquals("OrganizationsSupport true for UCC5 IMP.", true, configuration.OrganizationsSupport(declaration));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("OrganizationsSupport false for IMP.", false, configuration.OrganizationsSupport(declaration));

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					AssertEquals("OrganizationsSupport true for EXP.", true, configuration.OrganizationsSupport(declaration));
				}
			});
		}

		public override void TestInvoiceLinePaymentSupport()
		{
			AssertEquals(false, configuration.InvoiceLinePaymentSupport(declaration));
		}

		public override void TestValueIndicatorsSupport()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
				{
					AssertEquals("UCC5 Import", true, configuration.ValueIndicatorsSupport(declaration));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("UCC6 Import", true, configuration.ValueIndicatorsSupport(declaration));

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					AssertEquals("UCC6 Export", false, configuration.ValueIndicatorsSupport(declaration));
				}
			});
		}

		public override void TestAdditionalSupplyChainActorSupport()
		{
			AssertEquals(true, configuration.AdditionalSupplyChainActorSupport(declaration));
		}

		public void TestGetAdditionalInfoValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invocieLine = declaration
				.Invoices.AddNew()
				.InvoiceLines.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>("IsUCC6 IMP", configuration.GetAdditionalInfoValidationDecider(invocieLine));
			}
		}

		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invocieLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invocieLine));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
