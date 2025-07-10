using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Pentant
{
	public class PentantProcessor : BaseMessageProcessor
	{
		readonly ILogger serviceLogger;

		public PentantProcessor(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new PentantReportApplicationTypeMessageProcessor(serviceLogger, this));
			return result;
		}
	}
}
