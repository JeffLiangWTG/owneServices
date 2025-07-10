using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public sealed class G5MessageProcessorsProvider : IESG5MessageProcessorsProvider
{
	public IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger)
	{
		return new List<BranchCustomsApplicationTypeMessageProcessor>
		{
			new ExpeditionG5ResponseMessageProcessor(logger),
			new ExpAmendmentG5ResponseMessageProcessor(logger),
			new ReceptionG5ResponseMessageProcessor(logger),
			new ExpCancelG5ResponseMessageProcessor(logger),
			new G5ClearanceEmailResponseMessageProcessor(logger),
		};
	}
}
