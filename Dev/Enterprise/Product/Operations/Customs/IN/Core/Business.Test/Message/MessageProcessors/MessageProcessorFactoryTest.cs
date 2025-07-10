using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(MessageProcessorFactory))]
sealed class MessageProcessorFactoryTest : TestCaseWithFactory
{
	public void TestGetProcessor()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = Messaging.Business.EDIMessage.Direction.Receive;

		var log = new LoggingInformation();

		incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithoutAttachment.eml");
		var processor = MessageProcessorFactory.GetMessageProcessor(incomingMessage, log);
		CombineAssertions(() =>
		{
			AssertType<EmailMessageProcessor>("MessageData is an email without attachment", processor);

			incomingMessage.EM_MessageData = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.IN.Business.Testing.Message.MessageProcessors.TestFiles.EmailWithAttachment.eml");
			processor = MessageProcessorFactory.GetMessageProcessor(incomingMessage, log);
			AssertType<EmailMessageProcessor>("MessageData is an email with attachment", processor);

			incomingMessage.EM_MessageType = Constants.MessageType.XtErrorResponse;
			processor = MessageProcessorFactory.GetMessageProcessor(incomingMessage, log);
			AssertType<XtErrorMessageProcessor>("XER message type", processor);
		});
	}
}
