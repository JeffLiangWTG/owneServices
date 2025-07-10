using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public EXDOCApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			EXDOCMessageProcessor messageProcessor = new EXDOCMessageProcessor(Logger);
			messageProcessor.ProcessMessage(message);
		}

		protected override string MessageFriendlyNameCore
		{
			get { return ""; }
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.EXDOC;
	}
}
