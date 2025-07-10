using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.ESumA.Testing
{
	[TestedType(typeof(ESumAAIVNOTMessageProcessor))]
	class ESumAAIVNOTMessageProcessorTest : MessageProcessorAbstractTest<ESumAAIVNOTMessageProcessor, AtlasInboundEDIMessage<IESumADataProvider>>
	{
		protected override ZString MessageFriendlyName => "ESumA AIVNOT Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>> Processor => new ESumAAIVNOTMessageProcessor(logger);
	}
}
