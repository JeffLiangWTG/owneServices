using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsDepartureSingleWindowIncomingMessageProcessorTest<TMessageProcessor> : NctsDepartureIncomingCustomsMessageProcessorTest<TMessageProcessor, ISingleWindowCustomsLinkedObjectAdapter>
	where TMessageProcessor : IncomingCustomsMessageProcessor<ISingleWindowCustomsLinkedObjectAdapter>
{
	public void TestProcessorMetadata()
	{
		AssertSequencesEqual("MessageTypesToInclude", ExpectedMessageTypesToInclude, processor.MessageTypesToInclude);
		var processorForTesting = new NctsDepartureSingleWindowIncomingMessageProcessorForTesting(logger);
		Assert("ShouldCloneMessage", !processorForTesting.ShouldCloneMessageExposed);
	}

	protected abstract IReadOnlyList<ZString> ExpectedMessageTypesToInclude { get; }

	protected void AssertStatusUpdatedLog(NctsHeader nctsHeader, ZString expectedType)
	{
		var statusUpdatedLog = nctsHeader.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
		AssertNotNull("Log has been created", statusUpdatedLog);
		AssertEquals("Log Reference", $"|SER=CCC|TYP={expectedType}", statusUpdatedLog.SL_Reference);
	}

	protected (NctsHeader nctsHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestData(string messageText = null, NctsHeader nctsHeader = null)
	{
		if (nctsHeader == null)
		{
			nctsHeader = Factory.NewDepartureNctsHeader();
		}

		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		sentInterchange.IsTransmitInterchange = true;
		var sentMessage = Factory.NewWithValidTestData<EDIMessage>();
		sentMessage.EM_LinkedObject = nctsHeader;
		sentMessage.EM_LinkUniqueID = nctsHeader.PK;
		sentMessage.EM_LinkTable = nctsHeader.TablePrefix;
		sentInterchange.ContainedMessages.Add(sentMessage);
		nctsHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		receivedInterchange.EI_HeaderText = $"<eHubTrackingIDFromSentInterchange>{sentInterchange.EI_SessionGUID}</eHubTrackingIDFromSentInterchange>";
		var receivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedMessage.EM_MessageText = messageText ?? ZString.Empty;
		return (nctsHeader, sentMessage, receivedMessage);
	}

	protected void TestThrowsExceptionIfEmptyXml()
	{
		(_, _, var receivedMessage) = PrepareTestData();

		receivedMessage.EM_MessageText = "";
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty EM_MessageText");
	}

	protected void TestThrowsExceptionIfInvalidXml()
	{
		(_, _, var receivedMessage) = PrepareTestData();

		receivedMessage.EM_MessageText = "<bad><xml>";
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Invalid XML in EM_MessageText");
	}

	#region SingleWindowIncomingMessageProcessorForTesting

	class NctsDepartureSingleWindowIncomingMessageProcessorForTesting : SingleWindowIncomingMessageProcessor<int>
	{
		public NctsDepartureSingleWindowIncomingMessageProcessorForTesting(LoggingInformation logger) : base(logger)
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
