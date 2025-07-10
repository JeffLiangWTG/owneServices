namespace Enterprise.Customs.GB.MCP.ServiceTasks.Misc
{
	using System.Collections.Generic;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;

	/// <summary>
	/// Simply plumbs a MiscTextAndIslEmailsToEdiMessagesPoller to a MiscTextAndIslApplicationTypeMessageProcessor.  It's a BaseMessageProcessor.
	/// </summary>
	public class MiscTextAndIslBaseMessageProcessor : BaseMessageProcessor
	{
		public MiscTextAndIslBaseMessageProcessor(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new MiscTextAndIslApplicationTypeMessageProcessor(ServiceLogger));
			return result;
		}
		ILogger ServiceLogger { get; set; }
	}
}
