using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAllMessageProcessor : CMRBaseMessageProcessor
	{
		public CMRAllMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToExcludeCore => new ZString[]
		{
			CMRMessage.CMRMessageTypes.CONTRL,
			CMRMessage.CMRMessageTypes.CARST,
			CMRMessage.CMRMessageTypes.SEI,
			CMRMessage.CMRMessageTypes.UBMREQE,
			CMRMessage.CMRMessageTypes.UBMREQR
		};
	}
}
