using System;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CharteraOutputErrorMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Chartera Output Error Message Processor";

	protected override string ApplicationCode => CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.CharteraOutputError;

	protected override string GetResponseMessage() => GetErrorMessageContent(ZGuid.NewZGuid().ToString());

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new CharteraOutputErrorMessageProcessor(Logger);

	public void TestErrorLoggedIfNoOutgoingMessage()
	{
		var company = GlbCompany.GetCurrentCompany(Factory);

		var (_, getMessageMessage) = SendGetMessageRequest(company, ZGuid.NewZGuid());
		var processId = ZGuid.NewZGuid();
		var errorMessage = CreateIncomingErrorMessage(processId.ToString(), getMessageMessage.Interchange.EI_SessionGUID);
		Factory.Save();

		CreateMessageProcessor().ProcessMessage(errorMessage);

		AssertEquals("EM_Status", EDIMessage.Status.Warning, errorMessage.EM_Status);
	}

	[TestDate(2023, 1, 1)]
	[TestUtcOffset(3, 0, 0)]
	public void TestProcess_C01() => CombineAssertions(() =>
	{
		var company = GlbCompany.GetCurrentCompany(Factory);

		var (documentSearchTransaction, documentSearchMessage, processId) = SendDocumentSearchRequest(company);
		var (messageTransaction, getMessageMessage) = SendGetMessageRequest(company, ZGuid.NewZGuid());
		var errorMessage = CreateIncomingErrorMessage(processId, getMessageMessage.Interchange.EI_SessionGUID);
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		CreateMessageProcessor().ProcessMessage(errorMessage);

		AssertSame("EM_LinkedObject", getMessageMessage.EM_LinkedObject, errorMessage.EM_LinkedObject);
		AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, errorMessage.EM_Status);
		AssertTransaction(messageTransaction, CompanyPollingTransaction.StatusCodes.Closed);
		AssertTransaction(documentSearchTransaction, CompanyPollingTransaction.StatusCodes.Error);
		EventsTestHelper.AssertEventAdded(documentSearchTransaction.ParentObject, Events.ExternalValidationFailed, expectedReference: $"|ITN={documentSearchMessage.Interchange.EI_InterchangeNum}|SRC={ApplicationCode}|TYP={documentSearchMessage.EM_MessageSubType}");
	});

	[TestDate(2023, 1, 1)]
	[TestUtcOffset(3, 0, 0)]
	public void TestProcess_C04() => CombineAssertions(() =>
	{
		var documentId1 = Guid.NewGuid().ToString();
		var documentId2 = Guid.NewGuid().ToString();

		var company = GlbCompany.GetCurrentCompany(Factory);

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var documentTransaction1 = testHelper.AddPollingTransaction(company, ApplicationCode, CompanyPollingTransaction.TransactionTypes.DocumentDelivery, CompanyPollingTransaction.StatusCodes.AwaitingResponse, documentId1, 1);
		var documentTransaction2 = testHelper.AddPollingTransaction(company, ApplicationCode, CompanyPollingTransaction.TransactionTypes.DocumentDelivery, CompanyPollingTransaction.StatusCodes.AwaitingResponse, documentId2, 2);
		var transaction3 = testHelper.AddPollingTransaction(company, ApplicationCode, CompanyPollingTransaction.TransactionTypes.DocumentSearchRequest, CompanyPollingTransaction.StatusCodes.AwaitingResponse, documentId1);
		var (documentDeliveryMessage, processId) = SendDocumentDeliveryRequest(company, documentId1, documentId2);
		var (messageTransaction, getMessageMessage) = SendGetMessageRequest(company, ZGuid.NewZGuid());
		var errorMessage = CreateIncomingErrorMessage(processId, getMessageMessage.Interchange.EI_SessionGUID);

		Factory.Save();
		var originalTime = ZDateTime.UtcNow;

		TestDateAttribute.AddMinutes(1);
		CreateMessageProcessor().ProcessMessage(errorMessage);

		AssertSame("EM_LinkedObject", documentDeliveryMessage.EM_LinkedObject, errorMessage.EM_LinkedObject);
		AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, errorMessage.EM_Status);
		AssertTransaction(messageTransaction, CompanyPollingTransaction.StatusCodes.Closed);
		AssertTransaction(documentTransaction1, CompanyPollingTransaction.StatusCodes.Error);
		AssertTransaction(documentTransaction2, CompanyPollingTransaction.StatusCodes.Error);
		EventsTestHelper.AssertEventAdded(documentDeliveryMessage.EM_LinkedObject, Events.ExternalValidationFailed, expectedReference: $"|ITN={documentDeliveryMessage.Interchange.EI_InterchangeNum}|SRC={ApplicationCode}|TYP={documentDeliveryMessage.EM_MessageSubType}");

		AssertEquals("transaction3: Not touched", originalTime, transaction3.CPT_SystemLastEditTimeUtc);
	});

	void AssertTransaction(CusPollingTransaction transaction, string expectedStatus)
	{
		AssertEquals($"{transaction.CPT_Type} CPT_Status", expectedStatus, transaction.CPT_Status);
		AssertEquals($"{transaction.CPT_Type} CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
	}

	(CusPollingTransaction documentSearchTransaction, EDIMessage documentSearchMessage, ZString transactionId) SendDocumentSearchRequest(GlbCompany company)
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory)
		{
			CreationTimeFrom = ZDateTime.UtcNow.AddHours(-1),
			CreationTimeTo = ZDateTime.UtcNow,
		};
		var messageBuilder = (IXmlMessageBuilder)new DocumentSearchRequestV1MessageBuilder(sendingObject);
		var messageText = messageBuilder.GenerateXmlMessage().GetSerializedString();

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var documentSearchTransaction = testHelper.AddPollingTransaction(company, ApplicationCode, CompanyPollingTransaction.TransactionTypes.DocumentSearchRequest, CompanyPollingTransaction.StatusCodes.AwaitingResponse, sendingObject.ProcessId);
		var documentSearchMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, ApplicationCode, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, messageText, sendingObject.ProcessId, linkedObject: documentSearchTransaction);
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, ZGuid.NewZGuid(), ApplicationCode, EDIInterchange.Direction.Transmit, documentSearchMessage);
		return (documentSearchTransaction, documentSearchMessage, sendingObject.ProcessId);
	}

	(EDIMessage message, string processId) SendDocumentDeliveryRequest(GlbCompany company, params string[] documentIds)
	{
		var processId = ZGuid.NewZGuid().ToString();
		var outgoingMessageText = GetDocumentDeliveryRequestContent(documentIds, processId);
		var documentDeliveryRequestMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest, ApplicationCode, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, outgoingMessageText, processId, linkedObject: company);
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, ZGuid.NewZGuid(), ApplicationCode, EDIInterchange.Direction.Transmit, documentDeliveryRequestMessage);
		return (documentDeliveryRequestMessage, processId);
	}

	(CusPollingTransaction getMessageTransaction, EDIMessage getMessageMessage) SendGetMessageRequest(GlbCompany company, ZGuid sessionGuid)
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var getMessageTransaction = testHelper.AddPollingTransaction(company, ApplicationCode, CompanyPollingTransaction.TransactionTypes.MessageId, CompanyPollingTransaction.StatusCodes.AwaitingResponse);
		var getMessageMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.MSG, applicationCode: ApplicationCode, direction: EDIMessage.Direction.Transmit, status: EDIMessageStatusList.Codes.Sent, applicationReference: getMessageTransaction.CPT_TransactionID, linkedObject: company);
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, sessionGuid, ApplicationCode, EDIInterchange.Direction.Transmit, getMessageMessage);
		return (getMessageTransaction, getMessageMessage);
	}

	EDIMessage CreateIncomingErrorMessage(ZString processId, ZGuid sessionGUID, string messageId = null)
	{
		var incomingMessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: GetErrorMessageContent(processId, messageId));
		var incomingErrorMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.CharteraOutputError, ApplicationCode, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, incomingMessageText);
		MessageProcessorTestHelper.CreateEDIInterchange(Factory, sessionGUID, ApplicationCode, EDIInterchange.Direction.Receive, incomingErrorMessage);
		return incomingErrorMessage;
	}

	string GetErrorMessageContent(string processId, string messageId = null)
	{
		var message = new CargoWise.Customs.CH.MessageDefinitions.Chartera.error_v1.Error
		{
			ProcessId = processId ?? Guid.NewGuid().ToString(),
			MessageId = messageId ?? Guid.NewGuid().ToString(),
			ErrorType = CargoWise.Customs.CH.MessageDefinitions.Chartera.error_v1.ErrorTypeType.ClientError,
			ErrorCode = "400",
			ErrorDetail = "cvc-complex-type.2.4.a: Invalid content was found."
		};
		return XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(message);
	}

	string GetDocumentDeliveryRequestContent(string[] documentIds, string processId = null)
	{
		var message = new CargoWise.Customs.CH.MessageDefinitions.Chartera.documentdeliveryrequest_v1.DocumentDeliveryRequest
		{
			ProcessId = processId ?? Guid.NewGuid().ToString(),
			DocumentIds = documentIds.ToCollection(),
		};
		return XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(message);
	}
}
