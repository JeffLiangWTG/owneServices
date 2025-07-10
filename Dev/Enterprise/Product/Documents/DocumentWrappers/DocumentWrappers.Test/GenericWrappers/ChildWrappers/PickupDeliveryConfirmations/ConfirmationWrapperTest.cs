using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PickupDeliveryConfirmationsWrapper))]
	sealed class ConfirmationWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PickupDeliveryConfirmationsWrapper wrapperEmpty = new PickupDeliveryConfirmationsWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.PackagesConfirmed", 0, wrapperEmpty.PackagesConfirmed);
			AssertEquals("wrapperEmpty.SignedFor", ZString.Empty, wrapperEmpty.SignedFor);
			AssertEquals("wrapperEmpty.PickupDeliveryTime", ZDateTime.Empty, wrapperEmpty.PickupDeliveryTime);
			AssertEquals("wrapperEmpty.Notes", ZString.Empty, wrapperEmpty.Notes);
			AssertEquals("wrapperEmpty.GatePassID", ZString.Empty, wrapperEmpty.GatePassID);
			AssertEquals("wrapperEmpty.DriversName", ZString.Empty, wrapperEmpty.DriversName);
			AssertEquals("wrapperEmpty.VehicleReg", ZString.Empty, wrapperEmpty.VehicleReg);
			AssertEquals("wrapperEmpty.TransportCoName", ZString.Empty, wrapperEmpty.TransportCoName);
			AssertEquals("wrapperEmpty.DeliveredWeight", ZDecimal.Zero, wrapperEmpty.DeliveredWeight);
			AssertEquals("wrapperEmpty.DeliveredVolume", ZDecimal.Zero, wrapperEmpty.DeliveredVolume);
		}

		public void TestWrapperMappingFull()
		{
			#region Setup

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapper freightWrapper = FreightWrapper.New(shipment, Factory)[0];
			CommonPickupDeliveryConfirmCollection pickupDeliveryConfirmCollection = new CommonPickupDeliveryConfirmCollection(Factory);
			PickupDeliveryConfirmationsWrapperCollection collection = new PickupDeliveryConfirmationsWrapperCollection(freightWrapper, Factory, pickupDeliveryConfirmCollection);

			ForwardingPackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 11;
			CommonPickupDeliveryConfirm pickupConfirmation = shipment.PickupConfirms.AddNew();
			pickupConfirmation.EU_GoodsSignForBy = "JJLo";
			pickupConfirmation.EU_PickupDeliveryTime = new ZDateTime(2008, 8, 6);
			pickupConfirmation.EU_PickupDeliveryInstruction = "pickupConfirmation.EU_PickupDeliveryInstruction";
			pickupConfirmation.EU_DriversName = "Driver Name";
			pickupConfirmation.EU_VehicleRegistration = "VR111";
			pickupConfirmation.EU_TransportCoName = "ABC Co";
			shipment.JS_UniqueConsignRef = "SP111";
			pickupConfirmation.UniqueID = "12";
			pickupConfirmation.TotalDeliveredWeight = new ZDecimal("6.000");
			pickupConfirmation.TotalDeliveredVolume = new ZDecimal("3");

			PickupDeliveryConfirmationsWrapper pickupConfirmationWrapperFull = new PickupDeliveryConfirmationsWrapper(pickupConfirmation, Factory);
			pickupConfirmationWrapperFull.ParentWrapper = FreightWrapper.New(Factory.New<ForwardingShipment>(), Factory)[0];

			ForwardingPackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 22;
			CommonPickupDeliveryConfirm deliveryConfirmation = shipment.DeliveryConfirms.AddNew();
			deliveryConfirmation.EU_GoodsSignForBy = "JJHi";
			deliveryConfirmation.EU_PickupDeliveryTime = new ZDateTime(2008, 8, 8);
			deliveryConfirmation.EU_PickupDeliveryInstruction = "deliveryConfirmation.EU_PickupDeliveryInstruction";
			deliveryConfirmation.EU_DriversName = "Driver Name1";
			deliveryConfirmation.EU_VehicleRegistration = "VR1112";
			deliveryConfirmation.EU_TransportCoName = "XYZ Co";
			deliveryConfirmation.TotalDeliveredWeight = new ZDecimal("5.000");
			deliveryConfirmation.TotalDeliveredVolume = new ZDecimal("4.000");

			PickupDeliveryConfirmationsWrapper deliveryConfirmationWrapperFull = new PickupDeliveryConfirmationsWrapper(deliveryConfirmation, Factory);
			deliveryConfirmationWrapperFull.ParentWrapper = FreightWrapper.New(Factory.New<ForwardingShipment>(), Factory)[0];

			#endregion

			AssertEquals("pickupConfirmationWrapperFull.ToString()", "JJLo", pickupConfirmationWrapperFull.ToString());
			AssertEquals("pickupConfirmationWrapperFull.PackagesConfirmed", 11, pickupConfirmationWrapperFull.PackagesConfirmed);
			AssertEquals("pickupConfirmationWrapperFull.SignedFor", "JJLo", pickupConfirmationWrapperFull.SignedFor);
			AssertEquals("pickupConfirmationWrapperFull.PickupDeliveryTime", new ZDateTime(2008, 8, 6), pickupConfirmationWrapperFull.PickupDeliveryTime);
			AssertEquals("pickupConfirmation.EU_PickupDeliveryInstruction", "pickupConfirmation.EU_PickupDeliveryInstruction", pickupConfirmationWrapperFull.Notes);
			AssertEquals("pickupConfirmationWrapperFull.DriversName", "Driver Name", pickupConfirmationWrapperFull.DriversName);
			AssertEquals("pickupConfirmationWrapperFull.VehicleReg", "VR111", pickupConfirmationWrapperFull.VehicleReg);
			AssertEquals("pickupConfirmationWrapperFull.TransportCoName", "ABC Co", pickupConfirmationWrapperFull.TransportCoName);
			AssertEquals("pickupConfirmationWrapperFull.DeliveredWeight", (ZDecimal)6.000, pickupConfirmationWrapperFull.DeliveredWeight);
			AssertEquals("pickupConfirmationWrapperFull.DeliveredVolume", (ZDecimal)3, pickupConfirmationWrapperFull.DeliveredVolume);

			AssertEquals("deliveryConfirmationWrapperFull.ToString()", "JJHi", deliveryConfirmationWrapperFull.ToString());
			AssertEquals("deliveryConfirmationWrapperFull.PackagesConfirmed", 33, deliveryConfirmationWrapperFull.PackagesConfirmed);
			AssertEquals("deliveryConfirmationWrapperFull.SignedFor", "JJHi", deliveryConfirmationWrapperFull.SignedFor);
			AssertEquals("deliveryConfirmationWrapperFull.PickupDeliveryTime", new ZDateTime(2008, 8, 8), deliveryConfirmationWrapperFull.PickupDeliveryTime);
			AssertEquals("deliveryConfirmation.EU_PickupDeliveryInstruction", "deliveryConfirmation.EU_PickupDeliveryInstruction", deliveryConfirmationWrapperFull.Notes);
			AssertEquals("deliveryConfirmationWrapperFull.DriversName", "Driver Name1", deliveryConfirmationWrapperFull.DriversName);
			AssertEquals("deliveryConfirmationWrapperFull.VehicleRegistration", "VR1112", deliveryConfirmationWrapperFull.VehicleReg);
			AssertEquals("deliveryConfirmationWrapperFull.TransportCoName", "XYZ Co", deliveryConfirmationWrapperFull.TransportCoName);
			AssertEquals("deliveryConfirmationWrapperFull.DeliveredWeight", (ZDecimal)5.000, deliveryConfirmationWrapperFull.DeliveredWeight);
			AssertEquals("deliveryConfirmationWrapperFull.DeliveredVolume", (ZDecimal)4.000, deliveryConfirmationWrapperFull.DeliveredVolume);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PickupDeliveryConfirmations                 (Default Field: SignedFor)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConfirmationType                        String
DeliveredVolume                         Decimal
DeliveredWeight                         Decimal
DriversName                             String
GatePassID                              String
Notes                                   String
PackagesConfirmed                       Int
PickupDeliveryTime                      DateTime
SignedFor                               String
TransportCoName                         String
VehicleReg                              String

Containers                              Container Collection
Packages                                Package Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CommonPickupDeliveryConfirm confirmation = Factory.New<CommonPickupDeliveryConfirm>();
			return new PickupDeliveryConfirmationsWrapper(confirmation, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CommonPickupDeliveryConfirm confirmation = Factory.New<CommonPickupDeliveryConfirm>();
			return new PickupDeliveryConfirmationsWrapper(confirmation, Factory);
		}
	}
}
