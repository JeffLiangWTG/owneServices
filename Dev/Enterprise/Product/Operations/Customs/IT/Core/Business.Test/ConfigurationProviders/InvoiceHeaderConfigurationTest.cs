using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InvoiceHeaderConfiguration))]
sealed class InvoiceHeaderConfigurationTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
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

	public override void TestAdditionalInfosSupport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("Declaration Type IMP, AdditionalInfosSupport", false, configuration.AdditionalInfosSupport(declaration));

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("Declaration Type EXP UCC6, AdditionalInfosSupport", true, configuration.AdditionalInfosSupport(declaration));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("Declaration Type EXP nonUCC6, AdditionalInfosSupport", false, configuration.AdditionalInfosSupport(declaration));
		}
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(declaration));
	}

	public override void TestPreviousDocumentsSupport()
	{
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("When Declaration is UCC6, PreviousDocumentsSupport", false, configuration.PreviousDocumentsSupport(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("When Declaration is not UCC6, PreviousDocumentsSupport", true, configuration.PreviousDocumentsSupport(declaration));
			}
		});
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
		declaration.JE_MessageType = "IMP";
		AssertEquals("Declaration Type IMP, AgreedPlaceCodeSupport", true, configuration.AgreedPlaceCodeSupport(declaration));

		declaration.JE_MessageType = "EXP";
		AssertEquals("Declaration Type EXP, AgreedPlaceCodeSupport", false, configuration.AgreedPlaceCodeSupport(declaration));
	}

	public override void TestInvoicePaymentSupport()
	{
		AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
	}

	public void TestGetValidationDecider()
	{
		var invoiceHeader = declaration.Invoices.AddNew();
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("UCC6 EXP", configuration.GetValidationDecider(invoiceHeader));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportInvoiceHeaderValidationDecider>("UCC6 IMP", configuration.GetValidationDecider(invoiceHeader));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertNull("Not UCC6 EXP", configuration.GetValidationDecider(invoiceHeader));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertNull("Not UCC6 IMP", configuration.GetValidationDecider(invoiceHeader));
			}
		});
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
