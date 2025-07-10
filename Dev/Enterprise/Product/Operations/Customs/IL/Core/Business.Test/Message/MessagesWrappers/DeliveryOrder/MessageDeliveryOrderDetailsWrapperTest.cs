using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderDetailsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrderDetails>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderWrapper.NewOrNull(null, false));
			AssertNotNull("When shipment deliveryOrderDocData is not null", Provider);
		}

		public void TestGeneral()
		{
			AssertNotNull(Provider.General);
			AssertType<MessageDeliveryOrderGeneralWrapper>(Provider.General);
		}

		public void TestHeader()
		{
			AssertNotNull(Provider.Header);
			AssertType<MessageDeliveryOrderHeaderWrapper>(Provider.Header);
		}

		public void TestProducerDetails()
		{
			AssertNotNull(Provider.ProducerDetails);
			AssertType<MessageDeliveryOrderProducerDetailsWrapper>(Provider.ProducerDetails);
		}

		public void TestReceiverDetails()
		{
			AssertNotNull(Provider.ReceiverDetails);
			AssertType<MessageDeliveryOrderReceiverDetailsWrapper>(Provider.ReceiverDetails);
		}

		protected override IMessageDeliveryOrderDetails GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			return MessageDeliveryOrderDetailsWrapper.NewOrNull(deliveryOrderDocData, false);
		}
	}
}
