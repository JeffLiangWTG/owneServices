using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class InboxNotificationDetailCommonMessageBuilder<TProvider, TObject> : XMLMessageBuilder<TProvider, TObject>
		where TProvider : IInboxNotificationDetailCommonMessageDataProvider
	{
		protected InboxNotificationDetailCommonMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected T GenerateDetailXMLMessage<T>()
			where T : IDetailCommon, new()
		{
			return new T()
			{
				Key = provider.Key
			};
		}
	}
}
