using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCFSLoadList))]
	sealed class FreightWrapperFromCFSLoadListTest : FreightWrapperTest
	{
		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			return ((CFSLoadListConsol)parent).Containers.AddNew();
		}

		public void TestExportReceivingDepotAddress()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromCFSLoadList wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
		}

		public void TestExportReceivingCTOAddress()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromCFSLoadList wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestExportReceivalAddress()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromCFSLoadList wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestPickupDeliveryConfirmations()
		{
			CFSLoadListConsol consol1 = Factory.New<CFSLoadListConsol>();
			CFSLoadListConsol consol2 = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment1 = consol1.Shipments.AddNew();
			CFSShipment shipment2 = consol2.Shipments.AddNew();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();
			packline1.SetContainer(consol1, container1);
			packline2.SetContainer(consol2, container2);

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
			CommonPickupDeliveryConfirm shipmentOriginArrivalShouldntBeInCollection = shipment2.OriginCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipmentOriginDepartureShouldntBeInCollection = shipment2.OriginCFSDepartures.AddNew();
			CommonPickupDeliveryConfirm shipmentDestinationArrivalShouldntBeInCollection = shipment2.DestinationCFSArrivals.AddNew();
			CommonPickupDeliveryConfirm shipmentDestinationDepartureShouldntBeInCollection = shipment2.DestinationCFSDepartures.AddNew();

			List<CommonPickupDeliveryConfirm> expected = new List<CommonPickupDeliveryConfirm>();
			expected.Add(containerOriginArrival);
			expected.Add(containerOriginDeparture);
			expected.Add(containerDestinationArrival);
			expected.Add(containerDestinationDeparture);
			expected.Add(shipmentOriginArrival);
			expected.Add(shipmentOriginDeparture);
			expected.Add(shipmentDestinationArrival);
			expected.Add(shipmentDestinationDeparture);

			FreightWrapperFromCFSLoadList wrapper = new FreightWrapperFromCFSLoadList(consol1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expected, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));
		}

		public void TestMasterBillHeading()
		{
			CFSLoadListConsol loadList1 = Factory.New<CFSLoadListConsol>();
			FreightWrapper wrapper1 = FreightWrapper.New(loadList1, Factory)[0];

			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();
			FreightWrapper wrapper2 = FreightWrapper.New(loadList2, Factory)[0];

			CFSLoadListConsol loadList3 = Factory.New<CFSLoadListConsol>();
			FreightWrapper wrapper3 = FreightWrapper.New(loadList3, Factory)[0];

			loadList1.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.MasterBillHeading", "MAWB", wrapper1.MasterBillHeading);

			loadList2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.MasterBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);

			loadList3.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.MasterBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CFSLoadListConsol>();
		}

		[TestDate(2007, 1, 1)]
		public void TestFullConsolWrapper()
		{
			#region Setup

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_Address1 = "SLMA";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress slAddr = shippingLine.Addresses.AddNew();
			slAddr.OA_Address1 = "SHIPPING LINE SECOND ADDRESS";
			slAddr.OA_RN_NKCountryCode = "AU";

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SENDING FORWARDER";
			sendingForwarder.MainAddress.OA_Address1 = "SFMA";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress sfAddr = shippingLine.Addresses.AddNew();
			sfAddr.OA_Address1 = "SENDING FORWARDER SECOND ADDRESS";
			sfAddr.OA_RN_NKCountryCode = "AU";

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			receivingForwarder.MainAddress.OA_Address1 = "RFMA";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress rfAddr = shippingLine.Addresses.AddNew();
			rfAddr.OA_Address1 = "RECEIVING FORWARDER SECOND ADDRESS";
			rfAddr.OA_RN_NKCountryCode = "AU";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO ANOHTER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST WHAT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentsReference = "AGENTS REF";
			consol.JK_ShippedOnBoardDate = new ZDateTime(2007, 6, 15);
			consol.JK_NoOriginalBills = 5;
			consol.JK_NoCopyBills = 12;
			consol.JK_BookingReference = "BOOKING REF";
			consol.JK_UniqueConsignRef = "C09328403-3";
			consol.JK_MasterBillNum = "MBL342ADSF";
			consol.JK_MasterBillIssueDate = new ZDateTime(2007, 12, 15);
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_ConsolStatus = "ASF";
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = slAddr.PK;
			consol.JK_OA_SendingForwarderAddress = sfAddr.PK;
			consol.JK_OA_ReceivingForwarderAddress = rfAddr.PK;
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.JK_OA_UnpackDepotAddress = unpackdepotAddress.PK;
			consol.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;
			consol.JK_TotalShipmentActOtherUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentActWeightCheck = 234.5m;
			consol.JK_TotalShipmentActVolumeCheck = 156.9m;
			consol.JK_TotalShipmentChargableCheck = 8997.3m;

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_ArrivalReference = "ARRIVAL REFERENCE";
			voyageDestination.JB_Berth = "BOOTH A23";
			voyageDestination.JB_JV = voyage.PK;
			VoyageOrigin voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_JV = voyage.PK;
			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = sailing.PK;
			transport.JW_TerminalAvailabilityDate = new ZDateTime(2007, 4, 28);
			transport.JW_DepotAvailabilityDate = new ZDateTime(2006, 5, 18);
			transport.JW_TerminalStorageDate = new ZDateTime(2006, 5, 13);
			transport.JW_DepotStorageDate = new ZDateTime(2007, 8, 24);
			transport.JW_TerminalCutOff = new ZDateTime(2007, 7, 24);
			transport.JW_DepotCutOff = new ZDateTime(2006, 8, 29);

			CFSShipment shipment1 = consol.Shipments.AddNew();
			CFSShipment shipment2 = consol.Shipments.AddNew();

			shipment1.JS_OuterPacks = 2;
			shipment1.JS_F3_NKPackType = "ART";
			shipment2.JS_OuterPacks = 3;
			shipment2.JS_F3_NKPackType = "ART";

			shipment1.JS_TotalPackageCount = 6;
			shipment2.JS_TotalPackageCount = 2;

			shipment1.JS_F3_NKTotalCountPackType = "MIN";
			shipment2.JS_F3_NKTotalCountPackType = "MIN";

			consol.Containers.AddNew();
			consol.Containers.AddNew();

			shipment1.DocsAndCartage.RequiredDocuments.AddNew();
			shipment2.DocsAndCartage.RequiredDocuments.AddNew();

			#endregion

			FreightWrapperFromCFSLoadList fullWrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("fullWrapper.ConsolDateCreated", ZDateTime.Empty, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.LocalForwarderReference", "AGENTS REF", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.ExportAgentsReference", "AGENTS REF", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "AGENTS REF", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2007, 6, 15), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.IncoTerm.Code", ZString.Empty, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", ZString.Empty, fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.NoOriginalBills", 5, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.NoCopyBills", 12, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.ShippersReference", "BOOKING REF", fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.JobNumber", "C09328403-3", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.BookingReference", "BOOKING REF", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.MasterBill", "MBL342ADSF", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "MAWB", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", ZString.Empty, fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.MasterBillIssue", new ZDateTime(2007, 12, 15), fullWrapper.MasterBillIssue);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.ConsolNumber", "C09328403-3", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.ConsolType.Code", "AGT", fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", "LSE", fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", "AIR", fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "ASF", fullWrapper.ShipmentStatus.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", "LSE", fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", "AIR", fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ReleaseType.Code", Constants.ShipmentReleaseTypes.ExpressBofL, fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.Carrier.CompanyNameAndAddress", "SHIPPING LINE\nSHIPPING LINE SECOND ADDRESS\nAUSTRALIA", fullWrapper.Carrier.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ConsolCreditor.CompanyName", "CREDITOR", fullWrapper.ConsolCreditor.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", ZString.Empty, fullWrapper.LocalForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", ZString.Empty, fullWrapper.ExportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", ZString.Empty, fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "CTO ANOHTER COMPANY", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.UnpackCFSAddress.CompanyName", "TEST WHAT ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt.CompanyName", "TEST WHAT ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.Weight.Value", 234.5m, fullWrapper.Weight.Value);
			AssertEquals("fullWrapper.Volume.Value", 156.9m, fullWrapper.Volume.Value);
			AssertEquals("fullWrapper.ChargeableWeight.Value", 8997.3m, fullWrapper.ChargeableWeight.Value);
			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.Containers.Count", 2, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.FreightJobs.Count", 2, fullWrapper.FreightJobs.Count);
			AssertEquals("fullWrapper.FreightConsolidations.Count", 0, fullWrapper.FreightConsolidations.Count);
			AssertEquals("fullWrapper.RequiredDocuments.Count", 0, fullWrapper.RequiredDocuments.Count);
			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", 5, fullWrapper.ShipmentOuterPacksQty.Value.ToZInt());
			AssertEquals("fullWrapper.ShipmentInnerPacksQty", 8, fullWrapper.ShipmentInnerPacksQty.Value.ToZInt());
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", "ART", fullWrapper.ShipmentOuterPacksQty.Unit.ToString());
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", "MIN", fullWrapper.ShipmentInnerPacksQty.Unit.ToString());
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", ZString.Empty, fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", ZString.Empty, fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", ZString.Empty, fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", ZString.Empty, fullWrapper.SendingForwarder.CompanyName);
		}

		public override void TestWrapperNotes()
		{
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Some dangerous goods crap");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "Just certificate of origin for testing");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "Pre alert arrival notice remarks as nothing ever arrives why have this");

			FreightWrapperFromCFSLoadList noteWrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("noteWrapper.DangerousGoodsAdditionalHandlingInformation", "Some dangerous goods crap", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("noteWrapper.CertificateOfOriginNotes", "Just certificate of origin for testing", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("noteWrapper.PreAlertArrivalNoticeRemarks", "Pre alert arrival notice remarks as nothing ever arrives why have this", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestPorts()
		{
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "ERZZZ";

			FreightWrapperFromCFSLoadList portWrapper = new FreightWrapperFromCFSLoadList(consol, Factory);
			AssertEquals("portWrapper.ConsolRoutes[First].Origin.UNLOCO", "ERZZZ", portWrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("portWrapper.ConsolRoutes[Last].Destination.UNLOCO", "AUSYD", portWrapper.ConsolRoutes["Last"].Destination.UNLOCO);
			AssertEquals("portWrapper.ShipmentType.Code", "EXP", portWrapper.ShipmentType.Code);
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

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Air;
			loadList.JK_TotalShipmentActOtherUnit = Core.Constants.Volume.CubicMetres;
			loadList.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;
			loadList.JK_TotalShipmentActWeightCheck = 234.541m;
			loadList.JK_TotalShipmentActVolumeCheck = 156.915m;
			loadList.JK_TotalShipmentChargableCheck = 8997.355m;

			var wrapper = new FreightWrapperFromCFSLoadList(loadList, Factory);

			AssertEquals(234.55m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("234.55 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(156.91m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("156.91 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(8997.36m, wrapper.ChargeableWeight.Value);
			AssertEquals("KG", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("8997.36 KG", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		public void TestSetPackageOverride()
		{
			var wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageOverride(packLine);

			AssertEquals(1, wrapper.Packages.Count);
		}

		public void TestSetPackageCollectionOverride()
		{
			var wrapper = new FreightWrapperFromCFSLoadList(consol, Factory);

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageCollectionOverride(
				new List<PackLine>(new[] { packLine1, packLine2 }));

			AssertEquals(2, wrapper.Packages.Count);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Load List" },
					{ "MasterBillHeading", "MAWB" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Carrier : SHIPPING LINE\nAUSTRALIA
ChargeableWeight : 8997.300 KG
ConsolContainerMode : LSE - Loose
ConsolCreditor : CREDITOR\nAUSTRALIA
ConsolTransportMode : AIR - Air Freight
ConsolType : AGT - Agent
CTOArrival : CTO ANOHTER COMPANY\nAUSTRALIA
ExportReceivalAddress : TEST PACK ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST PACK ADDRESS\nAUSTRALIA
GoodsAvailableAt : TEST WHAT ADDRESS\nAUSTRALIA
ImportArrivalCTOAddress : CTO ANOHTER COMPANY\nAUSTRALIA
PickupCFSAddress : TEST PACK ADDRESS\nAUSTRALIA
ReleaseType : EBL - Express Bill of Lading
ShipmentContainerMode : LSE - Loose
ShipmentStatus : ASF
ShipmentTransportMode : AIR - Air Freight
ShipmentType : EXP - Export
UnpackCFSAddress : TEST WHAT ADDRESS\nAUSTRALIA
Volume : 156.900 M3
Weight : 234.500 KG";
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<CFSLoadListConsol>();
		}
		CFSLoadListConsol consol;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SENDING FORWARDER";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO ANOHTER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST WHAT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress packdepotAddress = Factory.New<OrgAddress>();
			packdepotAddress.OA_Address1 = "TEST PACK ADDRESS";
			packdepotAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "ERZZZ";
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolStatus = "ASF";
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.JK_OA_UnpackDepotAddress = unpackdepotAddress.PK;
			consol.JK_OA_PackDepotAddress = packdepotAddress.PK;
			consol.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;
			consol.JK_TotalShipmentActOtherUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentActWeightCheck = 234.5m;
			consol.JK_TotalShipmentActVolumeCheck = 156.9m;
			consol.JK_TotalShipmentChargableCheck = 8997.3m;

			return new FreightWrapperFromCFSLoadList(consol, Factory);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((CFSLoadListConsol)WrappedBO).JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			return new FreightWrapperFromCFSLoadList((CFSLoadListConsol)WrappedBO, Factory);
		}
	}
}
