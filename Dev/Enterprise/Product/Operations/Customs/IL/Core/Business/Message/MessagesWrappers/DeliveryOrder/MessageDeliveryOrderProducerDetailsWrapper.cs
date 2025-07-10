using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderProducerDetailsWrapper : IMessageDeliveryOrderProducerDetails
	{
		MessageDeliveryOrderProducerDetailsWrapper(DeliveryOrderDocDataObject deliveryOrderDocData)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
		}

		public static IMessageDeliveryOrderProducerDetails NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData)
			=> deliveryOrderDocData == null ? null : new MessageDeliveryOrderProducerDetailsWrapper(deliveryOrderDocData);

		int IMessageDeliveryOrderProducerDetails.ProducerIdentityNumber
			=> int.TryParse(deliveryOrderDocData.ForwarderVat, out var customsReg) ? customsReg : ZInt.Zero;

		int IMessageDeliveryOrderProducerDetails.ProducerTypeCode => CustomsDeliveryOrder.ProducerTypeCode;

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}
