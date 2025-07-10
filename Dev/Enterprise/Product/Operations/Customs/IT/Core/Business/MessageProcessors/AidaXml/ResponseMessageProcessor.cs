using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using IXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

public sealed class ResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType };

	protected override string MessageFriendlyNameCore => Ucc6ResponseMessageProcessorFriendlyName;

	protected override bool ShouldSetReceivedMessageNumberEqualToOriginalMessageNumber => true;

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage sentMessage)
	{
		if (sentMessage.EM_LinkTable == AsycudaManifestHeaderSchema.Constants.TableName)
		{
			var responseMessageWithWrapper = new TemporaryStorageResponseMessage(receivedMessage.EM_MessageText);
			var status = responseMessageWithWrapper.ResponseStatus;

			if (!IsPositive(status))
			{
				entryAdapter.SetStatusAsError();
				return;
			}

			new NewTemporaryStorageResponseMessageSubProcessor().ProcessMessage(entryAdapter, responseMessageWithWrapper.GetResponseMessageContents());
			return;
		}

		var originalSentMessage = GetEffectiveSentMessage(entryAdapter, sentMessage);
		if (!TryParseMessageContent(receivedMessage, originalSentMessage, out var responseStatus, out var aidaXmlResponseMessage))
		{
			return;
		}

		if (IsPositive(responseStatus))
		{
			var subProcessor = ResponseMessageSubProcessorFactory.GetSubProcessor(originalSentMessage);
			subProcessor.ProcessMessage(entryAdapter, aidaXmlResponseMessage);
		}
		else
		{
			entryAdapter.SetStatusAsError();

			if (entryAdapter is IGuaranteeTransactionSupporter guaranteeTransactionSupporter)
			{
				guaranteeTransactionSupporter.DeletePendingTransactions(sentMessage.EM_MessageNum);
			}
		}

		entryAdapter.ProcessBondedWarehouseIfRequired(Logger, receivedMessage);
	}

	bool TryParseMessageContent(EDIMessage receivedMessage, EDIMessage originalSentMessage, out string responseStatus, out IXmlResponseMessage aidaXmlResponseMessage)
	{
		var responseMessageWithWrapper = Ucc6ResponseMessageFactory.GetResponseMessage(receivedMessage.EM_MessageText, originalSentMessage.EM_ApplicationReference);

		try
		{
			responseStatus = responseMessageWithWrapper.ResponseStatus;
			aidaXmlResponseMessage = responseMessageWithWrapper.GetResponseMessageContents();
			return true;
		}
		catch (InvalidOperationException)
		{
			responseStatus = null;
			aidaXmlResponseMessage = null;
			return false;
		}
	}

	bool IsPositive(string responseStatus) => positiveStatusCollection.Contains(responseStatus);

	EDIMessage GetEffectiveSentMessage(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage sentMessage)
	{
		if (sentMessage.EM_MessageType == EDIMessageTypeList.Codes.UniqueTransactionId)
		{
			var uniqueTransactionId = UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier(sentMessage.EM_MessageText);
			var originalSentMessage = entryAdapter.GetOriginalSentMessageByUniqueTransactionIdentifier(uniqueTransactionId);
			return originalSentMessage ?? throw new CustomsMessageProcessorException(FormattableString.Invariant($"Unable to retrieve the original sent message for the IUT request '{uniqueTransactionId}'"));
		}

		return sentMessage;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
	const string Ucc6ResponseMessageProcessorFriendlyName = "UCC6 Response Message Processor";

	readonly ImmutableArray<ZString> positiveStatusCollection = new ZString[]
	{
		Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithoutResult,
		Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithResult,
	}.ToImmutableArray();
}
