using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCPushNotificationInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCPushNotificationInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override (ZString Type, ZString SubType) GetMessageTypeAndSubType(EDIInterchange interchange, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			var messageType = MessageTypeList.Codes.PUS;
			var messageSubType = ZString.Empty;

			if (BRMessageHelper.DeserializeObject<DueEvent>(responseMessage, throwExceptionIfOccurs: false) is DueEvent jsonDueEvent
				&& jsonDueEvent.due != null)
			{
				messageSubType = EDIMessageSubTypeList.Codes.Export;
			}
			else if (BRMessageHelper.DeserializeObject<DuimpEvent>(responseMessage, throwExceptionIfOccurs: false) is DuimpEvent jsonDuimpEvent
				&& !string.IsNullOrEmpty(jsonDuimpEvent.identificacao?.numero))
			{
				messageSubType = EDIMessageSubTypeList.Codes.Import;
			}
			else if (BRMessageHelper.DeserializeObject<LpcoEvent>(responseMessage, throwExceptionIfOccurs: false) is LpcoEvent jsonLpcoEvent
				&& !string.IsNullOrEmpty(jsonLpcoEvent.numeroLPCO))
			{
				messageSubType = EDIMessageSubTypeList.Codes.LPCO;
			}
			else if (BRMessageHelper.DeserializeObject<ProductCatalogEvent>(responseMessage, throwExceptionIfOccurs: false) is ProductCatalogEvent jsonProductCatalogEvent
				&& !string.IsNullOrEmpty(jsonProductCatalogEvent.cpfCnpjRaiz))
			{
				messageSubType = EDIMessageSubTypeList.Codes.GoodsCatalog;
			}

			return (messageType, messageSubType);
		}
	}
}
