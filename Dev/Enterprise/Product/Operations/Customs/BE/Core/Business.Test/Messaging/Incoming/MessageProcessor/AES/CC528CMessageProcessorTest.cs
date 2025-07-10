using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC528CMessageProcessor))]
sealed class CC528CMessageProcessorTest : MessageProcessorTestCase<CC528CMessageProcessor, ICC528CDataProvider>
{
	public void TestPreProcessCC528C_MatchingLRN_BadBusinessObject()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		entry.CH_EntryStatus = StatusCodes.DeclarationAccepted;
		AssertPreProcess(EDIMessageStatusList.Codes.Discarded);
	}

	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC528C }, processor.MessageTypesToInclude);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC528C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC528CMessageInterpreter);

	protected override CC528CMessageProcessor Processor => processor;

	#region TestValidateCC528CMessageDeclaration

	public void TestPreProcessCC528C_NonMatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
	}

	public void TestPreProcessCC528C_MatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		foreach (var status in new ZString[] { "", StatusCodes.Rejected, StatusCodes.DeclarationAcknowledged })
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
	public void TestProcessMessage_WithoutMrn()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		entry.CH_Status = StatusCodes.Rejected;
		Factory.Save();
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status should be MRN", StatusCodes.MRNAllocated, entry.CH_EntryStatus);
		});
	}

	public void TestProcessMessage_WithMrn()
	{
		var dateTime = new DateTime(1994, 2, 2, 10, 11, 12);
		mockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(dateTime);
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

		entry.CH_Status = StatusCodes.Rejected;
		Factory.Save();
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		var mrnEntryNumber = CusEntryNumber.Load(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Belgium);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status Should be MRN", StatusCodes.MRNAllocated, entry.CH_EntryStatus);
			AssertEquals("CusEntryHeader MRN number should be filled", "22BE000000000012J1", mrnEntryNumber.CE_EntryNum);
			AssertEquals("CusEntryHeader Issue Date should be filled", dateTime, mrnEntryNumber.CE_IssueDate);
			AssertEquals("Customs Message status", StatusCodes.DeclarationAcknowledged, entry.CH_Status);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC528CDataProvider>();
		mockProvider.CallBase = true;
		logger = new LoggingInformation();
		var mockProcessor = new Mock<CC528CMessageProcessor>(logger);
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		Factory.Save();
	}

	Mock<ICC528CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC528CMessageProcessor processor;
	CusEntryHeader entry;
	LoggingInformation logger;
}
