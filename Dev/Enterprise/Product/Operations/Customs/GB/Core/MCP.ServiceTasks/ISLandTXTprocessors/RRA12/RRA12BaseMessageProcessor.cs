namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	using System.Collections.Generic;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;

	/// <summary>
	/// Simply plumbs a RRA12EmailsToEdiMessagesPoller to a RRA12ApplicationTypeMessageProcessor.  It's a BaseMessageProcessor.
	/// </summary>
	public class RRA12BaseMessageProcessor : BaseMessageProcessor
	{
		public RRA12BaseMessageProcessor(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new RRA12ApplicationTypeMessageProcessor(ServiceLogger, this));
			return result;
		}
		ILogger ServiceLogger { get; set; }
	}
}
