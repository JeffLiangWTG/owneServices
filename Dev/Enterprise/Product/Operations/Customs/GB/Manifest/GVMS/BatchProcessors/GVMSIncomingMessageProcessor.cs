using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSIncomingMessageProcessor : BaseMessageProcessor<GVMSEDIMessage>
	{
		public GVMSIncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new GVMSResponseMessageProcessor(Logger));
			return result;
		}
	}
}
