using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CartageInfoWrapperFromContainer))]
	sealed class CartageInfoWrapperFromContainerTest : CartageInfoWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CartageInfoWrapper wrapperEmpty = (CartageInfoWrapperFromContainer)GetNewDocumentWrapper();

			AssertEquals("wrapperEmpty.EmailSubjectNumber", "", wrapperEmpty.EmailSubjectNumber);
			AssertEquals("wrapperEmpty.JourneyOnePickUpHeading", "PICKUP", wrapperEmpty.JourneyOnePickUpHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToHeading", "DELIVER TO", wrapperEmpty.JourneyOneDeliverToHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpHeading", "PICKUP", wrapperEmpty.JourneyTwoPickUpHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToHeading", "DELIVER TO", wrapperEmpty.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpAddress", "", wrapperEmpty.JourneyOnePickUpAddress.Address);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToAddress", "", wrapperEmpty.JourneyOneDeliverToAddress.Address);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpAddress", "", wrapperEmpty.JourneyTwoPickUpAddress.Address);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToAddress", "", wrapperEmpty.JourneyTwoDeliverToAddress.Address);
			AssertEquals("wrapperEmpty.JourneyOnePickUpContactName", "", wrapperEmpty.JourneyOnePickUpContactName);
			AssertEquals("wrapperEmpty.JourneyOnePickUpContactPhone", "", wrapperEmpty.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToContactName", "", wrapperEmpty.JourneyOneDeliverToContactName);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToContactPhone", "", wrapperEmpty.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpContactName", "", wrapperEmpty.JourneyTwoPickUpContactName);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpContactPhone", "", wrapperEmpty.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToContactName", "", wrapperEmpty.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToContactPhone", "", wrapperEmpty.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperEmpty.PrintAsContainers", true, wrapperEmpty.PrintAsContainers);
			AssertEquals("wrapperEmpty.PrintTwoJourneys", true, wrapperEmpty.PrintTwoJourneys);
			AssertEquals("wrapperEmpty.EquipmentType", "", wrapperEmpty.EquipmentType);
			AssertEquals("wrapperEmpty.FullHandlingInstructions", "", wrapperEmpty.FullHandlingInstructions);
			AssertEquals("wrapperEmpty.FullCartageInstructions", "", wrapperEmpty.FullCartageInstructions);
			AssertEquals("wrapperEmpty.AddressesWithWareHousing", 0, wrapperEmpty.AddressesWithWareHousing.Count);
			AssertEquals("wrapperEmpty.CartageAdvice", "", wrapperEmpty.CartageAdvice.DateAsUniqueIdentifier);
			AssertNotNull("wrapperEmpty.CurrentCompany", wrapperEmpty.CurrentCompany);
			AssertEquals("wrapperEmpty.IsAir", false, wrapperEmpty.IsAir);

			AssertEquals("wrapperEmpty.JourneyOnePickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "", wrapperEmpty.JourneyOnePickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDateHeading", "", wrapperEmpty.JourneyOneDeliverToDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDateHeading", "", wrapperEmpty.JourneyTwoPickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDateHeading", "", wrapperEmpty.JourneyTwoDeliverToDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoDeliverToRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyTwoDeliverToRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpReleaseNum", "", wrapperEmpty.JourneyOnePickUpReleaseNum);
			AssertEquals("wrapperEmpty.JourneyOnePickUpSlofRef", "", wrapperEmpty.JourneyOnePickUpSlofRef);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToReleaseNum", "", wrapperEmpty.JourneyTwoDeliverToReleaseNum);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToSlofRef", "", wrapperEmpty.JourneyTwoDeliverToSlofRef);
			AssertEquals("wrapperEmpty.LegNotes", "", wrapperEmpty.LegNotes);
		}

		[SetOrgAllowMixedCase(true)]
		public override void TestWrapperMappingFull()
		{
			var container = GetTestContainer();
			var wrapperFull = GetWrapperWithDirection(container, "DEP");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP FULL", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "ContainerYardAddress_Container", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "ContainerYardContact_Container", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "123456", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", ZDateTime.Empty, wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", ZDateTime.Empty, wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", ZDateTime.Empty, wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", ZDateTime.Empty, wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 15), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", ZDateTime.Empty, wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "PICKUP DATE", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFullWithAgencyShipment()
		{
			var container = GetTestContainer();
			var shipment = GetTestAgencyShipment();
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;

			var wrapperFull = GetWrapperWithDirection(container, shipment, "DEP");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP FULL", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "ContainerYardAddress_Container", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "ConfirmAddress_Container", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "ConfirmAddress_Container", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "ExportReceivingDepotAddress_Shipment", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "ContainerYardContact_Container", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "123456", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "ConfirmContact_Container", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "345678", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "ConfirmContact_Container", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "345678", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "ExportReceivingDepotContact_Shipment", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "987654", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "WUP - Wait for Pack/Unpack", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2009, 1, 10), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", ZDateTime.Empty, wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2009, 1, 8), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2009, 1, 9), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "PICKUP DATE", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFullWithConsolAndShipment()
		{
			var container = GetTestContainer();
			var shipment = GetTestForwardingShipment();
			var consol = GetTestConsol();

			consol.Containers.Add(container);
			consol.Shipments.Add(shipment);

			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);

			var wrapperFull = GetWrapperWithDirection(container, shipment, "DEP");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP FULL", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "ContainerYardAddress_Container", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "ConfirmAddress_Container", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "ConfirmAddress_Container", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "DepartureCTOAddress_Consol", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "ContainerYardContact_Container", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "123456", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "ConfirmContact_Container", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "345678", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "ConfirmContact_Container", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "345678", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "DepartureCTOContact_Consol", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "456789", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "SDL - Drop Container with Sideloader", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2009, 1, 9), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", new ZDateTime(2009, 1, 9), wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2009, 1, 7), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2009, 1, 9), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "PICKUP DATE", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFull_ARV()
		{
			var container = GetTestContainer();
			var wrapperFull = GetWrapperWithDirection(container, "ARV");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP FULL", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "ArrivalContainerYardAddress_Container", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "ArrivalContainerYardContact_Container", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "484848", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ArrivalContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", ZDateTime.Empty, wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", ZDateTime.Empty, wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", ZDateTime.Empty, wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", ZDateTime.Empty, wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 15), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2009, 1, 15), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "STORAGE STARTS", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFullWithAgencyShipment_ARV()
		{
			var container = GetTestContainer();
			var shipment = GetTestAgencyShipment();
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;

			var wrapperFull = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP FULL", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "DeliveryConfirmAddress_Container", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "DeliveryConfirmAddress_Container", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "ArrivalContainerYardAddress_Container", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "DeliveryConfirmContact_Container", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "565656", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "DeliveryConfirmContact_Container", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "565656", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "ArrivalContainerYardContact_Container", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "484848", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "TRL - Drop Trailer", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ArrivalContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2009, 1, 10), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2009, 1, 8), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "STORAGE STARTS", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFullWithConsolAndShipment_ARV()
		{
			var container = GetTestContainer();
			var shipment = GetTestForwardingShipment();
			var consol = GetTestConsol();

			consol.Containers.Add(container);
			consol.Shipments.Add(shipment);

			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);

			var wrapperFull = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("wrapperFull.EmailSubjectNumber", "ABC123", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP FULL", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO FULL", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP EMPTY", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO EMPTY", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "ArrivalCTOAddress_Consol", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "DeliveryConfirmAddress_Container", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "DeliveryConfirmAddress_Container", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "ArrivalContainerYardAddress_Container", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "ArrivalCTOContact_Consol", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "991993", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "DeliveryConfirmContact_Container", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "565656", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "DeliveryConfirmContact_Container", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "565656", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "ArrivalContainerYardContact_Container", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "484848", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", true, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", true, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "TRL - Drop Trailer", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "ArrivalContainerYardAddress_Container", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", false, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2009, 1, 9), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", new ZDateTime(2009, 1, 23), wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2009, 1, 7), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", new ZDateTime(2009, 1, 21), wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "STORAGE STARTS", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		public void TestContainerPickupDatesFallbackToShipment()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var container = Factory.New<CommonContainer>();

			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 01, 01);
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2011, 01, 02);

			var wrapper = GetWrapperWithDirection(container, shipment, "DEP");

			AssertEquals("Pickup date from Shipment", wrapper.JourneyTwoPickUpDate, new ZDateTime(2011, 01, 01));
			AssertEquals("Pickup required by date from Shipment", wrapper.JourneyTwoPickUpRequiredByDate, new ZDateTime(2011, 01, 02));

			container.JC_DepartureEstimatedPickup = new ZDateTime(2011, 01, 03);
			container.OriginConfirm.EU_RequestedPickupDeliveryTime = new ZDateTime(2011, 01, 04);

			wrapper = GetWrapperWithDirection(container, shipment, "DEP");

			AssertEquals("Pickup date from Confirmation", wrapper.JourneyTwoPickUpDate, new ZDateTime(2011, 01, 03));
			AssertEquals("Pickup required by date from Confirmation", wrapper.JourneyTwoPickUpRequiredByDate, new ZDateTime(2011, 01, 04));
		}

		public void TestContainerDeliveryDatesFallbackToShipment()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var container = Factory.New<CommonContainer>();

			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 01, 01);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2011, 01, 02);

			var wrapper = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("Delivery date from Shipment", wrapper.JourneyOneDeliverToDate, new ZDateTime(2011, 01, 01));
			AssertEquals("Delivery required by date from Shipment", wrapper.JourneyOneDeliverToRequiredByDate, new ZDateTime(2011, 01, 02));

			container.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 01, 03);
			container.DestinationConfirm.EU_RequestedPickupDeliveryTime = new ZDateTime(2011, 01, 04);

			wrapper = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("Delivery date from Confirmation", wrapper.JourneyOneDeliverToDate, new ZDateTime(2011, 01, 03));
			AssertEquals("Delivery required by date from Confirmation", wrapper.JourneyOneDeliverToRequiredByDate, new ZDateTime(2011, 01, 04));
		}

		public void TestFullCartageInstructionsOnConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			string consolPickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string consolDeliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			OrgHeader orgReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgSendingAgent = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = orgReceivingAgent.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgSendingAgent.MainAddress.PK;

			FreightHelperClass.AddNote(orgReceivingAgent, consolPickupDesc, "Receiving Agent Pickup Instructions");
			FreightHelperClass.AddNote(orgReceivingAgent, consolDeliveryDesc, "Receiving Agent Delivery Instructions");
			FreightHelperClass.AddNote(orgSendingAgent, consolPickupDesc, "Sending Agent Pickup Instructions");
			FreightHelperClass.AddNote(orgSendingAgent, consolDeliveryDesc, "Sending Agent Delivery Instructions");

			var wrapper = GetWrapperWithDirection(container, "DEP");
			AssertEquals("All Cartage Instructions - should return instructions from organizations only", "Receiving Agent Pickup Instructions\nSending Agent Pickup Instructions", wrapper.FullCartageInstructions);

			wrapper = GetWrapperWithDirection(container, "ARV");
			AssertEquals("All Cartage Instructions - should return instructions from organizations only", "Receiving Agent Delivery Instructions\nSending Agent Delivery Instructions", wrapper.FullCartageInstructions);

			FreightHelperClass.AddNote(consol, consolPickupDesc, "Consol Pickup Instructions");
			FreightHelperClass.AddNote(consol, consolDeliveryDesc, "Consol Delivery Instructions");

			wrapper = GetWrapperWithDirection(container, "DEP");
			AssertEquals("Consol Cartage Instructions", "Consol Pickup Instructions", wrapper.FullCartageInstructions);

			wrapper = GetWrapperWithDirection(container, "ARV");
			AssertEquals("Consol Cartage Instructions", "Consol Delivery Instructions", wrapper.FullCartageInstructions);
		}

		public void TestFullCartageInstructionsOnShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();

			string shipmentPickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string shipmentDeliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			OrgHeader orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgPickup = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = orgConsignor.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = orgPickup.PK;
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = orgDelivery.PK;

			FreightHelperClass.AddNote(shipment, shipmentPickupDesc, "Shipment Pickup Instructions");
			FreightHelperClass.AddNote(shipment, shipmentDeliveryDesc, "Shipment Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignor, shipmentPickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, shipmentDeliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, shipmentPickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, shipmentDeliveryDesc, "Consignee Delivery Instructions");
			FreightHelperClass.AddNote(orgPickup, shipmentPickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgPickup, shipmentDeliveryDesc, "Delivery Instructions");
			FreightHelperClass.AddNote(orgDelivery, shipmentPickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgDelivery, shipmentDeliveryDesc, "Delivery Instructions");

			var wrapper = GetWrapperWithDirection(container, shipment, "DEP");
			AssertEquals("All Cartage Instructions", "Shipment Pickup Instructions\r\nPickup Instructions", wrapper.FullCartageInstructions);

			wrapper = GetWrapperWithDirection(container, shipment, "ARV");
			AssertEquals("All Cartage Instructions", "Shipment Delivery Instructions\r\nDelivery Instructions", wrapper.FullCartageInstructions);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestAddressAndContactOverrides()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var container = Factory.New<CommonContainer>();

			OrgAddress deliveryAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			deliveryAddress1.OA_Address1 = "DeliveryAddress1";

			OrgAddress deliveryAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			deliveryAddress2.OA_Address1 = "DeliveryAddress2";

			JobDocAddress deliveryDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryDocAddress.E2_AddressType = "CEG";
			deliveryDocAddress.E2_OA_Address = deliveryAddress1.PK;

			OrgHeader deliveryAddressOrg = Factory.Load<OrgHeader>(deliveryAddress1.OA_OH);
			var deliveryContact1 = deliveryAddressOrg.Contacts.AddNew();
			deliveryContact1.OC_ContactName = "DeliveryContact1";
			deliveryContact1.OC_Phone = "345678";

			var deliveryContact2 = deliveryAddressOrg.Contacts.AddNew();
			deliveryContact2.OC_ContactName = "DeliveryContact2";
			deliveryContact2.OC_Phone = "567890";

			var deliveryContactDocument = deliveryContact1.Documents.AddNew();
			deliveryContactDocument.OD_DocumentGroup = "TRN";

			shipment.DocAddresses.Add(deliveryDocAddress);

			Factory.Save();

			var wrapper = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("JourneyOneDeliverTo DeliveryAddress1", "DeliveryAddress1", wrapper.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("JourneyOneDeliverTo DeliveryContact1", "DeliveryContact1", wrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverTo DeliveryPhone1", "345678", wrapper.JourneyOneDeliverToContactPhone);

			deliveryDocAddress.E2_Contact = "DeliveryContact2";
			deliveryDocAddress.E2_OA_Address = deliveryAddress2.PK;

			Factory.Save();

			wrapper = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("JourneyOneDeliverTo DeliveryAddress2", "DeliveryAddress2", wrapper.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("JourneyOneDeliverTo DeliveryContact2", "DeliveryContact2", wrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverTo DeliveryPhone2", "567890", wrapper.JourneyOneDeliverToContactPhone);

			deliveryDocAddress.E2_AddressOverride = true;
			deliveryDocAddress.E2_Address1 = "DeliveryAddressOverride";
			deliveryDocAddress.E2_Contact = "DeliveryContactOverride";
			deliveryDocAddress.E2_Phone = "123456";

			Factory.Save();

			wrapper = GetWrapperWithDirection(container, shipment, "ARV");

			AssertEquals("JourneyOneDeliverTo DeliveryAddressOverride", "DeliveryAddressOverride", wrapper.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("JourneyOneDeliverTo DeliveryContactOverride", "DeliveryContactOverride", wrapper.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverTo DeliveryPhoneOverride", "123456", wrapper.JourneyOneDeliverToContactPhone);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CartageInfoWrapperFromContainer(Factory.GetNull<CommonContainer>(), Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
JourneyOneDeliverToAddress : 
JourneyOnePickUpAddress : 
JourneyTwoDeliverToAddress : 
JourneyTwoPickUpAddress : 
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CartageInfoWrapperFromShipment(null, Factory);
		}

		#region Implementation

		CartageInfoWrapperFromContainer GetWrapperWithDirection(CommonContainer container, ZString direction)
		{
			return GetWrapperWithDirection(container, Factory.GetNull<CommonShipment>(), direction);
		}

		CartageInfoWrapperFromContainer GetWrapperWithDirection(CommonContainer container, CommonShipment shipment, ZString direction)
		{
			FreightWrapperFromShipment parentWrapper = new FreightWrapperFromShipment(Factory.GetNull<ForwardingShipment>(), Factory);
			parentWrapper.SetDocumentDirectionForTesting(direction);

			return new CartageInfoWrapperFromContainer(container, shipment, Factory);
		}

		CommonContainer GetTestContainer()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "ABC123";
			container.JC_LCLStorageCommences = new ZDateTime(2009, 1, 15);
			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2009, 1, 21);
			container.JC_FCLAvailable = new ZDateTime(2009, 1, 23);

			var departureContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>();
			departureContainerYardAddress.OA_Address1 = "ContainerYardAddress_Container";
			departureContainerYardAddress.OA_Phone = "123456";
			departureContainerYardAddress.OA_DockLeveler = true;
			container.JC_OA_DepartureContainerYardAddress = departureContainerYardAddress.PK;

			var departureContainerYardOrg = Factory.Load<OrgHeader>(departureContainerYardAddress.OA_OH);
			var departureContainerYardContact = departureContainerYardOrg.Contacts.AddNew();
			departureContainerYardContact.OC_ContactName = "ContainerYardContact_Container";

			var departureContainerYardContactDocument = departureContainerYardContact.Documents.AddNew();
			departureContainerYardContactDocument.OD_DocumentGroup = "TRN";

			var arrivalContainerYardAddress = Factory.NewWithValidTestData<OrgAddress>();
			arrivalContainerYardAddress.OA_Address1 = "ArrivalContainerYardAddress_Container";
			arrivalContainerYardAddress.OA_Phone = "484848";
			arrivalContainerYardAddress.OA_DockLeveler = true;
			container.JC_OA_ArrivalContainerYardAddress = arrivalContainerYardAddress.PK;

			var arrivalContainerYardOrg = Factory.Load<OrgHeader>(arrivalContainerYardAddress.OA_OH);
			var arrivalContainerYardContact = arrivalContainerYardOrg.Contacts.AddNew();
			arrivalContainerYardContact.OC_ContactName = "ArrivalContainerYardContact_Container";

			var arrivalContainerYardContactDocument = arrivalContainerYardContact.Documents.AddNew();
			arrivalContainerYardContactDocument.OD_DocumentGroup = "TRN";

			var confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			confirmAddress.OA_Address1 = "ConfirmAddress_Container";
			container.OriginConfirm.ConfirmAddress.E2_OA_Address = confirmAddress.PK;

			var confirmAddressOrg = Factory.Load<OrgHeader>(confirmAddress.OA_OH);
			var confirmContact = confirmAddressOrg.Contacts.AddNew();
			confirmContact.OC_ContactName = "ConfirmContact_Container";
			confirmContact.OC_Phone = "345678";

			var confirmContactDocument = confirmContact.Documents.AddNew();
			confirmContactDocument.OD_DocumentGroup = "TRN";

			var deliveryConfirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			deliveryConfirmAddress.OA_Address1 = "DeliveryConfirmAddress_Container";
			container.DestinationConfirm.ConfirmAddress.E2_OA_Address = deliveryConfirmAddress.PK;

			var deliveryConfirmOrg = Factory.Load<OrgHeader>(deliveryConfirmAddress.OA_OH);
			var deliveryConfirmContact = deliveryConfirmOrg.Contacts.AddNew();
			deliveryConfirmContact.OC_ContactName = "DeliveryConfirmContact_Container";
			deliveryConfirmContact.OC_Phone = "565656";

			var deliveryConfirmContactDocument = deliveryConfirmContact.Documents.AddNew();
			deliveryConfirmContactDocument.OD_DocumentGroup = "TRN";

			return container;
		}

		AgencyShipment GetTestAgencyShipment()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentNo";
			shipment.JS_IsBooking = true;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "NLRTM";

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "SDL";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "TRL";
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2009, 1, 9);
			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2009, 1, 20);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2009, 1, 22);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, true);
			consignorPickupAddress.OA_Address1 = "ConsignorAddress_Shipment";
			consignorPickupAddress.OA_Phone = "234567";
			consignorPickupAddress.OA_FCLEquipmentNeeded = "SDL";

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "ConsignorContact_Shipment";

			var consignorDocument = consignorContact.Documents.AddNew();
			consignorDocument.OD_DocumentGroup = "TRN";

			shipment.ConsignorPK = consignor.PK;

			var exportReceivingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			exportReceivingDepotAddress.OA_Address1 = "ExportReceivingDepotAddress_Shipment";
			exportReceivingDepotAddress.OA_Phone = "987654";

			var depot = Factory.Load<OrgHeader>(exportReceivingDepotAddress.OA_OH);
			var depotContact = depot.Contacts.AddNew();
			depotContact.OC_ContactName = "ExportReceivingDepotContact_Shipment";

			var depotContactDocument = depotContact.Documents.AddNew();
			depotContactDocument.OD_DocumentGroup = "TRN";

			var departureTransport = shipment.Transports.AddNew();
			departureTransport.JW_RL_NKLoadPort = "USCHI";
			departureTransport.JW_RL_NKDiscPort = "DEHAM";
			departureTransport.JW_OA_DepartureLocation = exportReceivingDepotAddress.PK;
			departureTransport.JW_TerminalCutOff = new ZDateTime(2009, 1, 10);
			departureTransport.JW_TerminalReceivalCommences = new ZDateTime(2009, 1, 8);

			return shipment;
		}

		ForwardingShipment GetTestForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentNo";
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "NLRTM";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "SDL";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "TRL";
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2009, 1, 9);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, true);
			consignorPickupAddress.OA_Address1 = "ConsignorAddress_Shipment";
			consignorPickupAddress.OA_Phone = "234567";
			consignorPickupAddress.OA_FCLEquipmentNeeded = "SDL";

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "ConsignorContact_Shipment";

			var consignorDocument = consignorContact.Documents.AddNew();
			consignorDocument.OD_DocumentGroup = "TRN";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneePickupAddress = consignee.Addresses.AddNew(OrgAddressType.Pickup, true);
			consigneePickupAddress.OA_Address1 = "ConsigneeAddress_Shipment";
			consigneePickupAddress.OA_Phone = "765432";
			consigneePickupAddress.OA_FCLEquipmentNeeded = "TRL";

			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "ConsigneeContact_Shipment";

			var consigneeDocument = consigneeContact.Documents.AddNew();
			consigneeDocument.OD_DocumentGroup = "TRN";

			return shipment;
		}

		ForwardingConsol GetTestConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";

			var departureCTOAddress = Factory.NewWithValidTestData<OrgAddress>();
			departureCTOAddress.OA_Address1 = "DepartureCTOAddress_Consol";
			departureCTOAddress.OA_Phone = "456789";
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.PK;

			var departureCTOOrg = Factory.Load<OrgHeader>(departureCTOAddress.OA_OH);
			var departureCTOContact = departureCTOOrg.Contacts.AddNew();
			departureCTOContact.OC_ContactName = "DepartureCTOContact_Consol";

			var departureCTOContactDocument = departureCTOContact.Documents.AddNew();
			departureCTOContactDocument.OD_DocumentGroup = "TRN";

			var arrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>();
			arrivalCTOAddress.OA_Address1 = "ArrivalCTOAddress_Consol";
			arrivalCTOAddress.OA_Phone = "991993";
			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.PK;

			var arrivalCTOOrg = Factory.Load<OrgHeader>(arrivalCTOAddress.OA_OH);
			var arrivalCTOContact = arrivalCTOOrg.Contacts.AddNew();
			arrivalCTOContact.OC_ContactName = "ArrivalCTOContact_Consol";

			var arrivalCTOContactDocument = arrivalCTOContact.Documents.AddNew();
			arrivalCTOContactDocument.OD_DocumentGroup = "TRN";

			var departureTransport = consol.Transports[0];
			departureTransport.JW_RL_NKLoadPort = "USCHI";
			departureTransport.JW_RL_NKDiscPort = "DEHAM";
			departureTransport.JW_TerminalReceivalCommences = new ZDateTime(2009, 1, 7);
			departureTransport.JW_TerminalCutOff = new ZDateTime(2009, 1, 9);

			var arrivalTransport = consol.Transports.AddNew();
			arrivalTransport.JW_RL_NKLoadPort = "DEHAM";
			arrivalTransport.JW_RL_NKDiscPort = "NLRTM";
			arrivalTransport.JW_TerminalAvailabilityDate = new ZDateTime(2009, 1, 23);

			return consol;
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}

		#endregion
	}
}
