using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RouteWrapper))]
	sealed class RouteWrapperTest : GenericWrapperTest
	{
		public void TestIsInternationalZBooleanValueCorrectlyRetured()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var date = new ZDateTime(2006, 3, 1);

			var routeWrapper = new RouteWrapper(1, "USPHX", date, "USLAX", date, factory);
			AssertEquals("route is domestic", false, routeWrapper.IsInternational);

			routeWrapper = new RouteWrapper(1, "USPHX", date, "AUSYD", date, factory);
			AssertEquals("route is international", true, routeWrapper.IsInternational);

			routeWrapper = new RouteWrapper(1, null, date, "AUSYD", date, factory);
			AssertEquals("route is invalid", false, routeWrapper.IsInternational);

			routeWrapper = new RouteWrapper(1, null, date, null, date, factory);
			AssertEquals("route is invalid", false, routeWrapper.IsInternational);
		}

		public void TestWrapperMappingsFromForwardingTransportBO()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_DGReceivalCommences = new ZDateTime(2006, 3, 17);
			origin.JA_DGCutOff = new ZDateTime(2006, 3, 19);

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRY ME HOME";

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "PLEASE PAY ME";

			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "VESSELFFF";
			transport.JW_VoyageFlight = "GROUND";
			transport.JW_RL_NKLoadPort = "ZZDKA";
			transport.JW_ETD = new ZDateTime(2006, 3, 5);
			transport.JW_ATD = new ZDateTime(2006, 3, 4);
			transport.JW_RL_NKDiscPort = "ZZFDE";
			transport.JW_ETA = new ZDateTime(2006, 6, 5);
			transport.JW_ATA = new ZDateTime(2006, 6, 4);
			transport.CarrierPK = carrier.PK;
			transport.CreditorPK = creditor.PK;
			transport.JW_CarrierBookingReference = "DRUNK OR SOBER";
			transport.JW_JX = sailing.PK;
			transport.JW_TerminalReceivalCommences = new ZDateTime(2006, 3, 2);
			transport.JW_DepotReceivalCommences = new ZDateTime(2006, 3, 2);
			transport.JW_TerminalCutOff = new ZDateTime(2006, 3, 4);
			transport.JW_DepotCutOff = new ZDateTime(2006, 3, 3);
			transport.JW_TerminalAvailabilityDate = new ZDateTime(2006, 3, 11);
			transport.JW_DepotAvailabilityDate = new ZDateTime(2006, 3, 12);
			transport.JW_TerminalStorageDate = new ZDateTime(2006, 3, 13);
			transport.JW_DepotStorageDate = new ZDateTime(2006, 3, 14);
			transport.JW_TransportType = "MAI";
			transport.JW_PL_NKCarrierServiceLevel = "DTD";

			RouteWrapper wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("wrapper.LegNo", 1, wrapper.LegNo);
			AssertEquals("wrapper.Origin.UNLOCO", "ZZDKA", wrapper.Origin.UNLOCO);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2006, 3, 5), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.ActualDeparture", new ZDateTime(2006, 3, 4), wrapper.ActualDeparture);
			AssertEquals("wrapper.DateOfDeparture", new ZDateTime(2006, 3, 4), wrapper.DateOfDeparture);
			AssertEquals("wrapper.Destination.UNLOCO", "ZZFDE", wrapper.Destination.UNLOCO);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2006, 6, 5), wrapper.EstimatedArrival);
			AssertEquals("wrapper.ActualArrival", new ZDateTime(2006, 6, 4), wrapper.ActualArrival);
			AssertEquals("wrapper.DateOfArrival", new ZDateTime(2006, 6, 4), wrapper.DateOfArrival);
			AssertEquals("wrapper.Carrier.CompanyCode", "CARRY ME HOME", wrapper.Carrier.CompanyName);
			AssertEquals("wrapper.Creditor.CompanyCode", "PLEASE PAY ME", wrapper.Creditor.CompanyName);
			AssertEquals("wrapper.CarriersReference", "DRUNK OR SOBER", wrapper.CarriersReference);
			AssertEquals("wrapper.ServiceLevel", "DTD", wrapper.ServiceLevel.Code);

			AssertEquals("wrapper.Transport.Mode.Code", "SEA", wrapper.Transport.Mode.Code);
			AssertEquals("wrapper.Transport.VesselName", "VESSELFFF", wrapper.Transport.VesselName);
			AssertEquals("wrapper.Transport.VoyageNo", "GROUND", wrapper.Transport.VoyageNo);
			AssertEquals("wrapper.TransportType", "MAI", wrapper.TransportType.Code);

			AssertEquals("wrapper.FCLReceivalCommences", new ZDateTime(2006, 3, 2), wrapper.FCLReceivalCommences);
			AssertEquals("wrapper.LCLReceivalCommences", new ZDateTime(2006, 3, 2), wrapper.LCLReceivalCommences);
			AssertEquals("wrapper.DGFCLReceivalCommences", new ZDateTime(2006, 3, 17), wrapper.DGFCLReceivalCommences);
			AssertEquals("wrapper.FCLCutOff", new ZDateTime(2006, 3, 4), wrapper.FCLCutOff);
			AssertEquals("wrapper.LCLCutOff", new ZDateTime(2006, 3, 3), wrapper.LCLCutOff);
			AssertEquals("wrapper.DGFCLCutOff", new ZDateTime(2006, 3, 19), wrapper.DGFCLCutOff);
			AssertEquals("wrapper.AvailabilityDate", new ZDateTime(2006, 3, 11), wrapper.AvailabilityDate);
			AssertEquals("wrapper.LCLAvailabilityDate", new ZDateTime(2006, 3, 12), wrapper.LCLAvailabilityDate);
			AssertEquals("wrapper.StorageCommences", new ZDateTime(2006, 3, 13), wrapper.FCLStorageCommences);
			AssertEquals("wrapper.LCLStorageCommences", new ZDateTime(2006, 3, 14), wrapper.LCLStorageCommences);

			JobSailing sailing1 = Factory.New<JobSailing>();
			transport.JW_JX = sailing1.PK;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			VoyageOrigin vO = voyage.Origins.AddNew();
			sailing1.JX_JA = vO.PK;
			wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("wrapper.IsCargoOnly", ZBool.True, wrapper.IsCargoOnly);
		}

		public override void TestWrapperMappingsEmpty()
		{
			RouteWrapper wrapperEmpty = new RouteWrapper(ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, null, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, null, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, null, null, ZString.Empty, null, Factory);
			AssertEquals("wrapperEmpty.Origin.UNLOCO", ZString.Empty, wrapperEmpty.Origin.UNLOCO);
			AssertEquals("wrapperEmpty.EstimatedDeparture", ZDateTime.Empty, wrapperEmpty.EstimatedDeparture);
			AssertEquals("wrapperEmpty.ActualDeparture", ZDateTime.Empty, wrapperEmpty.ActualDeparture);
			AssertEquals("wrapperEmpty.DateOfDeparture", ZDateTime.Empty, wrapperEmpty.DateOfDeparture);
			AssertEquals("wrapperEmpty.Destination.UNLOCO", ZString.Empty, wrapperEmpty.Destination.UNLOCO);
			AssertEquals("wrapperEmpty.EstimatedArrival", ZDateTime.Empty, wrapperEmpty.EstimatedArrival);
			AssertEquals("wrapperEmpty.ActualArrival", ZDateTime.Empty, wrapperEmpty.ActualArrival);
			AssertEquals("wrapperEmpty.DateOfArrival", ZDateTime.Empty, wrapperEmpty.DateOfArrival);
			AssertEquals("wrapperEmpty.AvailabilityDate", ZDateTime.Empty, wrapperEmpty.AvailabilityDate);
			AssertEquals("wrapperEmpty.FCLReceivalCommences", ZDateTime.Empty, wrapperEmpty.FCLReceivalCommences);
			AssertEquals("wrapperEmpty.LCLReceivalCommences", ZDateTime.Empty, wrapperEmpty.LCLReceivalCommences);
			AssertEquals("wrapperEmpty.DGFCLReceivalCommences", ZDateTime.Empty, wrapperEmpty.DGFCLReceivalCommences);
			AssertEquals("wrapperEmpty.FCLCutOff", ZDateTime.Empty, wrapperEmpty.FCLCutOff);
			AssertEquals("wrapperEmpty.LCLCutOff", ZDateTime.Empty, wrapperEmpty.LCLCutOff);
			AssertEquals("wrapperEmpty.DGFCLCutOff", ZDateTime.Empty, wrapperEmpty.DGFCLCutOff);
			AssertEquals("wrapperEmpty.LCLAvailabilityDate", ZDateTime.Empty, wrapperEmpty.LCLAvailabilityDate);
			AssertEquals("wrapperEmpty.StorageCommences", ZDateTime.Empty, wrapperEmpty.FCLStorageCommences);
			AssertEquals("wrapperEmpty.LCLStorageCommences", ZDateTime.Empty, wrapperEmpty.LCLStorageCommences);
			AssertEquals("wrapperEmpty.Carrier.CompanyCode", ZString.Empty, wrapperEmpty.Carrier.CompanyCode);
			AssertEquals("wrapperEmpty.Creditor.CompanyCode", ZString.Empty, wrapperEmpty.Creditor.CompanyCode);
			AssertEquals("wrapperEmpty.CarriersReference", ZString.Empty, wrapperEmpty.CarriersReference);
			AssertEquals("wrapperEmpty.LegNo", ZInt.Zero, wrapperEmpty.LegNo);
			AssertEquals("wrapperEmpty.IsCargoOnly", ZBool.False, wrapperEmpty.IsCargoOnly);
			AssertEquals("wrapperEmpty.AdditionalTransportMode", ZString.Empty, wrapperEmpty.AdditionalTransportMode);

			AssertEquals(ZString.Empty, wrapperEmpty.Transport.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperEmpty.Transport.FlightDate);
			AssertEquals(ZString.Empty, wrapperEmpty.Transport.FlightNo);
			AssertEquals(ZString.Empty, wrapperEmpty.Transport.Reference);
			AssertEquals(ZString.Empty, wrapperEmpty.Transport.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperEmpty.Transport.VesselName);
			AssertEquals(ZString.Empty, wrapperEmpty.Transport.VoyageNo);
		}

		public void TestWrapperMappingsAir()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			RouteWrapper wrapperAir = new RouteWrapper(1, air, ZString.Empty, "", "QF253", loco1, null, date1, date2, loco2, null, date3, date4, date5,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				carrier1, creditor1, ref1, voyage, Factory);
			AssertEquals("wrapperAir.Origin.UNLOCO", loco1, wrapperAir.Origin.UNLOCO);
			AssertEquals("wrapperAir.EstimatedDeparture", date1, wrapperAir.EstimatedDeparture);
			AssertEquals("wrapperAir.ActualDeparture", date2, wrapperAir.ActualDeparture);
			AssertEquals("wrapperAir.DateOfDeparture", date2, wrapperAir.DateOfDeparture);
			AssertEquals("wrapperAir.Destination.UNLOCO", loco2, wrapperAir.Destination.UNLOCO);
			AssertEquals("wrapperAir.EstimatedArrival", date3, wrapperAir.EstimatedArrival);
			AssertEquals("wrapperAir.ActualArrival", date4, wrapperAir.ActualArrival);
			AssertEquals("wrapperAir.DateOfArrival", date4, wrapperAir.DateOfArrival);
			AssertEquals("wrapperAir.AvailabilityDate", date5, wrapperAir.AvailabilityDate);
			AssertEquals("wrapperAir.Carrier.CompanyCode", carrier1.OH_Code, wrapperAir.Carrier.CompanyCode);
			AssertEquals("wrapperAir.Creditor.CompanyCode", creditor1.OH_Code, wrapperAir.Creditor.CompanyCode);
			AssertEquals("wrapperAir.CarriersReference", ref1, wrapperAir.CarriersReference);
			AssertEquals("wrapperAir.LegNo", 1, wrapperAir.LegNo);
			AssertEquals("wrapperAir.IsCargoOnly", ZBool.True, wrapperAir.IsCargoOnly);

			AssertEquals("AIR - Air Freight", wrapperAir.Transport.Mode.ToString());
			AssertEquals(date2, wrapperAir.Transport.FlightDate);
			AssertEquals("QF253", wrapperAir.Transport.FlightNo);
			AssertEquals("QF253 / 02-Jan", wrapperAir.Transport.Reference);
			AssertEquals("Flight / Date", wrapperAir.Transport.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperAir.Transport.VesselName);
			AssertEquals(ZString.Empty, wrapperAir.Transport.VoyageNo);
		}

		public void TestWrapperMappingsSea()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			RouteWrapper wrapperSea = new RouteWrapper(2, sea, ZString.Empty, "GOOD SHIP LOLLYPOP", "2324", loco2, null, date0, date4, loco3, null, date1, date0, date5,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				carrier2, creditor2, ref1, voyage, Factory);
			AssertEquals("wrapperSea.Origin.UNLOCO", loco2, wrapperSea.Origin.UNLOCO);
			AssertEquals("wrapperSea.EstimatedDeparture", date0, wrapperSea.EstimatedDeparture);
			AssertEquals("wrapperSea.ActualDeparture", date4, wrapperSea.ActualDeparture);
			AssertEquals("wrapperSea.DateOfDeparture", date4, wrapperSea.DateOfDeparture);
			AssertEquals("wrapperSea.Destination.UNLOCO", loco3, wrapperSea.Destination.UNLOCO);
			AssertEquals("wrapperSea.EstimatedArrival", date1, wrapperSea.EstimatedArrival);
			AssertEquals("wrapperSea.ActualArrival", date0, wrapperSea.ActualArrival);
			AssertEquals("wrapperSea.DateOfArrival", date1, wrapperSea.DateOfArrival);
			AssertEquals("wrapperSea.AvailabilityDate", date5, wrapperSea.AvailabilityDate);
			AssertEquals("wrapperSea.Carrier.CompanyCode", carrier2.OH_Code, wrapperSea.Carrier.CompanyCode);
			AssertEquals("wrapperSea.Creditor.CompanyCode", creditor2.OH_Code, wrapperSea.Creditor.CompanyCode);

			AssertEquals("wrapperSea.CarriersReference", ref1, wrapperSea.CarriersReference);
			AssertEquals("wrapperSea.LegNo", 2, wrapperSea.LegNo);
			AssertEquals("wrapperSea.IsCargoOnly", ZBool.True, wrapperSea.IsCargoOnly);

			AssertEquals("SEA - Sea Freight", wrapperSea.Transport.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperSea.Transport.FlightDate);
			AssertEquals(ZString.Empty, wrapperSea.Transport.FlightNo);
			AssertEquals("GOOD SHIP LOLLYPOP / 2324", wrapperSea.Transport.Reference);
			AssertEquals("Vessel / Voyage / IMO(Lloyds)", wrapperSea.Transport.ReferenceLabel);
			AssertEquals("GOOD SHIP LOLLYPOP", wrapperSea.Transport.VesselName);
			AssertEquals("2324", wrapperSea.Transport.VoyageNo);
		}

		public void TestWrapperMappingsRoad()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			RouteWrapper wrapperRoad = new RouteWrapper(3, road, ZString.Empty, "ONE", "TWO", loco3, null, date3, date0, loco1, null, date0, date4, date5,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				carrier1, creditor1, ref2, voyage, Factory);
			AssertEquals("wrapperRoad.Origin.UNLOCO", loco3, wrapperRoad.Origin.UNLOCO);
			AssertEquals("wrapperRoad.EstimatedDeparture", date3, wrapperRoad.EstimatedDeparture);
			AssertEquals("wrapperRoad.ActualDeparture", date0, wrapperRoad.ActualDeparture);
			AssertEquals("wrapperRoad.DateOfDeparture", date3, wrapperRoad.DateOfDeparture);
			AssertEquals("wrapperRoad.Destination.UNLOCO", loco1, wrapperRoad.Destination.UNLOCO);
			AssertEquals("wrapperRoad.EstimatedArrival", date0, wrapperRoad.EstimatedArrival);
			AssertEquals("wrapperRoad.ActualArrival", date4, wrapperRoad.ActualArrival);
			AssertEquals("wrapperRoad.DateOfArrival", date4, wrapperRoad.DateOfArrival);
			AssertEquals("wrapperRoad.AvailabilityDate", date5, wrapperRoad.AvailabilityDate);
			AssertEquals("wrapperRoad.Carrier.CompanyCode", carrier1.OH_Code, wrapperRoad.Carrier.CompanyCode);
			AssertEquals("wrapperRoad.Creditor.CompanyCode", creditor1.OH_Code, wrapperRoad.Creditor.CompanyCode);
			AssertEquals("wrapperRoad.CarriersReference", ref2, wrapperRoad.CarriersReference);
			AssertEquals("wrapperRoad.LegNo", 3, wrapperRoad.LegNo);
			AssertEquals("wrapperRoad.IsCargoOnly", ZBool.True, wrapperRoad.IsCargoOnly);

			AssertEquals("ROA - Road", wrapperRoad.Transport.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperRoad.Transport.FlightDate);
			AssertEquals(ZString.Empty, wrapperRoad.Transport.FlightNo);
			AssertEquals("ONE / TWO / 03-Jan", wrapperRoad.Transport.Reference);
			AssertEquals("Road Reference", wrapperRoad.Transport.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperRoad.Transport.VesselName);
			AssertEquals(ZString.Empty, wrapperRoad.Transport.VoyageNo);
		}

		public void TestWrapperMappingsRail()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			RouteWrapper wrapperRail = new RouteWrapper(4, rail, ZString.Empty, "THREE", "FOUR", loco2, null, date4, date3, loco1, null, date4, date4, date5,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				carrier2, creditor2, ref2, voyage, Factory);
			AssertEquals("wrapperRail.Origin.UNLOCO", loco2, wrapperRail.Origin.UNLOCO);
			AssertEquals("wrapperRail.EstimatedDeparture", date4, wrapperRail.EstimatedDeparture);
			AssertEquals("wrapperRail.ActualDeparture", date3, wrapperRail.ActualDeparture);
			AssertEquals("wrapperRail.DateOfDeparture", date3, wrapperRail.DateOfDeparture);
			AssertEquals("wrapperRail.Destination.UNLOCO", loco1, wrapperRail.Destination.UNLOCO);
			AssertEquals("wrapperRail.EstimatedArrival", date4, wrapperRail.EstimatedArrival);
			AssertEquals("wrapperRail.ActualArrival", date4, wrapperRail.ActualArrival);
			AssertEquals("wrapperRail.DateOfArrival", date4, wrapperRail.DateOfArrival);
			AssertEquals("wrapperRail.AvailabilityDate", date5, wrapperRail.AvailabilityDate);
			AssertEquals("wrapperRail.Carrier.CompanyCode", carrier2.OH_Code, wrapperRail.Carrier.CompanyCode);
			AssertEquals("wrapperRail.Creditor.CompanyCode", creditor2.OH_Code, wrapperRail.Creditor.CompanyCode);
			AssertEquals("wrapperRail.CarriersReference", ref2, wrapperRail.CarriersReference);
			AssertEquals("wrapperRoad.LegNo", 4, wrapperRail.LegNo);
			AssertEquals("wrapperRoad.IsCargoOnly", ZBool.True, wrapperRail.IsCargoOnly);

			AssertEquals("RAI - Rail", wrapperRail.Transport.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperRail.Transport.FlightDate);
			AssertEquals(ZString.Empty, wrapperRail.Transport.FlightNo);
			AssertEquals("THREE / FOUR / 03-Jan", wrapperRail.Transport.Reference);
			AssertEquals("Rail Reference", wrapperRail.Transport.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperRail.Transport.VesselName);
			AssertEquals(ZString.Empty, wrapperRail.Transport.VoyageNo);
		}

		public void TestWrapperMappingsUnknown()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = ZBool.True;
			RouteWrapper wrapperUnknown = new RouteWrapper(5, "UNKNOWN", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null,
				ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, null, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				null, null, ZString.Empty, voyage, Factory);
			AssertEquals("wrapperUnknown.Origin.UNLOCO", ZString.Empty, wrapperUnknown.Origin.UNLOCO);
			AssertEquals("wrapperUnknown.EstimatedDeparture", ZDateTime.Empty, wrapperUnknown.EstimatedDeparture);
			AssertEquals("wrapperUnknown.ActualDeparture", ZDateTime.Empty, wrapperUnknown.ActualDeparture);
			AssertEquals("wrapperUnknown.DateOfDeparture", ZDateTime.Empty, wrapperUnknown.DateOfDeparture);
			AssertEquals("wrapperUnknown.Destination.UNLOCO", ZString.Empty, wrapperUnknown.Destination.UNLOCO);
			AssertEquals("wrapperUnknown.EstimatedArrival", ZDateTime.Empty, wrapperUnknown.EstimatedArrival);
			AssertEquals("wrapperUnknown.ActualArrival", ZDateTime.Empty, wrapperUnknown.ActualArrival);
			AssertEquals("wrapperUnknown.DateOfArrival", ZDateTime.Empty, wrapperUnknown.DateOfArrival);
			AssertEquals("wrapperUnknown.AvailabilityDate", ZDateTime.Empty, wrapperUnknown.AvailabilityDate);
			AssertEquals("wrapperUnknown.Carrier.CompanyCode", ZString.Empty, wrapperUnknown.Carrier.CompanyCode);
			AssertEquals("wrapperUnknown.Creditor.CompanyCode", ZString.Empty, wrapperUnknown.Creditor.CompanyCode);
			AssertEquals("wrapperUnknown.CarriersReference", ZString.Empty, wrapperUnknown.CarriersReference);
			AssertEquals("wrapperUnknown.LegNo", 5, wrapperUnknown.LegNo);
			AssertEquals("wrapperUnknown.IsCargoOnly", ZBool.True, wrapperUnknown.IsCargoOnly);

			AssertEquals("UNKNOWN", wrapperUnknown.Transport.Mode.ToString());
			AssertEquals(ZDateTime.Empty, wrapperUnknown.Transport.FlightDate);
			AssertEquals(ZString.Empty, wrapperUnknown.Transport.FlightNo);
			AssertEquals(ZString.Empty, wrapperUnknown.Transport.Reference);
			AssertEquals(ZString.Empty, wrapperUnknown.Transport.ReferenceLabel);
			AssertEquals(ZString.Empty, wrapperUnknown.Transport.VesselName);
			AssertEquals(ZString.Empty, wrapperUnknown.Transport.VoyageNo);
		}

		public void TestVesselVoyageOriginDestinationConstructor()
		{
			RouteWrapper wrapper = new RouteWrapper(9, Core.Constants.TransportModes.Sea, "INDO BOAT", "9999", "AUSYD", "USLAX",
				new ZDateTime(2007, 3, 2), new ZDateTime(2007, 9, 4), Factory);

			AssertEquals("wrapper.LegNo", 9, wrapper.LegNo);
			AssertEquals("wrapper.Transport.VesselName", "INDO BOAT", wrapper.Transport.VesselName);
			AssertEquals("wrapper.Transport.VoyageNo", "9999", wrapper.Transport.VoyageNo);
			AssertEquals("wrapper.Origin.UNLOCO", "AUSYD", wrapper.Origin.UNLOCO);
			AssertEquals("wrapper.Destination.UNLOCO", "USLAX", wrapper.Destination.UNLOCO);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 3, 2), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2007, 9, 4), wrapper.EstimatedArrival);
			AssertEquals("wrapper.Transport.Mode.Code", "SEA", wrapper.Transport.Mode.Code);
		}

		public void TestAirContructor()
		{
			RouteWrapper wrapper = new RouteWrapper(9, Core.Constants.TransportModes.Air, ZString.Empty, "QF453", "AUMEL", "USCHI",
				new ZDateTime(2007, 5, 9), new ZDateTime(2007, 7, 8), Factory);

			AssertEquals("wrapper.LegNo", 9, wrapper.LegNo);
			AssertEquals("wrapper.Transport.FlightNo", "QF453", wrapper.Transport.FlightNo);
			AssertEquals("wrapper.Origin.UNLOCO", "AUMEL", wrapper.Origin.UNLOCO);
			AssertEquals("wrapper.Destination.UNLOCO", "USCHI", wrapper.Destination.UNLOCO);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 5, 9), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2007, 7, 8), wrapper.EstimatedArrival);
			AssertEquals("wrapper.Transport.FlightDate", new ZDateTime(2007, 5, 9), wrapper.Transport.FlightDate);
			AssertEquals("wrapper.Transport.Mode.Code", "AIR", wrapper.Transport.Mode.Code);
		}

		public void TestDocAddresses()
		{
			JobDocAddress address1 = Factory.New<JobDocAddress>();
			address1.E2_CompanyName = "TEST 1 COMPANY";
			address1.E2_Address1 = "TEST 1";

			JobDocAddress address2 = Factory.New<JobDocAddress>();
			address2.E2_CompanyName = "TEST 2 COMPANY";
			address2.E2_Address1 = "TEST 2";

			RouteWrapper wrapper = new RouteWrapper(45, address1, address2, new ZDateTime(2007, 8, 9), new ZDateTime(2007, 10, 11), Factory);
			AssertEquals("wrapper.OriginAddress.CompanyName", "TEST 1 COMPANY", wrapper.OriginAddress.CompanyName);
			AssertEquals("wrapper.OriginAddress.Address", "TEST 1", wrapper.OriginAddress.Address);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 8, 9), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.DestinationAddress.CompanyName", "TEST 2 COMPANY", wrapper.DestinationAddress.CompanyName);
			AssertEquals("wrapper.DestinationAddress.Address", "TEST 2", wrapper.DestinationAddress.Address);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2007, 10, 11), wrapper.EstimatedArrival);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestCarrier()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Consol Carrier";

			OrgHeader carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_FullName = "Routing Carrier";
			carrier2.Addresses[0].Address1 = "Routing Carrier Address 1";

			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "SEA";

			RouteWrapper wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("No Carrier defined", "", wrapper.Carrier.CompanyName);

			consol.JK_OA_ShippingLineAddress = carrier.Addresses.AddNew().PK;

			wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("Should not fall back to consol carrier", "", wrapper.Carrier.CompanyName);

			transport.CarrierPK = carrier2.PK;
			transport.JW_OA_CarrierAddress = carrier2.Addresses[0].PK;

			wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("Routing Carrier", wrapper.Carrier.CompanyName);
			AssertEquals("Routing Carrier Address 1", wrapper.CarrierAddress.AddressLine1);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestCreditor()
		{
			OrgHeader consolCreditor = Factory.NewWithValidTestData<OrgHeader>();
			consolCreditor.OH_FullName = "Consol Creditor";

			OrgHeader routeCreditor = Factory.NewWithValidTestData<OrgHeader>();
			routeCreditor.OH_FullName = "Routing Creditor";

			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "SEA";

			RouteWrapper wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("No Creditor defined", "", wrapper.Creditor.CompanyName);

			consol.JK_OA_CreditorAddress = consolCreditor.Addresses.AddNew().PK;

			wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("Should not fall back to consol creditor", "", wrapper.Creditor.CompanyName);

			transport.CreditorPK = routeCreditor.PK;

			wrapper = new RouteWrapper(transport, Factory);
			AssertEquals("Routing Creditor", wrapper.Creditor.CompanyName);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Route                                       (Default Field: Transport)
======================================================================
Name                                    Type
----------------------------------------------------------------------
CarrierAddress                          Address
DestinationAddress                      Address
OriginAddress                           Address
ServiceLevel                            CodeAndDescription
TransportType                           CodeAndDescription
Destination                             Location
Origin                                  Location
Carrier                                 Organisation
Creditor                                Organisation
DestinationOrganisation                 Organisation
OriginOrganisation                      Organisation
Transport                               Transport
ActualArrival                           DateTime
ActualDeparture                         DateTime
AdditionalTransportMode                 String
AvailabilityDate                        DateTime
CarriersReference                       String
DateOfArrival                           DateTime
DateOfDeparture                         DateTime
DGFCLCutOff                             DateTime
DGFCLReceivalCommences                  DateTime
EstimatedArrival                        DateTime
EstimatedDeparture                      DateTime
FCLCutOff                               DateTime
FCLReceivalCommences                    DateTime
FCLStorageCommences                     DateTime
ForceShowVesselVoyage                   Bool
IsCargoOnly                             Bool
IsInternational                         Bool
LCLAvailabilityDate                     DateTime
LCLCutOff                               DateTime
LCLReceivalCommences                    DateTime
LCLStorageCommences                     DateTime
LegNo                                   Int
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Carrier : CARRY ME HOME\nAUSTRALIA
CarrierAddress :  is null
Creditor : PAY ME\nAUSTRALIA
Destination : AUSYD - Sydney
DestinationAddress : TEST 2 COMPANY\nTEST 2\nAUSTRALIA
DestinationOrganisation : TEST 2 COMPANY\nTEST 2\nAUSTRALIA
Origin : NZAKL - Auckland
OriginAddress : TEST 1 COMPANY\nTEST 1\nAUSTRALIA
OriginOrganisation : TEST 1 COMPANY\nTEST 1\nAUSTRALIA
Registry : (No Default Field Value Available on Registry)
ServiceLevel : 
Transport : VESSELFFF / GROUND
TransportType :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var loadOrg = Factory.New<OrgHeader>();
			loadOrg.OH_FullName = "TEST 1 COMPANY";
			loadOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			JobDocAddress loadAddress = Factory.New<JobDocAddress>();
			loadAddress.OrganisationPK = loadOrg.PK;
			loadAddress.Address.OA_Address1 = "TEST 1";

			var destOrg = Factory.New<OrgHeader>();
			destOrg.OH_FullName = "TEST 2 COMPANY";
			destOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			JobDocAddress destAddress = Factory.New<JobDocAddress>();
			destAddress.OrganisationPK = destOrg.PK;
			destAddress.Address.OA_Address1 = "TEST 2";

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRY ME HOME";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "PAY ME";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			return new RouteWrapper(1, "SEA", ZString.Empty, "VESSELFFF", "GROUND",
				"NZAKL", loadAddress, ZDateTime.Empty, ZDateTime.Empty,
				"AUSYD", destAddress, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty,
				carrier, creditor, ZString.Empty, null, Factory);
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RouteWrapper(ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null,
				ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, null, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				null, null, ZString.Empty, null, Factory);
		}

		readonly ZString air = TransportModeList.Codes.Airfreight;
		readonly ZString sea = TransportModeList.Codes.Seafreight;
		readonly ZString road = TransportModeList.Codes.Road;
		readonly ZString rail = TransportModeList.Codes.Rail;

		readonly ZDateTime date0 = ZDateTime.Empty;
		readonly ZDateTime date1 = new ZDateTime(2006, 1, 1);
		readonly ZDateTime date2 = new ZDateTime(2006, 1, 2);
		readonly ZDateTime date3 = new ZDateTime(2006, 1, 3);
		readonly ZDateTime date4 = new ZDateTime(2006, 1, 4);
		readonly ZDateTime date5 = new ZDateTime(2006, 1, 5);

		const string loco1 = "AUSYD";
		const string loco2 = "NZAKL";
		const string loco3 = "USLAX";

		OrgHeader carrier1
		{
			get
			{
				if (fcarrier1 == null)
				{
					fcarrier1 = Factory.New<OrgHeader>();
					fcarrier1.FillWithValidTestData();
					fcarrier1.OH_Code = "CARRIER1";
				}
				return fcarrier1;
			}
		}
		OrgHeader fcarrier1;

		OrgHeader carrier2
		{
			get
			{
				if (fcarrier2 == null)
				{
					fcarrier2 = Factory.New<OrgHeader>();
					fcarrier2.FillWithValidTestData();
					fcarrier2.OH_Code = "CARRIER2";
				}
				return fcarrier2;
			}
		}
		OrgHeader fcarrier2;

		const string ref1 = "CARRIER_REF_1";
		const string ref2 = "CARRIER_REF_2";

		OrgHeader creditor1
		{
			get
			{
				if (newCreditor1 == null)
				{
					newCreditor1 = Factory.New<OrgHeader>();
					newCreditor1.FillWithValidTestData();
					newCreditor1.OH_Code = "CREDITOR1";
				}
				return newCreditor1;
			}
		}

		OrgHeader newCreditor1;

		OrgHeader creditor2
		{
			get
			{
				if (newCreditor2 == null)
				{
					newCreditor2 = Factory.New<OrgHeader>();
					newCreditor2.FillWithValidTestData();
					newCreditor2.OH_Code = "CREDITOR2";
				}
				return newCreditor2;
			}
		}
		OrgHeader newCreditor2;

		#endregion
	}
}
