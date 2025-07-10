using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

public class CustomsMessageProcessorTestHelper
{
	public CustomsMessageProcessorTestHelper(BusinessObjectFactory factory)
	{
		Factory = factory;
	}
	BusinessObjectFactory Factory { get; }

	internal CusPollingTransaction AddPollingTransaction(GlbCompany company, string applicationCode, string type, string status, string transactionID = null, short sequenceNumber = 1, ZDateTime? earliestTimeOfNextAttemptUtc = null, ZInt? numberOfAttempts = null, string reference = null)
	{
		var transaction = Factory.New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = applicationCode;
		transaction.CPT_Type = type;
		transaction.CPT_TransactionID = transactionID ?? ZGuid.NewZGuid().ToString();
		transaction.CPT_ParentTableCode = company.TablePrefix;
		transaction.CPT_ParentID = company.PK;
		transaction.CPT_Status = status;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		transaction.CPT_NumberOfAttempts = (byte)(numberOfAttempts != null ? (byte)numberOfAttempts.Value : 1);
		transaction.CPT_SequenceNumber = sequenceNumber;
		transaction.CPT_Reference = reference ?? ZString.Empty;
		if (earliestTimeOfNextAttemptUtc != null)
		{
			transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc.Value;
		}
		return transaction;
	}

	internal ZString CreateMessageListUniversalEventXml(params (string messageId, string messageType)[] messages)
	{
		var builder = new StringBuilder();
		builder.Append("<messages>");
		foreach (var message in messages)
		{
			builder.Append("<message>");
			builder.Append("<messageId>").Append(message.messageId ?? Guid.NewGuid().ToString()).Append("</messageId>");
			builder.Append("<messageType>").Append(message.messageId).Append("</messageType>");
			builder.Append("</message>");
		}
		builder.Append("</messages>");
		return CreateUniversalEventXml(AutoEvents.InterchangeAcknowledgedCode, builder.ToString());
	}

	string CreateUniversalEventXml(string eventType, string value) => UniversalEventTestDataHelper.CreateUniversalEventXml(eventType: eventType, responseMessage: value);

	public (GlbCompany company, EDIMessage ediMessage, CusPollingTransaction transaction) CreateGetMessageResponseObjects(string applicationCode, string responseMessage = null, string eventType = Events.InterchangeAcknowledgedCode, string messageSubType = MessageSubTypeCodeList.Codes.Undefined, string messageId = null, string cptStatus = CompanyPollingTransaction.StatusCodes.New, short sequenceNumber = 1, string cptType = TransactionTypes.MessageId, string companyCode = null)
	{
		if (responseMessage == null)
		{
			responseMessage = $"<test><messageId>{messageId}</messageId></test>";
		}

		var applicationReference = messageId ?? Guid.NewGuid().ToString();
		var incomingMessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(eventType: eventType, responseMessage: responseMessage);
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, applicationCode, messageType: MessageTypeCodeList.Codes.MSG, messageSubType: messageSubType, incomingMessageText, sentApplicationReference: applicationReference, companyCode: companyCode);
		var transaction = AddPollingTransaction(company, applicationCode, cptType, cptStatus, applicationReference, sequenceNumber: sequenceNumber);
		return (company, ediMessage, transaction);
	}
}
