using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class IncomingCustomsMessageProcessorTest<TMessageProcessor, TCustomsLinkedObjectAdapter> : TestCaseWithFactory
	where TMessageProcessor : IncomingCustomsMessageProcessor<TCustomsLinkedObjectAdapter>
	where TCustomsLinkedObjectAdapter : ICustomsLinkedObjectAdapter
{
	public void TestReceivedMessageFailingProcessWhenUnableToLocateSentInterchange()
	{
		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		var receivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		receivedMessage.EM_EI = receivedInterchange.PK;
		receivedInterchange.EI_HeaderText = "";
		Factory.Save();

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedInterchange, logText: "Unable to locate the related sent interchange with Session ID = <empty>.", assertionMessage: "When Interchange Header Text is empty");

		var trackingId = "cfc0525d-491e-46bc-89d2-463dba237d42";
		receivedInterchange.EI_HeaderText = $"<eHubTrackingIDFromSentInterchange>{trackingId}</eHubTrackingIDFromSentInterchange>";
		receivedInterchange.EI_SessionGUID = new ZGuid(trackingId);
		receivedInterchange.Notes.RemoveAndDeleteAll();
		Factory.Save();

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedInterchange, logText: $"Unable to locate the related sent interchange with Session ID = {trackingId}.", assertionMessage: "When Interchange Header Text does not contain a valid eHubTrackingIDFromSentInterchange");
	}

	public void TestReceivedMessageWithNoInterchange()
	{
		var receivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		Factory.Save();

		receivedMessage.EM_EI = ZGuid.Empty;
		processor.ProcessMessage(receivedMessage);

		AssertEquals("Message status", "FAL", receivedMessage.EM_Status);
		AssertLoggerContainsLogText(logText: "Incoming Message is not linked to any Interchange");
	}

	public void TestGetBusinessObjectAdaptersThrowsNotSupportedException()
	{
		var sessionId = ZGuid.NewZGuid();
		var interchangeNum = 1;

		var sentEdiInterchange = Factory.New<ITEDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = interchangeNum.ToString();
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = sessionId;

		var sentMessage = sentEdiInterchange.ContainedMessages.AddNew();
		sentMessage.EM_Status = EDIInterchange.Status.Sent;
		sentMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(interchangeNum.ToString());
		sentMessage.EM_LinkedObject = Factory.New<DummyBusinessObject>();

		var receivedMessage = Factory.New<ITEDIMessage>();
		receivedMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		receivedMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var receivedInterchange = Factory.New<ITEDIInterchange>();
		receivedInterchange.EI_InterchangeNum = (++interchangeNum).ToString();
		receivedInterchange.EI_SessionGUID = new ZGuid(sessionId);
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedInterchange.EI_InterchangeType = "A";
		receivedInterchange.EI_HeaderText = FormattableString.Invariant($@"<ITMessage>	
	<MessageType>A</MessageType>
	<FileName>filenName</FileName>
	<eHubTrackingIDFromSentInterchange>{sessionId}</eHubTrackingIDFromSentInterchange>
</ITMessage>");
		Factory.Save();

		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedInterchange, logText: "'CargoWise.EntityFramework.Testing.DummyBusinessObject' type is not a ICustomsLinkedObjectAdapterProvider", assertionMessage: "When Related Linked Object is not a ICustomsLinkedObjectAdapterProvidery");
	}

	protected override void SetUp()
	{
		base.SetUp();

		logger = new LoggingInformationForTesting();
		processor = GetMessageProcessor(logger);
	}
	protected LoggingInformationForTesting logger;
	protected TMessageProcessor processor;

	protected abstract TMessageProcessor GetMessageProcessor(LoggingInformation logger);

	#region Helper Methods

	protected void AssertFailMessage(EDIMessage receivedEdiMessage, EDIInterchange receivedEdiInterchange, ZString logText, string assertionMessage = null)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("Message status", "FAL", receivedEdiMessage.EM_Status);
			AssertEquals("Interchange status", "FAL", receivedEdiInterchange.EI_Status);
			AssertInterchangeAndLoggerContainsErrorLogText(receivedEdiInterchange, logText);
		});
	}

	protected void AssertInterchangeAndLoggerContainsLogText(EDIInterchange receivedEdiInterchange, ZString logText, ZString logType)
	{
		var notes = receivedEdiInterchange.Notes.GetAllNotes().Cast<StmNote>();
		AssertNotNull(notes);
		AssertEquals(1, notes.Count());
		AssertEquals(logType, notes.ElementAt(0).ST_Description);
		AssertEquals(logText, notes.ElementAt(0).ST_NoteDataAsText);
		AssertLoggerContainsLogText(logText);
	}

	protected void AssertLoggerContainsLogText(ZString logText)
	{
		var logs = logger.AccumulatedLogMessages.ToString().Trim();
		AssertEquals("Logged Info", true, logs.Contains(logText));
	}

	protected void AssertInterchangeAndLoggerContainsErrorLogText(EDIInterchange receivedEdiInterchange, ZString logText) => AssertInterchangeAndLoggerContainsLogText(receivedEdiInterchange, logText, "CargoWiseOne error");
	protected void AssertInterchangeAndLoggerContainsInfoLogText(EDIInterchange receivedEdiInterchange, ZString logText) => AssertInterchangeAndLoggerContainsLogText(receivedEdiInterchange, logText, "CargoWiseOne info");

	protected void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryNum, ZString expectedCategory, ZString expectedEntryLineReference, ZDateTime expectedIssueDate, string expectedEntryStatus = "")
	{
		CombineAssertions("Entry Number", () =>
		{
			AssertEquals("CE_EntryType", expectedEntryType, entryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", expectedEntryNum, entryNumber.CE_EntryNum);
			AssertEquals("CE_Category", expectedCategory, entryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", expectedEntryLineReference, entryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", expectedIssueDate, entryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("CE_EntryStatus", expectedEntryStatus, entryNumber.CE_EntryStatus);
		});
	}

	#endregion
}

sealed class IncomingCustomsMessageProcessorBaseOnlyTest : IncomingCustomsMessageProcessorTest<MockIncomingCustomsMessageProcessor, ICustomsLinkedObjectAdapter>
{
	protected override MockIncomingCustomsMessageProcessor GetMessageProcessor(LoggingInformation logger) => new MockIncomingCustomsMessageProcessor(logger);

	public void TestProcessorAttachReceivedMessage()
	{
		var eHubTrackingIDFromSentInterchange = "A5CF3117-6BAF-40D0-9EAC-536C7143758B";

		(var entryHeader, var receivedInterchange, var receivedMessage) = GetTestData(eHubTrackingIDFromSentInterchange);

		processor.ProcessMessage(receivedMessage);

		entryHeader.Messages.Reload(true);
		AssertRcvStatus(receivedInterchange, receivedMessage);

		CombineAssertions("Check received message has been attached", () =>
		{
			var messages = entryHeader.Messages.Cast<EDIMessage>();
			AssertEquals("Attached Messages count", 2, messages.Count());
			AssertCollectionContains("Received message PK must be contained in entry header messages", receivedMessage.PK, messages.Select(x => x.PK));
			AssertCollectionContains("Received message text must be contained in entry header messages", "THIS IS A TEST MESSAGE", messages.Select(x => x.EM_MessageText));
		});
	}

	public void TestProcessorAttachClonedReceivedMessage()
	{
		var eHubTrackingIDFromSentInterchange = "ABD6E0A9-CA5D-4C82-89C8-ACF777838474";

		(var entryHeader, var receivedInterchange, var receivedMessage) = GetTestData(eHubTrackingIDFromSentInterchange);

		using (processor.EnableMessageCloning())
		{
			processor.ProcessMessage(receivedMessage);
		}

		entryHeader.Messages.Reload(true);
		AssertRcvStatus(receivedInterchange, receivedMessage);

		CombineAssertions("Check received message has been attached", () =>
		{
			var messages = entryHeader.Messages.Cast<EDIMessage>();
			AssertEquals("Attached Messages count", 2, messages.Count());
			AssertCollectionNotContains("Received message PK must be not contained in entry header messages", receivedMessage.PK, messages.Select(x => x.PK));
			AssertCollectionContains("Received message text must be contained in entry header messages", "THIS IS A TEST MESSAGE", messages.Select(x => x.EM_MessageText));
		});
	}

	public void TestProcessorDoNotAttachReceivedMessage()
	{
		var eHubTrackingIDFromSentInterchange = "82835E92-DD76-4802-B556-D618650DBBFE";

		(var entryHeader, var receivedInterchange, var receivedMessage) = GetTestData(eHubTrackingIDFromSentInterchange);

		using (processor.DoNotAttachReceivedMessage())
		{
			processor.ProcessMessage(receivedMessage);
		}

		entryHeader.Messages.Reload(true);
		AssertRcvStatus(receivedInterchange, receivedMessage);

		AssertEquals("Attached Message count", 1, entryHeader.Messages.Count);
	}

	public void TestProcessingFailureIfUnsupportedLinkedBusinessObjectFound()
	{
		processor.GetAdapterAction = (linkedObject) => throw new NotSupportedException();

		var eHubTrackingIDFromSentInterchange = "A5CF3117-6BAF-40D0-9EAC-536C7143758B";
		(var entryHeader, var receivedInterchange, var receivedMessage) = GetTestData(eHubTrackingIDFromSentInterchange);
		processor.ProcessMessage(receivedMessage);

		AssertFailMessage(receivedMessage, receivedInterchange, logText: "Unsupported linked business object found.", assertionMessage: "When an unsupported business object is found, processing failure is expected");
	}

	(CusEntryHeader entryHeader, ITEDIInterchange receivedInterchange, EDIMessage receivedMessage) GetTestData(string eHubTrackingIDFromSentInterchange)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sentMessage = Factory.NewWithValidTestData<EDIMessage>();
		sentMessage.EM_LinkedObject = entryHeader;
		sentMessage.EM_LinkUniqueID = entryHeader.PK;
		sentMessage.EM_LinkTable = entryHeader.TablePrefix;
		sentMessage.EM_Status = "SNT";
		sentMessage.EM_ReceiveTransmit = "TRX";
		sentMessage.EM_MessageType = "R";
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("1");

		var sentInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		sentMessage.EM_EI = sentInterchange.PK;
		sentInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentInterchange.EI_ReceiveTransmit = "TRX";

		var receivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		var receivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		receivedMessage.EM_EI = receivedInterchange.PK;
		receivedMessage.EM_MessageText = "THIS IS A TEST MESSAGE";
		receivedInterchange.EI_HeaderText = $"<eHubTrackingIDFromSentInterchange>{eHubTrackingIDFromSentInterchange}</eHubTrackingIDFromSentInterchange>";
		Factory.Save();

		return (entryHeader, receivedInterchange, receivedMessage);
	}

	void AssertRcvStatus(ITEDIInterchange receivedInterchange, EDIMessage receivedMessage)
	{
		CombineAssertions("Check interchange and message status", () =>
		{
			AssertEquals("Interchange Status", "RCV", receivedInterchange.EI_Status);
			AssertEquals("Message Status", "RCV", receivedMessage.EM_Status);
		});
	}
}

