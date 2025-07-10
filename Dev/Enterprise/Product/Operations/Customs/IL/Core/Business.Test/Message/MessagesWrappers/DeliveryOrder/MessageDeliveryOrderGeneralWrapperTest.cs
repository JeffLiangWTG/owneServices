using CargoWise.Customs.IL.MessageDefinitions.DLO;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageDeliveryOrderGeneralWrapperTest : Customs.Business.Testing.DataProviderTestCase<IMessageDeliveryOrderGeneral>
	{
		public void TestNewOrNull()
		{
			AssertNull("When deliveryOrderDocData is null", MessageDeliveryOrderGeneralWrapper.NewOrNull(null));
			AssertNotNull("When deliveryOrderDocData is not null", Provider);
		}

		public void TestDeliverySitenumber()
		{
			AssertEquals("1234", Provider.DeliverySitenumber);
		}

		public void TestEndorsementCode()
		{
			AssertEquals(1, Provider.EndorsementCode);
		}

		protected override IMessageDeliveryOrderGeneral GetProvider()
			=> MessageDeliveryOrderGeneralWrapper.NewOrNull(deliveryOrderDocData);

		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<ForwardingShipment>();
			deliveryOrderDocData = new DeliveryOrderDocDataObject(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, shipment.Factory);
			deliveryOrderDocData.DeliverySite = new CodeDescription(new CodeDescriptionPairList());
			deliveryOrderDocData.DeliverySite.Code = "1234";
		}

		DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}
