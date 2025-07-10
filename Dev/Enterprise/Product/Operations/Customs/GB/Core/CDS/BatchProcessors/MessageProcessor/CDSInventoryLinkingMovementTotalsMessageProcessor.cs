using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementTotalsMessageProcessor : CDSInventoryLinkingResponseMessageProcessor<CDSInventoryLinkingMovementTotalsResponseEDIMessage>
	{
		public CDSInventoryLinkingMovementTotalsMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "CDS Inventory Linking Movement Totals Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse;
	}
}
