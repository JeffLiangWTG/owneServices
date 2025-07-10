using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public sealed class H7MessageProcessorsProvider : IESH7MessageProcessorsProvider
	{
		public IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger)
		{
			return new List<BranchCustomsApplicationTypeMessageProcessor>
			{
				new DeclarationH7ResponseMessageProcessor(logger),
				new AnnexH7ResponseMessageProcessor(logger),
				new ReexportH7ResponseMessageProcessor(logger),
				new CancellationH7ResponseMessageProcessor(logger),
				new QueryH7ResponseMessageProcessor(logger),
			};
		}
	}
}
