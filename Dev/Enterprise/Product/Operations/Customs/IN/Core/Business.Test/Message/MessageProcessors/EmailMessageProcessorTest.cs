using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(FileProcessingErrorMessageProcessor))]
sealed class EmailMessageProcessorTest : TestCaseWithFactory
{
	public void TestGetLinkedObject()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		var attachment = "HREC\u001dZZ\u001dINBOM4\u001dZZ\u001dLIPLINDIA\u001dICES1_5\u001dP\u001d\u001dCHCMI02\u001d3447163\u001d20240108\u001d1106";

		var log = new LoggingInformation();

		AssertNull(new EmailMessageProcessorForTest(incomingMessage, attachment, log).GetLinkedObject());
		AssertEquals(" *** \tGet LinkedObject by AirCgmCHCMI02MessageProcessor", log.DebugLogStrings[0]);

		attachment = attachment.Replace("CHCMI02", "CHCMI21A");
		log.ClearLogs();
		AssertNull(new EmailMessageProcessorForTest(incomingMessage, attachment, log).GetLinkedObject());
		AssertEquals(" *** \tGet LinkedObject by SeaCgmCHCMI21AMessageProcessor", log.DebugLogStrings[0]);

		attachment = attachment.Replace("CHCMI21A", "CHCAE02A");
		log.ClearLogs();
		AssertNull("Invalid message ID", new EmailMessageProcessorForTest(incomingMessage, attachment, log).GetLinkedObject());
		AssertEquals(" *** \tGet LinkedObject by EmailMessageProcessorForTest", log.DebugLogStrings[0]);
		AssertEquals("Debug - Get LinkedObject by EmailMessageProcessorForTest", log.Logs.ElementAt(0).ToString());
		AssertEquals("Error - Could not find message processor for the email with Subject: 'Test Subject' with Message ID: 'CHCAE02A'.", log.Logs.ElementAt(1).ToString());
	}

	public void TestProcessMessage()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		var attachment = "HREC\u001dZZ\u001dINBOM4\u001dZZ\u001dLIPLINDIA\u001dICES1_5\u001dP\u001d\u001dCHCMI02\u001d3447163\u001d20240108\u001d1106";

		var log = new LoggingInformation();

		Assert(!new EmailMessageProcessorForTest(incomingMessage, attachment, log).Process());
		AssertEquals(" *** \tMessage processing by AirCgmCHCMI02MessageProcessor", log.DebugLogStrings[0]);

		attachment = attachment.Replace("CHCMI02", "CHCAE02A");
		log.ClearLogs();
		Assert("Invalid Message ID", !new EmailMessageProcessorForTest(incomingMessage, attachment, log).Process());
		AssertEquals(" *** \tMessage processing by EmailMessageProcessorForTest", log.DebugLogStrings[0]);
		AssertEquals("\tCould not find message processor for the email with Subject: 'Test Subject' with Message ID: 'CHCAE02A'.", log.DebugLogStrings[1]);

		attachment = attachment.Replace("CHCAE02A", "");
		log.ClearLogs();
		Assert("Cannot Extract Message ID", !new EmailMessageProcessorForTest(incomingMessage, attachment, log).Process());
		AssertEquals(" *** \tMessage processing by EmailMessageProcessorForTest", log.DebugLogStrings[0]);
		AssertEquals("\tCould not find message processor for the email with Subject: 'Test Subject'.", log.DebugLogStrings[1]);
	}

	public void TestMessageProcessors()
	{
		var type = typeof(EmailMessageProcessor);
		var field = type.GetProperty("messageProcessors", BindingFlags.Static | BindingFlags.NonPublic);
		var lazyProcessors = field.GetValue(null);
		var processors = ((System.Lazy<IReadOnlyList<IEmailMessageProcessor>>)lazyProcessors).Value;
		AssertEquals(5, processors.Count);
		var processorTypes = processors.Select(p => p.GetType().Name).ToList();
		AssertCollectionContains(nameof(ShippingBillQueryPendingMessageProcessor), processorTypes);
		AssertCollectionContains(nameof(FileProcessingErrorMessageProcessor), processorTypes);
		AssertCollectionContains("AirCgmCHCMI01MessageProcessor", processorTypes);
		AssertCollectionContains("AirCgmCHCMI02MessageProcessor", processorTypes);
		AssertCollectionContains("SeaCgmCHCMI21AMessageProcessor", processorTypes);
	}

	class EmailMessageProcessorForTest : EmailMessageProcessor
	{
		public EmailMessageProcessorForTest(EDIMessage message, string attachmentText, LoggingInformation logger) : base(message, logger)
		{
			EmailInfo.Subject = "Test Subject";
			EmailInfo.HasAttachments = true;
			EmailInfo.AttachmentText = attachmentText;
		}
	}
}
