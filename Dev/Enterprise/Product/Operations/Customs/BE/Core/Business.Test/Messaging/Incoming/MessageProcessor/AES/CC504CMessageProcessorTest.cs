using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC504CMessageProcessor))]
sealed class CC504CMessageProcessorTest : MessageProcessorTestCase<CC504CMessageProcessor, ICC504CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC504C }, processor.MessageTypesToInclude);
	}
	#region TestValidateCC504CMessageDeclaration
	public void TestValidateCC504CMessageDeclaration_MatchingLRN() => TestValidateCC504CMessageDeclaration("1234567890", "");
	public void TestValidateCC504CMessageDeclaration_MatchingMRN() => TestValidateCC504CMessageDeclaration("", "1234567890T");
	void TestValidateCC504CMessageDeclaration(string lrn, string mrn)
	{
		entry.CH_EntryStatus = "ABC";
		mockProvider.Setup(x => x.LRN).Returns(lrn);
		mockProvider.Setup(x => x.MRN).Returns(mrn);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("EDIMEssage Status Should BE FAL", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
	}
	#endregion
	#region TestDiscardMessage
	public void TestDiscardMessageWithHeaderStatusNotValid()
	{
		entry.CH_EntryStatus = "DCA";
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EDIMEssage Status Should be DCD(=Discarded)", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("Processing Log", "The message with interchange  is discarded, because the Status at Customs is not AMR.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		});
	}
	#endregion
	#region TestUpdateHeaderStatus
	public void TestUpdateHeaderStatusWithMRN()
	{
		entry.CH_EntryStatus = "AMR";
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE AMD", StatusCodes.Amended, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	public void TestUpdateHeaderStatusWithoutMRN()
	{
		entry.CH_EntryStatus = "AMR";
		entry.MovementReferenceNumberSetter(null);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE AMD ", StatusCodes.Amended, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}
	#endregion
	public void TestCheckMessageNoteText()
	{
		entry.CH_EntryStatus = "AMR";
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("Declaration is amended on 16/01/2023 4:44:30 PM<br />Status is set to AMD<br />", incomingMessage.EM_MessageInterpretation);
	}
	protected override string ExpectedMessageFriendlyName => "Export Declaration Amendment Acceptance";
	protected override Type ExpectedMessageInterpreterType => typeof(CC504CMessageInterpreter);
	protected override CC504CMessageProcessor Processor => processor;
	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC504CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
		mockProvider.Setup(m => m.MRN).Returns("22BEE00000000012J1");
		mockProvider.Setup(m => m.AmendmentDateAndTime).Returns(new DateTime(2023, 1, 16, 16, 42, 28));
		mockProvider.Setup(m => m.AmendmentAcceptanceDateAndTime).Returns(new DateTime(2023, 1, 16, 16, 44, 30));
		provider = mockProvider.Object;
		mockProcessor = new Mock<CC504CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BEE00000000012J1");
		Factory.Save();
	}
	Mock<ICC504CDataProvider> mockProvider;
	Mock<CC504CMessageProcessor> mockProcessor;
	CC504CMessageProcessor processor;
	ICC504CDataProvider provider;
	BEMessage incomingMessage;
	CusEntryHeader entry;
}
