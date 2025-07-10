using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderProducerDetailsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrderProducerDetails>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderProducerDetailsWrapper.NewOrNull(null));
			AssertNotNull("When deliveryOrderDocData is not null", Provider);
		}

		public void TestProducerIdentityNumber()
		{
			AssertEquals(5006, Provider.ProducerIdentityNumber);
		}

		public void TestProducerTypeCode()
		{
			AssertEquals(5, Provider.ProducerTypeCode);
		}

		protected override IMessageDeliveryOrderProducerDetails GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			deliveryOrderDocData.ForwarderVat = "5006";
			return MessageDeliveryOrderProducerDetailsWrapper.NewOrNull(deliveryOrderDocData);
		}
	}
}
