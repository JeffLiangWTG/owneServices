using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public interface IMessageProcessorFactory
{
	IMessageProcessor<IInboundMessageDataProvider> GetMessageProcessor(EDIMessage message);
}
