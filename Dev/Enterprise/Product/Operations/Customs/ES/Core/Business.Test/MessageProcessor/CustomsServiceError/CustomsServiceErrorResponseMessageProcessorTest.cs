using CargoWise.Customs.ES.MessageDefinitions.Xhub.Products.Customs;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CustomsServiceErrorResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<CustomsServiceErrorResponseMessageProcessor, BusinessObject, CommonCustomsServiceError>
	{
		public void TestProcessServiceErrorMessageForCusEntryHeader()
		{
			var entryHeader = SetUpEntryHeader();
			Factory.Save();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetServiceErrorMessage(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>400</td><td>Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.</td></tr>" +
						"</table>";

			GenericCommonAssertProcessResponseEntryHeader(message, entryHeader: entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, chStatus: EDIMessage.Status.Failed);
		}

		public void TestProcessServiceErrorMessageForNctsHeader()
		{
			var nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: ApplicationReference));

			SetSentInterchange(nctsHeader, InterchangeID);

			var message = CreateNewEDIMessage(ApplicationReference, GetServiceErrorMessage(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>400</td><td>Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.</td></tr>" +
						"</table>";

			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		public void TestProcessServiceErrorMessageForCusExitDetail()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = ApplicationReference;

			SetSentInterchange(exitDetail, InterchangeID);

			var message = CreateNewEDIMessage(ApplicationReference, GetServiceErrorMessage(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>400</td><td>Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.</td></tr>" +
						"</table>";

			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		public void TestProcessServiceErrorMessageForCusExitReport()
		{
			var report = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(mrn: ApplicationReference));

			SetSentInterchange(report, InterchangeID);

			var message = CreateNewEDIMessage(ApplicationReference, GetServiceErrorMessage(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H3>Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>400</td><td>Unknown service/operation pair: 'XXXXXX222323323'/'T2LReceptionV1'.</td></tr>" +
						"</table>";

			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		public void TestProcessMessageWrongXML()
		{
			var entryHeader = SetUpEntryHeader();
			Factory.Save();
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(message);

			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "Unable to read message text from message");
		}

		public void TestMessageProcessingError()
		{
			SetUpEntryHeader();
			var message = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "Unable to find business object for message");
		}

		public void TestMessageProcessingErrorNoReference()
		{
			SetUpEntryHeader();
			var message = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(message);
			GenericCommonAssertProcessResponseOthers(message, loggerDesc: "No Application Reference found for message");
		}

		string GetServiceErrorMessage()
		{
			return ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorTestFilePath, "ServiceErrorMessage.txt");
		}

		protected override CustomsServiceErrorResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CustomsServiceErrorResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Customs Service Error Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.CustomsServiceError };

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = entryHeader.CH_BGMReference;
			var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "There is an error in XML document (1, 1).", concatenatedUserLogStrings);
			AssertEquals("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: There is an error in XML document (1, 1).</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		CusEntryHeader SetUpEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			SetSentInterchange(entryHeader, InterchangeID);

			return entryHeader;
		}
	}
}
