using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingControlResponseMessageProcessor : CDSInventoryLinkingResponseMessageProcessor<CDSInventoryLinkingControlResponseEDIMessage>
	{
		public CDSInventoryLinkingControlResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetOutgoingMessageStatus(CDSEDIMessage message)
		{
			var inventoryLinkingControlResponseMessage = message as CDSInventoryLinkingControlResponseEDIMessage;

			return (inventoryLinkingControlResponseMessage?.MessageDataObject.ActionCode ?? ZString.Empty) == CDSActionCodeList.Codes.Rejected ? EDIMessageStatusList.Codes.Rejected : EDIMessageStatusList.Codes.Acknowledged;
		}

		protected override string MessageFriendlyNameCore => "CDS Inventory Linking Control Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse;

		protected override void ShutConsolIfRequired(CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
			if ((incomingMessage.Interchange.GBCustomsBusinessResponse.InventoryLinkingControlResponse.MessageCode == CDSInventoryLinkingConsolidationMessageCodeList.Codes.CST
				|| outgoingMessage.EM_MessageSubType == GbCusDecMessageFunctionsList.Codes.Close)
				&& incomingMessage.Interchange.GBCustomsBusinessResponse.InventoryLinkingControlResponse.ActionCode == CDSActionCodeList.Codes.AcknowledgedAndProcessed)
			{
				var consolWrapper = new CustomsExportConsolIntegrationWrapper(outgoingMessage.LinkedConsol, null);

				if (consolWrapper != null)
				{
					consolWrapper.MawbExportHelper.ME_ChiefConsolIsClosed = true;
				}
			}
		}

		protected override void ClearMUCRIfRequired(CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
			if (outgoingMessage.EM_MessageSubType == GbCusDecMessageFunctionsList.Codes.Disassociate && incomingMessage.Interchange.GBCustomsBusinessResponse.InventoryLinkingControlResponse.ActionCode == CDSActionCodeList.Codes.AcknowledgedAndProcessed)
			{
				var entry = outgoingMessage.LinkedEntry;

				if (entry != null)
				{
					entry.Declaration.JE_MasterUCR = ZString.Empty;
				}
			}
		}
	}
}
