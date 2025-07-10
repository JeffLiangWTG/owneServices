using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(DeclarationMessageProcessorForTest))]
sealed class DeclarationMessageProcessorBaseOnlyTest : MessageProcessorTestCase<DeclarationMessageProcessorForTest, IInboundProvider>
{
	public void TestNoteForUnableToFindALinkedBusinessObject()
	{
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
	}

	public void TestGetBranchPkFromJobBO()
	{
		var entry = Factory.New<CusEntryHeader>();
		processor.Entry = entry;
		processor.PreProcessMessage(incomingMessage);
		AssertEquals(entry.RegistryBranchPK, incomingMessage.EM_GB);
	}

	public void TestProcessMessageCore_Specific()
	{
		var entry = Factory.New<CusEntryHeader>();
		processor.Entry = entry;
		processor.JobAndMessageStatus = ("CES", "REJ", "Unknown");
		processor.MessageInterpreterTypeExposed = typeof(MessageInterpreterForTest);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", "CES", entry.CH_EntryStatus);
			AssertEquals("EM_Status", "REJ", incomingMessage.EM_Status);
			AssertEquals("ProcessingLog", "Unknown", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
			AssertEquals("EM_MessageInterpretation", "Interpret", incomingMessage.EM_MessageInterpretation);
		});
	}

	public void TestProcessMessageCore_Default()
	{
		var entry = Factory.New<CusEntryHeader>();
		processor.Entry = entry;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
			AssertEquals("ProcessingLog", 0, incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Count(x => x.ST_IsCustomDescription));
			AssertEquals("EM_MessageInterpretation", ZString.Empty, incomingMessage.EM_MessageInterpretation);
		});
	}

	protected override string ExpectedMessageFriendlyName => "AES Message Processing Base";

	protected override Type ExpectedMessageInterpreterType => null;

	protected override DeclarationMessageProcessorForTest Processor => processor;

	protected override void SetUp()
	{
		base.SetUp();
		processor = new DeclarationMessageProcessorForTest(new LoggingInformation());
		incomingMessage = CreateIncomingMessage(Factory);
	}

	DeclarationMessageProcessorForTest processor;
	BEMessage incomingMessage;
}

class DeclarationMessageProcessorForTest : DeclarationMessageProcessor<IInboundProvider>
{
	public DeclarationMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => "AES Message Processing Base";

	public CusEntryHeader Entry { get; set; }

	public (ZString jobStatus, ZString messageStatus, ZString processLog) JobAndMessageStatus { get; set; }

	public Type MessageInterpreterTypeExposed { get; set; }

	protected internal override IInboundProvider GetMessageDataProvider(BEMessage message) => new Mock<IInboundProvider>().Object;

	protected override BusinessObject FindParentOfMessage(BEMessage message, IInboundProvider messageDataProvider) => Entry;

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, IInboundProvider messageDataProvider) => JobAndMessageStatus;

	protected override Type MessageInterpreterType => MessageInterpreterTypeExposed;
}
