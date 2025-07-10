using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.DETALLEV4ENT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class InboxNotificationDetailV4MessageBuilder : InboxNotificationDetailCommonMessageBuilder<IInboxNotificationDetailCommonMessageDataProvider, DetalleV4Ent>
	{
		public InboxNotificationDetailV4MessageBuilder(IInboxNotificationDetailCommonMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override DetalleV4Ent GenerateXMLMessage() => GenerateDetailXMLMessage<DetalleV4Ent>();
	}
}
