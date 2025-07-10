using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderHeaderWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrderHeader>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderHeaderWrapper.NewOrNull(null, false));
			AssertNotNull("When deliveryOrderDocData is not null", Provider);
		}

		public void TestActionTypeCode()
		{
			AssertEquals("When Is Cancel", 1, Provider.ActionTypeCode);

			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);

			var wrapper = MessageDeliveryOrderHeaderWrapper.NewOrNull(deliveryOrderDocData, false);
			AssertEquals("When Is New", 2, wrapper.ActionTypeCode);
		}

		public void TestCargoIdentifier()
		{
			AssertNotNull(Provider.CargoIdentifier);
			AssertType<CargoIdentifierWrapper>(Provider.CargoIdentifier);
		}

		[TestDate(2024, 09, 09)]
		public void TestDeliveryOrderDate()
		{
			AssertEquals(new ZDateTime(2024, 09, 09), Provider.DeliveryOrderDate);
		}

		public void TestDeliveryOrderNumber()
		{
			AssertEquals(0, Provider.DeliveryOrderNumber);
		}

		protected override IMessageDeliveryOrderHeader GetProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);

			deliveryOrderDocData.DealNumber = "1000123";

			return MessageDeliveryOrderHeaderWrapper.NewOrNull(deliveryOrderDocData, true);
		}
	}
}
