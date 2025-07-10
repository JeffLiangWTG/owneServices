using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.ESumA.Testing
{
	[TestedType(typeof(ESumAENSSTAMessageProcessor))]
	class ESumAENSSTAMessageProcessorTest : MessageProcessorAbstractTest<ESumAENSSTAMessageProcessor, AtlasInboundEDIMessage<IESumADataProvider>>
	{
		protected override ZString MessageFriendlyName => "ESumA ENSSTA Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>> Processor => new ESumAENSSTAMessageProcessor(logger);
	}
}
