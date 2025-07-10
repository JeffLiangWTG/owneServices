using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing;

sealed class GuidedDecisionMakingSingleInvoiceLineSourceTest : BaseGuidedDecisionMakingSourceAbstractTest<GuidedDecisionMakingSingleInvoiceLineSource>
{
	public override void TestIsCustomsFirstQuantityEditable()
	{
		AssertEquals(false, wrapper.IsCustomsFirstQuantityReadOnly);
	}

	public override void TestIsCustomsSecondQuantityEditable()
	{
		AssertEquals(false, wrapper.IsCustomsSecondQuantityReadOnly);
	}

	public override void TestIsCustomsThirdQuantityEditable()
	{
		AssertEquals(false, wrapper.IsCustomsThirdQuantityReadOnly);
	}

	protected override GuidedDecisionMakingSingleInvoiceLineSource GetGuidedDecisionMakingSource(JobComInvoiceLine invoiceLine)
	{
		return new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine);
	}
}
