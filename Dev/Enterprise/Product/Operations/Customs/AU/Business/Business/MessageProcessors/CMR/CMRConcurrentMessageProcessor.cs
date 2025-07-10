using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRConcurrentMessageProcessor : CMRBaseMessageProcessor
	{
		public CMRConcurrentMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[]
		{
			CMRMessage.CMRMessageTypes.SEI,
			CMRMessage.CMRMessageTypes.UBMREQE,
			CMRMessage.CMRMessageTypes.UBMREQR
		};
	}
}
