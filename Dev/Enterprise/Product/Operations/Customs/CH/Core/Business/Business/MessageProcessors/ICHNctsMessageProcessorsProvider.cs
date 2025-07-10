using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public interface ICHNctsMessageProcessorsProvider
{
	IEnumerable<ApplicationTypeMessageProcessor> GetMessageProcessors(LoggingInformation logger);
}
