using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;
sealed class AccountingSummaryDownloadMessageProcessor : XmlIncomingMessageProcessor
{
	public AccountingSummaryDownloadMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => AccountingSummaryDownloadMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.AccountingSummaryDownload };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new ProspectusDownloadMessage(receivedMessage.EM_MessageText, EDIMessageTypeList.Codes.AccountingSummaryDownload);
		if (!responseMessage.IsPositive() || !responseMessage.IsFileContentFilled)
		{
			return;
		}
		entryAdapter.AddEDoc(responseMessage.ContentData, responseMessage.FileName, responseMessage.DocumentType);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string AccountingSummaryDownloadMessageProcessorFriendlyName = "Accounting Summary Download Message Processor";
}
