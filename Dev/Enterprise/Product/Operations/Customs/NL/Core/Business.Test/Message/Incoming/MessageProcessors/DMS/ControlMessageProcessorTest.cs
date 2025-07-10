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

namespace Enterprise.Customs.NL.Business.Testing
{
	sealed class ControlMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "TestReferenceABC";
			var mockMessage = Factory.NewMoq<NLEDIMessage>();
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyControlIncomingDataProvider();
			dataProviderMock.Setup(x => x.Response.FunctionalReferenceId).Returns("TestReferenceABC");

			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

			var message = mockMessage.Object;
			messageProcessor.PreProcessMessage(message);
			messageProcessor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EM_MessageInterpretation", "YYY", message.EM_MessageInterpretation);
				AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tMessage linked to entry header"));
			});
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("Control Message", messageProcessor.MessageFriendlyName);
		}

		public void TestFindParentOfMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_BGMReference = "LRN123";

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
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyControlIncomingDataProvider();
			Factory.Save();

			CombineAssertions(() =>
			{
				dataProviderMock.Setup(x => x.Response.FunctionalReferenceId).Returns("LRN123");
				messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
				var result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
				AssertEquals("FindParentOfMessage based on LRN", entryHeader.PK, result.PK);

				dataProviderMock.Setup(x => x.Response.FunctionalReferenceId).Returns(string.Empty);
				testMessage.EM_EI = incomingInterchange.PK;
				messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
				result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
				AssertEquals("FindParentOfMessage based on SessionID", entryHeader.PK, result.PK);

				dataProviderMock.Setup(x => x.Response.FunctionalReferenceId).Returns("InvalidLRN");
				testMessage.EM_EI = incomingInterchange.PK;
				messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);
				result = (CusEntryHeader)messageProcessor.FindParentOfMessageExposed(testMessage);
				AssertEquals("FindParentOfMessage based on LRN, fallback on SessionID", entryHeader.PK, result.PK);
			});
		}

		public void TestFindParentOfMessage_CusEntryHeaderIsNull()
		{
			var mockMessage = Factory.NewMoq<NLEDIMessage>();
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyControlIncomingDataProvider();
			messageProcessor.SetMockedMessageDataProvider(dataProviderMock.Object);

			var message = mockMessage.Object;
			messageProcessor.PreProcessMessage(message);
			messageProcessor.ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
				AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tEntry header not found. LRN and MRN not found in message."));
				AssertEquals("Note found", true, message.Notes.GetAllNotes().Cast<StmNote>().Any(n => n.ST_Description == NLConstants.Notes.Descriptions.DataImportLogText
																								   && n.ST_NoteText == "Entry header not found. LRN and MRN not found in message."));
			});
		}

		public void TestProcessMessage_CorrectBranchAndCompany()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "TestReferenceABC";
			var mockMessage = Factory.NewMoq<NLEDIMessage>();
			var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyControlIncomingDataProvider();
			dataProviderMock.Setup(x => x.Response.FunctionalReferenceId).Returns("TestReferenceABC");

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
				AssertEquals("EM_MessageInterpretation", "YYY", message.EM_MessageInterpretation);
				AssertEquals("Log found", true, logger.UserLogStrings.Contains("\tMessage linked to entry header"));
				AssertEquals("Company And Branch have been corrected", entryHeader.RegistryBranchPK, message.EM_GB);
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
			messageProcessor = new ControlMessageProcessorForTest(logger);
		}
		LoggingInformation logger;
		ControlMessageProcessorForTest messageProcessor;

		sealed class ControlMessageProcessorForTest : ControlMessageProcessor
		{
			public ControlMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			protected override ZString InterpretMessage(EDIMessage message, IControlIncomingDataProvider dataProvider) => "YYY";

			protected override IIncomingDataProvider GetMessageDataProvider(EDIMessage message) => mockedIncomingDataProvider;
			IControlIncomingDataProvider mockedIncomingDataProvider;

			public void SetMockedMessageDataProvider(IControlIncomingDataProvider provider)
			{
				mockedIncomingDataProvider = provider;
			}

			public BusinessObject FindParentOfMessageExposed(EDIMessage message) => base.FindParentOfMessage(message);
		}
	}
}
