using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderReceiverDetailsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrderReceiverDetails>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderReceiverDetailsWrapper.NewOrNull(null));
			AssertNotNull("When deliveryOrderDocData is not null", Provider);
		}

		public void TestCustomerIdentification()
		{
			AssertNotNull(Provider.CustomerIdentification);
			AssertType<CustomerIdentificationWrapper>(Provider.CustomerIdentification);
		}

		public void TestReceiverName()
		{
			AssertEquals("Customs Broker", Provider.ReceiverName);
		}

		public void TestReceiverTypeCode()
		{
			AssertEquals(3, Provider.ReceiverTypeCode);

			var shipment = Factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			deliveryOrderDocData.ReceiverType = new CodeDescription(new CodeDescriptionPairList());
			deliveryOrderDocData.ReceiverType.Code = "5";

			var wrapper = MessageDeliveryOrderReceiverDetailsWrapper.NewOrNull(deliveryOrderDocData);
			AssertEquals(5, wrapper.ReceiverTypeCode);
		}

		protected override IMessageDeliveryOrderReceiverDetails GetProvider()
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			var deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			deliveryOrderDocData.CustomsBroker = new Address(factory) { CompanyName = "Customs Broker" };
			deliveryOrderDocData.ReceiverType = new CodeDescription(new CodeDescriptionPairList());
			deliveryOrderDocData.ReceiverType.Code = "3";
			return MessageDeliveryOrderReceiverDetailsWrapper.NewOrNull(deliveryOrderDocData);
		}
	}
}
