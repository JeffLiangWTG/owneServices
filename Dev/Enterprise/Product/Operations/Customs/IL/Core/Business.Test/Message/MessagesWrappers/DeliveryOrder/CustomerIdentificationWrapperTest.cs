using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CustomerIdentificationWrapperTest : Customs.Business.Testing.DataProviderTestCase<ICustomerIdentification>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", CustomerIdentificationWrapper.NewOrNull(null));
			AssertNotNull("When deliveryOrderDocData is not null", Provider);
		}

		public void TestExternalId()
		{
			AssertEquals(4006, Provider.ExternalId);
		}

		protected override ICustomerIdentification GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			deliveryOrderDocData.CustomsBrokerVat = "4006";

			return CustomerIdentificationWrapper.NewOrNull(deliveryOrderDocData);
		}
	}
}
