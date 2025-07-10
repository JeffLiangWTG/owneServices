using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IInboxNotificationDetailCommonMessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString Key { get; }
	}
}
