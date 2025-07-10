using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MailManager.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEmptyMessageText()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = ZString.Empty;
			eXDOCApplicationTypeMessageProcessor.ProcessMessage(message);

			AssertEquals("The message is in error", EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestInvalidReference()
		{
			ZInt emailCount = Factory.GetDatabaseCount(typeof(MailItem));
			message.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.EXDOC.TestFiles.ResponseMessageInvalidReference.edi");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			eXDOCApplicationTypeMessageProcessor.ProcessMessage(message);

			AssertEquals("The message is in error", EDIMessage.Status.Error, message.EM_Status);
		}

		EXDOCApplicationTypeMessageProcessor eXDOCApplicationTypeMessageProcessor;
		EDIMessage message;

		protected override void SetUp()
		{
			base.SetUp();
			eXDOCApplicationTypeMessageProcessor = new EXDOCApplicationTypeMessageProcessor(new LoggingInformation());
			message = Factory.New<EDIMessage>();
		}
	}
}
