using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class AcknowledgementResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public AcknowledgementResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string MessageFriendlyNameCore => AckResponseMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageProcessorConstants.InterchangeTypes.Ucc6AcknowledgementType };

	protected override bool ShouldSetReceivedMessageNumberEqualToOriginalMessageNumber => true;

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new AcknowledgementResponseMessage(receivedMessage.EM_MessageText);

		if (!entryAdapter.IsAwaitingMessage)
		{
			return;
		}

		ProcessEntryMessageStatus(entryAdapter, responseMessage, originalSentMessage);
		ProcessUniqueTransactionIdentifierRequest(entryAdapter, receivedMessage);
		entryAdapter.ProcessBondedWarehouseIfRequired(Logger, receivedMessage);
	}

	#region Implementation

	void ProcessUniqueTransactionIdentifierRequest(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage)
	{
		if (!entryAdapter.IsDeposited)
		{
			return;
		}

		var lastNewDeclarationSentInterchange = entryAdapter.GetLastSuccessfullySentMessageForDepositedStatus()?.Interchange;
		if (lastNewDeclarationSentInterchange == null)
		{
			return;
		}

		var uniqueTransactionIdentifierProvider = new UniqueTransactionIdentifierProvider(lastNewDeclarationSentInterchange);
		var uniqueTransactionID = uniqueTransactionIdentifierProvider.GetUniqueTransactionID();
		if (!uniqueTransactionID.IsEmpty)
		{
			var uniqueTransactionIdentifierRequestContext = entryAdapter.GetUniqueTransactionIdentifierRequestContext(uniqueTransactionID, receivedMessage.EM_MessageNum);
			var uniqueTransactionIdentifierRequestStrategy = (IOutgoingCustomsMessageCreationStrategy)new UniqueTransactionIdentifierRequestMessageCreationStrategy(entryAdapter.Factory, uniqueTransactionIdentifierRequestContext);
			uniqueTransactionIdentifierRequestStrategy.GenerateMessage();
		}
	}

	void ProcessEntryMessageStatus(IXmlCustomsLinkedObjectAdapter entryAdapter, AcknowledgementResponseMessage responseMessage, EDIMessage originalSentMessage)
	{
		var isPositiveAcknowledgement = positiveStatusCollection.Contains(responseMessage.ResponseStatus);
		if (isPositiveAcknowledgement)
		{
			entryAdapter.SetStatusAsAcknowledged();
		}
		else
		{
			entryAdapter.SetStatusAsError();

			if (entryAdapter is IGuaranteeTransactionSupporter guaranteeTransactionSupporter)
			{
				guaranteeTransactionSupporter.DeletePendingTransactions(originalSentMessage.EM_MessageNum);
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant response message")]
	const string AckResponseMessageProcessorFriendlyName = "UCC6 Acknowledgement Response Message Processor";

	readonly ImmutableArray<ZString> positiveStatusCollection = new ZString[]
	{
		Ucc6AcknowledgementStatusList.Codes.MessageNotYetProcessed,
		Ucc6AcknowledgementStatusList.Codes.MessageAcquired,
		Ucc6AcknowledgementStatusList.Codes.MessageInElaboration,
		Ucc6AcknowledgementStatusList.Codes.MessageInElaborationSubstantialVerificationPassed,
	}.ToImmutableArray();

	#endregion
}
