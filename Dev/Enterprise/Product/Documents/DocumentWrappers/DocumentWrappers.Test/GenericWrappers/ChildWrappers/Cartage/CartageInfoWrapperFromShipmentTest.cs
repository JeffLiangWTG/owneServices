using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CartageInfoWrapperFromShipment))]
	sealed class CartageInfoWrapperFromShipmentTest : CartageInfoWrapperTest
	{
		public void TestShipmentPickupDatesFallbackToShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 01, 01);

			var wrapper = GetWrapperWithDirection(shipment, "DEP");

			AssertEquals("Pickup date from Shipment", wrapper.JourneyOnePickUpDate, new ZDateTime(2011, 01, 01));

			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2011, 01, 02);

			wrapper = GetWrapperWithDirection(shipment, "DEP");

			AssertEquals("Pickup date from Shipment", wrapper.JourneyOnePickUpDate, new ZDateTime(2011, 01, 01));
			AssertEquals("Pickup required by date from Shipment", wrapper.JourneyOnePickUpRequiredByDate, new ZDateTime(2011, 01, 02));
		}

		public override void TestWrapperMappingsEmpty()
		{
			CartageInfoWrapper wrapperEmpty = (CartageInfoWrapperFromShipment)GetNewDocumentWrapper();
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
			AssertEquals("wrapperEmpty.PrintAsContainers", false, wrapperEmpty.PrintAsContainers);
			AssertEquals("wrapperEmpty.PrintTwoJourneys", false, wrapperEmpty.PrintTwoJourneys);
			AssertEquals("wrapperEmpty.EquipmentType", "", wrapperEmpty.EquipmentType);
			AssertEquals("wrapperEmpty.FullHandlingInstructions", "", wrapperEmpty.FullHandlingInstructions);
			AssertEquals("wrapperEmpty.FullCartageInstructions", "", wrapperEmpty.FullCartageInstructions);
			AssertEquals("wrapperEmpty.AddressesWithWareHousing", 0, wrapperEmpty.AddressesWithWareHousing.Count);
			AssertEquals("wrapperEmpty.CartageAdvice", "", wrapperEmpty.CartageAdvice.DateAsUniqueIdentifier);
			AssertNotNull("wrapperEmpty.CurrentCompany", wrapperEmpty.CurrentCompany);
			AssertEquals("wrapperEmpty.IsAir", false, wrapperEmpty.IsAir);

			AssertEquals("wrapperEmpty.JourneyOnePickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpDateHeading", "Date:", wrapperEmpty.JourneyOnePickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOnePickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOnePickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToDateHeading", "Date:", wrapperEmpty.JourneyOneDeliverToDateHeading);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyOneDeliverToRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpDateHeading", "Date:", wrapperEmpty.JourneyTwoPickUpDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDate);
			AssertEquals("wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading", ZString.Empty, wrapperEmpty.JourneyTwoPickUpRequiredByDateHeading);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDate", ZDateTime.Empty, wrapperEmpty.JourneyTwoDeliverToDate);
			AssertEquals("wrapperEmpty.JourneyTwoDeliverToDateHeading", "Date:", wrapperEmpty.JourneyTwoDeliverToDateHeading);
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
			ForwardingShipment shipment = GetTestShipment();
			CartageInfoWrapper wrapperFull = GetWrapperWithDirection(shipment, "DEP");
			AssertEquals("wrapperFull.EmailSubjectNumber", "ShipmentNo", wrapperFull.EmailSubjectNumber);
			AssertEquals("wrapperFull.JourneyOnePickUpHeading", "PICKUP", wrapperFull.JourneyOnePickUpHeading);
			AssertEquals("wrapperFull.JourneyOneDeliverToHeading", "DELIVER TO", wrapperFull.JourneyOneDeliverToHeading);
			AssertEquals("wrapperFull.JourneyTwoPickUpHeading", "PICKUP", wrapperFull.JourneyTwoPickUpHeading);
			AssertEquals("wrapperFull.JourneyTwoDeliverToHeading", "DELIVER TO", wrapperFull.JourneyTwoDeliverToHeading);
			AssertEquals("wrapperFull.JourneyOnePickUpAddress", "JourneyOnePickupAddress", wrapperFull.JourneyOnePickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOneDeliverToAddress", "JourneyOneDeliverToAddress", wrapperFull.JourneyOneDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoPickUpAddress", "", wrapperFull.JourneyTwoPickUpAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyTwoDeliverToAddress", "", wrapperFull.JourneyTwoDeliverToAddress.AddressLine1);
			AssertEquals("wrapperFull.JourneyOnePickUpContactName", "JourneyOnePickupContactName", wrapperFull.JourneyOnePickUpContactName);
			AssertEquals("wrapperFull.JourneyOnePickUpContactPhone", "123456", wrapperFull.JourneyOnePickUpContactPhone);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactName", "JourneyOneDeliverToContactName", wrapperFull.JourneyOneDeliverToContactName);
			AssertEquals("wrapperFull.JourneyOneDeliverToContactPhone", "987654", wrapperFull.JourneyOneDeliverToContactPhone);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactName", "", wrapperFull.JourneyTwoPickUpContactName);
			AssertEquals("wrapperFull.JourneyTwoPickUpContactPhone", "", wrapperFull.JourneyTwoPickUpContactPhone);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactName", "", wrapperFull.JourneyTwoDeliverToContactName);
			AssertEquals("wrapperFull.JourneyTwoDeliverToContactPhone", "", wrapperFull.JourneyTwoDeliverToContactPhone);
			AssertEquals("wrapperFull.PrintAsContainers", false, wrapperFull.PrintAsContainers);
			AssertEquals("wrapperFull.PrintTwoJourneys", false, wrapperFull.PrintTwoJourneys);
			AssertEquals("wrapperFull.EquipmentType", "HSL - Haulier Supplies Lift", wrapperFull.EquipmentType);
			AssertEquals("wrapperFull.FullHandlingInstructions", "", wrapperFull.FullHandlingInstructions);
			AssertEquals("wrapperFull.FullCartageInstructions", "", wrapperFull.FullCartageInstructions);
			AssertEquals("wrapperFull.AddressesWithWareHousing", "JourneyOnePickupAddress", wrapperFull.AddressesWithWareHousing[0].AddressLine1);
			AssertNotNull("wrapperFull.CartageAdvice", wrapperFull.CartageAdvice);
			AssertNotNull("wrapperFull.CurrentCompany", wrapperFull.CurrentCompany);
			AssertEquals("wrapperFull.IsAir", true, wrapperFull.IsAir);
			AssertEquals("wrapperFull.CutOffDate", new ZDateTime(2011, 1, 14), wrapperFull.CutOffDate);
			AssertEquals("wrapperFull.AvailableDate", new ZDateTime(2011, 1, 25), wrapperFull.AvailableDate);
			AssertEquals("wrapperFull.CutOffOrAvailableDate", ZDateTime.Empty, wrapperFull.CutOffOrAvailableDate);
			AssertEquals("wrapperFull.PickupDate", new ZDateTime(2011, 1, 13), wrapperFull.ReceivalDate);
			AssertEquals("wrapperFull.StorageCommenceDate", new ZDateTime(2011, 1, 27), wrapperFull.StorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDate", ZDateTime.Empty, wrapperFull.PickupOrStorageCommenceDate);
			AssertEquals("wrapperFull.PickupOrStorageCommenceDateHeading", "PICKUP DATE", wrapperFull.PickupOrStorageCommenceDateHeading);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CartageInfoWrapperFromShipment(Factory.GetNull<ForwardingShipment>(), Factory);
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

		CartageInfoWrapperFromShipment GetWrapperWithDirection(ForwardingShipment shipment, ZString direction)
		{
			FreightWrapperFromShipment parentWrapper = new FreightWrapperFromShipment(shipment, Factory);
			parentWrapper.SetDocumentDirectionForTesting(direction);

			return new CartageInfoWrapperFromShipment(shipment, Factory);
		}

		ForwardingShipment GetTestShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentNo";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LCL";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, true);
			consignorPickupAddress.OA_Address1 = "JourneyOnePickupAddress";
			consignorPickupAddress.OA_FCLEquipmentNeeded = "TRL";
			consignorPickupAddress.OA_DockLeveler = true;

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "JourneyOnePickupContactName";
			consignorContact.OC_Phone = "123456";

			var consignorDocument = consignorContact.Documents.AddNew();
			consignorDocument.OD_DocumentGroup = "TRN";

			shipment.ConsignorPK = consignor.PK;

			var exportReceivingDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			exportReceivingDepotAddress.OA_Address1 = "JourneyOneDeliverToAddress";
			exportReceivingDepotAddress.OA_Phone = "987654";

			var depot = Factory.Load<OrgHeader>(exportReceivingDepotAddress.OA_OH);
			var depotContact = depot.Contacts.AddNew();
			depotContact.OC_ContactName = "JourneyOneDeliverToContactName";

			var depotContactDocument = depotContact.Documents.AddNew();
			depotContactDocument.OD_DocumentGroup = "TRN";

			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepotAddress.PK;

			var transport = shipment.Transports.AddNew();
			transport.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 13);
			transport.JW_DepotCutOff = new ZDateTime(2011, 1, 14);

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "HSL";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "HUL";
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 1, 25);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 1, 27);

			return shipment;
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}

		#endregion
	}
}
