using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC551CMessageProcessor))]
sealed class CC551CMessageProcessorTest : MessageProcessorTestCase<CC551CMessageProcessor, ICC551CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC551C }, processor.MessageTypesToInclude);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC551C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC551CMessageInterpreter);

	protected override CC551CMessageProcessor Processor => processor;

	#region TestValidateCC551CMessageDeclaration

	public void TestPreProcessCC551C_NonMatchingMRN()
	{
		mockProvider.Setup(x => x.MRN).Returns("FAKE");
		AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
	}

	public void TestPreProcessCC551C_MatchingMRN_BadEntryStatus()
	{
		mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
		foreach (var status in new ZString[] { StatusCodes.Rejected, StatusCodes.DecisionToControl, StatusCodes.DeclarationAccepted, StatusCodes.DeclarationCancelled, StatusCodes.DeclarationNotReleased, StatusCodes.DeclarationReleased, StatusCodes.DeclarationAcknowledged, StatusCodes.GoodsExitedEU, StatusCodes.GoodsNotExitedEU, StatusCodes.IntentionToControl, StatusCodes.RejectedAmendment, StatusCodes.RejectedPresentation, StatusCodes.RejectedInvalidation, StatusCodes.RejectedNonExitExport, StatusCodes.ACK, StatusCodes.Presented, StatusCodes.PreLodged })
		{
			entry.CH_EntryStatus = status;
			AssertPreProcess(EDIMessageStatusList.Codes.Discarded);
		}
	}

	public void TestPreProcessCC551C_MatchingMRN_GoodEntryStatus()
	{
		mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
		foreach (var status in new ZString[] { StatusCodes.MRNAllocated, StatusCodes.Control, StatusCodes.ACK })
		{
			entry.CH_EntryStatus = status;
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}
	}

	void AssertPreProcess(string expectedStatus)
	{
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("EDIMEssage Status Should BE " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
	}

	#endregion

	public void TestProcessMessage()
	{
		mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
		entry.CH_Status = StatusCodes.DeclarationAccepted;
		entry.CH_EntryStatus = StatusCodes.MRNAllocated;
		Factory.Save();
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status Should be NRL", StatusCodes.NotReleasedForExport, entry.CH_EntryStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC551CDataProvider>();
		mockProvider.CallBase = true;
		var mockProcessor = new Mock<CC551CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BE000000000012J1");
		Factory.Save();
	}

	Mock<ICC551CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC551CMessageProcessor processor;
	CusEntryHeader entry;
}
