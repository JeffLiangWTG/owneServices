using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchRetrySender))]
sealed class CharteraOutputDocumentSearchRetrySenderTest : TestCaseWithFactory
{
	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestMessagesCreated_Error() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.MaxNumberOfSearchAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6))
		{
			var (transaction1, message1) = CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.Error);
			var (transaction2, message2) = CreateSearchRequest(cptNumberOfAttempts: 2, cptStatus: StatusCodes.Error);
			var (transaction3, message3) = CreateSearchRequest(cptNumberOfAttempts: 3, cptStatus: StatusCodes.Error);
			var (transaction4, message4) = CreateSearchRequest(cptNumberOfAttempts: 4, cptStatus: StatusCodes.Error);
			var (transaction5, message5) = CreateSearchRequest(cptNumberOfAttempts: 5, cptStatus: StatusCodes.Error);
			var (transaction6, message6) = CreateSearchRequest(cptNumberOfAttempts: 6, cptStatus: StatusCodes.Error);
			CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.Skip);
			Factory.Save();

			var logger = new LoggingInformationForTesting();
			var results = SendRequestAndGetResult(logger).ToArray();
			AssertEquals("# of created messages", 5, results.Length);
			AssertEquals("6 Chartera Output Document Search Retry Request(s) for company C01 has been processed.", logger.Logs.Last().Message);
			AssertResult("Initial NumberOfAttempts=1", transaction1, results, message1, true, 1, 2, ZDateTime.UtcNow.AddMinutes(5), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=2", transaction2, results, message2, true, 2, 3, ZDateTime.UtcNow.AddMinutes(10), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=3", transaction3, results, message3, true, 3, 4, ZDateTime.UtcNow.AddMinutes(30), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=4", transaction4, results, message4, true, 4, 5, ZDateTime.UtcNow.AddMinutes(60), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=5", transaction5, results, message5, true, 5, 6, ZDateTime.UtcNow.AddMinutes(60), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=6", transaction6, results, message6, false, null, null, null, StatusCodes.Skip);
		}
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestMessagesCreated_AwaitingResponse() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.MaxNumberOfSearchAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6))
		{
			var (transaction1, message1) = CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse, emSystemCreateTimeUtc: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction2, message2) = CreateSearchRequest(cptNumberOfAttempts: 2, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction3, message3) = CreateSearchRequest(cptNumberOfAttempts: 3, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction4, message4) = CreateSearchRequest(cptNumberOfAttempts: 4, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction5, message5) = CreateSearchRequest(cptNumberOfAttempts: 5, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction6, message6) = CreateSearchRequest(cptNumberOfAttempts: 6, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			var (transaction7, message7) = CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-4));
			CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse, emSystemCreateTimeUtc: ZDateTime.UtcNow);
			CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse, emHeldUntilDate: ZDateTime.UtcNow);
			CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.Skip, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-5));
			Factory.Save();

			var logger = new LoggingInformationForTesting();
			var results = SendRequestAndGetResult(logger).ToArray();
			AssertEquals("# of created messages", 5, results.Length);
			AssertEquals("6 Chartera Output Document Search Retry Request(s) for company C01 has been processed.", logger.Logs.Last().Message);
			AssertResult("Initial NumberOfAttempts=1", transaction1, results, message1, true, 1, 2, ZDateTime.UtcNow.AddMinutes(5), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=2", transaction2, results, message2, true, 2, 3, ZDateTime.UtcNow.AddMinutes(10), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=3", transaction3, results, message3, true, 3, 4, ZDateTime.UtcNow.AddMinutes(30), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=4", transaction4, results, message4, true, 4, 5, ZDateTime.UtcNow.AddMinutes(60), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=5", transaction5, results, message5, true, 5, 6, ZDateTime.UtcNow.AddMinutes(60), StatusCodes.AwaitingResponse);
			AssertResult("Initial NumberOfAttempts=6", transaction6, results, message6, false, null, null, null, StatusCodes.Skip);
			AssertResult("HeldUntilDate=4 minutes ago", transaction7, results, message7, false, null, null, null, StatusCodes.AwaitingResponse, expectedCptStatusTimeUtc: ZDateTime.UtcNow.AddMinutes(-1));
		}
	});

	public void TestNotIncludeTransactionStatus()
	{
		CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.Skip);
		Factory.Save();

		var results = SendRequestAndGetResult().ToArray();
		AssertEquals("# of created messages", 0, results.Length);
	}

	public void TestNudgedServiceTask()
	{
		(_, var message) = CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse);
		message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(-10);
		Factory.Save();

		var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
		using (ObjectFactory.Substitute(nameof(IServiceTaskNudger), serviceTaskNudgerMock.Object))
		{
			SendRequestAndGetResult().ToArray();
			AssertNoExceptionThrown(() =>
			{
				serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
			});
		}
	}

	[TestDate(2020, 8, 15, 12, 0, 0)]
	public void TestMessageNotFound_Error() => TestMessageNotFound(StatusCodes.Error);

	[TestDate(2020, 8, 15, 12, 0, 0)]
	public void TestMessageNotFound_AwaitingResponse() => TestMessageNotFound(StatusCodes.AwaitingResponse);

	void TestMessageNotFound(string status) => CombineAssertions(() =>
	{
		var logger = new LoggingInformationForTesting();
		var (transaction1, message1) = CreateSearchRequest(cptStatus: status);
		Factory.Save();
		message1.EM_ApplicationCode = ZString.Empty;
		var (transaction2, message2) = CreateSearchRequest(cptStatus: status);
		message2.EM_ReceiveTransmit = ZString.Empty;
		var (transaction3, message3) = CreateSearchRequest(cptStatus: status);
		message3.EM_MessageType = ZString.Empty;
		var (transaction4, message4) = CreateSearchRequest(cptStatus: status);
		message4.EM_MessageSubType = ZString.Empty;
		Factory.Save();

		var results = SendRequestAndGetResult(logger).ToArray();
		AssertEquals("No messages created", 0, results.Length);

		AssertNoLinkedMessage(transaction1, message1, logger);
		AssertNoLinkedMessage(transaction2, message2, logger);
		AssertNoLinkedMessage(transaction3, message3, logger);
		AssertNoLinkedMessage(transaction4, message4, logger);
	});

	[TestDate(2020, 1, 1, 20, 0, 0)]
	public void TestTimeLimit()
	{
		var (transaction, _) = CreateSearchRequest(cptNumberOfAttempts: 1, cptStatus: StatusCodes.AwaitingResponse);
		TestDateAttribute.AddMinutes(10);

		var config = new PassarSearchRequestConfig();
		config.TimeLimit = 5;
		using (CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.SetTemporaryValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, config))
		{
			AssertSender(false, 6);
			AssertSender(false, 5);
			AssertSender(true, 4);
			AssertSender(true, 3);
			AssertSender(true, null);
		}

		void AssertSender(bool retryExpected, int? ageOfEarliestTimeOfNextAttempt, [CallerLineNumber] int line = 0)
		{
			transaction.CPT_EarliestTimeOfNextAttemptUtc = ageOfEarliestTimeOfNextAttempt == null ? ZDateTime.Empty : ZDateTime.UtcNow.AddHours(-ageOfEarliestTimeOfNextAttempt.Value);	
			transaction.CPT_NumberOfAttempts = 1;
			Factory.Save();
			TestDateAttribute.AddMinutes(10);
			var results = SendRequestAndGetResult().ToArray();
			AssertEquals($"[{line}] ageOfEarliestTimeOfNextAttempt={ageOfEarliestTimeOfNextAttempt}  - Retry message created?", retryExpected, results.Any());
		}
	}

	[TestDate(2020, 8, 15, 12, 0, 0)]
	public void TestFindMessageQueryOrderedByLatest_Error() => TestFindMessageQueryOrderedByLatest(StatusCodes.Error);

	[TestDate(2020, 8, 15, 12, 0, 0)]
	public void TestFindMessageQueryOrderedByLatest_AwaitingResponse() => TestFindMessageQueryOrderedByLatest(StatusCodes.AwaitingResponse);

	void TestFindMessageQueryOrderedByLatest(string status) => CombineAssertions(() =>
	{
		const string NewerMessageTag = "<!-- newer message -->";

		var (transaction1, message1) = CreateSearchRequest(cptStatus: status, emHeldUntilDate: ZDateTime.UtcNow.AddMinutes(-10));
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		var message2 = Factory.New<CHEDIMessage>();
		message2.CopyPersistentValuesFrom(message1);
		message2.EM_MessageText += NewerMessageTag;
		message2.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(-10);
		Factory.Save();

		var result = SendRequestAndGetResult().First();
		Assert("Newer message duplicated", result.message.EM_MessageText.Contains(NewerMessageTag));
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	public void TestZSaveConcurrencyException() => CombineAssertions(() =>
	{
		const string errorMessage = "Concurrency Exception: Some customs messages may have been processed in the meantime.";
		var successMessage = $"Information: 1 Chartera Output Document Search Retry Request(s) for company {Company.GC_Code} has been processed.";

		var logger = new LoggingInformationForTesting();
		var (transaction, _) = CreateSearchRequest(cptStatus: StatusCodes.Error);
		var (concurrentTransaction, _) = CreateSearchRequest(cptStatus: StatusCodes.AwaitingResponse);
		Factory.Save();

		var hookOnce = true;
		BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => DoConcurrentUpdate(factory));

		AssertNoExceptionThrown("Generate ZSaveConcurrencyException", () => ExecuteSend(6));
		AssertEquals("Called DoConcurrentUpdate", false, hookOnce);
		AssertEquals("ZSaveConcurrencyException Message", true, logger.ContainsLogEntry(errorMessage));
		AssertEquals("Success Message", false, logger.ContainsLogEntry(successMessage));

		logger.ClearLogs();
		AssertNoExceptionThrown("Generate ZSaveConcurrencyException", () => ExecuteSend(11));
		AssertEquals("ZSaveConcurrencyException Message", false, logger.ContainsLogEntry(errorMessage));
		AssertEquals("Success Message", true, logger.ContainsLogEntry(successMessage));

		void DoConcurrentUpdate(BusinessObjectFactory factory)
		{
			if (hookOnce && IsConcurrentTransactionSaved())
			{
				hookOnce = false;
				var concurrentSaveFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var transaction = concurrentSaveFactory.Load<CusPollingTransaction>(concurrentTransaction.PK);
				transaction.CPT_Status = StatusCodes.Closed;
				concurrentSaveFactory.Save();
			}

			bool IsConcurrentTransactionSaved() => factory.GetChanges().GetChangedObjects().Any(c => c.SessionInstance.PK == concurrentTransaction.PK);
		}

		void ExecuteSend(int delay)
		{
			TestDateAttribute.AddMinutes(delay);
			using (DisposableEnvironment.ForCompany(Company.GC_Code))
			{
				new CharteraOutputDocumentSearchRetrySender(logger).Send();
			}
		}
	});

	void AssertResult(string assertionMessage, CusPollingTransaction transaction, IEnumerable<(CusPollingTransaction transaction, CHEDIMessage message)> results, EDIMessage originalMessage,
		bool expectNewMessageCreated, int? expectedEmRetryCount, int? expectedCptNumberOfAttempts, ZDateTime? expectedEmHeldUntilDate, string expectedCptStatus, ZDateTime? expectedCptStatusTimeUtc = null)
	{
		var newMessage = results.Where(x => x.transaction == transaction).Select(x => x.message).FirstOrDefault();
		if (expectNewMessageCreated)
		{
			AssertNotNull($"{assertionMessage}: Message created for transaction", newMessage);
			AssertEquals($"{assertionMessage}: EM_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, newMessage.EM_ApplicationCode);
			AssertEquals($"{assertionMessage}: EM_MessageType", MessageTypeCodeList.Codes.REQ, newMessage.EM_MessageType);
			AssertEquals($"{assertionMessage}: EM_MessageSubType", MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, newMessage.EM_MessageSubType);
			AssertEquals($"{assertionMessage}: EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, newMessage.EM_ReceiveTransmit);
			AssertNotEquals($"{assertionMessage}: EM_MessageNum", ZString.Empty, newMessage.EM_MessageNum);
			AssertNotEquals($"{assertionMessage}: EM_MessageNum", originalMessage.EM_MessageNum, newMessage.EM_MessageNum);
			AssertEquals($"{assertionMessage}: EM_Status", EDIMessageStatusList.Codes.Queued, newMessage.EM_Status);
			AssertEquals($"{assertionMessage}: EM_LinkTable", CusPollingTransactionSchema.Constants.TableName, newMessage.EM_LinkTable);
			AssertEquals($"{assertionMessage}: EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, newMessage.EM_LinkUniqueID);
			AssertEquals($"{assertionMessage}: EM_GP", originalMessage.EM_GP, newMessage.EM_GP);
			AssertEquals($"{assertionMessage}: EM_ApplicationReference", originalMessage.EM_ApplicationReference, newMessage.EM_ApplicationReference);
			AssertEquals($"{assertionMessage}: EM_RetryCount", expectedEmRetryCount.Value, newMessage.EM_RetryCount);
			AssertEquals($"{assertionMessage}: EM_HeldUntilDate", expectedEmHeldUntilDate, newMessage.EM_HeldUntilDate);
		}
		else
		{
			AssertNull($"{assertionMessage}: No new message created for transaction", newMessage);
		}

		if (expectedCptNumberOfAttempts != null)
		{
			AssertEquals($"{assertionMessage}: CPT_NumberOfAttempts", expectedCptNumberOfAttempts.Value, transaction.CPT_NumberOfAttempts);
		}
		AssertEquals($"{assertionMessage}: CPT_Status", expectedCptStatus, transaction.CPT_Status);
		AssertEquals($"{assertionMessage}: CPT_StatusTimeUtc", expectedCptStatusTimeUtc ?? ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
	}

	void AssertNoLinkedMessage(CusPollingTransaction transaction, EDIMessage message, LoggingInformationForTesting logger)
	{
		var assertionMessage = $"Invalid EDIMessage should not have been found:\nEM_ApplicationCode={message.EM_ApplicationCode} EM_ReceiveTransmit={message.EM_ReceiveTransmit} EM_MessageType={message.EM_MessageType} EM_MessageSubType={message.EM_MessageSubType} EM_Status={message.EM_Status}";
		var expectedLogMessage = $"No Document Search Request sent for transaction PK={transaction.PK}";
		Assert(assertionMessage, logger.ContainsLogEntry(expectedLogMessage));
	}

	(CusPollingTransaction transaction, EDIMessage message) CreateSearchRequest(int cptNumberOfAttempts = 1, string cptStatus = CompanyPollingTransaction.StatusCodes.Error, ZDateTime? emHeldUntilDate = null, ZDateTime? emSystemCreateTimeUtc = null)
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var transaction = testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, CompanyPollingTransaction.TransactionTypes.DocumentSearchRequest, cptStatus, numberOfAttempts: cptNumberOfAttempts);
		var message = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, ApplicationCodes.CHCustomsCharteraOutput, ReceiveTransmitList.Codes.Transmit, EDIMessageStatusList.Codes.Sent);
		message.EM_LinkedObject = transaction;
		if (emHeldUntilDate != null)
		{
			message.EM_HeldUntilDate = emHeldUntilDate.Value;
		}
		if (emSystemCreateTimeUtc != null)
		{
			message.EM_SystemCreateTimeUtc = emSystemCreateTimeUtc.Value;
		}
		return (transaction, message);
	}

	IEnumerable<(CusPollingTransaction transaction, CHEDIMessage message)> SendRequestAndGetResult(LoggingInformation logger = null)
	{
		TestDateAttribute.AddMinutes(1);
		using (DisposableEnvironment.ForCompany(Company.GC_Code))
		{
			new CharteraOutputDocumentSearchRetrySender(logger ?? new LoggingInformationForTesting()).Send();
		}
		var messageQuery = new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow);
		messageQuery.AddToFilter(EDIMessageSchema.EM_GB, Company.Branches[0].PK);
		var messages = Factory.Load<CHEDIMessage>(messageQuery);
		foreach (var message in messages)
		{
			var transaction = Factory.Load<CusPollingTransaction>(message.EM_LinkUniqueID);
			yield return (transaction, message);
		}
	}

	GlbCompany Company => company ??= MessageProcessorTestHelper.CreateCompany(Factory, tokenCredential: true);
	GlbCompany company;
}
