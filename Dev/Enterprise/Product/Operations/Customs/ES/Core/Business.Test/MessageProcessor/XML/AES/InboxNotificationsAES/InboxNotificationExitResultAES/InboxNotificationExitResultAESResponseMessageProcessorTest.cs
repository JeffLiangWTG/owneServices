using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaResulSalidaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationExitResultAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationExitResultAESResponseMessageProcessor, IMessagePrettyFormatter, ComunicaResulSalidaV1Sal>
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
					"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CSALID - Exit Result</td></tr></table>" +
					"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>09-03-2022, 19:51:49</td></tr></table>" +
					"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100050B2</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>B1 - Dissatisfied</td></tr></table>" +
					"<table border=\"0\"><tr><td>Stop at Exit Date:</td><td>&nbsp;&nbsp;</td><td>09-03-2022</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>ST - Stop at Exit</td></tr></table>";

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: OriginalEntryStatus, movementReferenceNumber: MRNCodeClearedNotPending, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExitResultAESTestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExitResultAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCodeClearedNotPending = "22ES000101100050B2";
		const string MessageNum = "20220309195149749089";

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

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

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Exit Result Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitResultCommunication };

		protected override InboxNotificationExitResultAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationExitResultAESResponseMessageProcessor(logger);
	}
}
