using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ComunicaAnulacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class InboxNotificationInvalidationH1ResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationInvalidationH1ResponseMessageProcessor, IMessagePrettyFormatter, ComunicaAnulacionV1Sal>
{
	public void TestProcessAcceptedMessage_InvalidatedByCustoms_WithMRN()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustomsWithMRN(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>dit</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Invalidated, movementReferenceNumber: MRNCode, messageNum: MessageNum);
	}

	public void TestProcessAcceptedMessage_InvalidatedByCustoms_WithRegistrationNumber()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustomsWithRegistrationNumber(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Register (CRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>dit</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Invalidated, movementReferenceNumber: MRNCode, messageNum: MessageNum);
	}

	public void TestProcessAcceptedMessage_NotInvalidatedByCustoms()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999I001H2R9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs:</td><td>&nbsp;&nbsp;</td><td>NO</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>02-12-2024, 13:11:35</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>dit</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cancelled, movementReferenceNumber: MRNCode, messageNum: MessageNum);
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader.CH_EntryStatus = OriginalEntryStatus;
		entryHeader.MovementReferenceNumberSetter(MRNCode);

		SetSentInterchange(entryHeader, InterchangeID);

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "INV", "Invalidated", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();
	}

	string GetAcceptanceTestFileInvalidatedByCustomsWithMRN() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationH1TestFilePath, "AcceptedMessageInvalidatedByCustomsWithMRN.txt");
	string GetAcceptanceTestFileInvalidatedByCustomsWithRegistrationNumber() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationH1TestFilePath, "AcceptedMessageInvalidatedByCustomsWithRegistrationNumber.txt");
	string GetAcceptanceTestFileNotInvalidatedByCustoms() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationH1TestFilePath, "AcceptedMessageNotInvalidatedByCustoms.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationH1TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	const string MRNCode = "24ES009999I001H2R9";
	const string MessageNum = "20241202131135792055";

	protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

	protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(MRNCode);

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
		message.EM_MessageText = GetAcceptanceTestFileInvalidatedByCustomsWithMRN();

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
		message.EM_MessageText = GetAcceptanceTestFileInvalidatedByCustomsWithMRN();

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

	protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification for Import H1 Invalidation Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportH1InvalidationCommunication };

	protected override InboxNotificationInvalidationH1ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationInvalidationH1ResponseMessageProcessor(logger);
}
