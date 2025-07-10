using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCFSShipment))]
	sealed class FreightWrapperFromCFSShipmentTest : FreightWrapperTest
	{
		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			CommonContainer container = ((CFSShipment)parent).Consols[0].Containers.AddNew();
			PackLine packLine = ((CFSShipment)parent).OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			return container;
		}

		public void TestExportReceivingDepotAddress()
		{
			CFSLoadListConsol cfsLoadList = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = cfsLoadList.Shipments.AddNew();
			FreightWrapperFromCFSShipment wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			cfsLoadList.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
		}

		public void TestExportReceivingCTOAddress()
		{
			CFSLoadListConsol cfsLoadList = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = cfsLoadList.Shipments.AddNew();
			FreightWrapperFromCFSShipment wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			cfsLoadList.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestShipmentNumberBarcodeForLabels()
		{
			var shipment = Factory.New<CFSShipment>();
			var shipmentWrapper = new FreightWrapperFromCFSShipment(shipment, shipment.Factory);

			AssertEquals("Precondition: Origin", "", shipment.JS_UniqueConsignRef);

			AssertEquals("Missing job number in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeText);

			shipment.JS_UniqueConsignRef = "S000001000";
			shipmentWrapper = new FreightWrapperFromCFSShipment(shipment, shipment.Factory);

			AssertEquals("Job Numbers exists for shipment, BarcodeTextForFont property has JobNumber", "S000001000", shipmentWrapper.BarcodeText);
		}

		public void TestExportReceivalAddress()
		{
			CFSLoadListConsol cfsLoadList = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = cfsLoadList.Shipments.AddNew();
			FreightWrapperFromCFSShipment wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			cfsLoadList.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			cfsLoadList.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestPickupDeliveryConfirmations()
		{
			CFSLoadListConsol consol1 = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment1 = consol1.Shipments.AddNew();
			CFSShipment shipment2 = consol1.Shipments.AddNew();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			CommonContainer container2 = consol1.Containers.AddNew();
			packline1.SetContainer(consol1, container1);
			packline2.SetContainer(consol1, container2);

			CommonPickupDeliveryConfirm containerOriginArrival = container1.OriginCFSArrival;
			CommonPickupDeliveryConfirm containerOriginDeparture = container1.OriginCFSDeparture;
			CommonPickupDeliveryConfirm containerDestinationArrival = container1.DestinationCFSArrival;
			CommonPickupDeliveryConfirm containerDestinationDeparture = container1.DestinationCFSDeparture;
			CommonPickupDeliveryConfirm shipmentOriginArrival = shipment1.OriginCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipmentOriginDeparture = shipment1.OriginCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm shipmentDestinationArrival = shipment1.DestinationCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipmentDestinationDeparture = shipment1.DestinationCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm container2OriginArrivalShouldntBeInCollection = container2.OriginCFSArrival;
			CommonPickupDeliveryConfirm container2OriginDepartureShouldntBeInCollection = container2.OriginCFSDeparture;
			CommonPickupDeliveryConfirm container2DestinationArrivalShouldntBeInCollection = container2.DestinationCFSArrival;
			CommonPickupDeliveryConfirm container2DestinationDepartureShouldntBeInCollection = container2.DestinationCFSDeparture;
			CommonPickupDeliveryConfirm shipment2OriginArrivalShouldntBeInCollection = shipment2.OriginCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipment2OriginDepartureShouldntBeInCollection = shipment2.OriginCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm shipment2DestinationArrivalShouldntBeInCollection = shipment2.DestinationCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipment2DestinationDepartureShouldntBeInCollection = shipment2.DestinationCFSDepartures.AddNew();

			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;

			List<CommonPickupDeliveryConfirm> expectedFCL = new List<CommonPickupDeliveryConfirm>();
			expectedFCL.Add(containerOriginArrival);
			expectedFCL.Add(containerOriginDeparture);
			expectedFCL.Add(containerDestinationArrival);
			expectedFCL.Add(containerDestinationDeparture);
			expectedFCL.Add(shipmentOriginArrival);
			expectedFCL.Add(shipmentOriginDeparture);
			expectedFCL.Add(shipmentDestinationArrival);
			expectedFCL.Add(shipmentDestinationDeparture);

			wrapper = new FreightWrapperFromCFSShipment(shipment1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedFCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));
		}

		protected override void SetupBusinessObjectForContainerLayoutStyleTest(BusinessObject parent)
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.Shipments.Add((CFSShipment)parent);
		}

		public void TestMasterBillHeading()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			FreightWrapper wrapper1 = FreightWrapper.New(shipment1, Factory)[0];

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			FreightWrapper wrapper2 = FreightWrapper.New(shipment2, Factory)[0];

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			FreightWrapper wrapper3 = FreightWrapper.New(shipment3, Factory)[0];

			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.HouseBillHeading", "MAWB", wrapper1.MasterBillHeading);

			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.HouseBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);

			shipment3.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.HouseBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		public void TestHouseBillHeading()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			FreightWrapper wrapper1 = FreightWrapper.New(shipment1, Factory)[0];

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			FreightWrapper wrapper2 = FreightWrapper.New(shipment2, Factory)[0];

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			FreightWrapper wrapper3 = FreightWrapper.New(shipment3, Factory)[0];

			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.HouseBillHeading", "HAWB", wrapper1.HouseBillHeading);

			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.HouseBillHeading", "House Bill Of Lading", wrapper2.HouseBillHeading);

			shipment3.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.HouseBillHeading", "House Bill", wrapper3.HouseBillHeading);
		}

		public void TestConsignorAndConsigneeGetLabelledAsUltimateWhereAppropriate()
		{
			CFSShipment coLoadMasterShipment = Factory.New<CFSShipment>();
			coLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;

			AssertEquals("wrapper.Consignee.TypeDescription", "Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Consignor", wrapper.Consignor.TypeDescription);

			shipment.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.Consignee.TypeDescription", "Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Consignor", wrapper.Consignor.TypeDescription);

			OrgHeader coLoadConsignee = Factory.New<OrgHeader>();
			coLoadMasterShipment.ConsigneeDocumentaryAddress.OrganisationPK = coLoadConsignee.PK;
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.Consignee.TypeDescription", "Ultimate Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Ultimate Consignor", wrapper.Consignor.TypeDescription);

			coLoadConsignee.MiscServ.OM_FWDealDirectlyWithUltimates = true;
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.Consignee.TypeDescription", "Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Consignor", wrapper.Consignor.TypeDescription);
		}

		public void TestGoodsDescriptionWorksFromExtendedGoodsDescription()
		{
			AssertEquals("wrapper.GoodsDescription", ZString.Empty, wrapper.GoodsDescription);

			shipment.JS_GoodsDescription = "I LOVE CHIPS WITH SAUCE";
			AssertEquals("wrapper.GoodsDescription", "I LOVE CHIPS WITH SAUCE", wrapper.GoodsDescription);

			string longGoodsDescription = @"I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, 
I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I REALLY DO!!!!!";
			shipment.DetailedGoodsDescriptionNoteText = longGoodsDescription;
			AssertEquals("wrapper.GoodsDescription", longGoodsDescription, wrapper.GoodsDescription);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var shipment = Factory.New<CFSShipment>();
			var wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);

			AssertEquals("TrackingBusinessObjectPK", shipment.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestWrapperMappingMBLIssue()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = new ZDateTime(2007, 8, 24);
			shipment = consol.Shipments.AddNew();

			FreightWrapperFromCFSShipment fullWrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("fullWrapper.MasterBillIssue", new ZDateTime(2007, 8, 24), fullWrapper.MasterBillIssue);
		}

		public void TestWrapperMappingFull()
		{
			#region Setup

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_UniqueConsignRef = "C00012345";
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			OrgHeader sendingForwarder = GetOrgHeader("SND_FWDER");
			sendingForwarder.MainAddress.OA_Address1 = "SND MAIN ADDRESS";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress sfAddr2 = sendingForwarder.Addresses.AddNew();
			sfAddr2.OA_Address1 = "SND ADDRESS 2";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;

			OrgHeader rcvForwarder = GetOrgHeader("RCV_FWDER");
			rcvForwarder.MainAddress.OA_Address1 = "RCV MAIN ADDRESS";
			rcvForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress rfAddr2 = rcvForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RCV ADDRESS 2";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			OrgHeader carrier = GetOrgHeader("SHIPPINGLINE");
			carrier.MainAddress.OA_Address1 = "CARR MAIN ADDRESS";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress slAddr2 = carrier.Addresses.AddNew();
			slAddr2.OA_Address1 = "CARR ADDRESS 2";
			slAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = slAddr2.PK;

			consol.JK_OA_CreditorAddress = GetOrgHeader("CONS_CREDIT").MainAddress.PK;
			consol.JK_BookingReference = "BOOK_A_HOOKER";
			consol.JK_PrepaidCollect = "CCX";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO WHATEVER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_ArrivalReference = "ARRIVAL REFERENCE";
			voyageDestination.JB_Berth = "BOOTH A23";
			voyageDestination.JB_JV = voyage.PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2006, 5, 6);
			transport.JW_ATD = new ZDateTime(2006, 5, 7);
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETA = new ZDateTime(2006, 5, 8);
			transport.JW_ATA = new ZDateTime(2006, 5, 9);

			consol.JK_AgentsReference = "AGENT_1";
			consol.JK_MasterBillNum = "MASTERME";

			shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S000234567";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			shipment.JS_RL_NKDestination = "ERXXX";
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 13);
			shipment.JS_ConsolReference = "ConsolRef";

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2006, 1, 1);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2006, 1, 2);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2006, 1, 3);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2006, 1, 4);
			shipment.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2006, 2, 1);
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2006, 2, 2);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2006, 2, 3);
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2006, 2, 4);

			shipment.DocsAndCartage.JP_CustomAttrib1 = "ONE";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "TWO";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2001, 1, 1);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2002, 2, 2);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 1.1m;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 2.22m;
			shipment.DocsAndCartage.JP_CustomFlag1 = true;
			shipment.DocsAndCartage.JP_CustomFlag2 = false;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;

			shipment.JS_HouseBill = "HOUSEME";

			shipment.JS_HouseBillIssueDate = new ZDateTime(2005, 4, 8);
			shipment.JS_GoodsDescription = "JILTED LOVERS";
			shipment.DocsAndCartage.JP_OrderItemsAsString = "OWNERS";
			shipment.JS_MarksAndNumbers = "BROKEN HEARTS";

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000026";

			CFSPackLine packLine1 = shipment.OuterPackLines.Count == 0 ? shipment.OuterPackLines.AddNew() : shipment.OuterPackLines[0];
			packLine1.JL_JC = container.PK;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "PK";

			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_PackageCount = 22;
			packLine2.JL_F3_NKPackType = "PK";

			CFSPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container.PK;
			packLine3.JL_PackageCount = 33;
			packLine3.JL_F3_NKPackType = "PK";

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.4;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_GoodsValue = 665.33m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 898.22m;
			shipment.JS_RX_NKInsuranceCurrency = "EUR";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2006, 12, 25);
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_NoCopyBills = 5;
			shipment.JS_NoOriginalBills = 6;
			shipment.JS_InspectionTypeCode = "UNK";

			Transport shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKLoadPort = "ERYYY";
			shipmentTransport.JW_ETD = new ZDateTime(2006, 5, 10);
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";
			shipmentTransport.JW_ETA = new ZDateTime(2006, 5, 11);

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			transport.JW_Vessel = "VESSEL";
			transport.JW_VoyageFlight = "123";
			transport.JW_JX = sailing.PK;
			voyage1.ParentConsol = consol;
			voyage1.JV_RegistrationNo = "111";
			voyage1.JV_IsCargoOnly = true;
			sailing.JX_JB = voyageDestination.PK;
			transport.JW_TerminalCutOff = new ZDateTime(2007, 9, 23);
			transport.JW_DepotCutOff = new ZDateTime(2008, 10, 1);

			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2006, 8, 1);
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2006, 8, 3);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2006, 9, 6);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2006, 9, 9);

			shipment.DocsAndCartage.RequiredDocuments.AddNew();

			#endregion

			FreightWrapperFromCFSShipment fullWrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", Constants.ContainerModes.FCL, fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.LCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ConsolType.Code", Constants.AgentType.Agent, fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", "IMP", fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.IncoTerm.Code", Constants.IncoTerms.DeliveredDutyPaid, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", ZString.Empty, fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.InspectionType", "UNK - Unknown - No Security Measures Taken", fullWrapper.InspectionType.ToString());
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "MSA", fullWrapper.ShipmentStatus.Code);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.Carrier.CompanyNameAndAddress", "SHIPPINGLINE\nCARR ADDRESS 2\nAUSTRALIA", fullWrapper.Carrier.CompanyNameAndAddress);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", "PICKUPCARTAGE", fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "CTO WHATEVER COMPANY", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "DELIVERYCARTAGE", fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", ZString.Empty, fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", ZString.Empty, fullWrapper.ExportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", ZString.Empty, fullWrapper.LocalForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", "IMP_BROKER", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", "EXP_BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);

			AssertEquals("fullWrapper.NotifyParty", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.ConsolCreditor", "CONS_CREDIT", fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("Port Of Loading UNLOCO", "USLAX", fullWrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("Port Of Loading ETD", new ZDateTime(2006, 5, 8), fullWrapper.ShipmentRoutes["First"].EstimatedArrival);
			AssertEquals("Port Of Loading ATD", new ZDateTime(2006, 5, 9), fullWrapper.ShipmentRoutes["First"].ActualArrival);
			AssertEquals("Port Of Discharge UNLOCO", "NZAKL", fullWrapper.ShipmentRoutes["First"].Destination.UNLOCO);

			AssertEquals("Origin From Consol UNLOCO", "USLAX", fullWrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("Destination From Consol UNLOCO", "NZAKL", fullWrapper.ConsolRoutes["First"].Destination.UNLOCO);

			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "ERXXX", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 13), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 13), fullWrapper.Destination.ActualDate);

			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", "400 PKG", fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Weight.ValueAndUnitCodeBlankIfZero", "55.400 KG", fullWrapper.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero", "354 BOX", fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);

			AssertEquals("fullWrapper.ShippedOnBoardType.Code", "SOB", fullWrapper.ShippedOnBoardType.Code);
			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "123.45 AUD", fullWrapper.FreightRate.AmountAndCurrencyCode);

			ZDateTime dateTimeCreated = shipment.Logs.CreatedDateUtc;
			AssertEquals("fullWrapper.ConsolDateCreated", dateTimeCreated, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ShipmentDateCreated", dateTimeCreated, fullWrapper.ShipmentDateCreated);

			AssertEquals("fullWrapper.MasterBill", "MASTERME", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "Ocean Bill Of Lading", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", "House Bill Of Lading", fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.HouseBill", "HOUSEME", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2005, 4, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.GoodsDescription", "JILTED LOVERS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.MarksAndNumbers", "BROKEN HEARTS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.NoCopyBills", 5, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.NoOriginalBills", 6, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2006, 12, 25), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.ExportAgentsReference", "AGENT_1", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "S000234567", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", "S000234567", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.BookingReference", "BOOK_A_HOOKER", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.LCL, fullWrapper.HBLContainerMode);

			AssertEquals("fullWrapper.DeliveryCartageAdvised", new ZDateTime(2006, 1, 1), fullWrapper.DeliveryCartageAdvised);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2006, 1, 2), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2006, 1, 3), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.DeliveryRequiredBy", new ZDateTime(2006, 1, 4), fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.PickupCartageAdvised", new ZDateTime(2006, 2, 1), fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupFrom", new ZDateTime(2006, 2, 2), fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2006, 2, 3), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.PickupRequiredBy", new ZDateTime(2006, 2, 4), fullWrapper.PickupRequiredBy);
			AssertEquals("fullWrapper.PickupDateOfReceipt", ZDateTime.Empty, fullWrapper.PickupDateOfReceipt);

			AssertEquals("fullWrapper.CustomAttribute1", "ONE", fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", "TWO", fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", new ZDateTime(2001, 1, 1), fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", new ZDateTime(2002, 2, 2), fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 1.1m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 2.22m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", true, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", false, fullWrapper.CustomFlag2);

			AssertEquals("fullWrapper.CommercialInvoices.Count", 0, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 0, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "665.33 HKD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.InsuranceValue.AmountAndCurrencyCode", "898.22 EUR", fullWrapper.InsuranceValue.AmountAndCurrencyCode);

			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Packages.Count", 3, fullWrapper.Packages.Count);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.ShipmentRoutes.Count", 2, fullWrapper.ShipmentRoutes.Count);

			AssertEquals("fullWrapper.CustomsEntries.Count", 0, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Orders.Count", 0, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);

			AssertEquals("fullWrapper.ConsolNumber", "C00012345", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.JobNumber", "S000234567", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.SecondaryHeading", "Consol", fullWrapper.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", "C00012345", fullWrapper.SecondaryNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 55.4m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 32.45m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 334, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.ConsolReference", "ConsolRef", fullWrapper.ConsolReference);

			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);

			AssertEquals("fullWrapper.PickupLocation", "USDNV", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "ERXXX", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);

			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", ZString.Empty, fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", ZString.Empty, fullWrapper.SendingForwarder.CompanyName);

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		public void TestCFSShipmentDocumentWrapperNotNull()
		{
			AssertNotNull("Wrapper.FCFSShipment should not be null", Wrapper.CFSShipment);
		}

		public override void TestInsuranceRelatedAddressFields()
		{
			AssertEquals("wrapper.InsuredBy.CompanyName", ZString.Empty, wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", ZString.Empty, wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", ZString.Empty, wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", ZString.Empty, wrapper.SurveyReportParty.CompanyName);

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY THING";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY THING";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY THING";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY THING";

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY THING", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY THING", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY THING", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY THING", wrapper.SurveyReportParty.CompanyName);
		}

		public override void TestBuyer()
		{
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			JobDocAddress buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nERITREA", wrapper.Buyer.CompanyNameAndAddress);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperPickup()
		{
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ABX";
			shipment.DocsAndCartage.JP_PickupLabourCharge = 12.00m;
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now.AddHours(999);
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now.AddHours(-48);
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 45.34m;
			shipment.JS_InterimReceipt = "ASDFGZXC";
			shipment.JS_BookingReference = "SHIPPERS REF";
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 23;
			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_A_RCV = ZDateTime.Now;
			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST 2 ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			AssertEquals("wrapper.CaratagePickupMode.Code", "ABX", wrapper.CaratagePickupMode.Code);
			AssertEquals("wrapper.PickupCFSAddress.Address", "TEST 1 ADDRESS\nAUSTRALIA", wrapper.PickupCFSAddress.Address);
			AssertEquals("wrapper.PickupInterimReceipt", "ASDFGZXC", wrapper.PickupInterimReceipt);
			AssertEquals("wrapper.ActualReceive", new ZDateTime(2007, 1, 1), wrapper.ActualReceive);
			AssertEquals("wrapper.PickupLabourCharge", 12.00m, wrapper.PickupLabourCharge);
			AssertEquals("wrapper.PickupLabourTime", "999:00", wrapper.PickupLabourTime);
			AssertEquals("wrapper.PickupTruckWaitCharge", 45.34m, wrapper.PickupTruckWaitCharge);
			AssertEquals("wrapper.PickupTruckWaitTime", "-48:00", wrapper.PickupTruckWaitTime);
			AssertEquals("wrapper.ShippersReference", "SHIPPERS REF", wrapper.ShippersReference);
			AssertEquals("wrapper.StorgeTime", "23 Hours", wrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapper.UnpackCFSAddress.Address", "TEST 2 ADDRESS\nAUSTRALIA", wrapper.UnpackCFSAddress.Address);
			AssertEquals("wrapper.GoodsAvailableAt.Address", "TEST 2 ADDRESS\nAUSTRALIA", wrapper.GoodsAvailableAt.Address);
		}

		public void TestWrapperWarehouse()
		{
			shipment.JS_WarehouseLocation = "DARWIN";
			AssertEquals("wrapper.WarehouseLocation", "DARWIN", wrapper.WarehouseLocation);
		}

		public override void TestWrapperNotes()
		{
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST HANDLING INFO");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTE INCEST");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO");

			FreightWrapperFromCFSShipment noteWrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("fullWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST HANDLING INFO", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("fullWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTE INCEST", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("fullWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestCoLoadShipments()
		{
			CommonShipment coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_HouseBill = "HBL324324";

			FreightWrapperFromCFSShipment wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
			AssertEquals("wrapper.FreightJobs.Count", 1, wrapper.FreightJobs.Count);
			AssertEquals("wrapper.FreightJobs[0].HouseBill", "HBL324324", wrapper.FreightJobs[0].HouseBill);
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_Weight = collection.AddNew();
			defaultNumberOfDecimals_Weight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_Weight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_Weight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Weight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_Volume = collection.AddNew();
			defaultNumberOfDecimals_Volume.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_Volume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_Volume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Volume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 234.541m;
			shipment.JS_ActualVolume = 156.919m;

			var wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);

			AssertEquals(234.55m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("234.55 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(156.91m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("156.91 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(26151.67m, wrapper.ChargeableWeight.Value);
			AssertEquals("KG", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("26151.67 KG", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		public void TestSetPackageOverride()
		{
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);

			var packLine = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageOverride(packLine);

			AssertEquals(1, wrapper.Packages.Count);
		}

		public void TestSetPackageCollectionOverride()
		{
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageCollectionOverride(
				new List<PackLine>(new[] { packLine1, packLine2 }));

			AssertEquals(2, wrapper.Packages.Count);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.SetDefaultSendingForwarderAddress(GetOrgHeader("SND_FWDER"));
			consol.SetDefaultReceivingForwarderAddress(GetOrgHeader("RCV_FWDER"));
			consol.SetDefaultShippingLineAddress(GetOrgHeader("SHIPPINGLINE"));
			consol.JK_OA_CreditorAddress = GetOrgHeader("CONS_CREDIT").MainAddress.PK;
			consol.JK_BookingReference = "BOOK_A_HOOKER";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO WHATEVER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "NZAKL";

			shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_RL_NKDestination = "ERXXX";

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.4;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_GoodsValue = 665.33m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 898.22m;
			shipment.JS_RX_NKInsuranceCurrency = "EUR";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_ConsolReference = "ConsolRef";

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY THING";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY THING";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY THING";

			JobDocAddress buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY THING";

			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_A_RCV = new ZDateTime(2017, 9, 12);
			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST 2 ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			Transport shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;

			return new FreightWrapperFromCFSShipment(shipment, Factory);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			var loadlist = ((CFSShipment)WrappedBO).Consols.AddNew();
			loadlist.JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			return new FreightWrapperFromCFSShipment((CFSShipment)WrappedBO, Factory);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ActualReceive", "12-Sep-17 00:00:00" },
					{ "BookingReference", "BOOK_A_HOOKER" },
					{ "ConsolReference", "ConsolRef" },
					{ "HBLContainerMode", "LCL" },
					{ "HouseBillHeading", "House Bill Of Lading" },
					{ "JobNumberHeading", "Shipment" },
					{ "MasterBillHeading", "Ocean Bill Of Lading" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
					{ "SecondaryHeading", "Consol" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
AssuredParty : ASSUREDPARTY THING\nERITREA
Buyer : FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nERITREA
CaratagePickupMode : PSL - Premise Supplies Lift
Carrier : SHIPPINGLINE\nAUSTRALIA
ChargeableWeight : 32.450 M3
ClaimsPayableBy : CLAIMSPAYABLEBY THING\nERITREA
Consignee : IMPORTER\nAUSTRALIA
Consignor : SUPPLIER\nAUSTRALIA
ConsolContainerMode : FCL - Full Container Load
ConsolCreditor : CONS_CREDIT\nAUSTRALIA
ConsolTransportMode : SEA - Sea Freight
ConsolType : AGT - Agent
CTOArrival : CTO WHATEVER COMPANY\nAUSTRALIA
DeliveryAddress : IMPORTER\nAUSTRALIA
DeliveryAgent : DELIVERYCARTAGE\nAUSTRALIA
DeliveryLocation : ERXXX
Destination : ERXXX
ExportBroker : EXP_BROKER\nAUSTRALIA
ExportReceivalAddress : TEST 1 ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST 1 ADDRESS\nAUSTRALIA
FreightRate : 123.45 AUD
GoodsAvailableAt : TEST 2 ADDRESS\nAUSTRALIA
GoodsValue : 665.33 HKD
ImportArrivalCTOAddress : CTO WHATEVER COMPANY\nAUSTRALIA
ImportBroker : IMP_BROKER\nAUSTRALIA
IncoTerm : DDP - Delivered Duty Paid
InspectionType : UNK - Unknown - No Security Measures Taken
InsuranceValue : 898.22 EUR
InsuredBy : INSUREDBY THING\nERITREA
NotifyParty : NOTIFYME\nAUSTRALIA
Origin : USDNV - Dunnville
PickupAddress : SUPPLIER\nAUSTRALIA
PickupAgent : PICKUPCARTAGE\nAUSTRALIA
PickupCFSAddress : TEST 1 ADDRESS\nAUSTRALIA
PickupLocation : USDNV - Dunnville
ReleaseType : NXS
ServiceLevel : SLV
ShipmentContainerMode : LCL - Less Container Load
ShipmentInnerPacksQty : 354 BOX
ShipmentOuterPacksQty : 400 PKG
ShipmentStatus : MSA
ShipmentTransportMode : SEA - Sea Freight
ShipmentType : IMP - Import
ShippedOnBoardType : SOB
SurveyReportParty : SURVEYREPORTPARTY THING\nERITREA
UnpackCFSAddress : TEST 2 ADDRESS\nAUSTRALIA
Volume : 32.450 M3
Weight : 55.400 KG";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CFSShipment>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<CFSShipment>();
			wrapper = new FreightWrapperFromCFSShipment(shipment, Factory);
		}
		FreightWrapperFromCFSShipment wrapper;
		CFSShipment shipment;

		#endregion
	}
}
