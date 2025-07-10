using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class CustomsXTradeErrorResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public CustomsXTradeErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"UCC6 Customs xT Error Response Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.XtCustomsError };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		entryAdapter.ProcessBondedWarehouseIfRequired(Logger, receivedMessage);
		ProcessMauCertificateErrorResponse(entryAdapter, receivedMessage, originalSentMessage);
	}

	void ProcessMauCertificateErrorResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		if (!IsMauCertificateRelatedError(receivedMessage.EM_MessageText) ||
			!IsRelevantMessageType(originalSentMessage.EM_MessageType) ||
			!IsValidMauCertificate(entryAdapter))
		{
			return;
		}

		switch (originalSentMessage.EM_MessageType)
		{
			case EDIMessageTypeList.Codes.NewDeclaration:
			case EDIMessageTypeList.Codes.Amendment:
			case EDIMessageTypeList.Codes.Cancellation:
				TryCreateIUTMessage(entryAdapter, originalSentMessage);
				break;
			case EDIMessageTypeList.Codes.IvistoRequest:
				entryAdapter.CreateIvistoRequestMessage();
				originalSentMessage.Interchange.DeletePollingTransactions();
				break;
			case EDIMessageTypeList.Codes.IrildesRequest:
				entryAdapter.CreateIrildesRequestMessage();
				originalSentMessage.Interchange.DeletePollingTransactions();
				break;
		}
	}

	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Not localized message content")]
	bool IsMauCertificateRelatedError(string messageText)
	{
		const string badCertificateMessagePattern = "alert bad certificate";
		const string notAcceptedMessagePattern = "rejected by peer: result code 403 not accepted";

		return messageText.Contains(badCertificateMessagePattern, StringComparison.OrdinalIgnoreCase)
			|| messageText.Contains(notAcceptedMessagePattern, StringComparison.OrdinalIgnoreCase);
	}

	bool IsRelevantMessageType(ZString messageType)
	{
		return messageType == EDIMessageTypeList.Codes.NewDeclaration
			|| messageType == EDIMessageTypeList.Codes.Amendment
			|| messageType == EDIMessageTypeList.Codes.Cancellation
			|| messageType == EDIMessageTypeList.Codes.IvistoRequest
			|| messageType == EDIMessageTypeList.Codes.IrildesRequest;
	}

	bool TryGetAcknowledgmentMessage(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage sentMessage, out EDIMessage acknowledgmentMessage)
	{
		acknowledgmentMessage = entryAdapter.Messages.FirstOrDefault(x => x.InterchangeEHubID == sentMessage.InterchangeEHubID && x.EM_MessageType == EDIMessageTypeList.Codes.Acknowledgment);
		return acknowledgmentMessage != null;
	}

	bool IsValidMauCertificate(IXmlCustomsLinkedObjectAdapter entryAdapter)
	{
		var certificateProvider = new GlbCertificateProvider();
		var mauCertificate = certificateProvider.GetMauCertificatePassword(entryAdapter.CustomsProfile);
		return mauCertificate != null && mauCertificate.GP_ExpiryDate >= ZDateTime.Today;
	}

	bool TryGetUniqueTransactionIdentifier(EDIMessage acknowledgmentMessage, out ZString uniqueTransactionIdentifier)
	{
		uniqueTransactionIdentifier = UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier(acknowledgmentMessage.EM_MessageText);
		return !uniqueTransactionIdentifier.IsEmpty;
	}

	void TryCreateIUTMessage(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage originalSentMessage)
	{
		if (!TryGetAcknowledgmentMessage(entryAdapter, originalSentMessage, out var acknowledgmentMessage) ||
			!TryGetUniqueTransactionIdentifier(acknowledgmentMessage, out var uniqueTransactionIdentifier))
		{
			return;
		}

		var uniqueTransactionIdentifierRequestContext = entryAdapter.GetUniqueTransactionIdentifierRequestContext(uniqueTransactionIdentifier, acknowledgmentMessage.EM_MessageNum);
		var messageCreationStrategy = (IOutgoingCustomsMessageCreationStrategy)new UniqueTransactionIdentifierRequestMessageCreationStrategy(entryAdapter.Factory, uniqueTransactionIdentifierRequestContext);
		messageCreationStrategy.GenerateMessage();
	}
}
