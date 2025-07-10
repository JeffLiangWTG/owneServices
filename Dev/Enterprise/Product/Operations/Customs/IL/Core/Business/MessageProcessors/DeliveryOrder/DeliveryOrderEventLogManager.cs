using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	class DeliveryOrderEventLogManager : ElectronicFormEventLogManager<MnNg1220Msg22DeliveryOrderFeedBackMessage>
	{
		protected override string MessageType => ILMessageEventParameter.DeliveryOrderDocumentName;

		protected override Event DetermineEventType(MnNg1220Msg22DeliveryOrderFeedBackMessage response, EnterpriseBusinessObject enterpriseBusinessObject)
		{
			var isWithdrawResponse = IsWithdrawResponse(enterpriseBusinessObject);
			switch ((isWithdrawResponse, IsMessageRejected(response, isWithdrawResponse)))
			{
				case (true, true):
					return Events.MessageRejected;
				case (false, true):
					return Events.MessageRejected;
				case (true, false):
					return Events.MessageWithdrawCancelAccepted;
				case (false, false):
					return Events.MessageAccepted;
			}
		}

		protected override string GetReferenceNumberEventParameter(EnterpriseBusinessObject enterpriseBusinessObject)
			=> ((ForwardingShipment)enterpriseBusinessObject).JS_DLO;

		protected override string GetResponseEventParameter(Event eventType, MnNg1220Msg22DeliveryOrderFeedBackMessage response)
			=> null;

		bool IsMessageRejected(MnNg1220Msg22DeliveryOrderFeedBackMessage responseMessage, bool isWithdrawResponse)
		{
			var deliveryOrderResponseStatus = responseMessage.DeliveryOrderResponse?.FirstOrDefault()?.ResponseStatus.ToString();

			return !(deliveryOrderResponseStatus == DeliveryOrderResponseStatusList.Codes.Accepted || deliveryOrderResponseStatus == DeliveryOrderResponseStatusList.Codes.ReceivedWithErrors);
		}
	}
}
