using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeSalidaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class EALInboxNotificationExitNonConformityAESResponseMessageProcessorTest : EALCommonResponseMessageProcessorTest<EALInboxNotificationExitNonConformityAESResponseMessageProcessor, EALInboxNotificationExitNonConformityAESMessagePrettyFormatter, ComunicaDisconformeSalidaV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			AddMessageProcessAndAssertResult();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCode, AddMessageProcessAndAssertResult);
		}

		void AddMessageProcessAndAssertResult()
		{
			var message = CreateNewEDIMessage(MRNCode, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDISSA - Exit Stopped, Non-Conformity</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>27-10-2022, 10:12:18</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100171B8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Non-Conformity Date:</td><td>&nbsp;&nbsp;</td><td>09-03-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Despacho disconforme</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>ST - Stop at Exit</td></tr></table>";

			AssertEALCommonDeclaration(message, AESEntryStatusList.Codes.Refused, expectedMessageInterpretation: expectedMessageInterpretationText, messageNum: MessageNum);
		}

		const string TestFilePath = "Enterprise.Customs.ES.ExitControl.Business.Testing.MessageProcessor.TestFiles.EALInboxNotificationExitNonConformityAES";
		string GetAcceptanceTestFile() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESExitControlTestFileReader.GetEmbeddedFileText(TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString MessageStatusForWrongXML => ZString.Empty;

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Exit Non-Conformity Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification };

		protected override EALInboxNotificationExitNonConformityAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new EALInboxNotificationExitNonConformityAESResponseMessageProcessor(logger);

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			consignment.CXC_MovementReference = MRNCode;

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}
		protected override ZString GetLoggerMessagesWhenProcessMessageWrongXML() => "Unable to find business object for message";
		protected override ZString GetLoggerMessagesWhenProcessMessageError() => "Message Text is empty so can't continue with processing";
	}
}
