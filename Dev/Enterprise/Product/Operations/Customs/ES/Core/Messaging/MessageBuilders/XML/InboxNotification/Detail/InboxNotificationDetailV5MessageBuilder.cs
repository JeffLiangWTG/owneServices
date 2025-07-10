using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.DETALLEV5ENT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class InboxNotificationDetailV5MessageBuilder : InboxNotificationDetailCommonMessageBuilder<IInboxNotificationDetailCommonMessageDataProvider, DetalleV5Ent>
	{
		public InboxNotificationDetailV5MessageBuilder(IInboxNotificationDetailCommonMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override DetalleV5Ent GenerateXMLMessage() => GenerateDetailXMLMessage<DetalleV5Ent>();
	}
}
