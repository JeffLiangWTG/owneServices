using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.ESumA.Testing
{
	[TestedType(typeof(ESumAENSACKMessageProcessor))]
	class ESumAENSACKMessageProcessorTest : MessageProcessorAbstractTest<ESumAENSACKMessageProcessor, AtlasInboundEDIMessage<IESumADataProvider>>
	{
		protected override ZString MessageFriendlyName => "ESumA ENSACK Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>> Processor => new ESumAENSACKMessageProcessor(logger);
	}
}
