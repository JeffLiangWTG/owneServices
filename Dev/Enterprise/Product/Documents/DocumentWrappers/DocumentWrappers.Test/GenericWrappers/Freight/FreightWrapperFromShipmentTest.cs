using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using JobMessageTypeList = Enterprise.Customs.Common.CA.CAJobMessageTypeList;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromShipment))]
	sealed class FreightWrapperFromShipmentTest : FreightWrapperTest
	{
		public override void TestArrivalCFSTransport()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Arrival";

			forwardingConsol.JK_OA_ArrivalUnpackCFSTransportAddress = header.MainAddress.PK;

			var forwardingshipment = forwardingConsol.Shipments.AddNew();
			var wrapperForShipment = new FreightWrapperFromShipment(forwardingshipment, Factory);
			AssertNotNull(wrapperForShipment.ArrivalCFSTransport);
			AssertEquals("ARRIVAL", wrapperForShipment.ArrivalCFSTransport.CompanyName);
		}

		public override void TestDepartureCFSTransport()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Departure";

			forwardingConsol.JK_OA_DeparturePackCFSTransportAddress = header.MainAddress.PK;

			var forwardingshipment = forwardingConsol.Shipments.AddNew();
			var wrapperForShipment = new FreightWrapperFromShipment(forwardingshipment, Factory);
			AssertNotNull(wrapperForShipment.DepartureCFSTransport);
			AssertEquals("DEPARTURE", wrapperForShipment.DepartureCFSTransport.CompanyName);
		}

		public void TestGetEFreightStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_EFreightStatus = string.Empty;

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(string.Empty, wrapper.EFreightStatus);

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			shipment.JS_EFreightStatus = "EAP";
			AssertEquals("EAP", wrapper.EFreightStatus);
		}

		public void TestFullHandlingInstructions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Handle with no care", wrapper.FullHandlingInstructions);
		}

		#region TestAWBSecurityInspectionStatus

		public void TestAWBSecurityInspectionStatus()
		{
			//GlbBranch.CurrentBranch.HomePort
			var shipment = Factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AWBNonApprovedExporterText");
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AWBApprovedExporterText");

			//if shipment !isAir
			shipment.JS_TransportMode = Constants.TransportModes.Auto;
			AssertEquals("If shipment !isAir AWBSecurityInspectionStatus should be empty", "", wrapper.AWBSecurityInspectionStatus);

			//if shipment isDomestic
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "AUSYD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ERASA";
			AssertEquals("If shipment isDomestic AWBSecurityInspectionStatus should be empty", "", wrapper.AWBSecurityInspectionStatus);

			//if shipment isAir and Not Domestic
			shipment.JS_RL_NKOrigin = "DEFRA";
			if (shipment.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
			{
				AssertEquals("If InspectionTypeCode != 'UNK' AWBSecurityInspectionStatus should be from AWBApprovedExporterText", "AWBApprovedExporterText", wrapper.AWBSecurityInspectionStatus);
			}
			else
			{
				AssertEquals("If InspectionTypeCode == 'UNK' AWBSecurityInspectionStatus should be from AWBNonApprovedExporterText", "AWBNonApprovedExporterText", wrapper.AWBSecurityInspectionStatus);
			}
		}
		#endregion

		public void TestFullCartageInstructions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Pick it up");
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pick it up", wrapper.FullCartageInstructions);
		}

		public void TestPickupCFSAddress()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Address should be empty", ZString.Empty, wrapper.PickupCFSAddress.Address);

			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];

			OrgAddress consolPackAddress = Factory.New<OrgAddress>();
			consolPackAddress.OA_Address1 = "66 PACK DEPOT ST";
			consolPackAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackAddress.PK;
			wrapper = new FreightWrapperFromShipment(shipment, transport, Factory);
			AssertEquals("shipment.ExportReceivingDepot is null, should fallback to consol.PackDepotAddress", "66 PACK DEPOT ST\nAUSTRALIA", wrapper.PickupCFSAddress.Address);

			OrgAddress shipmentReceivingDepot = Factory.New<OrgAddress>();
			shipmentReceivingDepot.OA_Address1 = "83 RECEIVING DEPOT AVE";
			shipmentReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentReceivingDepot.PK;
			wrapper = new FreightWrapperFromShipment(shipment, transport, Factory);
			AssertEquals("shipment.ExportReceivingDepot is not null and should be returned", "83 RECEIVING DEPOT AVE\nAUSTRALIA", wrapper.PickupCFSAddress.Address);
		}

		public void TestAvailableAndStorageDates()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 12, 1);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 12, 5);

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("CFS available should come from DocsAndCartage", new ZDateTime(2011, 12, 1), wrapper.CartageInfo.AvailableDate);
			AssertEquals("CFS storage should come from DocsAndCartage", new ZDateTime(2011, 12, 5), wrapper.CartageInfo.StorageCommenceDate);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 12, 10);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 12, 15);

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("CTO available should come from DocsAndCartage", new ZDateTime(2011, 12, 10), wrapper.CartageInfo.AvailableDate);
			AssertEquals("CTO storage should come from DocsAndCartage", new ZDateTime(2011, 12, 15), wrapper.CartageInfo.StorageCommenceDate);
		}

		public void TestOrderNumbersWithOwnersReference()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order1 = shipment.DocsAndCartage.OrderItems.AddNew();
			order1.JT_Sequence = 1;
			order1.JT_OrderReference = "ORDER BIG";

			var order2 = shipment.DocsAndCartage.OrderItems.AddNew();
			order2.JT_Sequence = 2;
			order2.JT_OrderReference = "ORDER SMALL";

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(wrapper.OrderNumbersWithOwnersReference, "ORDER BIG, ORDER SMALL", wrapper.OrderNumbersWithOwnersReference);

			shipment.DocsAndCartage.OrderItems.RemoveAndDeleteAll();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(wrapper.OrderNumbersWithOwnersReference, "", wrapper.OrderNumbersWithOwnersReference);

			shipment.DocsAndCartage.JP_OrderItemsAsString = "one, two, three";
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(wrapper.OrderNumbersWithOwnersReference, "one, two, three", wrapper.OrderNumbersWithOwnersReference);
		}

		public void TestCartageInfoDocWrapperContext_Shipment()
		{
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			testShipment.JS_TransportMode = Constants.TransportModes.Air;
			testShipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
			var testWrapper = new FreightWrapperFromShipment(testShipment, Factory);
			testWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));

			AssertEquals(nameof(DocumentDirection.DEP), testWrapper.CartageInfo.DocumentDirection);
			AssertEquals(testShipment.DocsAndCartage.JP_PickupRequiredBy, testWrapper.CartageInfo.JourneyOnePickUpRequiredByDate);
		}

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add((ForwardingShipment)parent);

			var container = consol.Containers.AddNew();
			var packLine = ((ForwardingShipment)parent).OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_JC = container.PK;

			return container;
		}

		public void TestConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var wrapperWithNoConsols = new FreightWrapperFromShipment(shipment, Factory);

			AssertNotNull(wrapperWithNoConsols);
			AssertNull(wrapperWithNoConsols.Consol);

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";
			var transport1 = departureConsol.Transports.AddNew();

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";
			var transport2 = arrivalConsol.Transports.AddNew();

			var unrelatedConsol = shipment.Consols.AddNew();
			shipment.CurrentConsolForDocuments = unrelatedConsol;

			AssertEquals("Pre-condition", departureConsol, shipment.DepartureConsolForDocuments);
			AssertEquals("Pre-condition", arrivalConsol, shipment.ArrivalConsolForDocuments);
			AssertEquals("Pre-condition", unrelatedConsol, shipment.CurrentConsolForDocuments);

			var currentConsolWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Current consol should be used as the doc wrapper consol", unrelatedConsol, currentConsolWrapper.Consol);

			shipment.CurrentConsolForDocuments = unrelatedConsol;

			var wrapperWithTransport = new FreightWrapperFromShipment(shipment, transport1, Factory);
			AssertEquals("Expected to use transport1's consol for wrapper consol", departureConsol, wrapperWithTransport.Consol);

			wrapperWithTransport = new FreightWrapperFromShipment(shipment, transport2, Factory);
			AssertEquals("Expected to use transport2's consol for wrapper consol", arrivalConsol, wrapperWithTransport.Consol);

			shipment.CurrentConsolForDocuments = unrelatedConsol;

			var arrivalWrapper = new FreightWrapperFromShipment(shipment, Factory);
			arrivalWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("When current consol is not null, it should be used", unrelatedConsol, arrivalWrapper.Consol);

			shipment.CurrentConsolForDocuments = null;

			arrivalWrapper = new FreightWrapperFromShipment(shipment, Factory);
			arrivalWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("DocumentDirection.ARV - using arrival consol", arrivalConsol, arrivalWrapper.Consol);

			var departureWrapper = new FreightWrapperFromShipment(shipment, Factory);
			departureWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DocumentDirection.DEP - using departure consol", departureConsol, departureWrapper.Consol);

			var anyDocumentDirectionWrapper = new FreightWrapperFromShipment(shipment, Factory);
			anyDocumentDirectionWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("DocumentDirection.ANY - using departure consol", departureConsol, anyDocumentDirectionWrapper.Consol);
		}

		public void TestGetBookingReference()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(ZString.Empty, wrapper.BookingReference);

			consol.JK_BookingReference = "AAA, BBB";
			AssertEquals("AAA, BBB", wrapper.BookingReference);

			consol.JK_BookingReference = ZString.Empty;

			CusEntryNumber number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = "XXX";

			CusEntryNumber number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number2.CE_EntryNum = "YYY";

			CusEntryNumber number3 = consol.Numbers.AddNew();
			number3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number3.CE_EntryNum = "  ";

			AssertEquals("YYY", wrapper.BookingReference);

			CusEntryNumber number4 = consol.Numbers.AddNew();
			number4.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number4.CE_EntryNum = " ZZZ ";

			AssertEquals("YYY, ZZZ", wrapper.BookingReference);

			consol.JK_BookingReference = "CCC";
			AssertEquals("CCC, YYY, ZZZ", wrapper.BookingReference);
		}

		public void TestSuppressFlightDetails()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);

			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			AssertEquals("SuppressFlightDetails should be false", ZBool.False, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(2);
			AssertEquals("SuppressFlightDetails should be false", ZBool.False, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals("SuppressFlightDetails should be true", true, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(-1);
			ForwardingConsol consol = CreateExportConsol(shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today.AddDays(-1);
			AssertEquals("SuppressFlightDetails should be false", false, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			AssertEquals("SuppressFlightDetails should be true", false, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			AssertEquals("SuppressFlightDetails should be false", false, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(2);
			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals("SuppressFlightDetails should be true", true, Suppression.EnabledForAnyOfFields(wrapper.WrappedObject as IFlightDetailsSuppression, new[] { SuppressFields.MasterBill }, ContactType.Consignor));
		}

		public void TestExportReceivingDepotAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
		}

		public void TestExportReceivingCTOAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestExportReceivalAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestPickupDeliveryConfirmations()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			ForwardingShipment shipment2 = consol1.Shipments.AddNew();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			ForwardingContainer container1 = consol1.Containers.AddNew();
			ForwardingContainer container2 = consol1.Containers.AddNew();
			packline1.SetContainer(consol1, container1);
			packline2.SetContainer(consol1, container2);

			shipment1.JS_PackingMode = Constants.ContainerModes.LCL;

			CommonPickupDeliveryConfirm pickupLooseConfirmation = shipment1.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryLooseConfirmation = shipment1.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm pickupContainer1Confirmation = container1.OriginConfirm;
			CommonPickupDeliveryConfirm deliveryContainer1Confirmation = container1.DestinationConfirm;
			CommonPickupDeliveryConfirm pickupContainer2ConfirmationShouldntBeInCollection = container2.OriginConfirm;
			CommonPickupDeliveryConfirm deliveryContainer2ConfirmationShouldntBeInCollection = container2.DestinationConfirm;

			List<CommonPickupDeliveryConfirm> expectedLCL = new List<CommonPickupDeliveryConfirm>();
			expectedLCL.Add(pickupLooseConfirmation);
			expectedLCL.Add(deliveryLooseConfirmation);

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedLCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));

			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;

			List<CommonPickupDeliveryConfirm> expectedFCL = new List<CommonPickupDeliveryConfirm>();
			expectedFCL.Add(pickupContainer1Confirmation);
			expectedFCL.Add(deliveryContainer1Confirmation);

			wrapper = new FreightWrapperFromShipment(shipment1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedFCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));

			shipment1.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			wrapper = new FreightWrapperFromShipment(shipment1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedFCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));
		}

		public void TestCharges()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapper[] wrappers = FreightWrapper.New(shipment, Factory);
			AssertEquals(1, wrappers.Length);
			AssertNotNull(wrappers[0].Charges);
			AssertEquals(0, wrappers[0].Charges.Count);

			shipment = Factory.New<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreate();

			Charge charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = 500;

			wrappers = FreightWrapper.New(shipment, Factory);

			AssertEquals(1, wrappers.Length);
			AssertEquals(1, wrappers[0].Charges.Count);
			AssertEquals(charge, wrappers[0].Charges[0].WrappedObject);
		}

		public void TestTranshipmentFreightConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "NZWWW";

			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUBNE";

			ForwardingConsol nextConsol = shipment.Consols.AddNew();
			Transport nextTransport = nextConsol.Transports[0];
			nextConsol.JK_UniqueConsignRef = "CONSOL2";
			nextTransport.JW_RL_NKLoadPort = "AUBNE";
			nextTransport.JW_RL_NKDiscPort = "AUSYD";

			ForwardingConsol consolAfterTheNext = shipment.Consols.AddNew();
			consolAfterTheNext.JK_UniqueConsignRef = "CONSOL3";
			Transport transportAfterTheNext = consolAfterTheNext.Transports[0];
			transportAfterTheNext.JW_RL_NKLoadPort = "AUSYD";
			transportAfterTheNext.JW_RL_NKDiscPort = "NZWWW";

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals("CONSOL2", wrapper.TranshipmentFreightConsol.ConsolNumber);
		}

		public void TestTranshipmentFreightContainers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "NZWWW";
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUBNE";
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT000001";
			container2.JC_ContainerNum = "CONT000002";
			pack1.SetContainer(consol, container1);
			pack2.SetContainer(consol, container2);

			ForwardingConsol nextConsol = shipment.Consols.AddNew();
			Transport nextTransport = nextConsol.Transports[0];
			nextConsol.JK_UniqueConsignRef = "CONSOL2";
			nextTransport.JW_RL_NKLoadPort = "AUBNE";
			nextTransport.JW_RL_NKDiscPort = "AUSYD";
			var containerNextConsol1 = nextConsol.Containers.AddNew();
			var containerNextConsol2 = nextConsol.Containers.AddNew();
			var containerNextConsol3 = nextConsol.Containers.AddNew();
			containerNextConsol1.JC_ContainerNum = "CONT000001";
			containerNextConsol2.JC_ContainerNum = "CONT000002";
			containerNextConsol3.JC_ContainerNum = "CONT000003";

			ForwardingConsol consolAfterTheNext = shipment.Consols.AddNew();
			consolAfterTheNext.JK_UniqueConsignRef = "CONSOL3";
			Transport transportAfterTheNext = consolAfterTheNext.Transports[0];
			transportAfterTheNext.JW_RL_NKLoadPort = "AUSYD";
			transportAfterTheNext.JW_RL_NKDiscPort = "NZWWW";
			var containerConsolAfterTheNext1 = consolAfterTheNext.Containers.AddNew();
			var containerConsolAfterTheNext2 = consolAfterTheNext.Containers.AddNew();
			containerConsolAfterTheNext1.JC_ContainerNum = "CONT000004";
			containerConsolAfterTheNext2.JC_ContainerNum = "CONT000005";

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertNotNull(wrapper.TranshipmentContainers);

			var containerList = wrapper.TranshipmentContainers.Cast<ContainerWrapper>().Select(c => c.ContainerNo).ToList();
			AssertEquals(2, containerList.Count);
			AssertEquals(true, containerList.Contains("CONT000001"));
			AssertEquals(true, containerList.Contains("CONT000002"));
		}

		#region NotifyParty

		public void TestNotifyParty2()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.NotifyParty2 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty2.CompanyName);

			OrgHeader notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "NOTIFY PARTY TWO";
			shipment.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.NotifyParty2", "NOTIFY PARTY TWO", wrapper.NotifyParty2.CompanyName);
		}

		public void TestNotifyParty3()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.NotifyParty3 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty3.CompanyName);

			OrgHeader notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.OH_FullName = "NOTIFY PARTY THREE";
			shipment.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty3.PK;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.NotifyParty3", "NOTIFY PARTY THREE", wrapper.NotifyParty3.CompanyName);
		}

		#endregion

		#region TestBarcode

		public void TestBarcodeTextWithData()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			FreightWrapperFromShipment shipmentWrapper = new FreightWrapperFromShipment(shipment, shipment.Factory);

			AssertEquals("Precondition: Origin", "", shipment.JS_RL_NKOrigin);
			AssertEquals("Precondition: Destination", "", shipment.JS_RL_NKDestination);
			AssertEquals("Precondition: Housebill", "", shipment.JS_HouseBill);

			AssertEquals("Missing data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeText);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipmentWrapper = new FreightWrapperFromShipment(shipment, shipment.Factory);

			AssertEquals("Still missing data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeText);

			shipment.JS_HouseBill = "12345678901234567890";
			shipmentWrapper = new FreightWrapperFromShipment(shipment, shipment.Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "HBL";

			AssertEquals("All data now valid, barcode should exist", "[EDIHBLSYDLAX12345678901234567890]", shipmentWrapper.BarcodeText);
		}

		public void TestBarcodeTextWhenOriginOrDestinationDontHaveIATACodes()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USWZV"; // no IATA code
			shipment.JS_RL_NKDestination = "USOGI"; // no IATA code
			shipment.JS_HouseBill = "12345";

			FreightWrapperFromShipment shipmentWrapper = new FreightWrapperFromShipment(shipment, shipment.Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "HBL";

			AssertEquals("Should generate a barcode based on the last 3 letters of UNLOCO codes", "[EDIHBLWZVOGI12345]", shipmentWrapper.BarcodeText);
		}

		public void TestFreightLabelBarcodeWhenUnique()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_HouseBill = "12345";
			shipment.JS_UniqueConsignRef = "S00001003";

			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 6;
			packLine.JL_F3_NKPackType = "PKG";

			shipment.JS_OuterPacks = 50;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 40;

			FreightWrapperFromShipment shipmentWrapper = new FreightWrapperFromShipment(shipment, shipment.Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "LBL";

			((IPackLineOverrider)shipmentWrapper).SetPackageOverride(packLine);

			var enterpriseCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode;
			var serverCode = GlbCompany.CurrentCompany.LicenceServerID;

			shipmentWrapper.DocNumber = 12;
			AssertEquals("Should generate unique barcode for freight label if unique option checked",
				enterpriseCode + serverCode + shipment.JS_UniqueConsignRef + "-00012", shipmentWrapper.BarcodeTextWithUniqueID);

			shipmentWrapper.DocNumber = 124567;
			AssertEquals("Should generate unique barcode for freight label if unique option checked",
				enterpriseCode + serverCode + shipment.JS_UniqueConsignRef + "-124567", shipmentWrapper.BarcodeTextWithUniqueID);
		}

		#endregion

		public void TestMilestones()
		{
			DocumentsDataRegistry.Instance.ShowMilestonesOnPODDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ProcessTask pT1 = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
			ProcessTask pT2 = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
			FreightWrapperFromShipment result = new FreightWrapperFromShipment(shipment, shipment.Factory);
			AssertEquals(result.Milestones.Count, 2);
		}

		public void TestMultipleConsols()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_ETD = new ZDateTime(2008, 1, 1);
			transport.JW_ATD = new ZDateTime(2008, 1, 2);
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETA = new ZDateTime(2008, 1, 3);
			transport.JW_ATA = new ZDateTime(2008, 1, 4);

			FreightWrapperFromShipment wrapper1 = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];
			AssertEquals(consol, wrapper1.Consol);

			ForwardingConsol consol2 = shipment.Consols.AddNew();
			Transport transport2 = consol2.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2008, 1, 5);
			transport.JW_ATD = new ZDateTime(2008, 1, 6);
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETA = new ZDateTime(2008, 1, 7);
			transport.JW_ATA = new ZDateTime(2008, 1, 8);

			FreightWrapperFromShipment wrapper2 = (FreightWrapperFromShipment)FreightWrapper.New(shipment, transport2, Factory)[0];
			AssertEquals(consol2, wrapper2.Consol);

			FreightWrapperFromShipment wrapper3 = (FreightWrapperFromShipment)FreightWrapper.New(shipment, transport, Factory)[0];
			AssertEquals(consol, wrapper3.Consol);
		}

		public void TestConsolNumbers()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "CS0000011";

			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CS0000022";

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals("Shipment has two consol numbers", "CS0000011", wrapper.FreightConsolidations[0].ConsolNumber);
			AssertEquals("Shipment has two consol numbers", "CS0000022", wrapper.FreightConsolidations[1].ConsolNumber);
		}

		public void TestMasterBillHeading()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper1 = FreightWrapper.New(shipment1, Factory)[0];

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper2 = FreightWrapper.New(shipment2, Factory)[0];

			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper3 = FreightWrapper.New(shipment3, Factory)[0];

			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.MasterBillHeading", "MAWB", wrapper1.MasterBillHeading);

			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.MasterBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);

			shipment3.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.MasterBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		public void TestHouseBillHeading()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper1 = FreightWrapper.New(shipment1, Factory)[0];

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper2 = FreightWrapper.New(shipment2, Factory)[0];

			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			FreightWrapper wrapper3 = FreightWrapper.New(shipment3, Factory)[0];

			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.MasterBillHeading", "HAWB", wrapper1.HouseBillHeading);

			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.MasterBillHeading", "House Bill Of Lading", wrapper2.HouseBillHeading);

			shipment3.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.MasterBillHeading", "House Bill", wrapper3.HouseBillHeading);
		}

		public void TestConsignorAndConsigneeGetLabelledAsUltimateWhereAppropriate()
		{
			ForwardingShipment coLoadMasterShipment = Factory.New<ForwardingShipment>();
			coLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;

			AssertEquals("wrapper.Consignee.TypeDescription", "Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Consignor", wrapper.Consignor.TypeDescription);

			shipment.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.Consignee.TypeDescription", "Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Consignor", wrapper.Consignor.TypeDescription);

			OrgHeader coLoadConsignee = Factory.New<OrgHeader>();
			coLoadMasterShipment.ConsigneeDocumentaryAddress.OrganisationPK = coLoadConsignee.PK;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.Consignee.TypeDescription", "Ultimate Consignee", wrapper.Consignee.TypeDescription);
			AssertEquals("wrapper.Consignor.TypeDescription", "Ultimate Consignor", wrapper.Consignor.TypeDescription);

			coLoadConsignee.MiscServ.OM_FWDealDirectlyWithUltimates = true;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
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

		public void TestWeight()
		{
			shipment.JS_ActualWeight = 8.00m;
			shipment.JS_DocumentedWeight = 10.5m;
			shipment.JS_ManifestedWeight = 5.1m;

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual weight", "8.000 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented weight", "10.500 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier weight", "5.100 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);
		}

		public void TestVolume()
		{
			shipment.JS_ActualVolume = 8.00m;
			shipment.JS_DocumentedVolume = 10.5m;
			shipment.JS_ManifestedVolume = 5.05m;

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented volume", "10.500 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier volume", "5.050 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);
		}

		public void TestVolumeForManifestConsolExportDisplayVolumewhenAir()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				shipment.JS_ActualVolume = 8.00m;
				shipment.JS_DocumentedVolume = 10.5m;
				shipment.JS_ManifestedVolume = 5.1m;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.Shipments.Add(shipment);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "DEHAM";

				Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir = false;
				var wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscapeDetailed);
				AssertEquals("Should be the actual Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.Manifest);
				AssertEquals("Should be the Documented Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ColoadMasterManifest);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.SummaryManifest);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscape);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir = true;
				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscapeDetailed);
				AssertEquals("Should be the actual Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.Manifest);
				AssertEquals("Should be the Documented Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ColoadMasterManifest);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.SummaryManifest);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscape);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);
			}
		}

		public void TestLoadingMeters()
		{
			shipment.JS_LoadingMeters = 10m;
			shipment.JS_DocumentedLoadingMeters = 11m;
			shipment.JS_ManifestedLoadingMeters = 12m;

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual loading meters", 10m, wrapper.LoadingMeters);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented loading meters", 11m, wrapper.LoadingMeters);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier loading meters", 12m, wrapper.LoadingMeters);
		}

		public void TestChargeable()
		{
			shipment.JS_ActualChargeable = 8.00m;
			shipment.JS_DocumentedChargeable = 10.5m;
			shipment.JS_ManifestedChargeable = 5.1m;

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual Chargeable", "8.000 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented Chargeable", "10.500 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier Chargeable", "5.100 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
		}

		FreightWrapperFromShipment CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(string displayType, string reportName = null)
		{
			Env.Registry.PreAlertWeightAndVolumeDisplay = displayType;

			FreightWrapperFromShipment result = new FreightWrapperFromShipment(shipment, Factory);
			result.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			if (!string.IsNullOrEmpty(reportName))
			{
				result.SetReportNameForTesting(reportName);
			}
			else
			{
				result.SetReportNameForTesting(DocBaseWrapper.PreAlert);
			}

			return result;
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = (FreightWrapperFromShipment)GetSetupWrapperForDefaultFormatting();
			AssertEquals("TrackingBusinessObjectPK", shipment.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestWrapperMappingMBLIssue()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = new ZDateTime(2007, 8, 24);
			shipment = consol.Shipments.AddNew();

			FreightWrapperFromShipment fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.MasterBillIssue", new ZDateTime(2007, 8, 24), fullWrapper.MasterBillIssue);
		}

		public void TestWrapperMappingFull()
		{
			#region Setup

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00012345";
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			OrgHeader sendingForwarder = GetOrgHeader("SND_FWDER");
			sendingForwarder.MainAddress.OA_Address1 = "SF MAIN ADDRESS";
			OrgAddress sfAddr2 = sendingForwarder.Addresses.AddNew();
			sfAddr2.OA_Address1 = "SF ADDRESS";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;

			OrgHeader receivingForwarder = GetOrgHeader("RCV_FWDER");
			receivingForwarder.MainAddress.OA_Address1 = "RF MAIN ADDRESS";
			OrgAddress rfAddr2 = receivingForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RF ADDRESS";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			OrgHeader shippingLine = GetOrgHeader("SHIPPINGLINE");
			shippingLine.MainAddress.OA_Address1 = "SL MAIN ADDRESS";
			OrgAddress slAddr2 = shippingLine.Addresses.AddNew();
			slAddr2.OA_Address1 = "SL ADDRESS";
			slAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = slAddr2.PK;

			OrgHeader pickupAgent = GetOrgHeader("PICKUPAGENT");
			pickupAgent.MainAddress.OA_Address1 = "PA MAIN ADDRESS";

			OrgHeader deliveryAgent = GetOrgHeader("DELIVERYAGENT");
			deliveryAgent.MainAddress.OA_Address1 = "DA MAIN ADDRESS";

			consol.JK_OA_CreditorAddress = GetOrgHeader("CONS_CREDIT").MainAddress.PK;
			consol.JK_BookingReference = "BOOK_A_HOOKER";
			consol.JK_PrepaidCollect = "CCX";

			OrgHeader controllingAgent = GetOrgHeader("CONTROLLING AGENT");
			controllingAgent.MainAddress.OA_Address1 = "CA MAIN ADDRESS";

			OrgHeader controllingCustomer = GetOrgHeader("CONTROLLING CUSTOMER");
			controllingCustomer.MainAddress.OA_Address1 = "CC MAIN ADDRESS";

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
			shipment.JS_INCO = Constants.IncoTerms.CarriageAndInsurancePaidTo;
			shipment.JS_AdditionalTerms = "Hello AdditionalTerms!";
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			shipment.JS_RL_NKDestination = "ERXXX";
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 13);
			shipment.JS_ConsolReference = "ConsolRef";
			shipment.JS_InspectionTypeCode = "UNK";

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;
			shipment.ControllingAgentDocumentaryAddress.E2_OA_Address = controllingAgent.MainAddress.PK;
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			shipment.PickupAgentDocumentaryAddress.OrganisationPK = pickupAgent.PK;
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

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

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000026";

			ForwardingPackLine packLine1 = shipment.OuterPackLines.Count == 0 ? shipment.OuterPackLines.AddNew() : shipment.OuterPackLines[0];
			packLine1.JL_JC = container.PK;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "PK";

			ForwardingPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_PackageCount = 22;
			packLine2.JL_F3_NKPackType = "PK";

			ForwardingPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container.PK;
			packLine3.JL_PackageCount = 33;
			packLine3.JL_F3_NKPackType = "PK";

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.400;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_LoadingMeters = 10.24m;
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

			Transport shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKLoadPort = "ERYYY";
			shipmentTransport.JW_ETD = new ZDateTime(2006, 5, 10);
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";
			shipmentTransport.JW_ETA = new ZDateTime(2006, 5, 11);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.CarriageAndInsurancePaidTo;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1ONE1";
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2TWO2";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = this.GetOrgHeader("ALSONOTIFYME").MainAddress.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = this.GetOrgHeader("NOTIFYMEPLEASE").MainAddress.PK;

			transport.JW_JX = sailing.PK;
			sailing.JX_JB = voyageDestination.PK;

			transport.JW_TerminalCutOff = new ZDateTime(2006, 4, 23);
			transport.JW_DepotCutOff = new ZDateTime(2006, 4, 1);

			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2006, 8, 1);
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2006, 8, 3);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2006, 9, 6);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2006, 9, 9);

			shipment.DocsAndCartage.RequiredDocuments.AddNew();

			shipment.Services.AddNew();
			shipment.Services.AddNew();

			ZString orderItems = "Order1, Order2";
			shipment.DocsAndCartage.JP_OrderItemsAsString = orderItems;

			#endregion

			FreightWrapperFromShipment fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", Constants.ContainerModes.FCL, fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.LCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.InspectionType", "UNK - Unknown - No Security Measures Taken", fullWrapper.InspectionType.ToString());
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ConsolType.Code", Constants.AgentType.Agent, fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", "IMP", fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.Incoterm.Code", Constants.IncoTerms.CarriageAndInsurancePaidTo, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "PPD - Prepaid", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.AdditionalTerms", "Hello AdditionalTerms!", fullWrapper.AdditionalTerms);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "MSA", fullWrapper.ShipmentStatus.Code);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", "PICKUPCARTAGE", fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "CTO WHATEVER COMPANY", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "DELIVERYCARTAGE", fullWrapper.DeliveryAgent.CompanyName);

			AssertEquals("fullWrapper.Carrier.CompanyNameAndAddress", "SHIPPINGLINE\nSL ADDRESS\nAUSTRALIA", fullWrapper.Carrier.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", "DELIVERYAGENT\nDA MAIN ADDRESS\nAUSTRALIA", fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", "PICKUPAGENT\r\nPA MAIN ADDRESS\nAUSTRALIA", fullWrapper.ExportAgent.CompanyNameAndAddress);

			AssertEquals("fullWrapper.ImportBroker.CompanyName", "IMP_BROKER", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", "EXP_BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", "DELIVERYAGENT\r\nDA MAIN ADDRESS\nAUSTRALIA", fullWrapper.LocalForwarder.CompanyNameAndAddress);

			AssertEquals("fullWrapper.NotifyParty", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.NotifyParty2", "ALSONOTIFYME", fullWrapper.NotifyParty2.CompanyName);
			AssertEquals("fullWrapper.NotifyParty3", "NOTIFYMEPLEASE", fullWrapper.NotifyParty3.CompanyName);
			AssertEquals("fullWrapper.ConsolCreditor", "CONS_CREDIT", fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "CONTROLLING AGENT", fullWrapper.ControllingAgent.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "CONTROLLING CUSTOMER", fullWrapper.ControllingCustomer.CompanyName);

			AssertEquals("Port Of Loading UNLOCO", "USLAX", fullWrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("Port Of Loading ETA", new ZDateTime(2006, 5, 8), fullWrapper.ShipmentRoutes["First"].EstimatedArrival);
			AssertEquals("Port Of Loading ATA", new ZDateTime(2006, 5, 9), fullWrapper.ShipmentRoutes["First"].ActualArrival);
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
			AssertEquals("fullWrapper.LoadingMeters", 10.24m, fullWrapper.LoadingMeters);

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
			AssertEquals("fullWrapper.ConsolAgentsReference", "AGENT_1", fullWrapper.ConsolAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", "S000234567", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.BookingReference", "BOOK_A_HOOKER", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.LCL, fullWrapper.HBLContainerMode);

			AssertEquals("fullWrapper.DeliveryDueDate", ZDateTime.Empty, fullWrapper.DeliveryDueDate);
			AssertEquals("fullWrapper.RevisedDeliveryDueDate", ZDateTimeOffset.Empty, fullWrapper.RevisedDeliveryDueDate);
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

			AssertEquals("fullWrapper.CommercialInvoices.Count", 1, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 2, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "665.33 HKD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.InsuranceValue.AmountAndCurrencyCode", "898.22 EUR", fullWrapper.InsuranceValue.AmountAndCurrencyCode);

			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Packages.Count", 3, fullWrapper.Packages.Count);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.ShipmentRoutes.Count", 2, fullWrapper.ShipmentRoutes.Count);

			AssertEquals("fullWrapper.CustomsEntries.Count", 2, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Orders.Count", 2, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 2, fullWrapper.Services.Count);

			AssertEquals("fullWrapper.ConsolNumber", "C00012345", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.JobNumber", "S000234567", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.SecondaryHeading", "Consol", fullWrapper.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", "C00012345", fullWrapper.SecondaryNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.ConsolReference", "ConsolRef", fullWrapper.ConsolReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 55.4m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 32.45m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 334, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", "USDNV", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "ERXXX", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", "USDNV", fullWrapper.FreightPayableAt.UNLOCO);

			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.CustomerReference", orderItems, fullWrapper.CustomerReference);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyNameAndAddress", "DELIVERYAGENT\nDA MAIN ADDRESS\nAUSTRALIA", fullWrapper.ReceivingForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.SendingForwarder.CompanyNameAndAddress", "SND_FWDER\r\nSF ADDRESS\nAUSTRALIA", fullWrapper.SendingForwarder.CompanyNameAndAddress);

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetImportAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var receivingForwarder = GetOrgHeader("Our Receiving Forwarder");
			var forwarderContact1 = receivingForwarder.Contacts.AddNew();
			forwarderContact1.OC_ContactName = "Scott";
			var doc1 = forwarderContact1.Documents.AddNew();
			doc1.OD_DocumentGroup = "ALL";

			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			shipment = consol.Shipments.AddNew();

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Receiving Forwarder has 1 contact", 1, receivingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Receiving Forwarder", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Scott", wrapper.ImportAgent.ContactName);

			var forwarderContact2 = receivingForwarder.Contacts.AddNew();
			forwarderContact2.OC_ContactName = "Maria";
			var doc2 = forwarderContact2.Documents.AddNew();
			doc2.OD_DocumentGroup = "FWI";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Receiving Forwarder has 2 contacts", 2, receivingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Receiving Forwarder", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Maria", wrapper.ImportAgent.ContactName);

			var forwarderContact3 = receivingForwarder.Contacts.AddNew();
			forwarderContact3.OC_ContactName = "Tina";
			var doc3 = forwarderContact3.Documents.AddNew();
			doc3.OD_DocumentGroup = "FIA";

			var forwarderContact4 = receivingForwarder.Contacts.AddNew();
			forwarderContact4.OC_ContactName = "Rose";
			var doc4 = forwarderContact4.Documents.AddNew();
			doc4.OD_DocumentGroup = "FIS";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Receiving Forwarder has 4 contacts", 4, receivingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Receiving Forwarder", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Tina", wrapper.ImportAgent.ContactName);

			var deliveryAgent = GetOrgHeader("Our Delivery Agent");
			var agentContact1 = deliveryAgent.Contacts.AddNew();
			agentContact1.OC_ContactName = "Smith";
			var doc5 = agentContact1.Documents.AddNew();
			doc5.OD_DocumentGroup = "ALL";

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Delivery Agent has 1 contact", 1, deliveryAgent.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Delivery Agent", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Smith", wrapper.ImportAgent.ContactName);

			var agentContact2 = deliveryAgent.Contacts.AddNew();
			agentContact2.OC_ContactName = "Sonia";
			var doc6 = agentContact2.Documents.AddNew();
			doc6.OD_DocumentGroup = "FWI";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Delivery Agent has 2 contacts", 2, deliveryAgent.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Delivery Agent", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Sonia", wrapper.ImportAgent.ContactName);

			var agentContact3 = deliveryAgent.Contacts.AddNew();
			agentContact3.OC_ContactName = "Ruby";
			var doc7 = agentContact3.Documents.AddNew();
			doc7.OD_DocumentGroup = "FIA";

			var agentContact4 = deliveryAgent.Contacts.AddNew();
			agentContact4.OC_ContactName = "Pia";
			var doc8 = agentContact4.Documents.AddNew();
			doc8.OD_DocumentGroup = "FIS";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Delivery Agent has 4 contacts", 4, deliveryAgent.Contacts.Count);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Our Delivery Agent", wrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "Ruby", wrapper.ImportAgent.ContactName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetExportAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var sendingForwarder = GetOrgHeader("Our Sending Forwarder");
			var forwarderContact1 = sendingForwarder.Contacts.AddNew();
			forwarderContact1.OC_ContactName = "Scott";
			var doc1 = forwarderContact1.Documents.AddNew();
			doc1.OD_DocumentGroup = "ALL";

			consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			shipment = consol.Shipments.AddNew();

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Sending Forwarder has 1 contact", 1, sendingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Sending Forwarder", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Scott", wrapper.ExportAgent.ContactName);

			var forwarderContact2 = sendingForwarder.Contacts.AddNew();
			forwarderContact2.OC_ContactName = "Sonia";
			var doc2 = forwarderContact2.Documents.AddNew();
			doc2.OD_DocumentGroup = "FWE";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Sending Forwarder has 2 contacts", 2, sendingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Sending Forwarder", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Sonia", wrapper.ExportAgent.ContactName);

			var forwarderContact3 = sendingForwarder.Contacts.AddNew();
			forwarderContact3.OC_ContactName = "Gina";
			var doc3 = forwarderContact3.Documents.AddNew();
			doc3.OD_DocumentGroup = "FEA";

			var forwarderContact4 = sendingForwarder.Contacts.AddNew();
			forwarderContact4.OC_ContactName = "Rose";
			var doc4 = forwarderContact4.Documents.AddNew();
			doc4.OD_DocumentGroup = "FES";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Sending Forwarder has 4 contacts", 4, sendingForwarder.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Sending Forwarder", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Gina", wrapper.ExportAgent.ContactName);

			var pickupAgent = GetOrgHeader("Our Pickup Agent");
			var agentContact1 = pickupAgent.Contacts.AddNew();
			agentContact1.OC_ContactName = "Smith";
			var doc5 = agentContact1.Documents.AddNew();
			doc5.OD_DocumentGroup = "ALL";

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = pickupAgent.PK;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pickup Agent has 1 contact", 1, pickupAgent.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Pickup Agent", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Smith", wrapper.ExportAgent.ContactName);

			var agentContact2 = pickupAgent.Contacts.AddNew();
			agentContact2.OC_ContactName = "Tina";
			var doc6 = agentContact2.Documents.AddNew();
			doc6.OD_DocumentGroup = "FWE";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pickup Agent has 2 contacts", 2, pickupAgent.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Pickup Agent", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Tina", wrapper.ExportAgent.ContactName);

			var agentContact3 = pickupAgent.Contacts.AddNew();
			agentContact3.OC_ContactName = "Dora";
			var doc7 = agentContact3.Documents.AddNew();
			doc7.OD_DocumentGroup = "FEA";

			var agentContact4 = pickupAgent.Contacts.AddNew();
			agentContact4.OC_ContactName = "Doll";
			var doc8 = agentContact4.Documents.AddNew();
			doc8.OD_DocumentGroup = "FES";

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pickup Agent has 4 contacts", 4, pickupAgent.Contacts.Count);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "Our Pickup Agent", wrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.ContactName", "Dora", wrapper.ExportAgent.ContactName);
		}

		public void TestImportExportAgentContactNameWithNoConsolAndNoAgents()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "The Import Air Freight Manager", wrapper.ImportAgent.ContactName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "The Export Air Freight Manager", wrapper.ExportAgent.ContactName);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "The Import Sea Freight Manager", wrapper.ImportAgent.ContactName);
			AssertEquals("fullWrapper.ImportAgent.ContactName", "The Export Sea Freight Manager", wrapper.ExportAgent.ContactName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetReceivingForwarder_DeliveryAgentExists_UseDeliveryAgentAsForwarder()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.SetDefaultReceivingForwarderAddress(GetOrgHeader("McLaren"));

			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = GetOrgHeader("Manchester United").PK;

			var wrapperToTest = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals("Delivery agent was used as receiving forwarder", "Manchester United", wrapperToTest.ReceivingForwarder.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetReceivingForwarder_DeliveryAgentNotExists_UseConsolReceivingForwarderAsForwarder()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.SetDefaultReceivingForwarderAddress(GetOrgHeader("McLaren"));

			var shipment = consol.Shipments.AddNew();

			var wrapperToTest = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals("Consol's receiving forwarder is used as receiving forwarder", "McLaren", wrapperToTest.ReceivingForwarder.CompanyName);
		}

		public void TestGetOriginAndGetDestinationWithConsolArrivalDate()
		{
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_DEP = new ZDateTime(2012, 1, 1);
			shipment.JS_E_ARV = new ZDateTime(2012, 1, 2);

			FreightWrapperFromShipment fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.Origin.ActualDate should use shipment", new ZDateTime(2012, 1, 1), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.ActualDate should use shipment", new ZDateTime(2012, 1, 2), fullWrapper.Destination.ActualDate);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "ERASA";

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2009, 1, 2);
			transport.JW_ATD = new ZDateTime(2009, 1, 1);
			transport.JW_RL_NKDiscPort = "ERASA";
			transport.JW_ETA = new ZDateTime(2009, 2, 3);
			transport.JW_ATA = new ZDateTime(2009, 2, 4);

			shipment = consol.Shipments.AddNew();

			fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2009, 1, 2), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2009, 2, 3), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate with Consol Actual Departure date set", new ZDateTime(2009, 1, 1), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.ActualDate with Consol Actual Arrival date set", new ZDateTime(2009, 2, 4), fullWrapper.Destination.ActualDate);

			shipment.JS_E_DEP = new ZDateTime(2008, 10, 10);
			shipment.JS_E_ARV = new ZDateTime(2010, 10, 10);

			fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.Origin.ActualDate should use shipment if it's earlier", new ZDateTime(2008, 10, 10), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.ActualDate should use shipment if it's later", new ZDateTime(2010, 10, 10), fullWrapper.Destination.ActualDate);

			fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;
			AssertEquals("fullWrapper.Origin.ActualDate should use consol date", new ZDateTime(2009, 1, 1), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.ActualDate should use consol date", new ZDateTime(2009, 2, 4), fullWrapper.Destination.ActualDate);

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_DEP = new ZDateTime(2009, 3, 5);
			shipment.JS_E_ARV = new ZDateTime(2009, 3, 6);
			fullWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.Origin.EstimatedDate without Consol Actual Departure date set - no consol found", new ZDateTime(2009, 3, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Destination.EstimatedDate without Consol Actual Arrival date set - no consol found", new ZDateTime(2009, 3, 6), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate without Consol Actual Departure date set - no consol found", new ZDateTime(2009, 3, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.ActualDate without Consol Actual Arrival date set - no consol found", new ZDateTime(2009, 3, 6), fullWrapper.Destination.ActualDate);
		}

		public void TestFreightShipmentDocumentWrapperNotNull()
		{
			AssertNotNull("Wrapper.FreightShipment should not be null", Wrapper.FreightShipment);
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

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
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

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
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
			AssertEquals(ZString.Empty, wrapper.WarehouseLocation);
		}

		public override void TestWrapperNotes()
		{
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST HANDLING INFO");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTE INCEST");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO");

			FreightWrapperFromShipment noteWrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("fullWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST HANDLING INFO", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("fullWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTE INCEST", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("fullWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestCoLoadShipments()
		{
			CommonShipment coloadShipment = shipment.CoLoadShipments.AddNew();
			coloadShipment.JS_HouseBill = "HBL324324";

			FreightWrapperFromShipment wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("wrapper.FreightJobs.Count", 1, wrapper.FreightJobs.Count);
			AssertEquals("wrapper.FreightJobs[0].HouseBill", "HBL324324", wrapper.FreightJobs[0].HouseBill);
		}

		public void TestOrderLines()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var coload1 = shipment.CoLoadShipments.AddNew();
			var coload2 = shipment.CoLoadShipments.AddNew();

			var order1 = coload1.AttachedOrders.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = order1.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var order2 = coload2.AttachedOrders.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLine3 = order2.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;

			var order3 = coload2.AttachedOrders.AddNew();
			order3.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLine4 = order3.OrderLines.AddNew();
			orderLine4.JO_LineNo = 4;

			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "11112222";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_JH = header.PK;

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTORORG";
			invoice.AH_OH = debtor.PK;

			var wrapper = (FreightWrapperFromShipment)FreightWrapper.New(invoice, Factory)[0];
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3, 4 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			wrapper = (FreightWrapperFromShipment)FreightWrapper.New(invoice, Factory)[0];
			AssertEquals(0, wrapper.OrderLines.Count);
		}

		public void TestOrdersFromRelatedShipmentsAreIncludedWhenWrapperIsFromARInvoice()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			ForwardingShipment coload1 = shipment.CoLoadShipments.AddNew();
			ForwardingShipment coload2 = shipment.CoLoadShipments.AddNew();

			Order order1 = coload1.AttachedOrders.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Order order2 = coload2.AttachedOrders.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Order order3 = coload2.AttachedOrders.AddNew();
			order3.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Job header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "11112222";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_JH = header.PK;

			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTORORG";
			invoice.AH_OH = debtor.PK;

			FreightWrapperFromShipment wrapper = (FreightWrapperFromShipment)FreightWrapper.New(invoice, Factory)[0];
			AssertEquals("Orders wrapper collection contain orders from related shipments", 3, wrapper.Orders.Count);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			wrapper = (FreightWrapperFromShipment)FreightWrapper.New(invoice, Factory)[0];
			AssertEquals("No related orders when shipment is not a buyers consol lead", 0, wrapper.Orders.Count);
		}

		public void TestInvoicingJob()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "11112222";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var wrapper = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];
			AssertNotNull(wrapper.InvoicingJob);
			AssertNull(wrapper.InvoicingJob.Debtor);

			var localClient = Factory.New<OrgHeader>();
			header.LocalChargesPK = localClient.PK;

			var charge1 = header.Charges.AddNew();
			charge1.JR_OH_SellAccount = localClient.PK;

			var wrappers = new FreightWrapperFromShipment[] { (FreightWrapperFromShipment)FreightWrapper.New(shipment, localClient, Factory)[0] };

			AssertEquals(1, wrappers.Length);
			AssertEquals(localClient, wrappers[0].InvoicingJob.Debtor.WrappedObject);
			AssertEquals(1, wrappers[0].InvoicingJob.Charges.Count);

			var someDebtor = Factory.New<OrgHeader>();

			var charge2 = header.Charges.AddNew();
			charge2.JR_OH_SellAccount = someDebtor.PK;

			wrappers = new FreightWrapperFromShipment[]
									 {
								 (FreightWrapperFromShipment)FreightWrapper.New(shipment, localClient, Factory)[0],
					(FreightWrapperFromShipment)FreightWrapper.New(shipment, someDebtor, Factory)[0]
									 };

			AssertEquals(2, wrappers.Length);
			AssertEquals(localClient, wrappers[0].InvoicingJob.Debtor.WrappedObject);
			AssertEquals(someDebtor, wrappers[1].InvoicingJob.Debtor.WrappedObject);
			AssertEquals(1, wrappers[0].InvoicingJob.Charges.Count);
			AssertEquals(1, wrappers[1].InvoicingJob.Charges.Count);

			wrapper = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];

			AssertEquals(null, wrapper.InvoicingJob.Debtor);
			AssertEquals(2, wrapper.InvoicingJob.Charges.Count);
		}

		public void TestCustomsEntryNumber()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var branchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				FreightWrapperFromShipment wrapper = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];
				AssertEquals(ZString.Empty, wrapper.CustomsEntryNumber);

				shipment.CustomsEntryNumberType = CMRExportExemptionCodes.EXML.Code;
				wrapper = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];
				AssertEquals("EXML", wrapper.CustomsEntryNumber);

				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CAN;
				shipment.CustomsEntryNumber = "112233";
				wrapper = (FreightWrapperFromShipment)FreightWrapper.New(shipment, Factory)[0];
				AssertEquals("CAN 112233", wrapper.CustomsEntryNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CATOR";
				shipment.ResetCusEntryNumbers();
				shipment.JS_RL_NKOrigin = "CATOR";
				shipment.JS_RL_NKDestination = "AUSYD";
				var caDeclaration = Factory.New<Integration.Customs.CA.IJobDeclaration>();
				caDeclaration.JE_JS = shipment.PK;
				caDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				caDeclaration.JE_CERSProofOfReportNumber = "CAED";
				wrapper = new FreightWrapperFromShipment(shipment, Factory);
				AssertEquals("POR CAED", wrapper.CustomsEntryNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = branchPort;
			}
		}

		#region TestGoodsAvailableAt

		public void TestGoodsAvailableAtWhenTransportModeIsAir()
		{
			CreateConsolWithCTOAndCFSAddress();

			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("If consol's transport mode is Air, it will always return CFS address.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			consol1.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("CFS is Null.", null, null, wrapper.GoodsAvailableAt);
		}

		public void TestGoodsAvailableAtWhenShipmentIsROROOrBBK()
		{
			CreateConsolWithCTOAndCFSAddress();

			var deliveryCFS = Factory.New<OrgHeader>();
			shipment.JS_OA_ImportReleaseDepot = deliveryCFS.MainAddress.PK;

			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Depot is returned from shipment.", deliveryCFS.MainAddress, deliveryCFS, wrapper.GoodsAvailableAt);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("BCN consol will always return CTO Address irrespective of CFS Address", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in RORO container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			consol1.JK_OA_ArrivalCTOAddress = ZGuid.Empty;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in RORO container and CTO is null.", null, null, wrapper.GoodsAvailableAt);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BBK container and CTO is null.", null, null, wrapper.GoodsAvailableAt);

			consol1.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BBK container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);
		}

		public void TestGoodsAvailableAtWhenShipmentIsLCL()
		{
			CreateConsolWithCTOAndCFSAddress();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("BCN consol will always return CTO Address irrespective of CFS Address", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			#region CFSIsNotNull

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol1.PK;

			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in BCN and FCL conainers.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in FCL and GRP conainers.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			#endregion

			#region CFSIsNull

			consol1.JK_OA_UnpackDepotAddress = ZGuid.Empty;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("CFS is null.", null, null, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol2.PK;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			#endregion
		}

		public void TestGoodsAvailableAtWhenShipmentIsFCL()
		{
			CreateConsolWithCTOAndCFSAddress();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("BCN consol will always return CTO Address irrespective of CFS Address", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			#region CTOIsNotNull

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol1.PK;

			container2.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in BCN and GRP conainers.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			#endregion

			#region CTOIsNull

			consol1.JK_OA_ArrivalCTOAddress = ZGuid.Empty;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in BCN and GRP conainers.", null, null, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol2.PK;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", null, null, wrapper.GoodsAvailableAt);

			#endregion
		}

		public void TestGoodsAvailableAtWhenShipmentIsBCN()
		{
			CreateConsolWithCTOAndCFSAddress();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("BCN consol will always return CTO Address irrespective of CFS Address", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			#region CFSIsNotNull

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol1.PK;

			container2.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in GRP and BCN conainers.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in GRP and FCL conainers.", unpackCFS.MainAddress, unpackCFS, wrapper.GoodsAvailableAt);

			#endregion

			#region CFSIsNull

			consol1.JK_OA_UnpackDepotAddress = ZGuid.Empty;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in Groupage and FCL conainers.", arrivalCTO.MainAddress, arrivalCTO, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol2.PK;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in LCL container.", null, null, wrapper.GoodsAvailableAt);

			#endregion

			#region CTOIsNull

			consol1.JK_OA_ArrivalCTOAddress = ZGuid.Empty;

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in FCL container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in BCN container.", null, null, wrapper.GoodsAvailableAt);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("Packline is packed in GRP container.", null, null, wrapper.GoodsAvailableAt);

			container2.JC_JK = consol1.PK;

			container2.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in GRP and BCN conainers.", null, null, wrapper.GoodsAvailableAt);

			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertAddressAndOrg("The packlines are packed in GRP and FCL conainers.", null, null, wrapper.GoodsAvailableAt);

			#endregion
		}

		void AssertAddressAndOrg(string message, OrgAddress expectedAddress, OrgHeader expectedOrg, AddressWrapper propertyToTest)
		{
			AssertEquals(message + " - Address", expectedAddress, propertyToTest.WrappedObject);
			AssertEquals(message + " - Org", expectedOrg, propertyToTest.Organization.WrappedObject);
		}

		#endregion

		public void TestNullReferenceExceptionOnShipmentBO()
		{
			var wrapper1 = new FreightWrapperFromShipment(null, Factory);
			AssertEquals(ZString.Empty, wrapper1.HouseBill);
		}

		public void TestNoExceptionThrownWhenPassingInNonForwardingShipmentBusinessObject()
		{
			CFSLoadListConsol loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			AssertNoExceptionThrown(() => { FreightWrapperFromShipment.New(loadList, Factory); });
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 157.261m;
			shipment.JS_ActualVolume = 23.567m;

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals(157.27m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("157.27 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(23.56m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("23.56 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(23.56m, wrapper.ChargeableWeight.Value);
			AssertEquals("M3", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("23.56 M3", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		public void TestInspectionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_InspectionTypeCode = "XRY";

				var wrapper = new FreightWrapperFromShipment(shipment, Factory);
				AssertEquals("XRY", wrapper.InspectionType.Code);
				AssertEquals("X-Ray Equipment", wrapper.InspectionType.Description);

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var wrapper2 = new FreightWrapperFromShipment(shipment, Factory);
					AssertNull("Inspection type is not enabled", wrapper2.InspectionType);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "FSA";

				var seaConsol = shipment.Consols.AddNew();
				seaConsol.JK_TransportMode = "SEA";
				seaConsol.JK_RL_NKLoadPort = "ZACPT";
				seaConsol.JK_RL_NKDischargePort = "JPOSA";

				var airConsol = shipment.Consols.AddNew();
				airConsol.JK_TransportMode = "AIR";
				airConsol.JK_RL_NKLoadPort = "JPOSA";
				airConsol.JK_RL_NKDischargePort = "SGSIN";

				shipment.JS_RL_NKOrigin = "ZACPT";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_InspectionTypeCode = "XRY";

				var wrapper = new FreightWrapperFromShipment(shipment, Factory);
				AssertEquals("XRY", wrapper.InspectionType.Code);
				AssertEquals("X-Ray Equipment", wrapper.InspectionType.Description);

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var wrapper2 = new FreightWrapperFromShipment(shipment, Factory);
					AssertNull("Inspection type is not enabled", wrapper2.InspectionType);
				}
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
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
			cTO.MainAddress.OA_RN_NKCountryCode = string.Empty;

			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "NZAKL";

			shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_INCO = Constants.IncoTerms.CarriageAndInsurancePaidTo;
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_RL_NKDestination = "ERXXX";

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;
			shipment.JS_OH_DeliveryAgent = GetOrgHeader("DEL_AGENT").PK;

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

			JobDocAddress pickupAgentAddress = shipment.PickupAgentDocumentaryAddress;
			pickupAgentAddress.E2_AddressOverride = true;
			pickupAgentAddress.E2_CompanyName = "PICK_AGENT";

			OrgHeader deliveryAgent = Factory.New<OrgHeader>();
			deliveryAgent.OH_FullName = "DEL_AGENT";
			deliveryAgent.MainAddress.OA_Address1 = "TEST 2 ADDRESS";
			deliveryAgent.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

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
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYMEALSO").MainAddress.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYMEPLEASE").MainAddress.PK;

			return new FreightWrapperFromShipment(shipment, Factory);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((ForwardingShipment)WrappedBO).Consols.AddNew().JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			return new FreightWrapperFromShipment((ForwardingShipment)WrappedBO, Factory);
		}

		#region HouseACIDNo

		public void TestHouseACIDNo()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var acidNumber1 = shipment.Numbers.AddNew();
			acidNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			acidNumber1.CE_EntryNum = "1234567890123456789";
			acidNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;

			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("ACIDNo", "1234567890123456789", wrapper.HouseACIDNo);

			var acidNumber2 = shipment.Numbers.AddNew();
			acidNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			acidNumber2.CE_EntryNum = "9876543210987654321";
			acidNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			var acidNumber3 = shipment.Numbers.AddNew();
			acidNumber3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			acidNumber3.CE_EntryNum = "4564564560789789789";
			acidNumber3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("ACIDNo", "1234567890123456789,9876543210987654321", wrapper.HouseACIDNo);
		}

		#endregion

		public void TestImportContainerPenalties()
		{
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("No delivery penalties on shipment", 0, wrapper.ImportContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pickup penalties are export not import", 0, wrapper.ImportContainerPenalties.Count);

			shipment.DeliveryPenalties.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Delivery penalty is counted", 1, wrapper.ImportContainerPenalties.Count);
		}

		public void TestExportContainerPenalties()
		{
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("No Pickup penalties on shipment", 0, wrapper.ExportContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Pickup penalty is counted", 1, wrapper.ExportContainerPenalties.Count);

			shipment.DeliveryPenalties.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals("Delivery Penalties are import not export", 1, wrapper.ExportContainerPenalties.Count);
		}

		public void TestContainerPenalties()
		{
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(0, wrapper.ContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();
			shipment.PickupPenalties.AddNew();
			shipment.DeliveryPenalties.AddNew();
			shipment.DeliveryPenalties.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(4, wrapper.ContainerPenalties.Count);
		}

		public void TestCO2eEmissions()
		{
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(0, wrapper.CO2eEmissions.Count);

			shipment.Transports.AddNew();
			shipment.Transports.AddNew();

			wrapper = new FreightWrapperFromShipment(shipment, Factory);
			AssertEquals(2, wrapper.CO2eEmissions.Count);
		}

		public void TestSetPackageOverride()
		{
			wrapper = new FreightWrapperFromShipment(shipment, Factory);

			var packLine = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageOverride(packLine);

			AssertEquals(1, wrapper.Packages.Count);
		}

		public void TestSetPackageCollectionOverride()
		{
			wrapper = new FreightWrapperFromShipment(shipment, Factory);

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			((IPackLineOverrider)wrapper).SetPackageCollectionOverride(
				new List<PackLine>(new[] { packLine1, packLine2 }));

			AssertEquals(2, wrapper.Packages.Count);
		}

		public override void TestFormattedTotalCO2e()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = consol.Shipments.AddNew();
			shipment.SetTotalCO2e(9.07244m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var shipmentWrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals("9.072", shipmentWrapper.FormattedTotalCO2e);

			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			shipmentWrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals(ZString.Empty, shipmentWrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;
			shipment.SetCO2ePerTonneInKg(200m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			var shipmentWrapper = new FreightWrapperFromShipment(shipment, Factory);

			AssertEquals((shipment.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc, shipmentWrapper.CO2eCalculationDate);
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
ArrivalCFSTransport : 
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
CTOArrival : CTO WHATEVER COMPANY
DeliveryAddress : IMPORTER\nAUSTRALIA
DeliveryAgent : DELIVERYCARTAGE\nAUSTRALIA
DeliveryLocation : ERXXX
DepartureCFSTransport : 
Destination : ERXXX
ExportAgent : SND_FWDER\nAUSTRALIA
ExportBroker : EXP_BROKER\nAUSTRALIA
ExportReceivalAddress : TEST 1 ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST 1 ADDRESS\nAUSTRALIA
FreightPayableAt : USDNV - Dunnville
FreightRate : 123.45 AUD
GoodsAvailableAt : TEST 2 ADDRESS\nAUSTRALIA
GoodsValue : 665.33 HKD
ImportAgent : DEL_AGENT\nTEST 2 ADDRESS\nAUSTRALIA
ImportArrivalCTOAddress : CTO WHATEVER COMPANY
ImportBroker : IMP_BROKER\nAUSTRALIA
IncoTerm : CIP - Carriage and Insurance Paid To
InspectionType : UNK - Unknown - No Security Measures Taken
InsuranceValue : 898.22 EUR
InsuredBy : INSUREDBY THING\nERITREA
LocalForwarder : DEL_AGENT\nTEST 2 ADDRESS\nAUSTRALIA
NotifyParty : NOTIFYME\nAUSTRALIA
NotifyParty2 : NOTIFYMEALSO\nAUSTRALIA
NotifyParty3 : NOTIFYMEPLEASE\nAUSTRALIA
Origin : USDNV - Dunnville
PickupAddress : SUPPLIER\nAUSTRALIA
PickupAgent : PICKUPCARTAGE\nAUSTRALIA
PickupCFSAddress : TEST 1 ADDRESS\nAUSTRALIA
PickupLocation : USDNV - Dunnville
ReceivingForwarder : DEL_AGENT\nTEST 2 ADDRESS\nAUSTRALIA
Registry : (No Default Field Value Available on Registry)
ReleaseType : NXS
SendingForwarder : SND_FWDER\nAUSTRALIA
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
			return Factory.New<ForwardingShipment>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			wrapper = new FreightWrapperFromShipment(shipment, Factory);
		}

		void CreateConsolWithCTOAndCFSAddress()
		{
			shipment = Factory.New<ForwardingShipment>();
			wrapper = new FreightWrapperFromShipment(shipment, Factory);

			consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;

			consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			container1 = Factory.New<ForwardingContainer>();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			consol1.Containers.Add(container1);

			container2 = Factory.New<ForwardingContainer>();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			consol2.Containers.Add(container2);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(container1.PK);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(container2.PK);

			shipment.CurrentConsolForDocuments = consol1;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			arrivalCTO = Factory.New<OrgHeader>();
			unpackCFS = Factory.New<OrgHeader>();

			consol1.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol1.JK_OA_UnpackDepotAddress = unpackCFS.MainAddress.PK;
		}

		FreightWrapperFromShipment wrapper;
		ForwardingShipment shipment;
		ForwardingConsol consol1;
		ForwardingConsol consol2;
		ForwardingContainer container1;
		ForwardingContainer container2;
		OrgHeader arrivalCTO;
		OrgHeader unpackCFS;

		ForwardingConsol CreateExportConsol(ForwardingShipment relatedShipment)
		{
			ForwardingConsol consol = relatedShipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "USCHI";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			return consol;
		}

		#endregion
	}
}
