using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ValuationAdjustmentWrapperTest : DataProviderTestCase<ValuationAdjustmentWrapper>
{
	public void TestAdditionCode() => CombineAssertions(() =>
	{
		invoiceLine.JI_ValuationCode = "1";
		invoiceLine.RelatedIndicator = true;
		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = true;
		invoiceLine.RelatedIndicator4 = false;

		AssertEquals("JI_ValuationCode = 1", "1010", wrapper.AdditionCode);

		invoiceLine.JI_ValuationCode = "2";
		AssertEquals("JI_ValuationCode != 1", string.Empty, wrapper.AdditionCode);
	});

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		invoiceHeader.JZ_JE = declaration.PK;
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		wrapper = new ValuationAdjustmentWrapper(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	ValuationAdjustmentWrapper wrapper;

	protected override ValuationAdjustmentWrapper GetProvider() => wrapper;
}
