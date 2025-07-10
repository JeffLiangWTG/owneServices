using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingSingleInvoiceLineSource : EU.Business.GuidedDecisionMakingSingleInvoiceLineSource, IGuidedDecisionMakingSource
	{
		public GuidedDecisionMakingSingleInvoiceLineSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public ZString RegionOrTerritoryOfDestination => invoiceLine.Declaration?.JE_RegionOrTerritoryOfDestination ?? string.Empty;
	}
}
