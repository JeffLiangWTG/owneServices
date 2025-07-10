using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class MessageDeliveryOrderReceiverDetailsWrapper : IMessageDeliveryOrderReceiverDetails
	{
		MessageDeliveryOrderReceiverDetailsWrapper(DeliveryOrderDocDataObject deliveryOrderDocData)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
		}
		public static IMessageDeliveryOrderReceiverDetails NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData)
			=> deliveryOrderDocData == null ? null : new MessageDeliveryOrderReceiverDetailsWrapper(deliveryOrderDocData);

		ICustomerIdentification IMessageDeliveryOrderReceiverDetails.CustomerIdentification
			=> CustomerIdentificationWrapper.NewOrNull(deliveryOrderDocData);

		string IMessageDeliveryOrderReceiverDetails.ReceiverName
			=> deliveryOrderDocData.CustomsBroker?.CompanyName;

		int IMessageDeliveryOrderReceiverDetails.ReceiverTypeCode => int.TryParse(deliveryOrderDocData.ReceiverType.Code, out var result) ? result : ZInt.Zero;
		
		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}
