using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCTLMessageProcessor : CMRBaseMessageProcessor
	{
		public CMRCTLMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { CMRMessage.CMRMessageTypes.CONTRL };
	}
}
