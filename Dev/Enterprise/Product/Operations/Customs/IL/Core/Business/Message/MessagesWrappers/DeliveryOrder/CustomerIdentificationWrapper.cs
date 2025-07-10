using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class CustomerIdentificationWrapper : ICustomerIdentification
	{
		public CustomerIdentificationWrapper(DeliveryOrderDocDataObject deliveryOrderDocData)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
		}

		public static ICustomerIdentification NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData)
			=> deliveryOrderDocData == null ? null : new CustomerIdentificationWrapper(deliveryOrderDocData);

		int? ICustomerIdentification.ExternalId
			=> int.TryParse(deliveryOrderDocData.CustomsBrokerVat, out int result) ? result : ZInt.Zero;

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}
