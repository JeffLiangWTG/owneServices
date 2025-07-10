using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public interface IMessageAttacheeProvider<T> where T : IInboundMessageDataProvider
{
	IMessageAttachee GetAttachee(EDIMessage message, T dataProvider);
}
