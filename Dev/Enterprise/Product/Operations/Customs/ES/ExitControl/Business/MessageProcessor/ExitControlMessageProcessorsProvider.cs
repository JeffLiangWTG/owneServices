using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public sealed class ExitControlMessageProcessorsProvider : IESExitControlMessageProcessorsProvider
	{
		public IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger)
		{
			return new List<BranchCustomsApplicationTypeMessageProcessor>
			{
				new EALInboxNotificationExitNonConformityAESResponseMessageProcessor(logger),
				new EALInboxNotificationExitClearanceAESResponseMessageProcessor(logger),
				new EALAESResponseMessageProcessor(logger)
			};
		}
	}
}
