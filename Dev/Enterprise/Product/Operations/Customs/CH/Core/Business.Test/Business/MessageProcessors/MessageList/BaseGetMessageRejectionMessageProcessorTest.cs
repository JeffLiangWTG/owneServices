using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseGetMessageRejectionMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	[TestDate(2023, 1, 1, 0, 0, 0)]
	public void TestCusPollingtransactionUpdated_MoreAttempts() => CombineAssertions(() =>
	{
		var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageId = Guid.NewGuid().ToString();
		var (company, ediMessage, transaction) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: messageId, eventType: Events.InterchangeRejectedCode, messageSubType: MessageSubTypeCodeList.Codes.Rejected);
		transaction.CPT_NumberOfAttempts = 3;
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		using (CHCustomsDataRegistry.Instance.MaxNumberOfGetMessageAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
		{
			MessageProcessor.ProcessMessage(ediMessage);
		}

		AssertEquals("CPT_Status", StatusCodes.Rejected, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtcInfo", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_Type", TransactionTypes.MessageId, transaction.CPT_Type);
		AssertEquals("CPT_TransactionID", messageId, transaction.CPT_TransactionID);
		AssertSame("ParentObject", company, transaction.ParentObject);
	});

	[TestDate(2023, 1, 1, 0, 0, 0)]
	public void TestCusPollingtransactionUpdated_NoMoreAttempts()
	{
		var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageId = Guid.NewGuid().ToString();
		var (company, ediMessage, transaction) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: messageId, eventType: Events.InterchangeRejectedCode, messageSubType: MessageSubTypeCodeList.Codes.Rejected);
		transaction.CPT_NumberOfAttempts = 4;
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		using (CHCustomsDataRegistry.Instance.MaxNumberOfGetMessageAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
		{
			MessageProcessor.ProcessMessage(ediMessage);
		}

		AssertEquals("CPT_Status", StatusCodes.Skip, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtcInfo", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_Type", TransactionTypes.MessageId, transaction.CPT_Type);
		AssertEquals("CPT_TransactionID", messageId, transaction.CPT_TransactionID);
		AssertSame("ParentObject", company, transaction.ParentObject);
	}

	[TestDate(2023, 5, 8, 0, 0, 0)]
	public void TestProcessMessageNotInOrder()
	{
		var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);
		var (_, ediMessage1, transaction1) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "a", eventType: AutoEvents.InterchangeRejectedCode, messageSubType: MessageSubTypeCodeList.Codes.Rejected, sequenceNumber: 1);
		var (_, ediMessage2, transaction2) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageId: "b", eventType: AutoEvents.InterchangeRejectedCode, messageSubType: MessageSubTypeCodeList.Codes.Rejected, sequenceNumber: 2);
		Factory.Save();

		AssertMessageProcessed(ediMessage2, transaction2);
		AssertMessageProcessed(ediMessage1, transaction1);

		void AssertMessageProcessed(EDIMessage ediMessage, CusPollingTransaction transaction)
		{
			MessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("EM_Status updated", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("CPT_Status updated", StatusCodes.Rejected, transaction.CPT_Status);
		}
	}

	protected override string GetResponseMessage() => "<notused />";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.Rejected;

	protected override string EventType => Events.InterchangeRejectedCode;
}
