using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrder>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderWrapper.NewOrNull(null, false));
			AssertNotNull("When shipment deliveryOrderDocData is not null", Provider);
		}

		public void TestDeliveryOrder()
		{
			AssertNotNull(Provider.DeliveryOrder);
			AssertType<MessageDeliveryOrderDetailsWrapper>(Provider.DeliveryOrder.Single());
		}

		public void TestRequestContentHeader()
		{
			AssertNotNull(Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		protected override IMessageDeliveryOrder GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			return MessageDeliveryOrderWrapper.NewOrNull(deliveryOrderDocData, false);
		}
	}
}
