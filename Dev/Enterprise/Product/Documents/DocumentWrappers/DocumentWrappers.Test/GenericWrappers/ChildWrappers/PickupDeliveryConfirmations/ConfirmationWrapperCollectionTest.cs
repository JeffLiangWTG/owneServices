using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PickupDeliveryConfirmationsWrapperCollection))]
	sealed class ConfirmationWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<PickupDeliveryConfirmationsWrapperCollection>
	{
		public void TestLoadFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapper freightWrapper = FreightWrapper.New(shipment, Factory)[0];
			CommonPickupDeliveryConfirmCollection pickupDeliveryConfirmCollection = new CommonPickupDeliveryConfirmCollection(Factory);
			PickupDeliveryConfirmationsWrapperCollection collection = new PickupDeliveryConfirmationsWrapperCollection(freightWrapper, Factory, pickupDeliveryConfirmCollection);

			AssertEquals("collection.Count", 0, collection.Count);

			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm pickupConfirmation = shipment.PickupConfirms.AddNew();
			PickupDeliveryConfirmationsWrapper pickupConfirmationWrapper = new PickupDeliveryConfirmationsWrapper(pickupConfirmation, Factory);
			collection = new PickupDeliveryConfirmationsWrapperCollection(freightWrapper, Factory, pickupDeliveryConfirmCollection);
			AssertEquals("collection.Count", 1, collection.Count);

			shipment.DeliveryConfirms.AddNew();
			collection = new PickupDeliveryConfirmationsWrapperCollection(freightWrapper, Factory, pickupDeliveryConfirmCollection);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		protected override PickupDeliveryConfirmationsWrapperCollection GetNewDocumentWrapperCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new PickupDeliveryConfirmationsWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CommonPickupDeliveryConfirm confirmation = shipment.DeliveryConfirms.AddNew();
			return new PickupDeliveryConfirmationsWrapper(confirmation, Factory);
		}
	}
}
