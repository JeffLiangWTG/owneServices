using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.KR.Business
{
	public class KRCIncomingMessageProcessor : BaseMessageProcessor
	{
		public KRCIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new MessageProcessorFactory(Logger));
			return result;
		}
	}
}
