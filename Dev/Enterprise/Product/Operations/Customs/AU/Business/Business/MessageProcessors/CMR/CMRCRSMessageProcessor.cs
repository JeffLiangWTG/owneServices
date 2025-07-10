using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCRSMessageProcessor : CMRBaseMessageProcessor
	{
		public CMRCRSMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool RequiresPreProcessingCore => false;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { CMRMessage.CMRMessageTypes.CARST };
	}
}
