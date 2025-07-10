using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCRequestTTCEReferenceFileInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCRequestTTCEReferenceFileInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override (ZString Type, ZString SubType) GetMessageTypeAndSubType(EDIInterchange interchange, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			if (universalEventData == null || universalEventData.MessageType == MessageConstants.MessageType.RES)
			{
				var outgoingMessage = BRMessageHelper.GetOutgoingMessage(interchange);
				var outgoingMessageSubType = outgoingMessage?.EM_MessageSubType ?? ZString.Empty;
				return (MessageTypeList.Codes.RTT, outgoingMessageSubType);
			}
			return base.GetMessageTypeAndSubType(interchange, responseMessage, universalEventData);
		}
	}
}
