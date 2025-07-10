using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business;

public class GuidedDecisionMakingSingleInvoiceLineSource : BaseGuidedDecisionMakingSource
{
	public GuidedDecisionMakingSingleInvoiceLineSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	public override ZBool IsCustomsFirstQuantityReadOnly => false;

	public override ZBool IsCustomsSecondQuantityReadOnly => false;

	public override ZBool IsCustomsThirdQuantityReadOnly => false;
}
