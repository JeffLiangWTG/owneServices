using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SingleWindowIncomingMessageProcessorTest<TMessageProcessor> : IncomingCustomsMessageProcessorTest<TMessageProcessor, ISingleWindowCustomsLinkedObjectAdapter>
	where TMessageProcessor : IncomingCustomsMessageProcessor<ISingleWindowCustomsLinkedObjectAdapter>
{
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
		var processorForTesting = new SingleWindowIncomingMessageProcessorForTesting(logger);
		Assert("ShouldCloneMessage", !processorForTesting.ShouldCloneMessageExposed);
	}

	protected abstract IReadOnlyList<ZString> ExpectedMessageTypesToInclude { get; }

	protected void AssertStatusUpdatedLog(CusEntryHeader entryHeader, ZString expectedType)
	{
		var statusUpdatedLog = entryHeader.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
		AssertNotNull("Log has been created", statusUpdatedLog);
		AssertEquals("Log Reference", $"|SER=CCC|TYP={expectedType}", statusUpdatedLog.SL_Reference);
	}

	protected (JobDeclaration declaration, CusEntryHeader entryHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestData(string declarationType = null, string messageText = null, string messageType = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = declarationType ?? ZString.Empty;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<EDIMessage>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		entryHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		receivedInterchange.EI_HeaderText = $"<eHubTrackingIDFromSentInterchange>{sentInterchange.EI_SessionGUID}</eHubTrackingIDFromSentInterchange>";
		var receivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedMessage.EM_MessageText = messageText ?? ZString.Empty;
		receivedMessage.EM_MessageType = messageType ?? ZString.Empty;
		return (declaration, entryHeader, sentMessage, receivedMessage);
	}

	protected void TestThrowsExceptionIfEmptyXml()
	{
		(_, _, _, var receivedMessage) = PrepareTestData();

		receivedMessage.EM_MessageText = "";
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty EM_MessageText");
	}

	protected void TestThrowsExceptionIfInvalidXml()
	{
		(_, _, _, var receivedMessage) = PrepareTestData();

		receivedMessage.EM_MessageText = "<bad><xml>";
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Invalid XML in EM_MessageText");
	}

	#region SingleWindowIncomingMessageProcessorForTesting

	class SingleWindowIncomingMessageProcessorForTesting : SingleWindowIncomingMessageProcessor<int>
	{
		public SingleWindowIncomingMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}

		public bool ShouldCloneMessageExposed => ShouldCloneMessage;
		protected override string MessageFriendlyNameCore => throw new NotImplementedException();

		protected override int LoadCustomsResponseCore(ZString messageText)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessSingleWindowMessage(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, int customsResponse)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
