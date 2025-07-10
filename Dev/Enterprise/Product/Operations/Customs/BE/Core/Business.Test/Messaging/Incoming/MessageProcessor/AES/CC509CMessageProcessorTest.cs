using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC509CMessageProcessor))]
sealed class CC509CMessageProcessorTest : MessageProcessorTestCase<CC509CMessageProcessor, ICC509CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC509C }, processor.MessageTypesToInclude);
	}

	#region TestValidateCC509CMessageDeclaration

	public void TestStatusForUnableToFindALinkedBusinessObject()
	{
		mockProvider.Setup(x => x.LRN).Returns("1234567890");
		mockProvider.Setup(x => x.MRN).Returns(string.Empty);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals("EDIMEssage Status Should BE FAL", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
	}

	public void TestFindParentOfMessageByLRN()
	{
		entry.CH_EntryStatus = StatusCodes.InvalidationRequest;
		mockProvider.Setup(x => x.MRN).Returns(string.Empty);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	public void TestFindParentOfMessageByMRN()
	{
		entry.CH_EntryStatus = StatusCodes.InvalidationRequest;
		mockProvider.Setup(x => x.LRN).Returns(string.Empty);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	#endregion

	#region TestDiscardMessage

	public void TestDiscardMessageWithHeaderStatusDCA()
	{
		entry.CH_EntryStatus = "DCA";
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("EDIMEssage Status Should BE DCD(=Discarded)", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("Processing Log", "The message with interchange  is discarded, because the Status at Customs is not INR.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		});
	}

	#endregion

	#region TestUpdateHeaderStatus

	public void TestUpdateHeaderStatus()
	{
		entry.CH_EntryStatus = StatusCodes.InvalidationRequest;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should be CAN", StatusCodes.Cancelled, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
			AssertEquals("Processing Log", "The message with interchange  processed successfully.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		});
	}

	#endregion

	public void TestCheckMessageNoteText()
	{
		entry.CH_EntryStatus = "INR";
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("Declaration is invalidated/cancelled on 13-Jan-23 11:54:38<br />Status is set to CAN<br />", incomingMessage.EM_MessageInterpretation);
	}

	protected override string ExpectedMessageFriendlyName => "Export Invalidation Decision";

	protected override Type ExpectedMessageInterpreterType => typeof(CC509CMessageInterpreter);

	protected override CC509CMessageProcessor Processor => processor;

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC509CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
		mockProvider.Setup(m => m.MRN).Returns("22BEE00000000012J1");
		mockProvider.Setup(m => m.InvalidationDecisionDateAndTime).Returns(new DateTime(2023, 1, 13, 11, 54, 38));
		mockProvider.Setup(m => m.InvalidationInitiatedByCustoms).Returns("1");
		mockProvider.Setup(m => m.InvalidationJustification).Returns("The good were already transported with another MRN");
		mockProvider.Setup(m => m.InvalidationRequestDateAndTime).Returns(new DateTime(2023, 1, 13, 11, 59, 30));
		provider = mockProvider.Object;
		mockProcessor = new Mock<CC509CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BEE00000000012J1");
		Factory.Save();
	}

	Mock<ICC509CDataProvider> mockProvider;
	Mock<CC509CMessageProcessor> mockProcessor;
	CC509CMessageProcessor processor;
	ICC509CDataProvider provider;
	BEMessage incomingMessage;
	CusEntryHeader entry;
}
