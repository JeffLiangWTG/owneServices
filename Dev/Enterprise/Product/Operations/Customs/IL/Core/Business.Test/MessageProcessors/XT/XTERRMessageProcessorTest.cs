using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class XTERRMessageProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<XTERRMessageProcessor, ILXERResponseMessage>
	{
		protected override string ExpectedMessageFriendlyName => "IL xT Customs Error Message";

		protected override string ExpectedMessageTypesToInclude => "XER";

		public void TestExpectedMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "IL xT Customs Error Message", new XTERRMessageProcessor(new LoggingInformation()).MessageFriendlyName);
		}

		public void TestProcessMessage()
		{
			var factory = Factory;
			var message = factory.New<ILXERResponseMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ILCustoms;
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
		}

		protected override string BasicSuccessfulMessageText => null;

		protected override BusinessObject ExpectedLinkedObject => null;

		protected override ZGuid ExpectedBranchPk => GlbBranch.CurrentBranch.PK;

		protected override XTERRMessageProcessor CreateProcessor(LoggingInformation loggingInformation) => new XTERRMessageProcessor(new LoggingInformation());
	}
}
