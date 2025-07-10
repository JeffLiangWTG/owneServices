using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public class GuidedDecisionMakingSingleInvoiceLineSource : EU.Business.GuidedDecisionMakingSingleInvoiceLineSource, IESGuidedDecisionMakingSource
	{
		public GuidedDecisionMakingSingleInvoiceLineSource(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public ZBool DestinationStateIsCanaryIsland => invoiceLine.DestinationStateIsCanaryIsland;
	}
}
