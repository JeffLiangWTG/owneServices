using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class PassarGetMessageAcknowledgeMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	[TestDate(2023, 5, 8, 0, 0, 0)]
	public void TestProcessMessage() => AssertProcessMessage(null, _ => GetResponseMessage(), null);

[TestDate(2023, 5, 8, 0, 0, 0)]
public void TestProcessMessageInOrder()
{
	var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);

	var (_, ediMessage1, transaction1) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "b", cptStatus: CompanyPollingTransaction.StatusCodes.AwaitingResponse, sequenceNumber: 1);
	var (_, ediMessage2, transaction2) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "c", cptStatus: CompanyPollingTransaction.StatusCodes.Rejected, sequenceNumber: 2);
	Factory.Save();

	TestDateAttribute.AddMinutes(1);
	var (_, ediMessage3, transaction3) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "d", cptStatus: CompanyPollingTransaction.StatusCodes.New, sequenceNumber: 1);
	var (_, ediMessage4, transaction4) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "e", cptStatus: CompanyPollingTransaction.StatusCodes.AwaitingResponse, sequenceNumber: 2);
	var (_, ediMessage5, transaction5) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "a", cptStatus: CompanyPollingTransaction.StatusCodes.Closed, sequenceNumber: 1);
	Factory.Save();

	AssertMessageProcessLocked(ediMessage2, transaction2, transaction1);
	AssertMessageProcessLocked(ediMessage3, transaction3, transaction1);
	AssertMessageProcessLocked(ediMessage4, transaction4, transaction1);
	AssertMessageProcessed(ediMessage5, transaction5);

	AssertMessageProcessed(ediMessage1, transaction1);
	AssertMessageProcessLocked(ediMessage3, transaction3, transaction2);
	AssertMessageProcessLocked(ediMessage4, transaction4, transaction2);

	AssertMessageProcessed(ediMessage2, transaction2);
	AssertMessageProcessLocked(ediMessage4, transaction4, transaction3);

	AssertMessageProcessed(ediMessage3, transaction3);
	AssertMessageProcessed(ediMessage4, transaction4);

	void AssertMessageProcessLocked(EDIMessage ediMessage, CusPollingTransaction transaction, CusPollingTransaction pendingTransaction)
	{
		var originalStatus = transaction.CPT_Status;
		var exceptionMessage = $"MSG Messages must be processed in order, current Message Id: {transaction.CPT_TransactionID} - next pending Message Id: {pendingTransaction.CPT_TransactionID}";
		AssertExceptionThrown<MessageProcessLockException>("", exceptionMessage, () => MessageProcessor.ProcessMessage(ediMessage));
		AssertEquals("EM_Status not updated", EDIMessage.Status.Queued, ediMessage.EM_Status);
		AssertEquals("CPT_Status not updated", originalStatus, transaction.CPT_Status);
	}

	void AssertMessageProcessed(EDIMessage ediMessage, CusPollingTransaction transaction)
	{
		MessageProcessor.ProcessMessage(ediMessage);
		AssertEquals("EM_Status updated", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
		AssertEquals("CPT_Status updated", CompanyPollingTransaction.StatusCodes.Closed, transaction.CPT_Status);
	}
}

[TestDate(2023, 5, 8, 0, 0, 0)]
public void TestSetHeldUntilDate()
{
	GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).TokenCredentialsEnabled = true;

	var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);
	var (_, ediMessage2, transaction2) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "b", cptStatus: CompanyPollingTransaction.StatusCodes.AwaitingResponse, sequenceNumber: 2);
	var (_, ediMessage1, transaction1) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "c", cptStatus: CompanyPollingTransaction.StatusCodes.Rejected, sequenceNumber: 1);
	Factory.Save();

	var hub = new InboundMessageProcessorHub(new[] { new ZString(ApplicationCode) }, Array.Empty<ZString>());
	hub.Logger = Logger;

	hub.ExecuteBatch();
	var skipMessage = $"MSG Messages must be processed in order, current Message Id: {transaction2.CPT_TransactionID} - next pending Message Id: {transaction1.CPT_TransactionID}";
	AssertEquals($"Log message\r\n{Logger.AccumulatedLogMessages.ToStringWithNewLineBetweenAppends()}", true, Logger.ContainsLogEntry(skipMessage));

	ediMessage2.Reload();
	AssertEquals("Skip Message for Seq 2", EDIMessage.Status.Queued, ediMessage1.EM_Status);
	AssertEquals("EM_HeldUntilDate set", ZDateTime.UtcNow.AddMinutes(1), ediMessage2.EM_HeldUntilDate);

	ediMessage1.Reload();
	AssertEquals("Process Message for Seq 1", EDIMessage.Status.ProcessedOK, ediMessage1.EM_Status);
	AssertEquals("EM_HeldUntilDate not set", ZDateTime.Empty, ediMessage1.EM_HeldUntilDate);

	TestDateAttribute.AddMinutes(1);
	hub.ExecuteBatch();
	ediMessage2.Reload();
	AssertEquals("Process Message for Seq 2", EDIMessage.Status.ProcessedOK, ediMessage2.EM_Status);
}

	protected override string ExpectedFriendlyName => "Passar Get Message Acknowledge Message Processor";

protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

protected override string MessageSubType => MessageSubTypeCodeList.Codes.Undefined;

protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new PassarGetMessageAcknowledgeMessageProcessor(Logger);

	protected override string GetResponseMessage() => "<notused />";
}
