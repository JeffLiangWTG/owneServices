using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class XmlIncomingMessageProcessorTest<TMessageProcessor> : IncomingCustomsMessageProcessorTest<TMessageProcessor, IXmlCustomsLinkedObjectAdapter>
	where TMessageProcessor : IncomingCustomsMessageProcessor<IXmlCustomsLinkedObjectAdapter>
{
	public void TestApplicationCode()
	{
		var processor = GetMessageProcessor(logger);
		AssertEquals(EDIMessage.ApplicationCodes.ITCustomsXTrade, processor.ApplicationCode);
	}

	public void TestThrowsExceptionIfMultipleEntriesLinked()
	{
		(var declaration, _, var sentMessage, var receivedMessage) = PrepareTestData();

		var secondSentMessageForTheInterchange = sentMessage.Interchange.ContainedMessages.AddNew();

		var secondEntryHeaderForTheInterchange = declaration.CustomsEntryHeaders.AddNew();
		secondEntryHeaderForTheInterchange.Messages.Add(secondSentMessageForTheInterchange);

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "More than one entry header found.", "Multiple entries linked to the same interchange");
	}

	public void TestProcessorMetadata()
	{
		AssertSequencesEqual("MessageTypesToInclude", ExpectedMessageTypesToInclude, processor.MessageTypesToInclude);

		var processorForTesting = new Ucc6IncomingMessageProcessorForTesting(logger);
		Assert("ShouldCloneMessage", !processorForTesting.ShouldCloneMessageExposed);
	}

	protected abstract IReadOnlyList<ZString> ExpectedMessageTypesToInclude { get; }

	protected (JobDeclaration declaration, CusEntryHeader entryHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestData(string declarationType = null, string messageText = null, string messageType = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MessageType = declarationType ?? ZString.Empty;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		sentInterchange.EI_InterchangeType = MessageProcessorConstants.InterchangeTypes.ImportType;
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		sentMessage.EM_MessageNum = "0001";
		sentMessage.EM_ApplicationReference = declaration.JE_MessageType;
		sentInterchange.ContainedMessages.Add(sentMessage);
		entryHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
		var receivedMessage = AddNewMessage(messageText, messageType, receivedInterchange);
		return (declaration, entryHeader, sentMessage, receivedMessage);
	}

	protected EDIMessage AddNewMessage(string messageText, string messageType, EDIInterchange receivedInterchange)
	{
		var receivedMessage = Factory.NewWithValidTestData<ITEDIMessage>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedMessage.EM_MessageText = messageText ?? ZString.Empty;
		receivedMessage.EM_MessageType = messageType ?? ZString.Empty;
		return receivedMessage;
	}

	#region Ucc6IncomingMessageProcessorForTesting

	class Ucc6IncomingMessageProcessorForTesting : XmlIncomingMessageProcessor
	{
		public Ucc6IncomingMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}

		public bool ShouldCloneMessageExposed => ShouldCloneMessage;
		protected override string MessageFriendlyNameCore => throw new NotImplementedException();

		protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
