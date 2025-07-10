using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderGeneralWrapper : IMessageDeliveryOrderGeneral
	{
		public MessageDeliveryOrderGeneralWrapper(DeliveryOrderDocDataObject deliveryOrderDocData)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
		}

		public static IMessageDeliveryOrderGeneral NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData) => deliveryOrderDocData == null ? null : new MessageDeliveryOrderGeneralWrapper(deliveryOrderDocData);

		string IMessageDeliveryOrderGeneral.DeliverySitenumber
			=> deliveryOrderDocData.DeliverySite.Code;

		int IMessageDeliveryOrderGeneral.EndorsementCode => CustomsDeliveryOrder.EndorsementCode;

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}
