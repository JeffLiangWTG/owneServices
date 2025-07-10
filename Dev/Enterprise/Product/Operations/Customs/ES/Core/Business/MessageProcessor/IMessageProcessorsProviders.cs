using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public interface IESExitControlMessageProcessorsProvider
	{
		IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger);
	}

	public interface IESNctsMessageProcessorsProvider
	{
		IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger);
	}

	public interface IESG3MessageProcessorsProvider
	{
		IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger);
	}

	public interface IESG5MessageProcessorsProvider
	{
		IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger);
	}

	public interface IESH7MessageProcessorsProvider
	{
		IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger);
	}
}
