using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EmcsMessageProcessor<EmcsInboundEDIMessage<IEmcsDataProvider>, IEmcsDataProvider>))]
	abstract class EmcsMessageProcessorAbstractTest<T> : TestCaseWithFactory
		where T : EmcsMessageProcessor<EmcsInboundEDIMessage<IEmcsDataProvider>, IEmcsDataProvider>
	{
		public void TestProcessMessage()
		{
			EmcsMessageProcessor.ProcessMessage(EmcsInboundEDIMessage);

			CombineAssertions(() =>
			{
				var message = EmcsInboundEDIMessage;
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
			});
		}

		protected EmcsInboundEDIMessage<IEmcsDataProvider> EmcsInboundEDIMessage => emcsInboundEDIMessage ?? (emcsInboundEDIMessage = GetEmcsInboundEDIMessageToTest());
		EmcsInboundEDIMessage<IEmcsDataProvider> emcsInboundEDIMessage;

		protected T EmcsMessageProcessor => emcsMessageProcessor ?? (emcsMessageProcessor = GetEmcsMessageProcessor());
		T emcsMessageProcessor;

		protected abstract EmcsInboundEDIMessage<IEmcsDataProvider> GetEmcsInboundEDIMessageToTest();
		protected abstract T GetEmcsMessageProcessor();
	}
}
