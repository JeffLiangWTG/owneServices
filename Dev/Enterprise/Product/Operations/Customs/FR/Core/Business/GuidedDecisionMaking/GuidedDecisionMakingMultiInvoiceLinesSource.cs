using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM;

public class GuidedDecisionMakingMultiInvoiceLinesSource : EU.Business.GuidedDecisionMakingMultiInvoiceLinesSource, IGuidedDecisionMakingSource
{
	public GuidedDecisionMakingMultiInvoiceLinesSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public ZString RegionOrTerritoryOfDestination => invoiceLine.Declaration?.JE_RegionOrTerritoryOfDestination ?? string.Empty;
}
