namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	public interface IUniversalCustomsMessagingSubscribersProvider
	{
		IUniversalCustomsMessageProcessor GetMessageProcessor(string applicationCode);
	}
}
