namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	using System.Collections.Generic;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;

	public class McpRRA01RRA11AndRRA06MessageProcessor : BaseMessageProcessor
	{
		public McpRRA01RRA11AndRRA06MessageProcessor(ILogger serviceLogger)
			: base()
		{
			this.serviceLogger = serviceLogger;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new McpRRA01RRA11AndRRA06ApplicationMessageProcessor(Logger, serviceLogger, this));
			return result;
		}

		readonly ILogger serviceLogger;
	}
}
