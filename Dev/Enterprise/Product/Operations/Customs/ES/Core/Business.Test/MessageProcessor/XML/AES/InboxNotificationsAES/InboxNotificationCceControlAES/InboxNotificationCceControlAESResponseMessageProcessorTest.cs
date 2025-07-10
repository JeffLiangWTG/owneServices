using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaControlesCCEV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationCceControlAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationCceControlAESResponseMessageProcessor, IMessagePrettyFormatter, ComunicaControlesCcev1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			AddMessageProcessAndAssertResult();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult);
		}

		void AddMessageProcessAndAssertResult()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
					"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CONCCE - Control needed at CCE</td></tr></table>" +
					"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>15-02-2022, 11:35:28</td></tr></table>" +
					"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100044B4</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Control Notification Date:</td><td>&nbsp;&nbsp;</td><td>15-02-2022</td></tr></table>" +
					"<table border=\"0\"><tr><td>Notification Type:</td><td>&nbsp;&nbsp;</td><td>0 - Decission to Control (and requested documents if needed)</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
					"<H4>Type of Control</H4>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Item</strong></td><td><strong>Type</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>1</td><td>40 - Physical controls</td><td>Mirar detenidamente todas las cajas por escaner.</td></tr></table>" +
					"<H4>Required Documents</H4>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Item</strong></td><td><strong>Type</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>1</td><td>C055</td><td>Declaracion de conformidad (Anexo IV del Reglamento (UE) No 10/2011)</td></tr>" +
					"<tr><td>2</td><td>C077</td><td>Autorizacion expedida por la autoridad competente (anexo II, parte VIII, del Reglamento (UE) 2017/1509)</td></tr></table>";

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.ControlsAtEuOffice, movementReferenceNumber: MRNCodeClearedNotPending, acceptanceDate: movementReferenceNumberIssueDate, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending, movementReferenceNumberIssueDate);

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CCO", "Controls At EU Office", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationCceControlAESTestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationCceControlAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCodeClearedNotPending = "22ES000101100044B4";
		const string MessageNum = "20220215113527522089";
		readonly ZDateTime movementReferenceNumberIssueDate = new ZDateTime(2021, 01, 15, 05, 40, 55);

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending, movementReferenceNumberIssueDate);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

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
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

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

		protected override void AssertLoggerMessagesWhenProcessMessageWrongXML()
		{
			AssertContains("logger", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
		}

		protected override void AssertLoggerMessagesWhenMessageProcessingError()
		{
			base.AssertLoggerMessagesWhenMessageProcessingError();

			AssertContains("logger exception", "Message Text is empty so can't continue with processing", GetAllConcatenatedUserLogStrings());
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export CCE Control Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportCceControlCommunication };

		protected override InboxNotificationCceControlAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationCceControlAESResponseMessageProcessor(logger);
	}
}
