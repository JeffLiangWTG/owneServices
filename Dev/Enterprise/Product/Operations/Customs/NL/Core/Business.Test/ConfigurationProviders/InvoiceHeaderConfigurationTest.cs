using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(InvoiceHeaderConfiguration))]
public class InvoiceHeaderConfigurationTest : EU.Business.Testing.InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
{
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
		AssertEquals(false, configuration.PreviousDocumentsSupport(declaration));
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
		AssertEquals(true, configuration.AgreedPlaceCodeSupport(declaration));
	}

	public override void TestInvoicePaymentSupport()
	{
		AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
	}

	public override void TestExportCostCalculationsTotalsUISupport() => CombineAssertions(() =>
	{
		AssertEquals(true, configuration.ExportCostCalculationsTotalsUISupport);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
