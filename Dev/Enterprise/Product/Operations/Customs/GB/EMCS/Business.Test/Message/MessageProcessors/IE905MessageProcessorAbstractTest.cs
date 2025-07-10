using CargoWise.Types;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE905MessageProcessor))]
	abstract class IE905MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE905MessageProcessor, IIE905>
	{
		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE905;

		protected override IE905MessageProcessor Processor => new IE905MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
		}
	}
}
