using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DMSResponseMessageProcessorBaseOnlyTest : TestCaseWithFactory
{
	public void TestMessageFriendlyName()
	{
		AssertEquals("DMS Response", messageProcessor.MessageFriendlyName);
	}

	public void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("TestReferenceABC");

		messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

		var message = mockMessage.Object;
		messageProcessor.PreProcessMessage(message);
		messageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("CH_EntryStatus", "XXX", entryHeader.CH_EntryStatus);
			AssertEquals("EM_MessageInterpretation", "YYY", message.EM_MessageInterpretation);
			AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tMessage linked to entry header"));
		});
	}

	public void TestProcessMessage_CorrectBranchAndCompany()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("TestReferenceABC");

		messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

		var company = Factory.New<GlbCompany>();
		company.GC_Code = "NEW";
		company.GC_RN_NKCountryCode = "AU";
		var newBranch = company.Branches.AddNew();
		newBranch.GB_Code = "NEW";
		var message = mockMessage.Object;
		message.EM_GB = newBranch.PK;

		messageProcessor.PreProcessMessage(message);
		messageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("CH_EntryStatus", "XXX", entryHeader.CH_EntryStatus);
			AssertEquals("EM_MessageInterpretation", "YYY", message.EM_MessageInterpretation);
			AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tMessage linked to entry header"));
			AssertEquals("Company And Branch have been corrected", entryHeader.RegistryBranchPK, message.EM_GB);
		});
	}

	public void TestProcessMessage_CusEntryHeaderIsNull()
	{
		var mockMessage = Factory.NewMoq<NLEDIMessage>();
		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.Declaration).Returns(Mock.Of<IDMSDeclaration>(d => d.FunctionalReference == "LRNGiven" && d.Id == "MRNGiven"));
		messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

		var message = mockMessage.Object;
		messageProcessor.PreProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("Log found (both)", true, logger.UserLogStrings.Contains("\tEntry header not found with reference LRN 'LRNGiven' or MRN 'MRNGiven'."));
			AssertEquals("Note found (both)", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_Description == NLConstants.Notes.Descriptions.DataImportLogText && n.ST_NoteText == "Entry header not found with reference LRN 'LRNGiven' or MRN 'MRNGiven'."));

			dataProviderMock.Setup(x => x.Declaration).Returns(Mock.Of<IDMSDeclaration>(d => d.FunctionalReference == "LRNGiven"));
			messageProcessor.PreProcessMessage(message);
			AssertEquals("Log found (lrn)", true, logger.UserLogStrings.Contains("\tEntry header not found with reference LRN 'LRNGiven'."));
			AssertEquals("Note found (lrn)", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_Description == NLConstants.Notes.Descriptions.DataImportLogText && n.ST_NoteText == "Entry header not found with reference LRN 'LRNGiven'."));

			dataProviderMock.Setup(x => x.Declaration).Returns(Mock.Of<IDMSDeclaration>(d => d.Id == "MRNGiven"));
			messageProcessor.PreProcessMessage(message);
			AssertEquals("Log found (mrn)", true, logger.UserLogStrings.Contains("\tEntry header not found with reference MRN 'MRNGiven'."));
			AssertEquals("Note found (mrn)", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_Description == NLConstants.Notes.Descriptions.DataImportLogText && n.ST_NoteText == "Entry header not found with reference MRN 'MRNGiven'."));

			dataProviderMock.Setup(x => x.Declaration).Returns(Mock.Of<IDMSDeclaration>());
			messageProcessor.PreProcessMessage(message);
			AssertEquals("Log found (neither)", true, logger.UserLogStrings.Contains("\tEntry header not found. LRN and MRN not found in message."));
			AssertEquals("Note found (neither)", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_Description == NLConstants.Notes.Descriptions.DataImportLogText && n.ST_NoteText == "Entry header not found. LRN and MRN not found in message."));
		});
	}

	public void TestFindParentOfMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_BGMReference = "LRN123";
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var outgoingEdiMessage = Factory.New<NLEDIMessage>();
		outgoingEdiMessage.EM_Status = EDIMessage.Status.Sent;
		outgoingEdiMessage.EM_LinkedObject = entryHeader;
		outgoingEdiMessage.EM_MessageNum = "0000000010";
		var outgoingInterchange = Factory.New<EDIInterchange>();
		outgoingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
		outgoingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
		outgoingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
		outgoingInterchange.EI_From = "UT@CW";
		outgoingInterchange.EI_To = "NL.DMS";
		outgoingEdiMessage.EM_EI = outgoingInterchange.PK;
		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
		incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
		incomingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		incomingInterchange.EI_Status = EDIInterchange.Status.Sent;
		incomingInterchange.EI_From = "NL.DMS";
		incomingInterchange.EI_To = "UT@CW";

		var testMessage = Factory.New<NLEDIMessage>();
		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		Factory.Save();

		CombineAssertions(() =>
		{
			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("LRN123");
			dataProviderMock.Setup(x => x.Declaration.Id).Returns(string.Empty);
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			var result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on LRN", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns(string.Empty);
			dataProviderMock.Setup(x => x.Declaration.Id).Returns("MRN123");
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on MRN", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("InvalidLRN");
			dataProviderMock.Setup(x => x.Declaration.Id).Returns("MRN123");
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on LRN, fallback on MRN", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns(string.Empty);
			dataProviderMock.Setup(x => x.Declaration.Id).Returns(string.Empty);
			testMessage.EM_EI = incomingInterchange.PK;
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on SessionID", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("InvalidLRN");
			dataProviderMock.Setup(x => x.Declaration.Id).Returns(string.Empty);
			testMessage.EM_EI = incomingInterchange.PK;
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on LRN, fallback on SessionID", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns(string.Empty);
			dataProviderMock.Setup(x => x.Declaration.Id).Returns("InvalidMRN");
			testMessage.EM_EI = incomingInterchange.PK;
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on MRN, fallback on SessionID", entryHeader.PK, result.PK);

			dataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("InvalidLRN");
			dataProviderMock.Setup(x => x.Declaration.Id).Returns("InvalidMRN");
			testMessage.EM_EI = incomingInterchange.PK;
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
			result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
			AssertEquals("FindParentOfMessage based on LRN, fallback on MRN, fallback on SessionID", entryHeader.PK, result.PK);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		logger = new LoggingInformation();
		messageProcessor = new DMSResponseMessageProcessorForTest(logger);
	}
	LoggingInformation logger;
	DMSResponseMessageProcessorForTest messageProcessor;

	sealed class DMSResponseMessageProcessorForTest : DMSResponseMessageProcessor
	{
		public DMSResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
		{
			entryHeader.CH_EntryStatus = "XXX";
		}

		protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => "YYY";

		protected override IIncomingDataProvider GetMessageDataProvider(EDIMessage message) => mockedIncomingDataProvider ?? base.GetMessageDataProvider(message);
		IIncomingDataProvider mockedIncomingDataProvider;

		public void SetMockedMessageDataProvider(IIncomingDataProvider provider)
		{
			mockedIncomingDataProvider = provider;
		}

		public BusinessObject FindParentOfMessageExposed(EDIMessage message) => base.FindParentOfMessage(message);
	}
}
