using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class ReleaseProspectusResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public ReleaseProspectusResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => ReleaseProspectusResponseMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.ReleaseProspectusRequest };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new ReleaseProspectusResponseMessage(receivedMessage.EM_MessageText);
		if (!responseMessage.IsPositive() || !responseMessage.IsFileContentFilled)
		{
			return;
		}
		entryAdapter.AddEDoc(responseMessage.ContentData, responseMessage.FileName, responseMessage.DocumentType);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string ReleaseProspectusResponseMessageProcessorFriendlyName = "Release Prospectus Response Message Processor";
}
