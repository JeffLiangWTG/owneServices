namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	public interface IUCUSubscribersProvider
	{
		IUniversalCustomsInterchangeUnpacker GetInterchangeUnpacker(string applicationCode);
	}
}
