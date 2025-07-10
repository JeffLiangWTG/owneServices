using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(InvoiceHeaderConfiguration))]
sealed class InvoiceHeaderConfigurationTest : EU.Business.Testing.InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
{
	public void TestGetSupportingDocumentValidationDecider()
	{
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(invoice));
	}

	public override void TestAgreedPlaceCodeSupport()
	{
		AssertEquals(true, configuration.AgreedPlaceCodeSupport(declaration));
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
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("EXP", false, configuration.ValueIndicatorsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("IMP", true, configuration.ValueIndicatorsSupport(declaration));
		});
	}

	public override void TestInvoicePaymentSupport()
	{
		AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
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
