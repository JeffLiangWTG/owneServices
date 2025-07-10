using CargoWise.Types;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE837MessageProcessor))]
	abstract class IE837MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE837MessageProcessor, IIE837>
	{
		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE837;

		protected override IE837MessageProcessor Processor => new IE837MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
		}
	}
}
