using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business;

public class GuidedDecisionMakingMultiInvoiceLinesSource : EU.Business.GuidedDecisionMakingMultiInvoiceLinesSource, IESGuidedDecisionMakingSource
{
	public GuidedDecisionMakingMultiInvoiceLinesSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public ZBool DestinationStateIsCanaryIsland => invoiceLine.DestinationStateIsCanaryIsland;
}
