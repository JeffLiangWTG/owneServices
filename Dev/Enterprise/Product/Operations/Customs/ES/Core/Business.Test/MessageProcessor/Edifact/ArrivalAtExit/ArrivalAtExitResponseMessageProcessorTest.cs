using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Moq;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ArrivalAtExitResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<ArrivalAtExitResponseMessageProcessor, CusExitDetail, ICUSRESV921ESMessageProvider>
	{
		public void TestGetRelevantBusinessObjectFindQuery_EHub()
		{
			var exitDetail = GetNewCusExitDetail();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(exitDetail, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationReference = MrnCode;
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { exitDetail };

			var messageProcessor = new ArrivalAtExitResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, false);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct exitDetail", exitDetail, foudBusinessObject);
		}

		public void TestGetRelevantBusinessObjectFindQuery_DirectxT()
		{
			var exitDetail = GetNewCusExitDetail();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(exitDetail, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			var message = Factory.New<EDIMessage>();
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { exitDetail };

			var messageProcessor = new ArrivalAtExitResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, true);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct exitDetail", exitDetail, foudBusinessObject);
		}

		public void TestCED_Status()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			CombineAssertions(() =>
			{
				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("CED_Status rejected", EntryStatusCodes.Error, exitDetail.CED_Status);

				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.GreenCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("CED_Status green", EntryStatusCodes.Cleared, exitDetail.CED_Status);

				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.RedCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("CED_Status red", EntryStatusCodes.CustomsDeclarationAccepted, exitDetail.CED_Status);

				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.OrangeCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("CED_Status orange", EntryStatusCodes.CustomsDeclarationAccepted, exitDetail.CED_Status);
			});
		}

		public void TestZG_Circuit()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			CombineAssertions(() =>
			{
				exitDetail.ZG_Circuit = ZString.Empty;
				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.GreenCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("ZG_Circuit green", MessageFunctionCodeList.Codes.GreenCircuit, exitDetail.ZG_Circuit);

				exitDetail.ZG_Circuit = ZString.Empty;
				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("ZG_Circuit rejected", ZString.Empty, exitDetail.ZG_Circuit);

				exitDetail.ZG_Circuit = ZString.Empty;
				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.RedCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("ZG_Circuit red", MessageFunctionCodeList.Codes.RedCircuit, exitDetail.ZG_Circuit);

				exitDetail.ZG_Circuit = ZString.Empty;
				processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.OrangeCircuit, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
				processor.ProcessMessage(message);
				AssertEquals("ZG_Circuit orange", MessageFunctionCodeList.Codes.OrangeCircuit, exitDetail.ZG_Circuit);
			});
		}

		public void TestZG_AcceptanceDate()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), ""));
			processor.ProcessMessage(message);
			AssertEquals("ZG_AcceptanceDate", new ZDateTime(2020, 04, 26).Date, exitDetail.ZG_AcceptanceDate);
		}

		public void TestZG_CSVClearance()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "W3HK2AT5T6MFTJ28", new ZDateTime(2020, 04, 26), ""));
			processor.ProcessMessage(message);
			AssertEquals("ZG_CSVClearance", "W3HK2AT5T6MFTJ28", exitDetail.ZG_CSVClearance);
		}

		public void TestCED_ExitDate()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "A2", new ZDateTime(2020, 04, 26), ""));
			processor.ProcessMessage(message);
			AssertEquals("CED_ExitDate", new ZDateTime(2020, 04, 26).Date, exitDetail.CED_ExitDate);
		}

		public void TestEM_Status()
		{
			var message = GetTestEdiMessage();

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "A2", new ZDateTime(2020, 04, 26), ""));
			processor.ProcessMessage(message);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Doc()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.GreenCircuitText, new ZDateTime(2020, 04, 26), CSVClearance, new ZDateTime(2020, 04, 26), ""));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("ZG_CSVClearance", CSVClearance, exitDetail.ZG_CSVClearance);

				exitDetail.Messages.Reload(true);
				var docMessages = exitDetail.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCode + "_E_AEAT_EAL_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Doc_NewCSVClearance()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;
			exitDetail.ZG_CSVClearance = "AAAAAAAAAAAAAAAA";

			var docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_EAL_CLR.pdf", "CLR");
			docManagerInfo.Save();

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.GreenCircuitText, new ZDateTime(2020, 04, 26), CSVClearance, new ZDateTime(2020, 04, 26), ""));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("ZG_CSVClearance", CSVClearance, exitDetail.ZG_CSVClearance);

				docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var expectedeDocNames = new List<ZString>() { MrnCode + "_E_AEAT_EAL_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", expectedeDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				exitDetail.Messages.Reload(true);
				var docMessages = exitDetail.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCode + "_E_AEAT_EAL_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDoc()
		{
			var message = GetTestEdiMessage();
			var exitDetail = (CusExitDetail)message.EM_LinkedObject;

			var docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MrnCode + "_E_AEAT_EAL_CLR.pdf", "CLR");
			docManagerInfo.Save();

			Factory.Save();

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.GreenCircuitText, new ZDateTime(2020, 04, 26), CSVClearance, new ZDateTime(2020, 04, 26), ""));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("ZG_CSVClearance", "W3HK2AT5T6MFTJ28", exitDetail.ZG_CSVClearance);

				docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var expectedeDocNames = new List<ZString>() { MrnCode + "_E_AEAT_EAL_CLR.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", expectedeDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				exitDetail.Messages.Reload(true);
				var docMessages = exitDetail.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestProcessMessageRejected()
		{
			var message = GetTestEdiMessage();
			var error1 = SetUpError("211", "location 1", "error 1");
			var error2 = SetUpError("00338", "location 2", "error 2");
			var error3 = SetUpError("55000", "Other Error.", "Other Description..");

			processor = GetMockedProcessor(GetMockedProvider(MessageFunctionCodeList.Codes.Rejected, new ZDateTime(2020, 04, 26), "", new ZDateTime(2020, 04, 26), "", new List<ErrorMessage> { error1, error2, error3 }));

			processor.ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("EM_MessageSubType", "REJ", message.EM_MessageSubType);

				AssertEquals("EM_MessageInterpretation", "<H3>Rejected Declaration</H3>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Location</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>211</td><td>location 1</td><td>error 1</td></tr>" +
					"<tr><td>00338</td><td>location 2</td><td>error 2</td></tr>" +
					"<tr><td>55000</td><td>Other Error.</td><td>Other Description..</td></tr></table>", message.EM_MessageInterpretation);
			});
		}

		public void TestProcessAcceptedMessageGreenCircuitNotMocked()
		{
			var exitDetail = GetNewCusExitDetail();
			SetSentInterchange(exitDetail, InterchangeID);
			var messageText = ZString.Format("UNH+19100229549129+CUSRES:1:921:UN:ECSR02'BGM+962+00229@1+11'NAD+1+ESA78587268:167:148'DTM+148:2101191002:201'GIS+4:117:148'RFF+ABT:21ES00999910000235'AUT+W3HK2AT5T6MFTJ28'DTM+204:2101191002:201'UNT+9+19100229549129'UNZ+1+19100229549129'");
			var responseMessage = CreateNewEDIMessage(MrnCode, messageText, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_MessageSubType", "ACC", responseMessage.EM_MessageSubType);
				AssertEquals("CED_Status", EntryStatusCodes.Cleared, exitDetail.CED_Status);
				AssertEquals("ZG_Circuit", CircuitCodeList.Codes.GREEN, exitDetail.ZG_Circuit);
				AssertEquals("ZG_AcceptanceDate", new ZDateTime(2021, 01, 19, 10, 02, 00), exitDetail.ZG_AcceptanceDate);
				AssertEquals("ZG_CSVClearance", "W3HK2AT5T6MFTJ28", exitDetail.ZG_CSVClearance);
				AssertEquals("CED_ExitDate", new ZDateTime(2021, 01, 19, 00, 00, 00), exitDetail.CED_ExitDate);
				AssertEquals("EM_MessageInterpretation", "<H3>Accepted Declaration</H3><table border=\"0\"><tr>" +
					"<td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>19-01-2021, 10:02:00</td></tr><tr>" +
					"<td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999910000235</td></tr></table>" +
					"<br><table border=\"0\"><tr>" +
					"<td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<br><table border=\"0\"><tr>" +
					"<td>Clearance:</td><td>&nbsp;&nbsp;</td><td>W3HK2AT5T6MFTJ28</td></tr><tr>" +
					"<td>Date:</td><td>&nbsp;&nbsp;</td><td>19-01-2021, 10:02:00</td></tr>" +
					"</table>", responseMessage.EM_MessageInterpretation);
			});
		}

		public void TestErrorResponse()
		{
			var exitDetail = GetNewCusExitDetail();
			SetSentInterchange(exitDetail, InterchangeID);
			var responseMessage = CreateNewEDIMessage(MrnCode, GetEDIFactErrorMessage(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("EM_MessageInterpretation", "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>50052</td><td>Error Traducción:Segmento (TPL) Mensaje erroneo</td></tr>" +
						"</table>", responseMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessMessageWrongXML()
		{
			var exitDetail = GetNewCusExitDetail();
			SetSentInterchange(exitDetail, InterchangeID);
			var responseMessage = CreateNewEDIMessage(MrnCode, WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertContains("logger", "Unable to read message text from message", logger.UserLogStrings[0]);
			});
		}

		protected override ArrivalAtExitResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ArrivalAtExitResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Export Arrival at Exit Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ArrivalAtExit };

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

		CusExitDetail GetNewCusExitDetail()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = MrnCode;
			return exitDetail;
		}

		TestEdiMessage GetTestEdiMessage()
		{
			var receivedInterchange = CreateTestResponseInterchange(InterchangeID, EDIInterchange.TransportType.eHub);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_MessageType = DeclarationMessageTypeList.Codes.ArrivalAtExit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = "Message text";
			var exitDetail = GetNewCusExitDetail();
			message.EM_LinkedObject = exitDetail;
			message.EM_EI = receivedInterchange.PK;
			return message;
		}

		ICUSRESV921ESMessageProvider GetMockedProvider(ZString messageFunction, ZDateTime admissionDate, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString registrationNumber, List<ErrorMessage> errorList = null)
		{
			var mockProvider = new Mock<ICUSRESV921ESMessageProvider>();
			mockProvider.Setup(m => m.MessageFunction).Returns(messageFunction);
			mockProvider.Setup(m => m.AdmissionDate).Returns(admissionDate);
			mockProvider.Setup(m => m.CSVReleaseCode).Returns(csvReleaseCode);
			mockProvider.Setup(m => m.CSVReleaseCreationDate).Returns(csvReleaseCreationDate);
			mockProvider.Setup(m => m.RegistrationNumber).Returns(registrationNumber);
			mockProvider.Setup(m => m.FreeTextErrors).Returns(errorList);
			return mockProvider.Object;
		}

		ArrivalAtExitResponseMessageProcessor GetMockedProcessor(ICUSRESV921ESMessageProvider messageProvider)
		{
			var arrivalAtExitResponseMessageProcessorMock = new Mock<ArrivalAtExitResponseMessageProcessorForMockTest>(new object[] { logger, new BranchCustomsMessageProcessorForTest() });
			arrivalAtExitResponseMessageProcessorMock.CallBase = true;
			arrivalAtExitResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns(messageProvider);
			return arrivalAtExitResponseMessageProcessorMock.Object;
		}

		ErrorMessage SetUpError(ZString code, ZString location, ZString description)
		{
			return new ErrorMessage
			{
				Code = code,
				Location = location,
				Description = description
			};
		}

		string GetEDIFactErrorMessage()
		{
			return ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EdifactTestFilePath, "EdifactErrorMessage.txt");
		}

		const string MrnCode = "20ES00999910000035";
		const string CSVClearance = "W3HK2AT5T6MFTJ28";
	}
	class ArrivalAtExitResponseMessageProcessorForTest : ArrivalAtExitResponseMessageProcessor
	{
		public ArrivalAtExitResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override ICUSRESV921ESMessageProvider GetMessageProviderCore(EDIMessage message) => null;

		public CusExitDetail FindRelevantBusinessObjectCoreExposed(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage) => FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);

		protected override void ProcessMessageCore(EDIMessage message, CusExitDetail exitDetail, ICUSRESV921ESMessageProvider response)
		{
		}
	}

	public class ArrivalAtExitResponseMessageProcessorForMockTest : ArrivalAtExitResponseMessageProcessor
	{
		public ArrivalAtExitResponseMessageProcessorForMockTest(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		public virtual ICUSRESV921ESMessageProvider GetMessageProviderForTest
		{
			get;
		}
		protected override ICUSRESV921ESMessageProvider GetMessageProviderCore(EDIMessage message) => GetMessageProviderForTest;
	}
}
