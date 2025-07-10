
namespace Enterprise.Customs.GB.Chief.CusRes
{
	using System.Collections.Generic;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;

	/// <summary>
	/// Processor called by McpResponseMessageProcessor (which is called by a service). Will load a CusResResponseProcessor.
	/// </summary>
	public class CusResResponseMessageProcessor : BaseMessageProcessor
	{
		public CusResResponseMessageProcessor(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new CusResResponseProcessor(serviceLogger, Logger));
			return result;
		}

		readonly ILogger serviceLogger;

		protected override void SortProcessableMessageEvenFurther(EDIMessage[] messages)
		{
			// no, thank you, GetProcessableMessagesOrder is quite enough.
		}
	}
}
