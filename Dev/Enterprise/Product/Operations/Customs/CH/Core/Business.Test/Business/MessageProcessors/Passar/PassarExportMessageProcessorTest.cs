using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarExportMessageProcessor))]
sealed class PassarExportMessageProcessorTest : TestCaseWithFactory
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
			AssertEquals(MessageTypeCodeList.Codes.Export, messageProcessor.MessageTypesToInclude.Single());
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
		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarDeclaration, MessageSubTypeCodeList.Codes.Acknowledged, ZString.Empty);
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(ediMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", entryHeader, ediMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Acknowledged, entryHeader.CH_Status);

			var logEvent = entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange);
			AssertEquals("A MSC event has been added", Events.MessageStatusChangeCode, logEvent.SL_SE_NKEvent);
			AssertContains("MSC event reference contains CH_Status ", entryHeader.CH_Status, logEvent.SL_Reference);
		});
	}

	public void TestProcessResponseMessage_MessageIsRejected()
	{
		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarDeclaration, MessageSubTypeCodeList.Codes.Rejected, ZString.Empty);
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(ediMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", entryHeader, ediMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
			AssertEquals("MessageStatus", EDIMessageStatusList.Codes.Failed, entryHeader.CH_Status);

			var logEvent = entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange);
			AssertEquals("A MSC event has been added", Events.MessageStatusChangeCode, logEvent.SL_SE_NKEvent);
			AssertContains("MSC event reference contains CH_Status ", entryHeader.CH_Status, logEvent.SL_Reference);
		});
	}

	public void TestProcessResponseMessage_ShouldIgnoreMessage_WhenMessageTypeInvalid()
	{
		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.Export, MessageSubTypeCodeList.Codes.PassarDeclaration, "XXX", ZString.Empty);
		var logger = new LoggingInformation();
		var messageProcessor = CreateMessageProcessor(logger);

		messageProcessor.ProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_LinkedObject", entryHeader, ediMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, ediMessage.EM_Status);
			AssertEquals("MessageStatus", "AWO", entryHeader.CH_Status);
			AssertNull("No MSC event added", entryHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange));
			AssertEquals("Warning - Unexpected message sub type \"XXX\"", logger.Logs.First().ToString());
		});
	}

	public void TestProcessResponseMessage_ShouldIgnoreMessage_WhenMessageCantBeLinkedToParent()
	{
		var receivedEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory);
		var messageProcessor = CreateMessageProcessor();

		messageProcessor.ProcessMessage(receivedEdiMessage);

		CombineAssertions(() =>
		{
			AssertNull("EM_LinkedObject", receivedEdiMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, receivedEdiMessage.EM_Status);
			AssertEquals("MessageSubType", "XXX", receivedEdiMessage.EM_MessageSubType);
		});
	}

	PassarExportMessageProcessor CreateMessageProcessor(LoggingInformation logger = null) => new PassarExportMessageProcessor(logger ?? new LoggingInformation());
}
