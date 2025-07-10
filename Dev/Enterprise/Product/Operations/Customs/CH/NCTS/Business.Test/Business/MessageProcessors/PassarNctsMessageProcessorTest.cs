using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(PassarNctsMessageProcessor))]
sealed class PassarNctsMessageProcessorTest : TestCaseWithFactory
{
	public void TestApplicationCode()
	{
		var messageProcessor = CreateMessageProcessor();

		AssertEquals(ApplicationCodeList.Codes.CHCustomsPassar, messageProcessor.ApplicationCode);
	}

	public void TestMessageTypesToInclude()
	{
		var messageProcessor = CreateMessageProcessor();

		CombineAssertions(() =>
		{
			AssertEquals(1, messageProcessor.MessageTypesToInclude.Count);
			AssertEquals(MessageTypeCodeList.Codes.PassarNcts, messageProcessor.MessageTypesToInclude.Single());
		});
	}

	public void TestUnknownResponseMessage()
	{
		var logger = new LoggingInformation();
		var messageProcessor = CreateMessageProcessor(logger);
		var receivedEdiMessage = Factory.New<CHEDIMessage>();
		receivedEdiMessage.EM_MessageText = "<response></response>";
		receivedEdiMessage.EM_MessageNum = "123";

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertNull("EM_LinkedObject", receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, receivedEdiMessage.EM_Status);
			AssertEquals($"Warning - Unable to link EDI Message '{receivedEdiMessage.EM_MessageNum}' to an existing business object.", logger.Logs.Last().ToString());
		});
	}

	public void TestProcessResponseMessage_MessageIsAcknowledged()
	{
		var (nctsHeader, sentEdiMessage) = CreateSentMessage();
		var receivedEdiMessage = CreateReceivedMessage(sentEdiMessage.Interchange.EI_SessionGUID);
		receivedEdiMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Acknowledged;
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", nctsHeader.MovementHeader, receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Received, receivedEdiMessage.EM_Status);
			AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Acknowledged, nctsHeader.EffectiveMessageStatus);
			AssertNotEquals("Phase is not Acknowledged", NctsMovementHeaderTransactionStatusList.Codes.Acknowledged, nctsHeader.MovementHeader.BM_Phase);
			AssertNotEquals("A MSC event has not been added", Events.MessageStatusChangeCode, nctsHeader.Logs.MostRecentLog.SL_SE_NKEvent);
		});
	}

	public void TestProcessResponseMessage_MessageIsRejected()
	{
		var (nctsHeader, sentEdiMessage) = CreateSentMessage();
		var receivedEdiMessage = CreateReceivedMessage(sentEdiMessage.Interchange.EI_SessionGUID);
		receivedEdiMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Rejected;
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", nctsHeader.MovementHeader, receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Received, receivedEdiMessage.EM_Status);
			AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Failed, nctsHeader.EffectiveMessageStatus);
			AssertNotEquals("Phase is not negative Acknowledged", NctsMovementHeaderTransactionStatusList.Codes.NegativeAcknowledged, nctsHeader.MovementHeader.BM_Phase);
			AssertNotEquals("A MSC event has not been added", Events.MessageStatusChangeCode, nctsHeader.Logs.MostRecentLog.SL_SE_NKEvent);
		});
	}

	public void TestProcessResponseMessage_ShouldIgnoreMessage_WhenMessageTypeInvalid()
	{
		var (nctsHeader, sentEdiMessage) = CreateSentMessage();
		var receivedEdiMessage = CreateReceivedMessage(sentEdiMessage.Interchange.EI_SessionGUID);
		var logger = new LoggingInformation();
		var messageProcessor = CreateMessageProcessor(logger);

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", nctsHeader.MovementHeader, receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, receivedEdiMessage.EM_Status);
			AssertEquals("MessageStatus", ZString.Empty, nctsHeader.EffectiveMessageStatus);
			AssertEquals("Phase", string.Empty, nctsHeader.MovementHeader.BM_Phase);
			AssertNull("No MSC event added", nctsHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange));
			AssertEquals("Warning - Unexpected message sub type \"XXX\"", logger.Logs.First().ToString());
		});
	}

	public void TestProcessResponseMessage_ShouldIgnoreMessage_WhenMessageCantBeLinkedToParent()
	{
		var receivedEdiMessage = CreateReceivedMessage(ZGuid.NewZGuid());
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertNull("EM_LinkedObject", receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, receivedEdiMessage.EM_Status);
			AssertEquals("MessageSubType", "XXX", receivedEdiMessage.EM_MessageSubType);
		});
	}

	PassarNctsMessageProcessor CreateMessageProcessor(LoggingInformation logger = null) => new PassarNctsMessageProcessor(logger ?? new LoggingInformation());

	(NctsHeader nctsHeader, CHEDIMessage sentEdiMessage) CreateSentMessage()
	{
		var sessionGuid = ZGuid.NewZGuid();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_From = "CW1";
		sentEdiInterchange.EI_To = "Customs";
		sentEdiInterchange.EI_SessionGUID = sessionGuid;
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;

		var sentEdiMessage = Factory.New<CHEDIMessage>();
		sentEdiMessage.EM_Status = EDIMessage.Status.Sent;
		nctsHeader.MovementHeader.Messages.Add(sentEdiMessage);
		sentEdiInterchange.ContainedMessages.Add(sentEdiMessage);

		Factory.Save();
		return (nctsHeader, sentEdiMessage);
	}

	CHEDIMessage CreateReceivedMessage(ZGuid sessionGuid)
	{
		var receivedEdiInterchange = Factory.New<EDIInterchange>();
		receivedEdiInterchange.EI_From = "Customs";
		receivedEdiInterchange.EI_To = "CW1";
		receivedEdiInterchange.EI_SessionGUID = sessionGuid;
		receivedEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;

		var receivedEdiMessage = Factory.New<CHEDIMessage>();
		receivedEdiMessage.EM_Status = EDIMessage.Status.Received;
		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);

		Factory.Save();
		return receivedEdiMessage;
	}
}
