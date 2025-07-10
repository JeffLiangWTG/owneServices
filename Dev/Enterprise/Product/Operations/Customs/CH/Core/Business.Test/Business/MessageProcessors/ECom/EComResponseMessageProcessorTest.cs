using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComResponseMessageProcessorTest : TestCaseWithFactory
{
	ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EComResponseMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestMessageFriendlyName()
	{
		var processor = GetMessageProcessor(Logger);
		AssertEquals("Customs ECom Message Response Processor", processor.MessageFriendlyName);
	}

	public void TestMessageLinked()
	{
		CombineAssertions(() =>
		{
			TestMessageLinked(MessageSubTypeCodeList.Codes.Accepted, TestingData.InputEComResponseAcceptance);
			TestMessageLinked(MessageSubTypeCodeList.Codes.RuleError, TestingData.InputEComResponseRejection);
			TestMessageLinked(MessageSubTypeCodeList.Codes.XmlSchemaError, TestingData.InputEComResponseXMLSchemaErrors);
		});

		void TestMessageLinked(string messageSubType, string messageContent)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.ECM, messageSubType, messageSubType, messageContent);

			MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);

			AssertEquals($"MessageSubType {messageSubType}: EM_LinkTable", CusEntryHeader.Schema.TableName, ediMessage.EM_LinkTable);
			AssertEquals($"MessageSubType {messageSubType}: EM_LinkUniqueID", entryHeader.PK, ediMessage.EM_LinkUniqueID);
		}
	}

	public void TestLastEComplaintStatus()
	{
		CombineAssertions(() =>
		{
			TestLastEComplaintStatus(MessageSubTypeCodeList.Codes.Accepted, TestingData.InputEComResponseAcceptance, EComplaintStatusList.Codes.Accepted);
			TestLastEComplaintStatus(MessageSubTypeCodeList.Codes.RuleError, TestingData.InputEComResponseRejection, EComplaintStatusList.Codes.Rejected);
			TestLastEComplaintStatus(MessageSubTypeCodeList.Codes.XmlSchemaError, TestingData.InputEComResponseXMLSchemaErrors, EComplaintStatusList.Codes.Rejected);
		});

		void TestLastEComplaintStatus(string messageSubType, string messageContent, string expectedLastEComStatus)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.ECM, messageSubType, messageSubType, messageContent);

			MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);

			AssertEquals($"MessageSubType {messageSubType}:", expectedLastEComStatus, entryHeader.CH_LastEComplaintStatus);
		}
	}

	public void TestEvent()
	{
		CombineAssertions(() =>
		{
			TestEvent(MessageSubTypeCodeList.Codes.Accepted, TestingData.InputEComResponseAcceptance, EComplaintStatusList.Codes.Accepted);
			TestEvent(MessageSubTypeCodeList.Codes.RuleError, TestingData.InputEComResponseRejection, EComplaintStatusList.Codes.Rejected);
			TestEvent(MessageSubTypeCodeList.Codes.XmlSchemaError, TestingData.InputEComResponseXMLSchemaErrors, EComplaintStatusList.Codes.Rejected);
		});

		void TestEvent(string messageSubType, string messageContent, string expectedNewStatus)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.ECM, messageSubType, messageSubType, messageContent);
			entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Sent;

			MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);

			var logEntry = entryHeader.Logs.MostRecentLogByEventTime(Events.EComStatusChange);
			AssertEquals($"MessageSubType {messageSubType}:", $"|NEW={expectedNewStatus}|OLD={EComplaintStatusList.Codes.Sent}", logEntry.SL_Reference);
		}
	}
}
