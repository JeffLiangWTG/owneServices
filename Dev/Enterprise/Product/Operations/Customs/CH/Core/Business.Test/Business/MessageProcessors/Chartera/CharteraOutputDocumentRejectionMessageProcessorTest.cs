using System;
using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputDocumentRejectionMessageProcessorTest : TestCaseWithFactory
{
	public void TestFriendlyName()
	{
		var messageProcessor = CreateMessageProcessor();
		AssertEquals("Chartera Output Document Rejection Message Processor", messageProcessor.MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		var messageProcessor = CreateMessageProcessor();
		AssertEquals(ApplicationCode, messageProcessor.ApplicationCode);
	}

	public void TestMessageFilter()
	{
		var messageProcessor = CreateMessageProcessor();
		AssertEquals("MessageFilter", $"EM_ApplicationCode = '{ApplicationCode}' and EM_MessageType = '{MessageType}' and EM_MessageSubType = '{MessageSubType}'", messageProcessor.MessageFilter.LiteralTextADO);
	}

	[TestDate(2023, 1, 1)]
	[TestUtcOffset(3, 0, 0)]
	public void TestDocumentSearchRejection() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var (company, message, transaction) = testHelper.CreateGetMessageResponseObjects(ApplicationCode, null, eventType: Events.InterchangeRejectedCode, messageSubType: MessageSubType, cptType: TransactionTypes.DocumentSearchRequest, cptStatus: StatusCodes.AwaitingResponse);
		var outgoingMessage = MessageProcessorTestHelper.GetOutgoingMessage(message);
		outgoingMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest;
		outgoingMessage.EM_LinkedObject = transaction;
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		CreateMessageProcessor().ProcessMessage(message);

		AssertSame("EM_LinkedObject expected same as outgoing message", outgoingMessage.EM_LinkedObject, message.EM_LinkedObject);
		AssertEquals("CPT_Status", StatusCodes.Error, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		EventsTestHelper.AssertEventAdded(transaction.ParentObject, Events.MessageRejected, expectedReference: $"|ITN={message.Interchange.EI_InterchangeNum}|SRC={ApplicationCode}|TYP={MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest}");
	});

	[TestDate(2023, 1, 1)]
	[TestUtcOffset(3, 0, 0)]
	public void TestDocumentDeliveryRejection() => CombineAssertions(() =>
	{
		var documentId1 = Guid.NewGuid().ToString();
		var documentId2 = Guid.NewGuid().ToString();

		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var (company, message) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, messageType: MessageType, messageSubType: MessageSubType, messageResponse: "</test />");
		var outgoingMessage = MessageProcessorTestHelper.GetOutgoingMessage(message);
		outgoingMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest;
		outgoingMessage.EM_MessageText = GetDocumentDeliveryRequestForTesting(documentId1, documentId2);
		var transaction1 = testHelper.AddPollingTransaction(company, ApplicationCode, TransactionTypes.DocumentDelivery, StatusCodes.AwaitingResponse, documentId1);
		var transaction2 = testHelper.AddPollingTransaction(company, ApplicationCode, TransactionTypes.DocumentDelivery, StatusCodes.AwaitingResponse, documentId2);
		var transaction3 = testHelper.AddPollingTransaction(company, ApplicationCode, TransactionTypes.DocumentDelivery, StatusCodes.AwaitingResponse, Guid.NewGuid().ToString());
		var transaction4 = testHelper.AddPollingTransaction(company, ApplicationCode, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, documentId1);
		var transaction5 = testHelper.AddPollingTransaction(company, ApplicationCodes.CHCustomsPassar, TransactionTypes.MessageId, StatusCodes.AwaitingResponse, documentId1);
		Factory.Save();
		var originalTime = ZDateTime.UtcNow;

		TestDateAttribute.AddMinutes(1);
		CreateMessageProcessor().ProcessMessage(message);

		AssertSame("EM_LinkedObject expected same as outgoing message", outgoingMessage.EM_LinkedObject, message.EM_LinkedObject);
		AssertEquals("transaction1: CPT_Status", StatusCodes.Error, transaction1.CPT_Status);
		AssertEquals("transaction2: CPT_Status", StatusCodes.Error, transaction2.CPT_Status);
		AssertEquals("transaction1: CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction1.CPT_StatusTimeUtc);
		AssertEquals("transaction2: CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction2.CPT_StatusTimeUtc);
		EventsTestHelper.AssertEventAdded(outgoingMessage.EM_LinkedObject, Events.MessageRejected, expectedReference: $"|ITN={message.Interchange.EI_InterchangeNum}|SRC={ApplicationCode}|TYP={MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest}");

		AssertEquals("transaction3: Not touched", originalTime, transaction3.CPT_SystemLastEditTimeUtc);
		AssertEquals("transaction4: Not touched", originalTime, transaction4.CPT_SystemLastEditTimeUtc);
		AssertEquals("transaction5: Not touched", originalTime, transaction5.CPT_SystemLastEditTimeUtc);
	});

	CharteraOutputDocumentRejectionMessageProcessor CreateMessageProcessor() => new CharteraOutputDocumentRejectionMessageProcessor(new LoggingInformationForTesting());

	const string ApplicationCode = ApplicationCodes.CHCustomsCharteraOutput;
	const string MessageType = MessageTypeCodeList.Codes.REQ;
	const string MessageSubType = MessageSubTypeCodeList.Codes.Rejected;

	public static string GetDocumentDeliveryRequestForTesting(params string[] documentIds)
	{
		var dataProvider = new DocumentDeliveryRequestDataProviderForTesting();
		dataProvider.DocumentIds.AddRange(documentIds);
		var messageBuilder = (IXmlMessageBuilder)new DocumentDeliveryRequestV1MessageBuilder(dataProvider);
		return messageBuilder.GenerateXmlMessage().GetSerializedString();
	}

	class DocumentDeliveryRequestDataProviderForTesting : IDocumentDeliveryRequest
	{
		public string ProcessId { get; } = Guid.NewGuid().ToString();

		internal List<string> DocumentIds { get; } = new List<string>();

		IReadOnlyCollection<string> IDocumentDeliveryRequest.DocumentIds => DocumentIds;
	}
}
