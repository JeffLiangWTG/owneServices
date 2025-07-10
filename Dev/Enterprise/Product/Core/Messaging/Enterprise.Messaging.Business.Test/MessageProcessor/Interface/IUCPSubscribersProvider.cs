namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	public interface IUCPSubscribersProvider
	{
		IUniversalCustomsEDIMessagePacker GetMessagePacker(string applicationCode);
	}
}
