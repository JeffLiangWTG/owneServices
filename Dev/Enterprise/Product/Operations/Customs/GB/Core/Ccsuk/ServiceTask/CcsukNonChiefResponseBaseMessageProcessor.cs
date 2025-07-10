using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTask
{
	public class CcsukNonChiefResponseBaseMessageProcessor : BaseMessageProcessor
	{
		public CcsukNonChiefResponseBaseMessageProcessor(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new CcsukNonChiefResponseProcessor(serviceLogger, Logger));
			return result;
		}

		protected override void SortProcessableMessageEvenFurther(EDIMessage[] messages)
		{
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;

		readonly ILogger serviceLogger;
	}
}
