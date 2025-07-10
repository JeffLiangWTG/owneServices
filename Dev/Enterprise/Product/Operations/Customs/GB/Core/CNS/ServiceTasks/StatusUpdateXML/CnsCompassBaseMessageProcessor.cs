namespace Enterprise.Customs.GB.CNS
{
	using System.Collections.Generic;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.MessageProcessors;

	public class CnsCompassBaseMessageProcessor : BaseMessageProcessor
	{
		public CnsCompassBaseMessageProcessor(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new CnsXmlApplicationTypeMessageProcessor(ServiceLogger, this));
			result.Add(new CnsTextApplicationTypeMessageProcessor(ServiceLogger, this));
			return result;
		}

		ILogger ServiceLogger { get; set; }
	}
}
