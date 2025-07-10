using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public interface IMessageDataProviderFactory
{
	IInboundMessageDataProvider GetMessageDataProvider(EDIMessage message);
}
