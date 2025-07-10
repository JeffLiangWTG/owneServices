using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class InboxNotificationMessageBuilder : XMLMessageBuilder<IInboxNotificationMessageDataProvider, ListaDecV4Ent>
	{
		public InboxNotificationMessageBuilder(IInboxNotificationMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ListaDecV4Ent GenerateXMLMessage()
		{
			return new ListaDecV4Ent()
			{
				TipoRespuesta = provider.ResponseType,
				Declarante = new DeclaranteType()
				{
					NifDeclarante = provider.DeclarantID,
					NombreDeclarante = provider.DeclarantName,
				}
			};
		}
	}
}
