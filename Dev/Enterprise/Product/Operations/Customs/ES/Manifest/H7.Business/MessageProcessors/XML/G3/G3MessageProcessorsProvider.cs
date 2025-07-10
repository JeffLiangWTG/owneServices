using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public sealed class G3MessageProcessorsProvider : IESG3MessageProcessorsProvider
	{
		public IEnumerable<BranchCustomsApplicationTypeMessageProcessor> GetProcessors(LoggingInformation logger)
		{
			return new List<BranchCustomsApplicationTypeMessageProcessor>
			{
				new G3DeclarationResponseMessageProcessor(logger),
				new G3RevokeResponseMessageProcessor(logger),
			};
		}
	}
}
