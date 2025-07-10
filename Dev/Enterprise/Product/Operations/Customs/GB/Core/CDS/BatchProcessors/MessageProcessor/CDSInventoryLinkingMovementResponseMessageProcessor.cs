using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementResponseMessageProcessor : CDSInventoryLinkingResponseMessageProcessor<CDSInventoryLinkingMovementResponseEDIMessage>
	{
		public CDSInventoryLinkingMovementResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "CDS Inventory Linking Movement Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse;
	}
}
