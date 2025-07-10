using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC599CMessageProcessor))]
sealed class CC599CMessageProcessorTest : MessageProcessorTestCase<CC599CMessageProcessor, ICC599CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC599C }, processor.MessageTypesToInclude);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC599C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC599CMessageInterpreter);

	protected override CC599CMessageProcessor Processor => processor;

	#region TestValidateCC599CMessageDeclaration

	public void TestPreProcessCC599C_NonMatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
	}

	public void TestPreProcessCC599C_MatchingLRN_BadBusinessObject()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045991480600000001");
		entry.CH_EntryStatus = StatusCodes.DeclarationCancelled;
		AssertPreProcess(EDIMessageStatusList.Codes.Discarded);
	}

	public void TestPreProcessCC599C_MatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045991480600000001");
		entry.CH_EntryStatus = StatusCodes.ReleasedForExport;
		AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
	}

	public void TestPreProcessCC599C_MatchingMRN()
	{
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		Factory.Save();
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("22045281480600000002");
		entry.CH_EntryStatus = StatusCodes.ReleasedForExport;
		AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
	}

	void AssertPreProcess(string expectedStatus)
	{
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("EDIMEssage Status Should BE " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
	}

	#endregion

	public void TestProcessMessage_Processed()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045991480600000001");
		entry.CH_Status = StatusCodes.Rejected;
		entry.CH_EntryStatus = StatusCodes.ReleasedForExport;
		Factory.Save();
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status Should be EXT", StatusCodes.GoodsExitedEU, entry.CH_EntryStatus);
		});
	}

	public void TestProcessMessage_Discarded()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045991480600000001");
		entry.CH_Status = StatusCodes.Rejected;
		entry.CH_EntryStatus = StatusCodes.RejectedNonExitExport;
		Factory.Save();
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status Should be RJN", StatusCodes.RejectedNonExitExport, entry.CH_EntryStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC599CDataProvider>();
		mockProvider.CallBase = true;
		var mockProcessor = new Mock<CC599CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045991480600000001";
		Factory.Save();
	}

	Mock<ICC599CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC599CMessageProcessor processor;
	CusEntryHeader entry;
}
