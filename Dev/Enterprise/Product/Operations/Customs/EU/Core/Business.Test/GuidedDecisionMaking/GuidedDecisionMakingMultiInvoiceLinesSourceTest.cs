using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing;

sealed class GuidedDecisionMakingMultiInvoiceLinesSourceTest : BaseGuidedDecisionMakingSourceAbstractTest<GuidedDecisionMakingMultiInvoiceLinesSource>
{
	public override void TestIsCustomsFirstQuantityEditable()
	{
		AssertEquals(true, wrapper.IsCustomsFirstQuantityReadOnly);
	}

	public override void TestIsCustomsSecondQuantityEditable()
	{
		AssertEquals(true, wrapper.IsCustomsSecondQuantityReadOnly);
	}

	public override void TestIsCustomsThirdQuantityEditable()
	{
		AssertEquals(true, wrapper.IsCustomsThirdQuantityReadOnly);
	}

	protected override GuidedDecisionMakingMultiInvoiceLinesSource GetGuidedDecisionMakingSource(JobComInvoiceLine invoiceLine)
	{
		return new GuidedDecisionMakingMultiInvoiceLinesSource(invoiceLine);
	}
}
