using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLBranchCustomsApplicationTypeMessageProcessorBaseOnlyTest : TestCaseWithFactory
{
	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCodeList.Codes.NLCustoms, messageProcessor.ApplicationCode);
	}

	public void TestPreProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var message = mockMessage.Object;
		Factory.Save();

		var dataProviderMock = new Mock<IDMSIncomingDataProvider>();
		messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
		messageProcessor.ParentJobForTest = entryHeader;

		messageProcessor.PreProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.PreProcessedOK, message.EM_Status);
			AssertNotNull("EM_LinkedObject", message.EM_LinkedObject);
		});
	}

	public void TestPreProcessMessage_DataProviderIsNull()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var message = mockMessage.Object;
		Factory.Save();

		messageProcessor.PreProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tFailed to convert message content"));
			AssertEquals("Note found", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_NoteText == NLConstants.Notes.Texts.FailedToDeserialize));
		});
		ErrorReporter.Clear();
	}

	public void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var message = mockMessage.Object;
		message.EM_LinkedObject = entryHeader;
		Factory.Save();

		var dataProviderMock = new Mock<IDMSIncomingDataProvider>();
		messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

		messageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_MessageInterpretation", ZString.Empty, message.EM_MessageInterpretation);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		logger = new LoggingInformation();
		messageProcessor = new NLBranchCustomsApplicationTypeMessageProcessorForTest(logger);
	}
	LoggingInformation logger;
	NLBranchCustomsApplicationTypeMessageProcessorForTest messageProcessor;
}

sealed class NLBranchCustomsApplicationTypeMessageProcessorForTest : NLBranchCustomsApplicationTypeMessageProcessor<IDMSIncomingDataProvider>
{
	public NLBranchCustomsApplicationTypeMessageProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => throw new NotImplementedException();

	protected override ZBool LinkMessageToParentJob(EDIMessage message)
	{
		var result = false;
		if (ParentJobForTest != null)
		{
			message.EM_LinkedObject = ParentJobForTest;
			result = true;
		}
		return result;
	}

	protected override void ProcessMessage(NLEDIMessage message)
	{
		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
	}

	protected override BusinessObject FindParentOfMessage(EDIMessage message)
	{
		throw new NotImplementedException();
	}

	protected override ZString InterpretMessage(EDIMessage message)
	{
		throw new NotImplementedException();
	}

	protected override IDMSIncomingDataProvider GetMessageDataProvider(EDIMessage message) => mockedIncomingDataProvider;
	IDMSIncomingDataProvider mockedIncomingDataProvider;

	public void SetMockedMessageDataProvider(IDMSIncomingDataProvider provider)
	{
		mockedIncomingDataProvider = provider;
	}

	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => DMSResponseMessageHelper.GetBranchPkFromJobBO(linkedObject);

	internal BusinessObject ParentJobForTest { get; set; }
}
