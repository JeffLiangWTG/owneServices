using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderConfiguration))]
	class InvoiceHeaderConfigurationTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		public void TestGetValidationDecider()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("UCC6 EXP", configuration.GetValidationDecider(invoiceHeader));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertType<UCC6ImportInvoiceHeaderValidationDecider>("UCC6 IMP", configuration.GetValidationDecider(invoiceHeader));
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertType<UCC5ImportInvoiceHeaderValidationDecider>("UCC5 IMP", configuration.GetValidationDecider(invoiceHeader));
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
			AssertEquals(true, configuration.TaxSupport(declaration));
		}

		public override void TestValueIndicatorsSupport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IMP", true, configuration.ValueIndicatorsSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("EXP", false, configuration.ValueIndicatorsSupport(declaration));
			});
		}

		public override void TestAgreedPlaceCodeSupport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IMP", true, configuration.AgreedPlaceCodeSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("EXP", true, configuration.AgreedPlaceCodeSupport(declaration));
			});
		}

		public override void TestInvoicePaymentSupport()
		{
			AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
		}

		public void TestGetAdditionalInfoValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>("IsUCC6 IMP", configuration.GetAdditionalInfoValidationDecider(invoiceHeader));
			}
		}

		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invoiceHeader));
			}
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
