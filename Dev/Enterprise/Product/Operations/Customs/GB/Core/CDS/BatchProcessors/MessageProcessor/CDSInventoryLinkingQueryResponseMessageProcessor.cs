using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingQueryResponseMessageProcessor : CDSInventoryLinkingResponseMessageProcessor<CDSInventoryLinkingQueryResponseEDIMessage>
	{
		public CDSInventoryLinkingQueryResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "CDS Inventory Linking Query Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse;

		protected override void ShutConsolIfRequired(CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
			if (new InventoryLinkingQueryResponse(incomingMessage.EM_MessageText).QueriedMUCR?.Shut ?? false)
			{
				var consolWrapper = new CustomsExportConsolIntegrationWrapper(outgoingMessage.LinkedConsol, null);

				if (consolWrapper != null)
				{
					consolWrapper.MawbExportHelper.ME_ChiefConsolIsClosed = true;
				}
			}
		}
	}
}
