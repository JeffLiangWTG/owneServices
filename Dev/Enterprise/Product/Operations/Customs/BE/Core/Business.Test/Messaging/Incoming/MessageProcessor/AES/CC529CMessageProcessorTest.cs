using System;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC529CMessageProcessor))]
sealed class CC529CMessageProcessorTest : MessageProcessorTestCase<CC529CMessageProcessor, ICC529CDataProvider>
{
	public void TestCheckMessageSequenceIsValid() => CombineAssertions(() =>
	{
		var delayStatuses = typeof(StatusCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(p => (string)p.GetValue(null)).ToList();
		delayStatuses.Remove("MRN");
		delayStatuses.Remove("CTL");
		var message = (BEMessage)incomingMessage;
		mockProvider.Setup(x => x.MRN).Returns("123");

		foreach (var delayStatus in delayStatuses)
		{
			message.EM_RetryCount = 0;
			entry.CH_EntryStatus = delayStatus;
			processor.PreProcessMessage(incomingMessage);
			AssertEquals($"Entry Status '{delayStatus}'", false, CheckMessageSequenceIsValid(message, logger));
		}

		entry.CH_EntryStatus = "MRN";
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("Entry Status MRN", true, CheckMessageSequenceIsValid(message, logger));

		entry.CH_EntryStatus = "CTL";
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("Entry Status CTL", true, CheckMessageSequenceIsValid(message, logger));
	});

	public void TestMessageTypesToInclude()
	{
		AssertEquals(true, incomingMessage.MatchesFilter(processor.MessageFilter));
	}

	public void TestLocateEntryHeaderByLRN()
	{
		mockProvider.Setup(x => x.MRN).Returns("123");
		processor.PreProcessMessage(incomingMessage);
		AssertEquals(entry, incomingMessage.EM_LinkedObject);
	}

	public void TestLocateEntryHeaderFallbackByMRN()
	{
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		Factory.Save();
		mockProvider.Setup(x => x.LRN).Returns("123");
		mockProvider.Setup(x => x.MRN).Returns("22045281480600000002");
		processor.PreProcessMessage(incomingMessage);
		AssertEquals(entry, incomingMessage.EM_LinkedObject);
	}

	public void TestCannotFindLinkedBusinessObject()
	{
		mockProvider.Setup(x => x.LRN).Returns("123");
		processor.PreProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, incomingMessage.EM_Status);
			AssertEquals("Note", "The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		});
	}

	public void TestUpdatedEntryStatus_HasMRN()
	{
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		Factory.Save();
		entry.CH_EntryStatus = StatusCodes.MRNAllocated;

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("CH_EntryStatus updated to REL when MRN is not empty", StatusCodes.ReleasedForExport, entry.CH_EntryStatus);
	}

	public void TestUpdatedEntryStatus_NoMRN()
	{
		entry.CH_EntryStatus = StatusCodes.DeclarationAccepted;

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("CH_EntryStatus is not updated when MRN is empty", StatusCodes.DeclarationAccepted, entry.CH_EntryStatus);
	}

	public void TestUpdatedReleaseDate()
	{
		mockProvider.Setup(x => x.ReleaseDate).Returns(new DateTime(2022, 02, 08, 21, 42, 56));
		entry.CH_EntryStatus = StatusCodes.MRNAllocated;

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("CH_EntryReleaseDate is updated with the ReleaseDate of the message", new DateTime(2022, 02, 08, 21, 42, 56), entry.CH_EntryReleaseDate);
	}

	public void TestUpdatedMessageStatus()
	{
		entry.CH_EntryStatus = StatusCodes.MRNAllocated;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("EM_Status updated to 'ProcessedOK'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	public void TestMessageSequenceInvalidMessage()
	{
		var messageSequenceInvalidMessageProperty = typeof(CC529CMessageProcessor).GetProperty("MessageSequenceInvalidMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertEquals("Entry Status was not MRN or CTL", messageSequenceInvalidMessageProperty.GetValue(Processor));
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC529C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC529CMessageInterpreter);

	protected override CC529CMessageProcessor Processor => processor;

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC529CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		logger = new LoggingInformation();
		var mockProcessor = new Mock<CC529CMessageProcessor>(logger);
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		incomingMessage.EM_MessageType = BEIncomingMessageSubTypes.Codes.CC529C;
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		Factory.Save();
	}
	LoggingInformation logger;
	Mock<ICC529CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC529CMessageProcessor processor;
	CusEntryHeader entry;
}
