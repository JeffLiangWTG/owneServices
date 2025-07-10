using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class Eur1ResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public Eur1ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Eur1ResponseMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.Eur1Request };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new Eur1ResponseMessage(receivedMessage.EM_MessageText);
		if (!responseMessage.IsPositive() || !responseMessage.IsFileContentFilled)
		{
			return;
		}
		entryAdapter.AddEDoc(responseMessage.ContentData, responseMessage.FileName, responseMessage.DocumentType);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string Eur1ResponseMessageProcessorFriendlyName = "EUR1 Response Message Processor";
}
