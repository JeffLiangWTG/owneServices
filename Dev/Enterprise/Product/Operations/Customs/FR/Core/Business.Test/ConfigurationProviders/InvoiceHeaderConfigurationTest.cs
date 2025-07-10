using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderConfiguration))]
	public class InvoiceHeaderConfigurationTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invoiceHeader));
			}
		}

		public void TestGetPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				AssertType<UCC6ImportPreviousDocumentValidationDecider>("IsUCC6 IMP", configuration.GetPreviousDocumentValidationDecider(invoiceHeader));
			}
		}

		public override void TestInvoicePaymentSupport()
		{
			AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
		}

		public override void TestAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
		}

		public override void TestSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
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
			AssertEquals(false, configuration.ValueIndicatorsSupport(declaration));
		}

		public override void TestAgreedPlaceCodeSupport()
		{
			AssertEquals(false, configuration.AgreedPlaceCodeSupport(declaration));
		}

		public override void TestExportCostCalculationsTotalsUISupport()
		{
			AssertEquals(false, configuration.ExportCostCalculationsTotalsUISupport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
