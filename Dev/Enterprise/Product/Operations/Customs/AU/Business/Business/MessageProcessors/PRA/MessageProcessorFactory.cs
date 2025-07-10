using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageProcessorFactory : ApplicationTypeMessageProcessor
	{
		public MessageProcessorFactory(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.OneStop;

		protected override string MessageFriendlyNameCore
		{
			get { return "PRA Response"; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			MessageProcessor messageProcessor = new MessageProcessor(Logger);
			messageProcessor.ProcessMessage(message);
		}
	}
}
