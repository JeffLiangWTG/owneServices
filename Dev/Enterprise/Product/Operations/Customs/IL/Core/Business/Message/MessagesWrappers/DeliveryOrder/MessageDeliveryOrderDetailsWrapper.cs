using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderDetailsWrapper : IMessageDeliveryOrderDetails
	{
		MessageDeliveryOrderDetailsWrapper(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		public static IMessageDeliveryOrderDetails NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData, bool isCancelActionTypeCode)
			=> deliveryOrderDocData != null ? new MessageDeliveryOrderDetailsWrapper(deliveryOrderDocData, isCancelActionTypeCode) : null;

		IMessageDeliveryOrderGeneral IMessageDeliveryOrderDetails.General
			=> MessageDeliveryOrderGeneralWrapper.NewOrNull(deliveryOrderDocData);

		IMessageDeliveryOrderHeader IMessageDeliveryOrderDetails.Header
			=> MessageDeliveryOrderHeaderWrapper.NewOrNull(deliveryOrderDocData, isCancelActionTypeCode);

		IMessageDeliveryOrderProducerDetails IMessageDeliveryOrderDetails.ProducerDetails
			=> MessageDeliveryOrderProducerDetailsWrapper.NewOrNull(deliveryOrderDocData);

		IMessageDeliveryOrderReceiverDetails IMessageDeliveryOrderDetails.ReceiverDetails
			=> MessageDeliveryOrderReceiverDetailsWrapper.NewOrNull(deliveryOrderDocData);

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
		readonly bool isCancelActionTypeCode;
	}
}