class MockIncomingCustomsMessageProcessor : IncomingCustomsMessageProcessor<ICustomsLinkedObjectAdapter>
{
	public MockIncomingCustomsMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => "Mock Message processor for test";

	protected override bool ShouldCloneMessage => shouldCloneMessageIndex > 0;
	int shouldCloneMessageIndex;

	public IDisposable EnableMessageCloning() => new DisposableAction(() => shouldCloneMessageIndex++, () => shouldCloneMessageIndex--);

	public IDisposable DoNotAttachReceivedMessage() => new DisposableAction(() => doNotAttachReceivedMessageIndex++, () => doNotAttachReceivedMessageIndex--);
	int doNotAttachReceivedMessageIndex;

	protected override void ProcessIncomingCustomsMessage(IncomingCustomsMessageProcessData processData)
	{
		if (doNotAttachReceivedMessageIndex > 0)
		{
			processData.DoNotAttachAllMessagesToEntries();
		}
	}

	public Func<BusinessObject, ICustomsLinkedObjectAdapter> GetAdapterAction { get; set; }

	Func<ICustomsLinkedObjectAdapterProvider, ICustomsLinkedObjectAdapter> DefaultGetAdapterAction => (linkedObject) => new CusEntryHeaderCustomsLinkedObjectAdapter((CusEntryHeader)linkedObject);

	protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ITCustoms;

	protected override ICustomsLinkedObjectAdapter GetAdapter(ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider) => GetAdapterAction?.Invoke(null) ?? DefaultGetAdapterAction?.Invoke(customsLinkedObjectAdapterProvider);
}
