using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class IrildesResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public IrildesResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => IrildesResponseMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.IrildesResponse };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new IrildesResponseMessage(receivedMessage.EM_MessageText);
		if (!responseMessage.IsPositive())
		{
			return;
		}

		var (departureOffice, goodsWrittenOffClosedDate) = responseMessage.GetGoodsWrittenOffInfo();

		if (departureOffice.IsEmpty
			&& (goodsWrittenOffClosedDate.IsEmpty || !goodsWrittenOffClosedDate.IsValid))
		{
			return;
		}

		entryAdapter.SetStatusAsGoodsWrittenOffClosed();
		entryAdapter.UpdateOrInsertIrildesEntryNumber(departureOffice, goodsWrittenOffClosedDate);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string IrildesResponseMessageProcessorFriendlyName = "Irildes Response Message Processor";
}
