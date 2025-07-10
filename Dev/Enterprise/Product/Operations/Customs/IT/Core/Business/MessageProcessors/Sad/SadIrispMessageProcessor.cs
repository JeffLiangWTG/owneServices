using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class SadIrispMessageProcessor : SadIncomingCustomsMessageProcessor<IrispTypeR>
{
	public SadIrispMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString MessageTypeToInclude => SADConstants.CustomsInterchangeType.IrispX;

	protected override void PerformTypeSpecificActionsCore(IrispTypeR messageInterchange, IncomingCustomsMessageProcessData processData)
	{
		foreach (var entryAdapter in processData.EntryAdapters)
		{
			var lastSentMessage = entryAdapter.GetLastSuccessfullySentMessage();
			var mainSadMessageSubProcessor = GetMainSadMessageSubProcessor(entryAdapter);

			var isLastSentMessageNb = lastSentMessage.EM_MessageSubType.StartsWith(SADConstants.MessageSubTypes.NB);
			var messageNumber = lastSentMessage.EM_MessageNum;

			AddClonedMessageTextStrategy(messageInterchange, processData, entryAdapter, isLastSentMessageNb, messageNumber);

			if (!isLastSentMessageNb)
			{
				var responseMessagesWithSameMessageNo = GetRelatedResponseMessagesFromIncomingInterchange(messageInterchange, messageNumber);
				PerformTypeSpecificActionsForSadMessage(entryAdapter, mainSadMessageSubProcessor, responseMessagesWithSameMessageNo);
			}

			PerformTypeSpecificActionsForNbMessages(processData.SentInterchange, entryAdapter, messageInterchange.ResponseMessages);

			mainSadMessageSubProcessor.UpdateEntryStatusAfterChildMessagesProcessing();
		}
	}

	#region Implementation

	void PerformTypeSpecificActionsForSadMessage(ISadCustomsLinkedObjectAdapter entryAdapter, ISadImportExportMessageSubProcessor sadMessageSubProcessor, IEnumerable<UnifiedDeclarationResponseMessage> responseMessagesWithSameMessageNo)
	{
		var mainResponseMessage = GetMainSadResponseMessage(entryAdapter, responseMessagesWithSameMessageNo);
		if (mainResponseMessage != null)
		{
			if (mainResponseMessage is SadPositiveResponseMessage positiveResponseMessage)
			{
				sadMessageSubProcessor.PerformActionsForPositiveIrisp(positiveResponseMessage);
			}
			else if (mainResponseMessage is SadNegativeResponseMessage negativeResponseMessage)
			{
				sadMessageSubProcessor.PerformActionsForNegativeIrisp(negativeResponseMessage);
			}
		}
	}

	void PerformTypeSpecificActionsForNbMessages(EDIInterchange sentInterchange, ISadCustomsLinkedObjectAdapter entryAdapter, IEnumerable<UnifiedDeclarationResponseMessage> responseMessagesWithSameMessageNo)
	{
		if (entryAdapter.IsEntryRegisteredOrNbRejected)
		{
			var nbResponseMessages = responseMessagesWithSameMessageNo.Where(msg => msg.MessageCode == SADConstants.MessageSubTypes.NB);
			if (nbResponseMessages.Any())
			{
				var nbMessageSubProcessor = new SadNbIrispMessageSubProcessor(entryAdapter, sentInterchange);
				nbMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages);
			}
		}
	}

	UnifiedDeclarationResponseMessage GetMainSadResponseMessage(ISadCustomsLinkedObjectAdapter entryAdapter, IEnumerable<UnifiedDeclarationResponseMessage> responseMessagesWithSameMessageNo)
	{
		var mainMessageCode = string.Empty;
		if (entryAdapter.IsImport)
		{
			mainMessageCode = SADConstants.MessageSubTypes.IM;
		}
		else if (entryAdapter.IsExport)
		{
			mainMessageCode = SADConstants.MessageSubTypes.ET;
		}
		else
		{
			mainMessageCode = string.Empty;
		}
		return responseMessagesWithSameMessageNo.FirstOrDefault(x => x.MessageCode == mainMessageCode);
	}

	ISadImportExportMessageSubProcessor GetMainSadMessageSubProcessor(ISadCustomsLinkedObjectAdapter entryAdapter)
	{
		if (entryAdapter.IsImport)
		{
			return new SadImIrispMessageSubProcessor(entryAdapter);
		}
		else if (entryAdapter.IsExport)
		{
			return new SadEtIrispMessageSubProcessor(entryAdapter);
		}

		throw new CustomsMessageProcessorException(Res.GetString("6E4017B0-1DD6-4D3C-8317-BAA253807FB7", "Unable to identify the IRISP processor for entry '{0}'", entryAdapter.EntryReferenceNumber));
	}

	IEnumerable<UnifiedDeclarationResponseMessage> GetRelatedResponseMessagesFromIncomingInterchange(IrispTypeR incomingInterchange, string messageNumber)
	{
		var responseMessages = incomingInterchange.ResponseMessages.Where(msg => msg.DeclarationNumber == messageNumber);
		if (!responseMessages.Any())
		{
			throw new IncomingMessageDoesNotMatchWithIdocException(Res.GetString("BF24885A-CFC9-438B-8646-67D86F22F6D8", "Incoming Customs Message (IRISP) does not contain the Message {0}", messageNumber));
		}
		return responseMessages;
	}

	void AddClonedMessageTextStrategy(IrispTypeR messageInterchange, IncomingCustomsMessageProcessData processData, ISadCustomsLinkedObjectAdapter entryAdapter, bool isLastSentMessageNb, ZString messageNumber)
	{
		var actualMessageNumber = isLastSentMessageNb ? ZString.Empty : messageNumber;
		var irispMessageTextStrategy = new IrispMessageTextStrategy(messageInterchange, actualMessageNumber, processData.ReceivedMessage);

		processData.AddClonedMessageTextStrategy(entryAdapter, irispMessageTextStrategy);
	}

	#endregion

	class IrispMessageTextStrategy : IGetClonedMessageTextStrategy
	{
		public IrispMessageTextStrategy(IrispTypeR messageInterchange, ZString messageNumber, EDIMessage receivedMessage)
		{
			this.messageInterchange = messageInterchange;
			this.receivedMessage = receivedMessage;
			this.messageNumber = messageNumber;
		}
		readonly IrispTypeR messageInterchange;
		readonly ZString messageNumber;
		readonly EDIMessage receivedMessage;

		public string GetClonedMessageText() => messageNumber.IsEmpty ? receivedMessage.EM_MessageText : messageInterchange.Aggregator.GetTextForMessage(messageNumber);
	}
}
