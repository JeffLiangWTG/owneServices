using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business;

public class GuidedDecisionMakingMultiInvoiceLinesSource : BaseGuidedDecisionMakingSource
{
	public GuidedDecisionMakingMultiInvoiceLinesSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	public override ZBool IsCustomsFirstQuantityReadOnly => true;

	public override ZBool IsCustomsSecondQuantityReadOnly => true;

	public override ZBool IsCustomsThirdQuantityReadOnly => true;
}
