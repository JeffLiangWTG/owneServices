using System;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyShipment))]
	sealed class DocAgencyShipmentTest : DocumentWrapperTestCase
	{
		public void TestConsignee()
		{
			AssertNull("Consignee", DocWrapper.Consignee);

			Shipment.ConsigneePK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertNotNull("Consignee", DocWrapper.Consignee);
			AssertType("Consignee is of type DocOrganisation", typeof(DocOrganisation), DocWrapper.Consignee);
		}

		public void TestConsignor()
		{
			AssertNull("Consignor", DocWrapper.Consignor);

			Shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertNotNull("Consignor", DocWrapper.Consignor);
			AssertType("Consignor is of type DocOrganisation", typeof(DocOrganisation), DocWrapper.Consignor);
		}

		public void TestBookingMode()
		{
			ZString bookingMode = new ZString("MMM");
			Shipment.JS_PackingMode = bookingMode;
			AssertEquals("BookingMode", bookingMode, DocWrapper.BookingMode);
		}

		public void TestVoyageNo()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "crap";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("VoyageNo", "crap", DocWrapper.VoyageNo);
		}

		public void TestVesselName()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("VesselName", "MAJAPAHIT", DocWrapper.VesselName);
		}

		public void TestReceivingAgentAddress()
		{
			AssertNull("ReceivingAgentAddress should be null", DocWrapper.ReceivingAgentAddress);
			var rcvAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var principal = Factory.NewWithValidTestData<OrgHeader>();

			var port = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = rcvAgent.MainAddress.PK;

			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_OH_DeliveryAgent = principal.PK;

			AssertEquals("ReceivingagentAddress PK", rcvAgent.MainAddress.PK, DocWrapper.ReceivingAgentAddress.OrgAddress.PK);
		}

		public void TestTotalPackageCountPackType()
		{
			ZString totalPackageCountPackType = new ZString("TTT");
			Shipment.JS_F3_NKTotalCountPackType = totalPackageCountPackType;
			AssertEquals("TotalPackageCountPackType", totalPackageCountPackType, DocWrapper.TotalPackageCountPackType);
		}

		public void TestFCLPickupEquipmentNeeded()
		{
			ZString fCLPickupEquipmentNeeded = new ZString("PPP");
			Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = fCLPickupEquipmentNeeded;
			AssertEquals("FCLPickupEquipmentNeeded", fCLPickupEquipmentNeeded, DocWrapper.FCLPickupEquipmentNeeded);
		}

		public void TestFCLDeliveryEquipmentNeeded()
		{
			ZString fCLDeliveryEquipmentNeeded = new ZString("DDD");
			Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = fCLDeliveryEquipmentNeeded;
			AssertEquals("FCLDeliveryEquipmentNeeded", fCLDeliveryEquipmentNeeded, DocWrapper.FCLDeliveryEquipmentNeeded);
		}

		public void TestBookingDate()
		{
			ZDateTime bookingDate = new ZDateTime(2004, 04, 04);
			Shipment.JS_A_BKD = bookingDate;
			AssertEquals("BookingDate", bookingDate, DocWrapper.BookingDate);
		}

		public void TestTotalPackageCount()
		{
			Shipment.JS_TotalPackageCount = 1;
			AssertEquals("TotalPackageCount", 1, DocWrapper.TotalPackageCount);
		}

		public void TestUnitFreightRate()
		{
			ZDecimal unitFreightRate = new ZDecimal(12.12);
			Shipment.JS_UnitFreightRate = unitFreightRate;
			AssertEquals("UnitFreightRate", unitFreightRate, DocWrapper.UnitFreightRate);
		}

		public void TestServiceLevel()
		{
			RefServiceLevel serviceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());
			Shipment.JS_RS_NKServiceLevel = serviceLevel.RS_Code;
			AssertType("ServiceLevel is of type DocServiceLevel", typeof(DocServiceLevel), DocWrapper.ServiceLevel);
		}

		public void TestRateCurr()
		{
			Shipment.JS_RX_NKFrtRateCurrency = ZString.Empty;
			AssertNull("RateCurr", DocWrapper.RateCurr);

			Shipment.JS_RX_NKFrtRateCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			AssertType("RateCurr is of type DocCurrency", typeof(DocCurrency), DocWrapper.RateCurr);
		}

		public void TestOrderReferences()
		{
			AssertEquals("", DocWrapper.OrderReferences);
		}

		public void TestContainersByType()
		{
			AgencyShipmentContainer cont1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont3 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont4 = Shipment.BookedContainers.AddNew();
			AssertEquals("Empty container codes, grouped as 1", 1, DocWrapper.ContainersByTypeAndCommodity.Count);

			cont1.JC_ContainerCount = 1;
			cont2.JC_ContainerCount = 2;
			cont3.JC_ContainerCount = 3;
			cont4.JC_ContainerCount = 4;
			AssertEquals("Empty container codes, grouped as 1", 10, DocWrapper.ContainersByTypeAndCommodity[0].QuantityCount);

			RefContainer twentyGP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			RefContainer fortyGP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			cont1.JC_RC = twentyGP.PK;
			cont2.JC_RC = fortyGP.PK;
			cont3.JC_RC = twentyGP.PK;
			cont4.JC_RC = fortyGP.PK;

			DocAgencyContainerCollection coll = DocWrapper.ContainersByTypeAndCommodity;
			coll.Sort("QuantityCount", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("2 unique container codes", 2, coll.Count);
			AssertEquals("Container count on 20GP", 4, coll[0].QuantityCount);
			AssertEquals("Container 20GP", "20", coll[0].Container.Size);
			AssertEquals("Container 20GP", "GP", coll[0].Container.Type);
			AssertEquals("Container count on 40GP", 6, coll[1].QuantityCount);
			AssertEquals("Container 40GP", "40", coll[1].Container.Size);
			AssertEquals("Container 40GP", "GP", coll[1].Container.Type);

			cont1.JC_RC = twentyGP.PK;
			cont2.JC_RC = twentyGP.PK;
			cont3.JC_RC = twentyGP.PK;
			cont4.JC_RC = fortyGP.PK;
			cont2.JC_RH_NKContainerCommodityCode = "GEN";
			cont3.JC_RH_NKContainerCommodityCode = "GEN";

			coll = DocWrapper.ContainersByTypeAndCommodity;
			coll.Sort("QuantityCount", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("3 unique container codes with commodity", 3, coll.Count);
			AssertEquals("Container count on 20GP GEN", 1, coll[0].QuantityCount);
			AssertNull("Container 20GP commodity", coll[0].Commodity);
			AssertEquals("Container count on 40GP", 4, coll[1].QuantityCount);
			AssertNull("Container 40GP commodity empty", coll[1].Commodity);
			AssertEquals("Container count on 20GP GEN", 5, coll[2].QuantityCount);
			AssertEquals("Container 20GP commodity", "GEN", coll[2].Commodity.Code);

			cont3.JC_RH_NKContainerCommodityCode = "HAZ";
			coll = DocWrapper.ContainersByTypeAndCommodity;
			coll.Sort("QuantityCount", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("4 unique container codes with commodity", 4, coll.Count);
		}

		public void TestToString()
		{
			Shipment.JS_BookingReference = "BookingRef";
			AssertEquals("ToString()", "BookingRef", DocWrapper.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestETAString()
		{
			AssertEquals("", DocWrapper.ETAString);
			Shipment.JS_ClientRequestedETA = ZDateTime.Today;
			AssertEquals("Data should come from 'Client Requested ETA' if specified", ZDateTime.Today.ToString("dd-MMM-yy"), DocWrapper.ETAString);

			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			Shipment.JS_ClientRequestedETA = ZDateTime.Empty;
			AssertEquals("Data should come from 'ETA' if 'Client Requested ETA' is not specified", ZDateTime.Today.AddDays(1).ToString("dd-MMM-yy"), DocWrapper.ETAString);
		}

		public void PrintAsContainers()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Print as containers", ZBool.False, DocWrapper.PrintAsContainers);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Print as containers", ZBool.True, DocWrapper.PrintAsContainers);
		}

		public void TestDetailedDescriptionOfGoods()
		{
			AssertEquals("", DocWrapper.DetailedDescriptionOfGoods);

			var detailedDescription = Shipment.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			detailedDescription.ST_Table = Shipment.TableName;
			detailedDescription.ST_ParentID = Shipment.PK;
			detailedDescription.ST_NoteDataAsText = "This is the description of goods\r\nA long description of goods.";

			var otherNotes = Shipment.Notes.AddNew();
			otherNotes.ST_Table = Shipment.TableName;
			otherNotes.ST_ParentID = Shipment.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("This is the description of goods\r\nA long description of goods.", DocWrapper.DetailedDescriptionOfGoods);
		}

		public void TestMarksAndNumbers()
		{
			AssertEquals("", DocWrapper.MarksAndNumbers);

			var marksAndNumbersNote = Shipment.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_Table = Shipment.TableName;
			marksAndNumbersNote.ST_ParentID = Shipment.PK;
			marksAndNumbersNote.ST_NoteDataAsText = "Containers expiry date: 12/12/2005.\r\nContainer numbers 123456.\r\n";

			var otherNotes = Shipment.Notes.AddNew();
			otherNotes.ST_Table = Shipment.TableName;
			otherNotes.ST_ParentID = Shipment.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("Containers expiry date: 12/12/2005.\r\nContainer numbers 123456.", DocWrapper.MarksAndNumbers);
		}

		public void TestHandlingInstructions()
		{
			AssertEquals("", DocWrapper.HandlingInstructions);

			var handlingInstructionsNote = Shipment.Notes.AddNew();
			handlingInstructionsNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingInstructionsNote.ST_Table = Shipment.TableName;
			handlingInstructionsNote.ST_ParentID = Shipment.PK;
			handlingInstructionsNote.ST_NoteDataAsText = "Containers expiry date: 12/12/2005.\r\nContainer numbers 123456.\r\n";

			var otherNotes = Shipment.Notes.AddNew();
			otherNotes.ST_Table = Shipment.TableName;
			otherNotes.ST_ParentID = Shipment.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("Containers expiry date: 12/12/2005.\r\nContainer numbers 123456.", DocWrapper.HandlingInstructions);
		}

		public void TestBookingReference()
		{
			Shipment.JS_BookingReference = "BookingRef";
			AssertEquals("BookingReference", "BookingRef", DocWrapper.ToString());
		}

		public void TestCFSReference()
		{
			ZString cFSReference = new ZString("CFSReference");
			Shipment.JS_CFSReference = cFSReference;
			AssertEquals("CFSReference", cFSReference, DocWrapper.CFSReference);
		}

		public void TestConsolReference()
		{
			ZString consolReference = new ZString("ConsolReference");
			Shipment.JS_ConsolReference = consolReference;
			AssertEquals("ConsolReference", consolReference, DocWrapper.ConsolReference);
		}

		public void TestGoodsDescription()
		{
			ZString goodsDescription = new ZString("GoodsDescription");
			Shipment.JS_GoodsDescription = goodsDescription;
			AssertEquals("GoodsDescription", goodsDescription, DocWrapper.GoodsDescription);
		}

		public void TestOuterPacksPackType()
		{
			ZString outerPacksPackType = new ZString("PPP");
			Shipment.JS_F3_NKPackType = outerPacksPackType;
			AssertEquals("OuterPacksPackType", outerPacksPackType, DocWrapper.OuterPacksPackType);
		}

		public void TestPackingMode()
		{
			ZString packingMode = new ZString("PPP");
			Shipment.JS_PackingMode = packingMode;
			AssertEquals("PackingMode", packingMode, DocWrapper.PackingMode);
		}

		public void TestHouseBill()
		{
			ZString houseBill = new ZString("BILL");
			Shipment.JS_HouseBill = houseBill;
			AssertEquals("HouseBill", houseBill, DocWrapper.HouseBill);
		}

		public void TestInterimReceipt()
		{
			ZString interimReceipt = new ZString("InterimReceipt");
			Shipment.JS_InterimReceipt = interimReceipt;
			AssertEquals("InterimReceipt", interimReceipt, DocWrapper.InterimReceipt);
		}

		public void TestShipmentStatus()
		{
			ZString shipmentStatus = new ZString("SSS");
			Shipment.JS_ShipmentStatus = shipmentStatus;
			AssertEquals("ShipmentStatus", shipmentStatus, DocWrapper.ShipmentStatus);
		}

		public void TestShippedOnBoard()
		{
			ZString shippedOnBoard = new ZString("SSS");
			Shipment.JS_ShippedOnBoard = shippedOnBoard;
			AssertEquals("ShippedOnBoard", shippedOnBoard, DocWrapper.ShippedOnBoard);
		}

		public void TestTransportMode()
		{
			ZString transportMode = new ZString("MMM");
			Shipment.JS_TransportMode = transportMode;
			AssertEquals("TransportMode", transportMode, DocWrapper.TransportMode);
		}

		public void TestShipmentNumber()
		{
			ZString shipmentNumber = new ZString("S0001234");
			Shipment.JS_UniqueConsignRef = shipmentNumber;
			AssertEquals("ShipmentNumber", shipmentNumber, DocWrapper.ShipmentNumber);
		}

		public void TestVolumeUnit()
		{
			ZString volumeUnit = new ZString("M3");
			Shipment.JS_UnitOfVolume = volumeUnit;
			AssertEquals("VolumeUnit", volumeUnit, DocWrapper.VolumeUnit);
		}

		public void TestWeightUnit()
		{
			ZString weightUnit = new ZString("KG");
			Shipment.JS_UnitOfWeight = weightUnit;
			AssertEquals("WeightUnit", weightUnit, DocWrapper.WeightUnit);
		}

		public void TestEquipmentType()
		{
			string pickupType = Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Code;
			string deliveryType = Shipment.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Code;
			Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = pickupType;
			Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = deliveryType;
			string expected = pickupType + " - " + Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(pickupType);
			AssertEquals("EquipmentType", expected, DocWrapper.EquipmentType);
		}

		public void TestReceivalDepotReference()
		{
			AssertEquals("", DocWrapper.ReceivalDepotReference);

			Shipment.JS_CFSReference = "TESTTEST";
			AssertEquals("TESTTEST", DocWrapper.ReceivalDepotReference);
		}

		public void TestContainerVolumeHeadingDontBlowUp()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew();

			DocAgencyShipment shipmentWrapper = DocAgencyShipment.New(shipment, Factory);
			AssertNotNull("Should not blow up accessing this property", DocWrapper.ContainerVolumeHeading);
		}

		public void TestContainerWeightHeadingDontBlowUp()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew();

			DocAgencyShipment shipmentWrapper = DocAgencyShipment.New(shipment, Factory);
			AssertNotNull("Should not blow up accessing this property", shipmentWrapper.ContainerWeightHeading);
		}

		public void TestContainerPackageHeadingDontBlowUp()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew();

			DocAgencyShipment shipmentWrapper = DocAgencyShipment.New(shipment, Factory);
			AssertNotNull("Should not blow up accessing this property", shipmentWrapper.ContainerPackageHeading);
		}

		public void TestLoadingETDString()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			sailing.Origin.JA_E_DEP = now.AddDays(10);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			DocAgencyShipment wrapper = DocAgencyShipment.New(shipment, Factory);
			AssertEquals("date set on sailing", now.AddDays(10).ToShortDateString(), wrapper.LoadingETDString);

			shipment.JS_JX = ZGuid.Empty;
			AssertEquals("no sailing or consol", "", wrapper.LoadingETDString);
		}

		public void TestDischargeETAString()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.Destination.JB_E_ARV = now.AddDays(10);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			DocAgencyShipment wrapper = DocAgencyShipment.New(shipment, Factory);
			AssertEquals("date set on sailing", now.AddDays(10).ToShortDateString(), wrapper.DischargeETAString);

			shipment.JS_JX = ZGuid.Empty;
			AssertEquals("no sailing or consol", "", wrapper.DischargeETAString);
		}

		public void TestTransportHeaderAndInfo()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Floating Fruit-Mart";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "00456N";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			DocAgencyShipment wrapper = DocAgencyShipment.New(shipment, Factory);
			AssertEquals("heading when sailing attached", "VESSEL / VOYAGE / IMO(Lloyds)", wrapper.TransportHeading);
			AssertEquals("info when sailing attached", "Floating Fruit-Mart / 00456N /    ", wrapper.TransportInfo);

			shipment.JS_JX = ZGuid.Empty;
			AssertEquals("heading when no sailing or consol attached", "VESSEL / VOYAGE / IMO(Lloyds)", wrapper.TransportHeading);
			AssertEquals("info when no sailing or consol attached", "", wrapper.TransportInfo);
		}

		public void TestContainersByReleaseNum()
		{
			AgencyShipmentContainer cont1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont3 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer cont4 = Shipment.BookedContainers.AddNew();
			AssertEquals("Empty release nums, grouped as 1", 1, DocWrapper.ContainersByReleaseNum.Count);

			cont1.JC_ContainerCount = 1;
			cont2.JC_ContainerCount = 2;
			cont3.JC_ContainerCount = 3;
			cont4.JC_ContainerCount = 4;
			AssertEquals("Empty release nums, grouped as 1", 10, DocWrapper.ContainersByReleaseNum[0].QuantityCount);

			cont1.JC_ReleaseNum = "REL1";
			cont2.JC_ReleaseNum = "REL2";
			cont3.JC_ReleaseNum = "REL2";
			cont4.JC_ReleaseNum = "";

			DocAgencyContainerCollection coll = DocWrapper.ContainersByReleaseNum;
			coll.Sort("QuantityCount", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("3 unique release nums", 3, coll.Count);
			AssertEquals("Container count on REL1", 1, coll[0].QuantityCount);
			AssertEquals("Container REL1", "REL1", coll[0].ReleaseNum);
			AssertEquals("Container count on empty release", 4, coll[1].QuantityCount);
			AssertEquals("Container empty release", "", coll[1].ReleaseNum);
			AssertEquals("Container count on REL2", 5, coll[2].QuantityCount);
			AssertEquals("Container REL2", "REL2", coll[2].ReleaseNum);

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgAddress org1Address = org1.Addresses.AddNew();
			org1Address.OA_Address1 = "TEST ORG ADDRESS1";

			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgAddress org2Address = org1.Addresses.AddNew();
			org2Address.OA_Address1 = "TEST ORG ADDRESS2";

			cont2.JC_OA_DepartureContainerYardAddress = org2Address.PK;
			cont3.JC_OA_DepartureContainerYardAddress = org1Address.PK;

			coll = DocWrapper.ContainersByReleaseNum;
			coll.Sort("QuantityCount", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("4 unique release nums and Empty Container Park", 4, coll.Count);
			AssertEquals("Container count on REL1", 1, coll[0].QuantityCount);
			AssertEquals("Container count on REL2", 2, coll[1].QuantityCount);
			AssertEquals("Container count on REL3", 3, coll[2].QuantityCount);
			AssertEquals("Container count on REL4", 4, coll[3].QuantityCount);
		}

		public void TestDeliveryAddress()
		{
			AssertNull("DeliveryAddress", DocWrapper.DeliveryAddress);

			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertType("DeliveryAddress is of type DocDocAddress", typeof(DocDocAddress), DocWrapper.DeliveryAddress);
		}

		public void TestPickupAddress()
		{
			AssertNull("PickupAddress", DocWrapper.PickupAddress);

			Shipment.ConsignorPickupAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertType("PickupAddress is of type DocDocAddress", typeof(DocDocAddress), DocWrapper.PickupAddress);
		}

		public void TestCommodity()
		{
			AssertNotNull("Commodity not null", DocWrapper.Commodity);

			PackLine line = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("Commodity", "HAZ", DocWrapper.Commodity[0].Code);
		}

		public void TestDestinationLoco()
		{
			AssertNull("Destination", DocWrapper.DestinationLoco);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_RL_NKDestination = uNLOCO.RL_Code;
			AssertType("Destination is of type DocUNLOCO", typeof(DocUNLOCO), DocWrapper.DestinationLoco);
		}

		public void TestOriginLoco()
		{
			Shipment.JS_RL_NKOrigin = ZString.Empty;
			AssertNull("Origin", DocWrapper.OriginLoco);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_RL_NKOrigin = uNLOCO.RL_Code;
			AssertType("Origin is of type DocHazCat", typeof(DocUNLOCO), DocWrapper.OriginLoco);
		}

		public void TestGoodsCurr()
		{
			Shipment.JS_RX_NKGoodsValueCurr = "";
			AssertNull("GoodsCurr", DocWrapper.GoodsCurr);

			Shipment.JS_RX_NKGoodsValueCurr = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			AssertType("GoodsCurr is of type DocCurrency", typeof(DocCurrency), DocWrapper.GoodsCurr);
		}

		public void TestLoadPort()
		{
			Shipment.JS_NKLoadPort = "";
			AssertNull("LoadPort", DocWrapper.LoadPort);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_NKLoadPort = uNLOCO.RL_Code;
			AssertType("LoadPort is of type DocUNLOCO", typeof(DocUNLOCO), DocWrapper.LoadPort);
		}

		public void TestDischargePort()
		{
			AssertNull("DischargePort", DocWrapper.DischargePort);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_NKDischargePort = uNLOCO.RL_Code;
			AssertType("DischargePort is of type DocUNLOCO", typeof(DocUNLOCO), DocWrapper.DischargePort);
		}

		public void TestSailing()
		{
			AssertNull("Sailing", DocWrapper.Sailing);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			Shipment.JS_JX = voyage.Sailings[0].PK;

			AssertType("Sailing is of type DocSailing", typeof(DocSailing), DocWrapper.Sailing);
		}

		public void TestActualRCV()
		{
			ZDateTime actualRCV = new ZDateTime(2004, 04, 04);
			Shipment.JS_A_RCV = actualRCV;
			AssertEquals("ActualRCV", actualRCV, DocWrapper.ActualRCV);
		}

		public void TestClientRequestedETA()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.ClientRequestedETA);
			Shipment.JS_ClientRequestedETA = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.ClientRequestedETA);
		}

		public void TestVolume()
		{
			ZDecimal volume = new ZDecimal(12.12);
			Shipment.JS_ActualVolume = volume;
			AssertEquals("Volume", "12.12", DocWrapper.Volume);
		}

		public void TestWeight()
		{
			ZDecimal weight = new ZDecimal(12.12);
			Shipment.JS_ActualWeight = weight;
			AssertEquals("Weight", "12.12", DocWrapper.Weight);
		}

		public void TestGoodsValue()
		{
			ZDecimal goodsValue = new ZDecimal(12.12);
			Shipment.JS_GoodsValue = goodsValue;
			AssertEquals("GoodsValue", goodsValue, DocWrapper.GoodsValue);
		}

		public void TestOuterPacks()
		{
			Shipment.JS_OuterPacks = 1;
			AssertEquals("OuterPacks", 1, DocWrapper.OuterPacks);
		}

		public void TestCarrier()
		{
			AssertNull(DocWrapper.Carrier);

			OrgHeader line = Factory.New<OrgHeader>();
			line.OH_Code = "TESTLINE";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_OH_Line = line.PK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("TESTLINE", DocWrapper.Carrier.Code);

			OrgHeader line2 = Factory.New<OrgHeader>();
			line2.OH_Code = "SECONDLINE";
			Shipment.JS_OA_BookedShippingLineAddress = line2.MainAddress.PK;
			AssertEquals("The 'Carrier' field should on the shipment override that from the sailing.", "SECONDLINE", DocWrapper.Carrier.Code);
		}

		public void TestBookingPortOfLoading()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USCHI";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("USCHI", DocWrapper.BookingPortOfLoading.Code);
		}

		public void TestBookingPortOfDischargeSailing()
		{
			AssertNull(DocWrapper.BookingPortOfDischarge);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("USCHI", DocWrapper.BookingPortOfDischarge.Code);
		}

		public void TestBookingTransport()
		{
			AssertEquals("", DocWrapper.BookingTransport);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "VOY111";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;

			AssertEquals("MAJAPAHIT / VOY111 / 7920572", DocWrapper.BookingTransport);
		}

		public void TestBookingDepartureReference()
		{
			Shipment.JS_TransportMode = "SEA";
			AssertEquals("", DocWrapper.BookingDepartureReference);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("", DocWrapper.BookingDepartureReference);

			origin.JA_DepartReference = "DEP REF 123";
			AssertEquals("DEP REF 123", DocWrapper.BookingDepartureReference);
		}

		public void TestDepartureReferenceHeading()
		{
			AssertEquals("DEPARTURE REFERENCE", DocWrapper.BookingDepartureReferenceHeading);
		}

		public void TestBookingTransportHeading()
		{
			AssertEquals("Transport heading", "VESSEL / VOYAGE / IMO(Lloyds)", DocWrapper.BookingTransportHeading);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBookingETD()
		{
			AssertEquals(ZString.Empty, DocWrapper.BookingETD);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today;

			voyage.GenerateSailings();

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), DocWrapper.BookingETD);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBookingETA()
		{
			AssertEquals(ZString.Empty, DocWrapper.BookingETA);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = ZDateTime.Today;

			Shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), DocWrapper.BookingETA);
		}

		public void TestContainerCountOnBookingShipment()
		{
			AssertEquals("", DocWrapper.ContainerCount);
			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();

			RefContainer container20PL = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));
			RefContainer container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			container1.JC_RC = container20PL.PK;
			container3.JC_RC = container40GP.PK;

			AssertContainsExactElementsInAnyOrder(
				new string[] { "1 X 20PL", "1 X 40GP" },
				DocWrapper.ContainerCount.ToString().Split(new string[] { ", " }, StringSplitOptions.None));

			container2.JC_RC = container20PL.PK;

			AssertContainsExactElementsInAnyOrder(
				new string[] { "2 X 20PL", "1 X 40GP" },
				DocWrapper.ContainerCount.ToString().Split(new string[] { ", " }, StringSplitOptions.None));

			container2.JC_ContainerCount = 5;
			container3.JC_ContainerCount = 4;

			AssertContainsExactElementsInAnyOrder(
				new string[] { "6 X 20PL", "4 X 40GP" },
				DocWrapper.ContainerCount.ToString().Split(new string[] { ", " }, StringSplitOptions.None));
		}

		public void TestBookingCutOffDateOnSailing()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.BookingCutOffDate);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_CutOff = ZDateTime.Today;

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			Shipment.JS_JX = sailing.PK;
			AssertEquals(ZDateTime.Today, DocWrapper.BookingCutOffDate);
		}

		public void TestBookingNotes()
		{
			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;
			note.ST_NoteDataAsText = "Booking Notes\nNote1\nNote2\nNote3.\n";

			var otherNotes = Shipment.Notes.AddNew();
			otherNotes.ST_Table = Shipment.TableName;
			otherNotes.ST_ParentID = Shipment.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("Wrong Booking notes", "Booking Notes\r\nNote1\r\nNote2\r\nNote3.", DocWrapper.BookingNotes);
		}

		public void TestBookingContainerReleaseNumber()
		{
			AssertEquals("", DocWrapper.BookingContainerReleaseNumber);

			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();
			AssertEquals("", DocWrapper.BookingContainerReleaseNumber);

			container3.JC_ReleaseNum = "NOT EMPTY";
			AssertEquals("NOT EMPTY", DocWrapper.BookingContainerReleaseNumber);
		}

		public void TestBookingContainerEmptyRequiredDate()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.BookingContainerEmptyRequiredDate);
			AssertEquals(false, DocWrapper.ShowEmptyRequiredByHeading);

			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();

			AssertEquals(ZDateTime.Empty, DocWrapper.BookingContainerEmptyRequiredDate);
			AssertEquals(false, DocWrapper.ShowEmptyRequiredByHeading);

			container2.JC_EmptyRequired = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.BookingContainerEmptyRequiredDate);
			AssertEquals(true, DocWrapper.ShowEmptyRequiredByHeading);
		}

		public void TestBookingContainerPickupFullDate()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.BookingContainerPickupFullDate);
			AssertEquals("N", DocWrapper.ShowFullPickupByHeading.ToString());

			AgencyShipmentContainer container1 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = Shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container3 = Shipment.BookedContainers.AddNew();

			AssertEquals(ZDateTime.Empty, DocWrapper.BookingContainerPickupFullDate);
			AssertEquals("N", DocWrapper.ShowFullPickupByHeading.ToString());

			container1.JC_DepartureEstimatedPickup = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.BookingContainerPickupFullDate);
			AssertEquals("Y", DocWrapper.ShowFullPickupByHeading.ToString());
		}

		public void TestPortOfLoading()
		{
			Shipment.JS_NKLoadPort = "";
			AssertNull("PortOfLoading", DocWrapper.PortOfLoading);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_NKLoadPort = uNLOCO.RL_Code;
			AssertType("PortOfLoading is of type DocUNLOCO", typeof(DocUNLOCO), DocWrapper.PortOfLoading);
		}

		public void TestPortOfDischarge()
		{
			Shipment.JS_NKDischargePort = "";
			AssertNull("PortOfDischarge", DocWrapper.DischargePort);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_NKDischargePort = uNLOCO.RL_Code;
			AssertType("PortOfDischarge is of type DocUNLOCO", typeof(DocUNLOCO), DocWrapper.PortOfDischarge);
		}

		public void TestShippingLine()
		{
			OrgHeader bookedShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			bookedShippingLine.OH_Code = "SHPLINE";
			Shipment.JS_OA_BookedShippingLineAddress = bookedShippingLine.MainAddress.PK;
			AssertEquals("ShippingLine should come from JS_OA_BookedShippingLineAddress", "SHPLINE", DocWrapper.ShippingLine.Code);
		}

		public void TestTransportInfo()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			VoyageOrigin origin = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageDestination destination = Factory.NewWithValidTestData<VoyageDestination>();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			AssertEquals(ZString.Empty, docShipment.TransportInfo);

			shipment.JS_JX = sailing.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			voyage.JV_VoyageFlight = "23";

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234";
			vessel.RV_Code = "Admiral Freight";

			voyage.JV_RV_NKVessel = vessel.RV_FK;

			shipment.JS_IsShipping = true;

			AssertEquals("Admiral Freight / 23 / 1234", docShipment.TransportInfo);
		}

		public void TestPreAlertDocumentHeader()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			docShipment.SetReportNameForTesting("Report");

			AssertEquals("FCL Report", docShipment.PreAlertDocumentHeader);
		}

		public void TestIsAgencyBooking()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_IsShipping = true;
			shipment.JS_ShipmentStatus = Enterprise.Freight.Integration.ShipmentStatusList.Codes.Booked;
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);
			Assert(docShipment.IsAgencyBooking);
		}

		public void TestReceivalCommencesDate()
		{
			var sailing = GetSailing("USLAX", "AUSYD");
			sailing.Origin.JA_DGReceivalCommences = ZDateTime.BrettsBirthday;
			sailing.Origin.JA_ReceivalCommences = ZDateTime.BrettsBirthday.AddDays(1);

			Factory.Save();

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			var container = shipment.BookedContainers.AddNew();
			var docShipment = DocAgencyShipment.New(shipment, Factory);

			container.JC_RH_NKContainerCommodityCode = "GEN";
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), docShipment.ReceivalCommencesDate);

			container.JC_RH_NKContainerCommodityCode = "HAZ";
			AssertEquals(ZDateTime.BrettsBirthday, docShipment.ReceivalCommencesDate);
		}

		public void TestBookingParty()
		{
			Shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			Shipment.BookingPartyDocumentaryAddress.E2_CompanyName = "Company";

			AssertEquals("Company", DocWrapper.BookingParty.Name);
		}

		public void TestReceivingForwarderDocContacts_ShipmentHasValidReceivingForwarderAddress_ReturnValidContacts()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.ReceivingForwarderAddress.E2_AddressOverride = true;
			shipment.ReceivingForwarderAddress.E2_CompanyName = "McLaren";

			var wrapperToTest = DocAgencyShipment.New(shipment, Factory);

			AssertEquals("McLaren", wrapperToTest.ReceivingForwarder.Name);
		}

		public void TestReceivingForwarderDocContacts_ShipmentHasEmptyReceivingForwarderAddress_ReturnEmptyContacts()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.ReceivingForwarderAddress.E2_AddressOverride = true;

			var wrapperToTest = DocAgencyShipment.New(shipment, Factory);

			AssertEquals(ZString.Empty, wrapperToTest.ReceivingForwarder.Name);
		}

		public void TestSendingForwarderDocContacts_ShipmentHasValidSendingForwarderAddress_ReturnValidContacts()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.SendingForwarderAddress.E2_AddressOverride = true;
			shipment.SendingForwarderAddress.E2_CompanyName = "McLaren";

			var wrapperToTest = DocAgencyShipment.New(shipment, Factory);

			AssertEquals("McLaren", wrapperToTest.SendingForwarder.Name);
		}

		public void TestSendingForwarderDocContacts_ShipmentHasEmptySendingForwarderAddress_ReturnEmptyContacts()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.SendingForwarderAddress.E2_AddressOverride = true;

			var wrapperToTest = DocAgencyShipment.New(shipment, Factory);

			AssertEquals(ZString.Empty, wrapperToTest.SendingForwarder.Name);
		}

		public void TestHasHazardous()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			container.JC_RH_NKContainerCommodityCode = "GEN";
			Assert(!docShipment.HasHazardous);

			container.JC_RH_NKContainerCommodityCode = "HAZD";
			Assert(docShipment.HasHazardous);
		}

		public void TestCutOffDate()
		{
			var sailing = GetSailing("AUSYD", "USLAX");
			sailing.Origin.JA_DGCutOff = ZDateTime.BrettsBirthday;
			sailing.Origin.JA_CutOff = ZDateTime.BrettsBirthday.AddDays(1);

			Factory.Save();

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			var container = shipment.BookedContainers.AddNew();
			var docShipment = DocAgencyShipment.New(shipment, Factory);

			container.JC_RH_NKContainerCommodityCode = "GEN";
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), docShipment.CutOffDate);

			container.JC_RH_NKContainerCommodityCode = "HAZ";
			AssertEquals(ZDateTime.BrettsBirthday, docShipment.CutOffDate);
		}

		public void TestCTO()
		{
			var cto = Factory.NewWithValidTestData<OrgHeader>();
			cto.OH_FullName = "Some value";

			var sailing = GetSailing("USLAX", "AUSYD");
			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			Factory.Save();

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;
			var docShipment = DocAgencyShipment.New(shipment, Factory);

			AssertEquals("Some value", docShipment.CTO);
		}

		public void TestContext()
		{
			AssertEquals("AgencyShipment", DocWrapper.Context);
		}

		public void TestTACImage()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			shipment.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			AssertNull(docShipment.TACImage);

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertNull(docShipment.TACImage);

			DeliveryOrder element = collection.AddNew();
			element.PrincipalPK = principal.PK;
			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			element.Image = new Bitmap(10, 10);

			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertNotNull(docShipment.TACImage);
		}

		public void TestContainsTACImage()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			shipment.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			Assert(!docShipment.ContainsTACImage);

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DeliveryOrder element = collection.AddNew();
			element.PrincipalPK = principal.PK;
			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			element.Image = new Bitmap(10, 10);
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Assert(docShipment.ContainsTACImage);
		}

		public void TestPrintPerContainer()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocAgencyShipment docShipment = DocAgencyShipment.New(shipment, Factory);

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			shipment.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			Assert(!docShipment.PrintPerContainer);

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DeliveryOrder element = collection.AddNew();
			element.PrincipalPK = principal.PK;
			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			element.Image = new Bitmap(10, 10);
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Assert(!docShipment.PrintPerContainer);

			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PCT;
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Assert(docShipment.PrintPerContainer);
		}

		public void TestNotifyParties()
		{
			Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			Shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "NotifyParty1";
			Shipment.NotifyParty2DocumentaryAddress.E2_AddressOverride = true;
			Shipment.NotifyParty2DocumentaryAddress.E2_CompanyName = "NotifyParty2";
			Shipment.NotifyParty3DocumentaryAddress.E2_AddressOverride = true;
			Shipment.NotifyParty3DocumentaryAddress.E2_CompanyName = "NotifyParty3";

			AssertEquals(3, DocWrapper.NotifyPartyCount);
			AssertEquals("NotifyParty1", DocWrapper.NotifyParty.Name);
			AssertEquals("NotifyParty2", DocWrapper.NotifyParty2.Name);
			AssertEquals("NotifyParty3", DocWrapper.NotifyParty3.Name);

			Shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "";

			AssertEquals(2, DocWrapper.NotifyPartyCount);
			AssertEquals("NotifyParty2", DocWrapper.NotifyParty.Name);
			AssertEquals("NotifyParty3", DocWrapper.NotifyParty2.Name);
			AssertEquals(null, DocWrapper.NotifyParty3);

			Shipment.NotifyParty2DocumentaryAddress.E2_CompanyName = "";

			AssertEquals(1, DocWrapper.NotifyPartyCount);
			AssertEquals("NotifyParty3", DocWrapper.NotifyParty.Name);
			AssertEquals(null, DocWrapper.NotifyParty2);
			AssertEquals(null, DocWrapper.NotifyParty3);

			Shipment.NotifyParty3DocumentaryAddress.E2_CompanyName = "";

			AssertEquals(0, DocWrapper.NotifyPartyCount);
			AssertEquals(null, DocWrapper.NotifyParty);
			AssertEquals(null, DocWrapper.NotifyParty2);
			AssertEquals(null, DocWrapper.NotifyParty3);
		}

		public void TestPreAlertReference()
		{
			AssertEquals("Shippers Reference", DocWrapper.PreAlertReferenceHeading);
			AssertEquals("", DocWrapper.PreAlertReference);

			Shipment.JS_BookingReference = "Blaticus";
			AssertEquals("Blaticus", DocWrapper.PreAlertReference);
		}

		public void TestShowChargeable()
		{
			AssertEquals(false, DocWrapper.ShowChargeable);
		}

		public void TestExportReceivingDepot()
		{
			var cto = Factory.NewWithValidTestData<OrgHeader>();
			cto.OH_IsSeaCTO = true;
			cto.MainAddress.OA_Address1 = "ASONEHAAOEU";

			var sailing = GetSailing("AUBNE", "SGSIN");
			sailing.Origin.JA_OA_DepartureCTOAddress = cto.MainAddress.PK;

			Factory.Save();

			AssertEquals(null, DocWrapper.ExportReceivingDepot);

			Shipment.JS_JX = sailing.PK;
			AssertEquals("ASONEHAAOEU", DocWrapper.ExportReceivingDepot.PostalAddress);
		}

		public void TestBookingContainerLayout()
		{
			AssertEquals("4", DocWrapper.BookingContainerLayout);
		}

		public void TestShowMarksAndNumbersOnBookingConfirmation()
		{
			AssertEquals(false, DocWrapper.ShowMarksAndNumbersOnBookingConfirmation);
		}

		public void TestHouseBillHeading()
		{
			AssertEquals("Bill Of Lading", DocWrapper.HouseBillHeading);
			AssertEquals("Bill Of Lading", DocWrapper.HouseBillAndIssueHeading);
		}

		public void TestShowChargesOnBookingConfirmation()
		{
			DocumentsDataRegistry.Instance.ShowChargesOnAgencyBookingConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, DocWrapper.ShowChargesOnBookingConfirmation);

			DocumentsDataRegistry.Instance.ShowChargesOnAgencyBookingConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, DocWrapper.ShowChargesOnBookingConfirmation);
		}

		public void TestShowChargesOnArrivalNotice()
		{
			DocumentsDataRegistry.Instance.ShowChargesOnAgencyArrivalNotice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, DocWrapper.ShowChargesOnArrivalNotice);

			DocumentsDataRegistry.Instance.ShowChargesOnAgencyArrivalNotice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, DocWrapper.ShowChargesOnArrivalNotice);
		}

		public void TestShowExchangeRatesOnArrivalNotice()
		{
			AssertEquals(true, DocWrapper.ShowExchangeRatesOnArrivalNotice);
		}

		public void TestPortDisplayMode()
		{
			AssertEquals("LoadDischargeDestination", DocWrapper.PortDisplayMode);
		}

		public void TestContainers()
		{
			AgencyShipmentContainer container = Shipment.BookedContainers.AddNew();
			IDocContainerCollection containers = DocWrapper.Containers;

			AssertEquals(1, containers.Count);

			DocumentWrapper wrapper = (DocumentWrapper)containers[0];
			AssertEquals(typeof(DocAgencyContainer), wrapper.GetType());
			AssertEquals(container, wrapper.WrappedObject);
		}

		public void TestFreightDebtor()
		{
			AssertEquals("Precondition - FreightDebtor", null, DocWrapper.FreightDebtor);
			AssertEquals("Precondition - FreightChargeDebtor", null, DocWrapper.FreightChargeDebtor);

			Job header = GetNewHeader(Shipment);
			JobCharge charge1 = AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			JobCharge charge2 = AddCharge(header, "DLAB", 750);
			charge1.JR_OH_SellAccount = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B")).PK;
			charge2.JR_OH_SellAccount = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C")).PK;

			shipment.Job.LocalChargesPK = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A")).PK;

			AssertEquals("No freight charge - default to Local Client", shipment.Job.LocalCharges.OH_Code, DocWrapper.FreightDebtor.Code);
			AssertEquals("No freight charge - no FreightChargeDebtor", null, DocWrapper.FreightChargeDebtor);

			JobCharge charge3 = AddCharge(header, "FRT", 10000);
			charge3.JR_OH_SellAccount = ZGuid.Empty;

			AssertEquals("Freight charge has no Sell Account - default to Local Client", shipment.Job.LocalCharges.OH_Code, DocWrapper.FreightDebtor.Code);
			AssertEquals("No Sell Account - no FreightChargeDebtor", null, DocWrapper.FreightChargeDebtor);
		}

		public void TestFreightDebtor2()
		{
			Job header = GetNewHeader(Shipment);
			JobCharge charge1 = AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			JobCharge charge2 = AddCharge(header, "DLAB", 750);
			charge1.JR_OH_SellAccount = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B")).PK;
			charge2.JR_OH_SellAccount = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C")).PK;

			shipment.Job.LocalChargesPK = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A")).PK;
			JobCharge charge3 = AddCharge(header, "FRT", 10000);
			charge3.JR_OH_SellAccount = ZGuid.Empty;

			JobCharge charge4 = AddCharge(header, "FRT", 4000);
			charge4.JR_OH_SellAccount = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D")).PK;

			AssertEquals("Freight charge account", charge4.SellAccount.OH_Code, DocWrapper.FreightDebtor.Code);
			AssertEquals("FreightChargeDebtor", charge4.SellAccount.OH_Code, DocWrapper.FreightChargeDebtor.Code);
		}

		public void TestSeaLegs()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			Factory.Save();

			Shipment.JS_JX = voyage.Sailings[0].PK;

			AssertEquals("expecting 1 leg", 1, DocWrapper.SeaLegs.Count);
			AssertEquals("expecting the details to come from the sailing", "AUBNE", DocWrapper.SeaLegs[0].PortOfLoading.Code);
			AssertEquals("expecting the details to come from the sailing", "SGSIN", DocWrapper.SeaLegs[0].PortOfDischarge.Code);

			Transport transport2 = CreateShipmentTransport(Shipment, "SEA", Core.Constants.TransportPlanningType.MainVessel,
					"SGSIN", "JPOSA", ZDateTime.Empty, ZDateTime.Today.AddDays(6));

			Transport transport1 = CreateShipmentTransport(Shipment, "SEA", Core.Constants.TransportPlanningType.PreCarriage,
					"NZAKL", "AUBNE", ZDateTime.Today, ZDateTime.Empty);

			AssertEquals(3, DocWrapper.SeaLegs.Count);
			AssertEquals("First load", "NZAKL", DocWrapper.SeaLegs[0].PortOfLoading.Code);
			AssertEquals("First discharge", "AUBNE", DocWrapper.SeaLegs[0].PortOfDischarge.Code);
			AssertEquals("Second load", "AUBNE", DocWrapper.SeaLegs[1].PortOfLoading.Code);
			AssertEquals("Second discharge", "SGSIN", DocWrapper.SeaLegs[1].PortOfDischarge.Code);
			AssertEquals("Third load", "SGSIN", DocWrapper.SeaLegs[2].PortOfLoading.Code);
			AssertEquals("Third discharge", "JPOSA", DocWrapper.SeaLegs[2].PortOfDischarge.Code);

			Transport transport0 = CreateShipmentTransport(Shipment, "ROA", Core.Constants.TransportPlanningType.Other,
				"NZCHC", "NZAKL", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));

			Transport transport4 = CreateShipmentTransport(Shipment, "RAI", Core.Constants.TransportPlanningType.Other,
				"AUBNE", "AUCAN", ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(8));

			AssertEquals("Non-Sea legs are ignored", 3, DocWrapper.SeaLegs.Count);
			AssertEquals("First load", "NZAKL", DocWrapper.SeaLegs[0].PortOfLoading.Code);
			AssertEquals("First discharge", "AUBNE", DocWrapper.SeaLegs[0].PortOfDischarge.Code);
			AssertEquals("Second load", "AUBNE", DocWrapper.SeaLegs[1].PortOfLoading.Code);
			AssertEquals("Second discharge", "SGSIN", DocWrapper.SeaLegs[1].PortOfDischarge.Code);
			AssertEquals("Third load", "SGSIN", DocWrapper.SeaLegs[2].PortOfLoading.Code);
			AssertEquals("Third discharge", "JPOSA", DocWrapper.SeaLegs[2].PortOfDischarge.Code);
		}

		#region Implementation

		AgencyShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<AgencyShipment>()); }
		}
		AgencyShipment shipment;

		JobSailing GetSailing(ZString load, ZString discharge)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		DocAgencyShipment DocWrapper
		{
			get { return DocAgencyShipment.New(Shipment, Factory); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			DocAgencyShipment wrapper = DocAgencyShipment.New(shipment, shipment.Factory);
			return new DocumentWrapper[] { wrapper };
		}

		static Job GetNewHeader(IJobHeaderParent parent)
		{
			Job result = parent.Factory.NewJobForTesting<Job>();
			result.JH_ParentID = parent.PK;
			result.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.TableName);
			result.Parent = parent;
			return result;
		}

		static JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt)
		{
			return AddCharge(header, chargeCode, localSellAmt, AgencyInvoiceTypesList.Codes.LocalCollect);
		}

		static JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt, ZString invoiceType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = header.Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_LocalSellAmt = localSellAmt;
			charge.JR_InvoiceType = invoiceType;
			return charge;
		}

		Transport CreateShipmentTransport(AgencyShipment shipment, ZString mode, ZString type, ZString load, ZString disch, ZDateTime etd, ZDateTime eta)
		{
			Transport leg = shipment.Transports.AddNew();
			leg.JW_TransportMode = mode;
			leg.JW_TransportType = type;
			leg.JW_RL_NKLoadPort = load;
			leg.JW_RL_NKDiscPort = disch;
			leg.JW_ETD = etd;
			leg.JW_ETA = eta;

			return leg;
		}

		#endregion
	}
}
