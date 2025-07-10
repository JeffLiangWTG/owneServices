using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IInboxNotificationMessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString ResponseType { get; }
		ZString DeclarantName { get; }
		ZString DeclarantID { get; }
	}
}
