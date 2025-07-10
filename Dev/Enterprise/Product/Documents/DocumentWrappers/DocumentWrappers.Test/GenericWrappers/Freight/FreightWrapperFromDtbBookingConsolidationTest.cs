using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDtbBookingConsolidation))]
	sealed class FreightWrapperFromDtbBookingConsolidationTest : FreightWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ERASA";
			shipment.JS_RL_NKDestination = "NZAKL";

			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			bookingConsolidation.KB_JobID = "CB01020304";
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = shipment.TablePrefix;

			var consolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("JobNumberHeading", "Consolidation ID", consolidationWrapper.JobNumberHeading);
			AssertEquals("JobNumber", "CB01020304", consolidationWrapper.JobNumber);
			AssertEquals("ShipmentType", "EXP", consolidationWrapper.ShipmentType.Code);
		}

		#endregion

		#region TestWrappers

		public void TestEmptyWrappers()
		{
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			var consolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("emptyWrapper.MainShipToParty.CompanyName", ZString.Empty, consolidationWrapper.MainShipToParty.CompanyName);
			AssertEquals("emptyWrapper.SellingParty.CompanyName", ZString.Empty, consolidationWrapper.SellingParty.CompanyName);
			AssertEquals("emptyWrapper.Consolidator.CompanyName", ZString.Empty, consolidationWrapper.Consolidator.CompanyName);
			AssertEquals("emptyWrapper.StuffingLocation.CompanyName", ZString.Empty, consolidationWrapper.StuffingLocation.CompanyName);
			AssertEquals("emptyWrapper.ShippingFrom", ZDate.Empty, consolidationWrapper.StorageFromDate);
			AssertEquals("emptyWrapper.ShippingTo", ZDate.Empty, consolidationWrapper.StorageToDate);
			AssertEquals("emptyWrapper.BillingDate", ZDate.Empty, consolidationWrapper.BillingDate);
			AssertEquals("emptyWrapper.DocumentNumber", ZInt.Zero, consolidationWrapper.DocumentNumber);
			AssertEquals("emptyWrapper.DocumentTotal", ZInt.Zero, consolidationWrapper.DocumentTotal);
		}

		public void TestCarrier()
		{
			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_Code = "ABCDEFG";

			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			bookingConsolidation.KB_JobID = "CB01020304";

			var booking1 = bookingConsolidation.Bookings.AddNew();
			booking1.KM_JobID = "B00000001";
			booking1.KM_TransportReference = "TranRef1234";
			booking1.Address.OrganisationPK = transportCo.PK;

			var booking2 = bookingConsolidation.Bookings.AddNew();
			booking2.KM_JobID = "B00000001";
			booking2.KM_TransportReference = "TranRef1234";

			var consolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertNotNull("Carier", consolidationWrapper.Carrier);
			AssertEquals("Carier Org", "ABCDEFG", consolidationWrapper.Carrier.CompanyCode);
		}

		#endregion

		#region TestIPackingParentWrapper_SenderReference

		public void TestIPackingParentWrapper_SenderReference()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Origin, "TRANS123");

			var orgPickup = Helper.CreateOrganisation("Pickup");
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, orgPickup.MainAddress);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			pickupInstruction.Confirmations[0].KK_ReferenceNum = "123";

			Helper.CreatePackageDivot(pickupInstruction, package, 1);

			var wrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);
			AssertEquals("Sender reference not set", "123", wrapper.GetOwnerReference(package));

			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			pickupInstruction.Confirmations[1].KK_ReferenceNum = "123";
			AssertEquals("Both confirmations are equal, sender reference should be set.", "123", wrapper.GetOwnerReference(package));

			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			pickupInstruction.Confirmations[2].KK_ReferenceNum = "456";

			AssertEquals("No reference should be set.", ZString.Empty, wrapper.GetOwnerReference(package));

			pickupInstruction.Confirmations[2].KK_ReferenceNum = "123";
			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			pickupInstruction.Confirmations[3].KK_ReferenceNum = "346";
			AssertEquals("Should still return sender reference, the different reference number is of Delivery rather than Pickup type.", "123", wrapper.GetOwnerReference(package));

			var childPackage = package.Packages.AddNew();

			AssertEquals("Should have parent's sender reference.", "123", wrapper.GetOwnerReference(childPackage));
		}

		public void TestIPackingParentWrapper_SenderReferenceFirstBlank()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Origin, "TRANS123");

			var orgDelivery = Helper.CreateOrganisation("Pickup");
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, orgDelivery.MainAddress);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			pickupInstruction.Confirmations[0].KK_ReferenceNum = "";

			Helper.CreatePackageDivot(pickupInstruction, package, 1);

			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			pickupInstruction.Confirmations[1].KK_ReferenceNum = "123";

			var wrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("First confirmation reference number was blank, receiver reference should be set.", "123", wrapper.GetOwnerReference(package));
		}

		#endregion

		#region TestIPackingParentWrapper_ReceiverReference

		public void TestIPackingParentWrapper_ReceiverReference()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");

			var orgDelivery = Helper.CreateOrganisation("Delivery");
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			deliveryInstruction.Confirmations[0].KK_ReferenceNum = "123";

			Helper.CreatePackageDivot(deliveryInstruction, package, 1);

			var wrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);
			AssertEquals("Reference should come from the only Confirmation on Delivery Instruction.", "123", wrapper.GetCustomerReference(package));

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstruction.Confirmations[1].KK_ReferenceNum = "123";
			AssertEquals("Both delivery confirmations are equal, receiver reference should be set.", "123", wrapper.GetCustomerReference(package));

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstruction.Confirmations[2].KK_ReferenceNum = "456";

			AssertEquals("Because there are two different references in the delivery Confirmations we return empty string.", "", wrapper.GetCustomerReference(package));

			deliveryInstruction.Confirmations[2].KK_ReferenceNum = "";
			AssertEquals("Only one delivery confirmation has a reference, receiver reference should be set.", "123", wrapper.GetCustomerReference(package));

			deliveryInstruction.Confirmations[2].KK_ReferenceNum = "123";

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			deliveryInstruction.Confirmations[3].KK_ReferenceNum = "346";
			AssertEquals("Should still return receiver reference, the different reference number is of PickUp rather than Delivery type.", "123", wrapper.GetCustomerReference(package));

			var childPackage = package.Packages.AddNew();
			AssertEquals("Should have parent's receiver reference.", "123", wrapper.GetCustomerReference(childPackage));
		}

		public void TestIPackingParentWrapper_ReceiverReferenceLeadingBlanks()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");

			var orgDelivery = Helper.CreateOrganisation("Delivery");
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			deliveryInstruction.Confirmations[0].KK_ReferenceNum = "";

			Helper.CreatePackageDivot(deliveryInstruction, package, 1);

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstruction.Confirmations[1].KK_ReferenceNum = "123";

			var wrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("First confirmation reference number was blank, receiver reference should be set.", "123", wrapper.GetCustomerReference(package));

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstruction.Confirmations[1].KK_ReferenceNum = "";
			deliveryInstruction.Confirmations[2].KK_ReferenceNum = "123";
			AssertEquals("First two confirmation reference number was blank, receiver reference should be set.", "123", wrapper.GetCustomerReference(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetPickupAddress

		public void TestIPackingParentWrapper_GetPickupAddress()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Origin, "TRANS123");

			var orgPickup = Helper.CreateOrganisation("Pickup");
			var pickupInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, orgPickup.MainAddress);
			var pickupInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, null);
			var package = CreatePackageJobWithPackage(bookingConsolidation);

			var packingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Since package is null pickup address should be null.", null, packingParentWrapper.GetPickupAddress(null));
			AssertEquals("Since there are no delivery divots pickup address should be null.", null, packingParentWrapper.GetPickupAddress(package));

			Helper.CreatePackageDivot(pickupInstruction1, package, 1);
			AssertEquals("Since there is a delivery divot it should return pickup address.", pickupInstruction1.Address, packingParentWrapper.GetPickupAddress(package).WrappedObject);

			Helper.CreatePackageDivot(pickupInstruction1, package, 1);
			AssertEquals("Since the divot refers to the same instruction it should return pickup address.", pickupInstruction1.Address, packingParentWrapper.GetPickupAddress(package).WrappedObject);

			Helper.CreatePackageDivot(pickupInstruction2, package, 1);
			AssertEquals("Since there are two delivery divots for the same package, it should return null for pickup address.", null, packingParentWrapper.GetPickupAddress(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetDeliveryAddress

		public void TestIPackingParentWrapper_GetDeliveryAddress()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");

			var orgDelivery = Helper.CreateOrganisation("Delivery");
			var deliveryInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			var deliveryInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var package = CreatePackageJobWithPackage(bookingConsolidation);

			var packingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Since package is null delivery address should be null.", null, packingParentWrapper.GetDeliveryAddress(null));
			AssertEquals("Since there are no delivery divots delivery address should be null.", null, packingParentWrapper.GetDeliveryAddress(package));

			Helper.CreatePackageDivot(deliveryInstruction1, package, 1);
			AssertEquals("Since there is a delivery divot it should return delivery address.", deliveryInstruction1.Address, packingParentWrapper.GetDeliveryAddress(package).WrappedObject);

			Helper.CreatePackageDivot(deliveryInstruction1, package, 1);
			AssertEquals("Since the divot refers to the same instruction it should return delivery address.", deliveryInstruction1.Address, packingParentWrapper.GetDeliveryAddress(package).WrappedObject);

			Helper.CreatePackageDivot(deliveryInstruction2, package, 1);
			AssertEquals("Since there are two delivery divots for the same package, it should return null for delivery address.", null, packingParentWrapper.GetDeliveryAddress(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetDeliveryAddressOfChildPackage

		public void TestIPackingParentWrapper_GetDeliveryAddressOfChildPackage()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");

			var orgDelivery = Helper.CreateOrganisation("Delivery");
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			var packingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);
			var packingWrapper = bookingConsolidationWrapper;
			var childPackage = package.Packages.AddNew();

			var deliveryInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			Helper.CreatePackageDivot(deliveryInstruction1, package, 1);
			AssertEquals("Should return address of parent.", deliveryInstruction1.Address, packingParentWrapper.GetDeliveryAddress(package).WrappedObject);
		}
		#endregion

		#region TestIPackingParentWrapper_GetDeliveryRequiredBy

		public void TestIPackingParentWrapper_GetDeliveryRequiredBy()
		{
			ZDateTime now = ZDateTime.Today;
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, ZString.Empty);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			var orgDelivery = Helper.CreateOrganisation("Delivery");

			var iPackingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Since package is null delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since there are no divots delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(package));

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			var deliveryInstructionConfirmation = deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstructionConfirmation.KK_RequiredTo = now.AddDays(1);
			AssertEquals("Since package is null delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since there are no divots available required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(package));

			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = package.PK;
			AssertEquals("Since package is null delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since divot doesn't have a required date it should come from instructions.", now.AddDays(1), iPackingParentWrapper.GetDeliveryRequiredBy(package));

			var divotConfirmation = deliveryDivot.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Since package is null delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since divot doesn't have a required date it should come from instructions.", now.AddDays(1), iPackingParentWrapper.GetDeliveryRequiredBy(package));

			divotConfirmation.KK_RequiredTo = now.AddDays(3);
			AssertEquals("Since package is null delivery required by date should be empty.", ZDateTime.Empty, iPackingParentWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since there are no divots required date should come from instructions.", now.AddDays(3), iPackingParentWrapper.GetDeliveryRequiredBy(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetTransportReference

		public void TestIPackingParentWrapper_GetTransportReference()
		{
			var bookingConsolidation = CreateConsolidationBookingWithParentShipment();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, ZString.Empty);
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			var orgDelivery = Helper.CreateOrganisation("Delivery");

			var packingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Since package is null transport reference should be empty.", ZString.Empty, packingParentWrapper.GetTransportReference(null));
			AssertEquals("Since booking doesn't have a transport reference, transport reference should be empty", ZString.Empty, packingParentWrapper.GetTransportReference(package));
			booking.KM_TransportReference = "TRANS1234";
			AssertEquals("Since there are no divots, transport reference should be empty", ZString.Empty, packingParentWrapper.GetTransportReference(package));

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = package.PK;

			AssertEquals("Since package is null transport reference should be empty.", ZString.Empty, packingParentWrapper.GetTransportReference(null));
			AssertEquals("Return the transport reference from booking.", booking.KM_TransportReference, packingParentWrapper.GetTransportReference(package));
			booking.KM_TransportReference = ZString.Empty;
			AssertEquals("Since there is no transport reference in booking, it should return empty string.", ZString.Empty, packingParentWrapper.GetTransportReference(package));

			var childPackage = package.Packages.AddNew();
			booking.KM_TransportReference = "TRANS1234";
			AssertEquals("Child package should return parent's transport reference.", "TRANS1234", packingParentWrapper.GetTransportReference(childPackage));
		}

		#endregion

		#region TestGetCustomerReference

		public void TestGetCustomerReference()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var orgDeliveryAgent = Helper.CreateOrganisation("DelvAgent");
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);

			AssertEquals("Since we didn't specify customer reference it should return empty string.", ZString.Empty, bookingConsolidationWrapper.CustomerReference);

			shipment.JS_OH_DeliveryAgent = orgDeliveryAgent.PK;
			var shipmentOrder1 = shipment.DocsAndCartage.OrderItems.AddNew();
			var shipmentOrder2 = shipment.DocsAndCartage.OrderItems.AddNew();
			shipmentOrder1.JT_OrderReference = "Order1";
			shipmentOrder2.JT_OrderReference = "Order2";
			bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("Customer reference should be order references seperated by comma.", string.Join(", ", shipmentOrder1.JT_OrderReference, shipmentOrder2.JT_OrderReference), bookingConsolidationWrapper.CustomerReference);

			bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			ZString orderItemsString = "Order3, Order4";
			shipment.DocsAndCartage.JP_OrderItemsAsString = orderItemsString;
			AssertEquals("Customer reference in the string should get priority over individual order items specified. At a time either order items or orderitemsstring will be available.", orderItemsString, bookingConsolidationWrapper.CustomerReference);
		}

		#endregion

		#region TestGetCarrierServiceLevel

		public void TestGetCarrierServiceLevel()
		{
			var orgDeliveryAgent = Helper.CreateOrganisation("DelvAgent");
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();

			var package = CreatePackageJobWithPackage(bookingConsolidation);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");
			var deliveryInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDeliveryAgent.MainAddress);
			Helper.CreatePackageDivot(deliveryInstruction1, package, 1);
			booking.KM_PL_NKCarrierServiceLevel = "STD";

			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var packingParentWrapper = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Should return carrier service label.", "STD", packingParentWrapper.GetCarrierServiceLevel(package).Code);
		}

		#endregion

		#region TestGetShippersReference

		public void TestGetShippersReference()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);

			AssertEquals("Shippers reference should be empty.", ZString.Empty, bookingConsolidationWrapper.ShippersReference);

			shipment.JS_BookingReference = "Booking Reference 1";
			AssertEquals("Shippers reference should be from JS_BookingReference.", shipment.JS_BookingReference, bookingConsolidationWrapper.ShippersReference);
		}

		#endregion

		#region TestIPackingParentWrapper_GetTransportCompany

		public void TestIPackingParentWrapper_GetTransportCompany()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJobHeader = new JobHeader.Loader(shipment).TryCreate();
			var bookingConsolidation = Helper.CreateConsolidation(shipment);
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			var booking = Helper.CreateBooking(bookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, ZString.Empty);
			var orgTransCo = Helper.CreateOrganisation("TransportCo");
			var transportReference = ((IPackingParentWrapper)bookingConsolidationWrapper);

			AssertEquals("Since there are no divots transport company should be empty.", null, transportReference.GetTransportCompany(null));

			booking.Address.E2_OA_Address = orgTransCo.MainAddress.PK;
			AssertEquals("Since there are no divots transport company should be empty.", null, transportReference.GetTransportCompany(null));

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgTransCo.MainAddress);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			var package = CreatePackageJobWithPackage(bookingConsolidation);
			deliveryDivot.KD_KP_Package = package.PK;

			AssertEquals("Since a package is not availble GetTransportCompany should return null.", null, transportReference.GetTransportCompany(null));
			AssertEquals("Transport company information should be retrieved from booking.", orgTransCo, transportReference.GetTransportCompany(package).Organisation);

			var childPackage = package.Packages.AddNew();
			AssertEquals("Transport company information for child package should be retrieved from booking.", orgTransCo, transportReference.GetTransportCompany(childPackage).Organisation);
		}

		#endregion

		#region TestCollections

		#region TestGetTransportBookings

		public void TestGetTransportBookings()
		{
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();

			var booking1 = bookingConsolidation.Bookings.AddNew();
			var booking2 = bookingConsolidation.Bookings.AddNew();
			var asserts1 = SetupBooking(booking1);
			var asserts2 = SetupBooking(booking2);

			var transportBookingWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("Wrong number of instructions were created, should be 2*(1 + 2 + 2 + 4 + 2)", 11 * 2, transportBookingWrapper.TransportBookings.Count);

			for (int i = 0; i < asserts1.Length / 4; i++)
			{
				AssertContainInstructionFor(transportBookingWrapper, (DtbBooking)asserts1[i, 0], (DtbBookingInstruction)asserts1[i, 1], (DtbBookingInstructionPkgDivot)asserts1[i, 2], (DtbBookingConfirmation)asserts1[i, 3]);
			}

			for (int i = 0; i < asserts2.Length / 4; i++)
			{
				AssertContainInstructionFor(transportBookingWrapper, (DtbBooking)asserts2[i, 0], (DtbBookingInstruction)asserts2[i, 1], (DtbBookingInstructionPkgDivot)asserts2[i, 2], (DtbBookingConfirmation)asserts2[i, 3]);
			}
		}

		BusinessObject[,] SetupBooking(DtbBooking booking)
		{
			DtbBookingInstruction instruction_NoPkgDivotsOrConfirmations = booking.Instructions.AddNew();

			DtbBookingInstruction instruction_NoPkgDivots = booking.Instructions.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivots_Confirmation1 = instruction_NoPkgDivots.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivots_Confirmation2 = instruction_NoPkgDivots.Confirmations.AddNew();

			DtbBookingInstruction instruction_NoConfirmations = booking.Instructions.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoConfirmations_PkgDivot1 = instruction_NoConfirmations.PackageDivots.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoConfirmations_PkgDivot2 = instruction_NoConfirmations.PackageDivots.AddNew();

			DtbBookingInstruction instruction_NoPkgDivotConfirmations = booking.Instructions.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivotConfirmations_Confirmation1 = instruction_NoPkgDivotConfirmations.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoPkgDivotConfirmations_Confirmation2 = instruction_NoPkgDivotConfirmations.Confirmations.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoPkgDivotConfirmations_PkgDivot1 = instruction_NoPkgDivotConfirmations.PackageDivots.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoPkgDivotConfirmations_PkgDivot2 = instruction_NoPkgDivotConfirmations.PackageDivots.AddNew();

			DtbBookingInstruction instruction_NoInstructionConfirmations = booking.Instructions.AddNew();
			DtbBookingInstructionPkgDivot instruction_NoInstructionConfirmations_PkgDivot1 = instruction_NoInstructionConfirmations.PackageDivots.AddNew();
			DtbBookingConfirmation instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1 = instruction_NoInstructionConfirmations_PkgDivot1.Confirmations.AddNew();
			DtbBookingConfirmation instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2 = instruction_NoInstructionConfirmations_PkgDivot1.Confirmations.AddNew();

			// to be able to identify confirmations during assert.
			instruction_NoPkgDivots_Confirmation1.KK_Quantity = 1;
			instruction_NoPkgDivots_Confirmation2.KK_Quantity = 2;
			instruction_NoPkgDivotConfirmations_Confirmation1.KK_Quantity = 3;
			instruction_NoPkgDivotConfirmations_Confirmation2.KK_Quantity = 4;
			instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1.KK_Quantity = 5;
			instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2.KK_Quantity = 6;

			return new BusinessObject[11, 4]
			{
				{ booking, instruction_NoPkgDivotsOrConfirmations, null, null },
				{ booking, instruction_NoPkgDivots, null, instruction_NoPkgDivots_Confirmation1 },
				{ booking, instruction_NoPkgDivots, null, instruction_NoPkgDivots_Confirmation2 },
				{ booking, instruction_NoConfirmations, instruction_NoConfirmations_PkgDivot1, null },
				{ booking, instruction_NoConfirmations, instruction_NoConfirmations_PkgDivot2, null },
				{ booking, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot1, instruction_NoPkgDivotConfirmations_Confirmation1 },
				{ booking, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot1, instruction_NoPkgDivotConfirmations_Confirmation2 },
				{ booking, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot2, instruction_NoPkgDivotConfirmations_Confirmation1 },
				{ booking, instruction_NoPkgDivotConfirmations, instruction_NoPkgDivotConfirmations_PkgDivot2, instruction_NoPkgDivotConfirmations_Confirmation2 },
				{ booking, instruction_NoInstructionConfirmations, instruction_NoInstructionConfirmations_PkgDivot1, instruction_NoInstructionConfirmations_PkgDivot1_Confirmation1 },
				{ booking, instruction_NoInstructionConfirmations, instruction_NoInstructionConfirmations_PkgDivot1, instruction_NoInstructionConfirmations_PkgDivot1_Confirmation2 }
			};
		}

		void AssertContainInstructionFor(FreightWrapperFromDtbBookingConsolidation consolidationWrapper, DtbBooking booking, DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot instructionPkgDivot, DtbBookingConfirmation confirmation)
		{
			foreach (FreightWrapperFromDtbBooking bookingWrapper in consolidationWrapper.TransportBookings)
			{
				if (bookingWrapper.WrappedObject == booking &&
					bookingWrapper.BookingInstructions[0].Sequence == instruction.KN_Sequence &&
					(instructionPkgDivot == null || bookingWrapper.BookingInstructions[0].PackageDivotSequence == instruction.PackageDivots.IndexOf(instructionPkgDivot)) &&
					(confirmation == null || bookingWrapper.BookingInstructions[0].ConfirmationQuantity == confirmation.KK_Quantity))
				{
					return;
				}
			}

			Fail(string.Format("No Booking Instruction could be found for Instruction Sequence '{0}', Package Divot with index '{1}' and Confirmation with Qty '{2}'",
				instruction.KN_Sequence,
				(instructionPkgDivot != null) ? instruction.PackageDivots.IndexOf(instructionPkgDivot) : 0,
				(confirmation != null) ? confirmation.KK_Quantity : ZInt.Zero));
		}

		#endregion

		#region TestGetContainers

		public void TestGetContainers()
		{
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			var bookingA = bookingConsolidation.Bookings.AddNew();
			var bookingB = bookingConsolidation.Bookings.AddNew();

			var packageJobA = Factory.New<PkgPackageJob>();
			PkgPackage containerA1_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA2_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA3_InstructionA2 = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerA4_NoInstruction = packageJobA.Packages.AddNew(Constants.PkgUnit.Container);

			var packageJobB = Factory.New<PkgPackageJob>();
			PkgPackage containerB1_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB2_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB3_InstructionB2 = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);
			PkgPackage containerB4_NoInstruction = packageJobB.Packages.AddNew(Constants.PkgUnit.Container);

			// way to determine which container is which and that qty is used from packages rather that from Divots.
			containerA1_InstructionA1.KP_PackageQty = 1;
			containerA2_InstructionA1.KP_PackageQty = 2;
			containerA3_InstructionA2.KP_PackageQty = 3;
			containerA4_NoInstruction.KP_PackageQty = 4;

			containerB1_InstructionB1.KP_PackageQty = 5;
			containerB2_InstructionB1.KP_PackageQty = 6;
			containerB3_InstructionB2.KP_PackageQty = 7;
			containerB4_NoInstruction.KP_PackageQty = 8;

			// Booking A
			DtbBookingInstruction instructionA1 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA2 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA3 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA1 = Helper.CreatePackageDivot(instructionA1, containerA1_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA2 = Helper.CreatePackageDivot(instructionA1, containerA2_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA2_PkgDivotA1 = Helper.CreatePackageDivot(instructionA2, containerA3_InstructionA2, 1);

			// Booking B
			DtbBookingInstruction instructionB1 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB2 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB3 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB1 = Helper.CreatePackageDivot(instructionB1, containerB1_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB2 = Helper.CreatePackageDivot(instructionB1, containerB2_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB2_PkgDivotB1 = Helper.CreatePackageDivot(instructionB2, containerB3_InstructionB2, 1);

			var consolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("Wrong number of Containers were created", 6, consolidationWrapper.Containers.Count);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerA1_InstructionA1);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerA2_InstructionA1);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerA3_InstructionA2);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerB1_InstructionB1);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerB2_InstructionB1);
			AssertContainsContainerFor(consolidationWrapper.Containers, containerB3_InstructionB2);
		}

		void AssertContainsContainerFor(ContainerWrapperCollection containerWrapperCollection, PkgPackage expectedContainer)
		{
			foreach (ContainerWrapper containerWrapper in containerWrapperCollection)
			{
				if (containerWrapper.ContainerCount == expectedContainer.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Container could be found for container package with Qty '{0}'", expectedContainer.KP_PackageQty));
		}

		#endregion

		#region TestGetPackages

		public void TestGetPackages()
		{
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			var bookingA = bookingConsolidation.Bookings.AddNew();
			var bookingB = bookingConsolidation.Bookings.AddNew();

			var packageJobA = Factory.New<PkgPackageJob>();
			PkgPackage packageA1_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage packageA2_InstructionA1 = packageJobA.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageA3_InstructionA2 = packageJobA.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageA4_NoInstruction = packageJobA.Packages.AddNew(Constants.PkgUnit.Bag);

			var packageJobB = Factory.New<PkgPackageJob>();
			PkgPackage packageB1_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Box);
			PkgPackage packageB2_InstructionB1 = packageJobB.Packages.AddNew(Constants.PkgUnit.Package);
			PkgPackage packageB3_InstructionB2 = packageJobB.Packages.AddNew(Constants.PkgUnit.Pallet);
			PkgPackage packageB4_NoInstruction = packageJobB.Packages.AddNew(Constants.PkgUnit.Bag);

			// way to determine which package is which and that qty is used from packages rather that from Divots.
			packageA1_InstructionA1.KP_PackageQty = 1;
			packageA2_InstructionA1.KP_PackageQty = 2;
			packageA3_InstructionA2.KP_PackageQty = 3;
			packageA4_NoInstruction.KP_PackageQty = 4;
			packageB1_InstructionB1.KP_PackageQty = 5;
			packageB2_InstructionB1.KP_PackageQty = 6;
			packageB3_InstructionB2.KP_PackageQty = 7;
			packageB4_NoInstruction.KP_PackageQty = 8;

			// Booking A
			DtbBookingInstruction instructionA1 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA2 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionA3 = Helper.CreateInstruction(bookingA, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA1 = Helper.CreatePackageDivot(instructionA1, packageA1_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA1_PkgDivotA2 = Helper.CreatePackageDivot(instructionA1, packageA2_InstructionA1, 1);
			DtbBookingInstructionPkgDivot instructionA2_PkgDivotA1 = Helper.CreatePackageDivot(instructionA2, packageA3_InstructionA2, 1);

			// Booking B
			DtbBookingInstruction instructionB1 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB2 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstruction instructionB3 = Helper.CreateInstruction(bookingB, InstructionTypes.Codes.PickUp, "", null);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB1 = Helper.CreatePackageDivot(instructionB1, packageB1_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB1_PkgDivotB2 = Helper.CreatePackageDivot(instructionB1, packageB2_InstructionB1, 1);
			DtbBookingInstructionPkgDivot instructionB2_PkgDivotB1 = Helper.CreatePackageDivot(instructionB2, packageB3_InstructionB2, 1);

			var consolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals("Wrong number of Packages were created", 6, consolidationWrapper.Packages.Count);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageA1_InstructionA1);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageA2_InstructionA1);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageA3_InstructionA2);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageB1_InstructionB1);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageB2_InstructionB1);
			AssertContainsPackageFor(consolidationWrapper.Packages, packageB3_InstructionB2);
		}

		void AssertContainsPackageFor(PackageWrapperCollection packageWrapperCollection, PkgPackage expectedPackage)
		{
			foreach (PackageWrapper packageWrapper in packageWrapperCollection)
			{
				if (packageWrapper.Packages.Value == expectedPackage.KP_PackageQty)
				{
					return;
				}
			}
			Fail(string.Format("No Packages could be found for package with Qty '{0}'", expectedPackage.KP_PackageQty));
		}

		#endregion

		#endregion

		#region Test Overrides

		public override void TestJobHeaderBranchLogo()
		{
			var bookingConsolidation = (DtbBookingConsolidation)GetNewBusinessObjectToWrap();
			bookingConsolidation.KB_JobID = "B00000001";

			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertNull("No branch logo", bookingConsolidationWrapper.CompanyLogo);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			AssertEquals("Branch logo should be Company Logo", new Size(1, 1), bookingConsolidationWrapper.CompanyLogo.Size);
		}

		#endregion

		#region TestDocTypeCode

		public void TestDocTypeCode()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();

			var wrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			((IDocTypeCode)wrapper).DocTypeCode = "CAD";
			AssertEquals("CAD", ((IDocTypeCode)wrapper).DocTypeCode);

			((IDocTypeCode)wrapper).DocTypeCode = "CAR";
			AssertEquals("CAR", ((IDocTypeCode)wrapper).DocTypeCode);
		}

		#endregion

		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var bookingConsolidation = (DtbBookingConsolidation)bizO;
			bookingConsolidation.KB_JobID = "C1";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^DTC=C1;CAD;|mÊ";
		}

		#endregion

		#region TestConsolDateCreated

		public void TestConsolDateCreated()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals(ZDateTime.Empty, bookingConsolidationWrapper.ConsolDateCreated);

			Factory.Save();

			bookingConsolidationWrapper = new FreightWrapperFromDtbBookingConsolidation(bookingConsolidation, Factory);
			AssertEquals(ZDateTime.Now.ToShortDateString(), bookingConsolidationWrapper.ConsolDateCreated.ToShortDateString());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var consol = Factory.New<DtbBookingConsolidation>();
			return new FreightWrapperFromDtbBookingConsolidation(consol, Factory);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((DtbBookingConsolidation)WrappedBO).Bookings.AddNew().Address.OrganisationPK = Factory.LoadTop1<JobDocAddress>(new ZQuery()).OrganisationPK;
			return new FreightWrapperFromDtbBookingConsolidation(WrappedBO, Factory);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Consolidation ID" },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Carrier :  is null";
			}
		}

		protected override ZString ExpectedCarrierTypeDescription
		{
			get { return "Transport Company"; }
		}

		PkgPackage CreatePackageJobWithPackage(DtbBookingConsolidation bookingConsolidation)
		{
			var packageJob = Helper.CreatePackageJob(bookingConsolidation);
			return PackingHelper.CreatePackage(packageJob, 10, "PLT");
		}

		DtbBookingConsolidation CreateConsolidationBookingWithParentShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentJobHeader = new JobHeader.Loader(shipment).TryCreate();
			return Helper.CreateConsolidation(shipment);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}
		PackingTestHelper packingHelper;

		#endregion
	}
}
