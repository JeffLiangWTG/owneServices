using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseMessageListAcceptanceMessageProcessorTest : TestCaseWithFactory
{
	protected abstract ApplicationTypeMessageProcessor GetMessageProcessor();

	protected abstract string ExpectedFriendlyName { get; }

	protected abstract string ApplicationCode { get; }

	protected LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, GetMessageProcessor().MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, GetMessageProcessor().ApplicationCode);
	}

	public void TestLinkedToCompany() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageText = testHelper.CreateMessageListUniversalEventXml((null, "NT000"));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);
		GetMessageProcessor().ProcessMessage(ediMessage);

		AssertEquals("EM_LinkTable", company.TableName, ediMessage.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", company.PK, ediMessage.EM_LinkUniqueID);
	});

	[TestDate(2000, 1, 1, 0, 0, 0)]
	public void TestMessageIdCusPollingTransaction() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageId1 = Guid.NewGuid().ToString();
		var messageId2 = Guid.NewGuid().ToString();
		var messageId3 = Guid.NewGuid().ToString();
		var messageText = testHelper.CreateMessageListUniversalEventXml((messageId1, "NT001"), (messageId2, "NT002"));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);

		GetMessageProcessor().ProcessMessage(ediMessage);

		var transaction1 = LoadSingleTransaction(transactionID: messageId1, transactionType: TransactionTypes.MessageId);
		var transaction2 = LoadSingleTransaction(transactionID: messageId2, transactionType: TransactionTypes.MessageId);
		AssertTransaction("Transaction NT001 created", transaction1, company, type: TransactionTypes.MessageId, status: StatusCodes.New, statusTimeUtc: ZDateTime.UtcNow, sequenceNumber: 1);
		AssertTransaction("Transaction NT002 created", transaction2, company, type: TransactionTypes.MessageId, status: StatusCodes.New, statusTimeUtc: ZDateTime.UtcNow, sequenceNumber: 2);

		messageText = testHelper.CreateMessageListUniversalEventXml((messageId1, "NT001"), (messageId2, "NT002"), (messageId3, "NT003"));
		(company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);

		TestDateAttribute.AddDays(1);

		GetMessageProcessor().ProcessMessage(ediMessage);

		var transaction3 = LoadSingleTransaction(transactionID: messageId3, transactionType: TransactionTypes.MessageId);

		AssertEquals("Transaction NT001 exists", transaction1.PK, LoadSingleTransaction(transactionID: messageId1, transactionType: TransactionTypes.MessageId)?.PK);
		AssertEquals("Transaction NT002 exists", transaction2.PK, LoadSingleTransaction(transactionID: messageId2, transactionType: TransactionTypes.MessageId)?.PK);
		AssertEquals("Transaction NT003 created", transaction3.PK, LoadSingleTransaction(transactionID: messageId3, transactionType: TransactionTypes.MessageId)?.PK);

		AssertTransaction("transaction1", transaction1, company, type: TransactionTypes.MessageId, status: StatusCodes.New, statusTimeUtc: ZDateTime.UtcNow.AddDays(-1), sequenceNumber: 1);
		AssertTransaction("transaction2", transaction2, company, type: TransactionTypes.MessageId, status: StatusCodes.New, statusTimeUtc: ZDateTime.UtcNow.AddDays(-1), sequenceNumber: 2);
		AssertTransaction("transaction3", transaction3, company, type: TransactionTypes.MessageId, status: StatusCodes.New, statusTimeUtc: ZDateTime.UtcNow, sequenceNumber: 3);
	});

	[TestDate(2000, 1, 1, 0, 0, 0)]
	public void TestLastMessageIdCusPollingTransaction()
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageId1 = Guid.NewGuid().ToString();
		var messageId2 = Guid.NewGuid().ToString();
		var messageId3 = Guid.NewGuid().ToString();
		var messageText = testHelper.CreateMessageListUniversalEventXml((messageId1, "NT001"), (messageId2, "NT002"));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);

		GetMessageProcessor().ProcessMessage(ediMessage);

		var lastTransaction = LoadSingleTransaction(company: company, transactionType: TransactionTypes.LastMessageId);
		AssertTransaction("lastTransaction", lastTransaction, company, type: TransactionTypes.LastMessageId, transactionId: messageId2, statusTimeUtc: ZDateTime.UtcNow);

		messageText = testHelper.CreateMessageListUniversalEventXml((messageId1, "NT001"), (messageId2, "NT002"), (messageId3, "NT003"));
		(company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);

		TestDateAttribute.AddDays(1);

		GetMessageProcessor().ProcessMessage(ediMessage);

		AssertEquals("LastMessageId Transaction exists", lastTransaction.PK, LoadSingleTransaction(company: company, transactionType: TransactionTypes.LastMessageId)?.PK);
		AssertTransaction("lastTransaction", lastTransaction, company, type: TransactionTypes.LastMessageId, transactionId: messageId3, statusTimeUtc: ZDateTime.UtcNow.AddDays(-1));
	}

	public void TestLinkedObjectNotCompany()
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var messageText = testHelper.CreateMessageListUniversalEventXml((null, "NT001"));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, messageText);
		ediMessage.EM_LinkedObject = company.Branches.First();

		AssertNoExceptionThrown(() => GetMessageProcessor().ProcessMessage(ediMessage));
	}

	public CusPollingTransaction LoadSingleTransaction(GlbCompany company = null, string transactionType = null, string transactionID = null)
	{
		var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCode);
		if (company != null)
		{
			query.AddToFilter(CusPollingTransactionSchema.CPT_ParentTableCode, company.TablePrefix);
			query.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, company.PK);
		}
		if (transactionType != null)
		{
			query.AddToFilter(CusPollingTransactionSchema.CPT_Type, transactionType);
		}
		if (transactionID != null)
		{
			query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, transactionID);
		}
		var transactions = Factory.Load<CusPollingTransaction>(query);
		AssertLessThanOrEqualTo("Number of transactions", transactions.Length, 1);
		return (transactions.Length > 0) ? transactions[0] : null;
	}

	public void AssertTransaction(string assertionMessage, CusPollingTransaction transaction, GlbCompany company, string type, string transactionId = null, string status = null, ZDateTime? statusTimeUtc = null, int? sequenceNumber = null)
	{
		AssertNotNull($"{assertionMessage} -- should exist", transaction);
		if (transaction != null)
		{
			AssertEquals($"{assertionMessage} - CPT_ApplicationCode", ApplicationCode, transaction.CPT_ApplicationCode);
			AssertEquals($"{assertionMessage} - CPT_Type", type, transaction.CPT_Type);
			AssertEquals($"{assertionMessage} - CPT_ParentTableCode", company.TablePrefix, transaction.CPT_ParentTableCode);
			AssertEquals($"{assertionMessage} - CPT_ParentID", company.PK, transaction.CPT_ParentID);
			if (transactionId != null)
			{
				AssertEquals($"{assertionMessage} - CPT_TransactionID", transactionId, transaction.CPT_TransactionID);
			}
			if (status != null)
			{
				AssertEquals($"{assertionMessage} - CPT_Status", status, transaction.CPT_Status);
			}
			if (statusTimeUtc != null)
			{
				AssertEquals($"{assertionMessage} - CPT_StatusTimeUtc", statusTimeUtc, transaction.CPT_StatusTimeUtc);
			}
			if (sequenceNumber.HasValue)
			{
				AssertEquals($"{assertionMessage} -  CPT_SequenceNumber", sequenceNumber.Value, transaction.CPT_SequenceNumber);
			}
		}
	}
}
