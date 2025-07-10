using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.PHS11
{
	public class PHS11BaseMessageProcessor : BaseMessageProcessor
	{
		public PHS11BaseMessageProcessor(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new PHS11ApplicationTypeMessageProcessor(ServiceLogger, this));
			return result;
		}

		ILogger ServiceLogger { get; set; }
	}
}
