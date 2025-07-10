using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderConfiguration))]
	public class InvoiceHeaderConfigurationTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invoiceHeader));
			}
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
			AssertEquals(true, configuration.TaxSupport(declaration));
		}

		public override void TestValueIndicatorsSupport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(true, configuration.ValueIndicatorsSupport(declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(false, configuration.ValueIndicatorsSupport(declaration));
		}

		public override void TestAgreedPlaceCodeSupport()
		{
			AssertEquals(false, configuration.AgreedPlaceCodeSupport(declaration));
		}

		public override void TestInvoicePaymentSupport()
		{
			AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
		}

		public override void TestExportCostCalculationsTotalsUISupport()
		{
			AssertEquals(false, configuration.ExportCostCalculationsTotalsUISupport);
		}

		public void TestMultipleSupportingDocumentsForAllInvoiceNumbersSupport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(true, configuration.MultipleSupportingDocumentsForAllInvoiceNumbersSupport(declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(true, configuration.MultipleSupportingDocumentsForAllInvoiceNumbersSupport(declaration));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
