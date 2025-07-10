using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Routing = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RouteWrapperCollection))]
	public class RouteWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<RouteWrapperCollection>
	{
		public void TestLoadFromContainer()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			JobSailing sailing = Factory.New<JobSailing>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = new ZDateTime(2011, 6, 15);
			sailing.JX_JA = origin.PK;
			container.JC_JX = sailing.PK;

			RouteWrapperCollection collection = new RouteWrapperCollection(container, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertEquals("collection[0].Origin.UNLOCO", "NZAKL", collection[0].Origin.UNLOCO);
			AssertEquals("collection[0].EstimatedDeparture", new ZDateTime(2011, 6, 15), collection[0].EstimatedDeparture);
		}

		public void TestLoadFromOrderWithNoVesselVoyage()
		{
			Order order = Factory.New<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "GBLON";
			order.JD_Milestone_E_DEP = new ZDateTime(2011, 6, 5);
			order.JD_Milestone_E_ARV = new ZDateTime(2011, 6, 20);

			RouteWrapperCollection routes = new RouteWrapperCollection(order, RoutingLevel.Shipment, Factory);
			AssertEquals("routes.Count", 1, routes.Count);
			AssertEquals("routes[0].TransportMode", Constants.TransportModes.Sea, routes[0].Transport.Mode.Code);
			AssertEquals("routes[0].Origin", "AUSYD", routes[0].Origin.UNLOCO);
			AssertEquals("routes[0].Destination", "GBLON", routes[0].Destination.UNLOCO);
			AssertEquals("routes[0].DateOfDeparture", new ZDateTime(2011, 6, 5), routes[0].DateOfDeparture);
			AssertEquals("routes[0].DateOfArrival", new ZDateTime(2011, 6, 20), routes[0].DateOfArrival);
		}

		public virtual void TestLoadFromOrder()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			Order order = Factory.New<Order>();

			order.JD_RV_NKDepartureVessel = "TITANIC";
			order.JD_DepartureVoyage = "111";
			order.JD_Milestone_E_DEP = new ZDateTime(2011, 1, 1);
			order.JD_E_ARV_1stIntermediate = new ZDateTime(2011, 1, 31);

			order.JD_RV_NKIntermediateVessel = "BLACK PEARL";
			order.JD_IntermediateVoyage = "222";
			order.JD_E_DEP_2 = new ZDateTime(2011, 2, 1);
			order.JD_E_ARV_2ndIntermediate = new ZDateTime(2011, 2, 28);

			order.JD_RV_NKArrivalVessel = "JUGGERNAUT";
			order.JD_ArrivalVoyage = "333";
			order.JD_E_DEP_3 = new ZDateTime(2011, 3, 1);
			order.JD_Milestone_E_ARV = new ZDateTime(2011, 3, 31);

			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";

			RouteWrapperCollection routes = new RouteWrapperCollection(order, RoutingLevel.Consol, Factory);
			AssertEquals("routes.Count", 3, routes.Count);
			AssertEquals("routes[0].Transport", "TITANIC / 111 / 01-Jan", routes[0].Transport.Reference);
			AssertEquals("routes[0].EstimatedDeparture", new ZDateTime(2011, 1, 1), routes[0].EstimatedDeparture);
			AssertEquals("routes[0].EstimatedArrival", new ZDateTime(2011, 1, 31), routes[0].EstimatedArrival);
			AssertEquals("routes[0].Origin.UNLOCO", "AUSYD", routes[0].Origin.UNLOCO);
			AssertEquals("routes[1].Transport", "BLACK PEARL / 222 / 01-Feb", routes[1].Transport.Reference);
			AssertEquals("routes[1].EstimatedDeparture", new ZDateTime(2011, 2, 1), routes[1].EstimatedDeparture);
			AssertEquals("routes[1].EstimatedArrival", new ZDateTime(2011, 2, 28), routes[1].EstimatedArrival);
			AssertEquals("routes[2].Transport", "JUGGERNAUT / 333 / 01-Mar", routes[2].Transport.Reference);
			AssertEquals("routes[2].EstimatedDeparture", new ZDateTime(2011, 3, 1), routes[2].EstimatedDeparture);
			AssertEquals("routes[2].EstimatedArrival", new ZDateTime(2011, 3, 31), routes[2].EstimatedArrival);
			AssertEquals("routes[2].Destination.UNLOCO", "USLAX", routes[2].Destination.UNLOCO);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "HKHKG";

			routes = new RouteWrapperCollection(order, RoutingLevel.Consol, Factory);
			AssertEquals("routes.Count", 1, routes.Count);
			AssertEquals("routes[0].Origin.UNLOCO", "AUBNE", routes[0].Origin.UNLOCO);
			AssertEquals("routes[0].Destination.UNLOCO", "HKHKG", routes[0].Destination.UNLOCO);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			order.JD_JS = shipment.PK;
			Routing transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "GBLON";

			routes = new RouteWrapperCollection(order, RoutingLevel.Shipment, Factory);
			AssertEquals("routes.Count", 1, routes.Count);
			AssertEquals("routes[0].Origin.UNLOCO", "NZAKL", routes[0].Origin.UNLOCO);
			AssertEquals("routes[0].Destination.UNLOCO", "GBLON", routes[0].Destination.UNLOCO);
		}

		public void TestLoadFromQuotedBooking()
		{
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x42";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = new ZDateTime(2013, 08, 02);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2013, 08, 04);

			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USDAN";
			shipment.JS_JX = voyage.Sailings[0].PK;

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_Vessel = "BUNGA DELIMA";
			transport1.JW_VoyageFlight = "343";
			transport1.JW_RL_NKLoadPort = "NZCHC";
			transport1.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_ETD = new ZDateTime(2013, 08, 07);
			transport1.JW_ETA = new ZDateTime(2013, 08, 09);

			RouteWrapperCollection collection = new RouteWrapperCollection(booking, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
		}

		public void TestLoadFromQuotedBooking_NoValidSailing()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);

			shipment.JS_RL_NKLoadPort = "AUBNE";
			shipment.JS_RL_NKDischargePort = "USLAX";

			var collection = new RouteWrapperCollection(booking, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
		}

		public void TestLoadFromQuotedBooking_NoValidSailing_LoadAndDischargeEmpty()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var booking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);

			AssertEquals("Precondition: Load Port is empty", ZString.Empty, shipment.JS_RL_NKLoadPort);
			AssertEquals("Precondition: Discharge Port is empty", ZString.Empty, shipment.JS_RL_NKDischargePort);

			var collection = new RouteWrapperCollection(booking, Factory);
			AssertEquals("collection.Count", 0, collection.Count);
		}

		public void TestThatVesselAndVoyageInformationAppearOnDocBuilderCartageAdviceForShipmentDeclarationExport()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			ZQuery localCountryQuery = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			RefUNLOCO localPort = Factory.LoadTop1<RefUNLOCO>(localCountryQuery);
			ZQuery foreignCountryQuery = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			RefUNLOCO foreignPort = Factory.LoadTop1<RefUNLOCO>(foreignCountryQuery);
			ZQuery otherLocalPortQuery = new ZQuery(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, localPort.PK);
			otherLocalPortQuery.AddToFilter(localCountryQuery);
			RefUNLOCO localPort2 = Factory.LoadTop1<RefUNLOCO>(otherLocalPortQuery);
			ZQuery otherForeignPortQuery = new ZQuery(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, foreignPort.PK);
			otherForeignPortQuery.AddToFilter(foreignCountryQuery);
			RefUNLOCO foreignPort2 = Factory.LoadTop1<RefUNLOCO>(otherForeignPortQuery);

			OrgHeader localOrg = GetOrgHeader("LOCAL NAGARE");
			localOrg.OH_RL_NKClosestPort = localPort.RL_Code;
			OrgHeader foreignOrg = GetOrgHeader("FOREIGN NAGARE");
			foreignOrg.OH_RL_NKClosestPort = foreignPort.RL_Code;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = "CSH";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localOrg.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = foreignOrg.PK;

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = localPort.RL_Code;
			consol.JK_RL_NKDischargePort = foreignPort.RL_Code;
			consol.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2011, 03, 12, 13, 18, 20);
			consol.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2011, 03, 12, 13, 18, 20);

			consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Auto-created by consol data", 1, consol.Transports.Count);
			Routing leg1 = UpdateTransport(consol.Transports[0], 1, "VESSEL 1", "V1", foreignPort.RL_Code, localPort.RL_Code);
			Routing leg2 = UpdateTransport(consol.Transports.AddNew(), 2, "VESSEL 2", "V2", localPort.RL_Code, localPort2.RL_Code);
			Routing leg3 = UpdateTransport(consol.Transports.AddNew(), 3, "VESSEL 3", "V3", localPort2.RL_Code, foreignPort2.RL_Code);
			Routing leg4 = UpdateTransport(consol.Transports.AddNew(), 4, "VESSEL 4", "V4", foreignPort2.RL_Code, foreignPort.RL_Code);

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "DEAN5426450";
			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			AssertNotNull("Precondition: Should have been able to find a Container Type with a Code of: 20GP", containerType);
			container.JC_RC = containerType.PK;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;

			FreightWrapper genericFreightJobFromShipment = FreightWrapper.New(shipment, Factory)[0];

			AssertNotNull("Precondition: genericFreightJobFromShipment", genericFreightJobFromShipment);
			AssertNotNull("Precondition: genericFreightJobFromShipment.ConsolRoutes[MostInteresting]", genericFreightJobFromShipment.ConsolRoutes["MostInteresting"]);
			AssertNotNull("Precondition: genericFreightJobFromShipment.ConsolRoutes[\"MostInteresting\"].Transport", genericFreightJobFromShipment.ConsolRoutes["MostInteresting"].Transport);
			AssertEquals("MostInteresting Transport from Shipment", "VESSEL 1 / V1", genericFreightJobFromShipment.ConsolRoutes["MostInteresting"].Transport.ToString());

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			// Scenario 1: Match Load Port
			AssertEquals("Precondition: declaration.IsExport", true, declaration.IsExport);
			AssertEquals("Precondition: declaration.JE_RL_NKPortOfLoading", localPort2.RL_Code, declaration.JE_RL_NKPortOfLoading);

			FreightWrapper genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			var mostInterestingLeg = genericFreightJobFromDeclaration.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 3, localPort2.RL_Code, foreignPort2.RL_Code, "VESSEL 3 / V3");

			genericFreightJobFromShipment = FreightWrapper.New(shipment, Factory)[0];
			mostInterestingLeg = genericFreightJobFromShipment.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 3, localPort2.RL_Code, foreignPort2.RL_Code, "VESSEL 3 / V3");

			// Scenario 2: Match to first international port from country of export 
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			declaration.JE_RL_NKPortOfLoading = localPort.RL_Code;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertEquals("Precondition: declaration.IsExport", true, declaration.IsExport);
			AssertEquals("Precondition: declaration.JE_RL_NKPortOfLoading", localPort.RL_Code, declaration.JE_RL_NKPortOfLoading);

			genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			mostInterestingLeg = genericFreightJobFromDeclaration.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 2, localPort.RL_Code, localPort2.RL_Code, "VESSEL 2 / V2");

			genericFreightJobFromShipment = FreightWrapper.New(shipment, Factory)[0];
			mostInterestingLeg = genericFreightJobFromShipment.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 2, localPort.RL_Code, localPort2.RL_Code, "VESSEL 2 / V2");

			// Scenario 3: Match to first interantional
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertEquals("Precondition: declaration.IsExport", true, declaration.IsExport);
			AssertEquals("Precondition: declaration.JE_RL_NKPortOfLoading", ZString.Empty, declaration.JE_RL_NKPortOfLoading);

			genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			mostInterestingLeg = genericFreightJobFromDeclaration.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 1, foreignPort.RL_Code, localPort.RL_Code, "VESSEL 1 / V1");

			genericFreightJobFromShipment = FreightWrapper.New(shipment, Factory)[0];
			mostInterestingLeg = genericFreightJobFromShipment.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 1, foreignPort.RL_Code, localPort.RL_Code, "VESSEL 1 / V1");

			// Scenario 4 - One leg
			consol.Transports.RemoveAndDeleteAll();

			AssertEquals(1, consol.Transports.Count);
			Routing onlyLeg = consol.Transports[0];
			onlyLeg.JW_RL_NKLoadPort = foreignPort.RL_Code;
			onlyLeg.JW_RL_NKDiscPort = foreignPort2.RL_Code;
			onlyLeg.JW_Vessel = "VESSEL 1";
			onlyLeg.JW_VoyageFlight = "V1";

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertEquals("Precondition: declaration.IsExport", true, declaration.IsExport);

			genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			mostInterestingLeg = genericFreightJobFromDeclaration.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 1, foreignPort.RL_Code, foreignPort2.RL_Code, "VESSEL 1 / V1");

			genericFreightJobFromShipment = FreightWrapper.New(shipment, Factory)[0];
			mostInterestingLeg = genericFreightJobFromShipment.ConsolRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 1, foreignPort.RL_Code, foreignPort2.RL_Code, "VESSEL 1 / V1");
		}

		public void TestStandAloneAUImportDeclarationWithMultipleLegs()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "AUBNE";
			declaration.JE_RL_NKPortOfFirstArrival = "AUBNE";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "AUMEL";

			declaration.Transports.RemoveAndDeleteAll();
			declaration.Transports.AddNew();
			UpdateTransport(declaration.Transports[0], 1, "MSC Vaiga III", "V1", "NZAKL", "AUBNE");
			declaration.Transports.AddNew();
			UpdateTransport(declaration.Transports.AddNew(), 2, "PHO", "V2", "AUBNE", "AUSYD");
			declaration.Transports.AddNew();
			UpdateTransport(declaration.Transports.AddNew(), 3, "MSC Sandy", "V3", "AUSYD", "AUMEL");

			AssertEquals("Precondition: declaration.IsImport", true, declaration.IsImport);
			AssertEquals("Precondition: declaration.JE_RL_NKPortOfLoading", "NZAKL", declaration.JE_RL_NKPortOfLoading);

			var genericFreightJobFromDeclaration = FreightWrapper.New(declaration, Factory)[0];
			var mostInterestingLeg = genericFreightJobFromDeclaration.ShipmentRoutes["MostInteresting"];
			AssertMostInterestingLeg(mostInterestingLeg, 3, "AUSYD", "AUMEL", "MSC Sandy / V3");
		}

		void AssertMostInterestingLeg(RouteWrapper mostInterestingLeg, ZInt legNo, ZString origin, ZString destination, ZString vesselData)
		{
			CombineAssertions(delegate
			{
				AssertEquals("MostInteresting LegOrder", legNo, mostInterestingLeg.LegNo);
				AssertEquals("MostInteresting Origin", origin, mostInterestingLeg.Origin.UNLOCO);
				AssertEquals("MostInteresting Destination", destination, mostInterestingLeg.Destination.UNLOCO);
				AssertEquals("MostInteresting Transport from Declaration", vesselData, mostInterestingLeg.Transport.ToString());
			});
		}

		Routing UpdateTransport(Routing transport, int legNo, ZString vessel, ZString voyage, ZString loadPort, ZString discharge)
		{
			transport.JW_LegOrder = (byte)legNo;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyage;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discharge;
			return transport;
		}

		public void TestRoutesFromConsolLinkedShipmentWithDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZCHC";
			shipment.JS_RL_NKDestination = "USDAN";

			ForwardingConsol consol = shipment.Consols.AddNew();

			Routing transport = consol.Transports.Count == 1 ? consol.Transports[0] : consol.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = "BUNGA DELIMA";
			transport.JW_VoyageFlight = "343";
			transport.JW_ETD = new ZDateTime(2009, 4, 1);
			transport.JW_ETA = new ZDateTime(2009, 4, 5);
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "USLAX";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "NZCHC";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.JE_RL_NKFinalDestination = "USDAN";

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("Only One Leg 'MostInteresting'", collection[0], 1, "NZAKL", new ZDateTime(2009, 4, 1), "USLAX", new ZDateTime(2009, 4, 5));
		}

		public void TestCustomsDischargeAndCustomsLoadForDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUADL";
				shipment.JS_RL_NKDestination = "USCHI";

				var consol = shipment.Consols.AddNew();

				var transport = consol.Transports.Count == 1 ? consol.Transports[0] : consol.Transports.AddNew();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF343";
				transport.JW_ETD = new ZDateTime(2009, 4, 1);
				transport.JW_ETA = new ZDateTime(2009, 4, 2);
				transport.JW_RL_NKLoadPort = "AUADL";
				transport.JW_RL_NKDiscPort = "AUSYD";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_VoyageFlight = "QF345";
				transport2.JW_ETD = new ZDateTime(2009, 4, 2);
				transport2.JW_ETA = new ZDateTime(2009, 4, 3);
				transport2.JW_RL_NKLoadPort = "AUSYD";
				transport2.JW_RL_NKDiscPort = "USLAX";

				var transport3 = consol.Transports.AddNew();
				transport3.JW_LegOrder = 2;
				transport3.JW_TransportMode = Constants.TransportModes.Air;
				transport3.JW_VoyageFlight = "QF346";
				transport3.JW_ETD = new ZDateTime(2009, 4, 3);
				transport3.JW_ETA = new ZDateTime(2009, 4, 4);
				transport3.JW_RL_NKLoadPort = "USLAX";
				transport3.JW_RL_NKDiscPort = "USCHI";

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.ShipmentSynchroniser.Synchronise();
				var collection = new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);

				AssertEquals("Customs Loading.Origin", "AUSYD", collection["customs loading"].Origin.UNLOCO);
				AssertEquals("Customs Discharge.Destination", "USLAX", collection["customs discharge"].Destination.UNLOCO);
			}
		}

		public void TestRoutesFromAgencyShipment()
		{
			ZDateTime now = ZDateTime.Now;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "x42";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = now.AddDays(3);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = now.AddDays(4);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "NZCHC";
			shipment.JS_RL_NKDestination = "USDAN";
			shipment.JS_JX = voyage.Sailings[0].PK;

			Routing transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_Vessel = "BUNGA DELIMA";
			transport1.JW_VoyageFlight = "343";
			transport1.JW_RL_NKLoadPort = "NZCHC";
			transport1.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_ETD = now.AddDays(1);
			transport1.JW_ETA = now.AddDays(2);

			Routing transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_Vessel = "CONDOR";
			transport2.JW_VoyageFlight = "y42";
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USDAN";
			transport2.JW_ETD = now.AddDays(5);
			transport2.JW_ETA = now.AddDays(6);

			RouteWrapperCollection collection = new RouteWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertLeg("Leg 1", collection[0], 0, "NZCHC", now.AddDays(1), "AUBNE", now.AddDays(2));
			AssertLeg("Leg 2", collection[1], 0, "AUBNE", now.AddDays(3), "SGSIN", now.AddDays(4));
			AssertLeg("Leg 3", collection[2], 0, "SGSIN", now.AddDays(5), "USDAN", now.AddDays(6));
		}

		public void TestMostInterestingTransportWorksForImportDecs()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 5);
			declaration.JE_RL_NKFinalDestination = "AUMEL";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 6);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals("Precondition: declaration.IsImport", true, declaration.IsImport);
			AssertEquals("Precondition: ", 1, declaration.Transports.Count);

			RouteWrapperCollection collection1 = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection1.Count);
			AssertLeg("Only One Leg 'MostInteresting'", collection1["MostInteresting"], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 5));

			Routing leg1 = declaration.Transports[0];
			Routing leg2 = declaration.Transports.AddNew();

			leg1.JW_ETD = new ZDateTime(2006, 3, 2);
			leg1.JW_RL_NKLoadPort = "USLAX";
			leg1.JW_ETA = new ZDateTime(2006, 3, 4);
			leg1.JW_RL_NKDiscPort = "NZAKL";

			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2006, 3, 4);
			leg2.JW_RL_NKLoadPort = "NZAKL";
			leg2.JW_ETA = new ZDateTime(2006, 3, 5);
			leg2.JW_RL_NKDiscPort = "AUSYD";

			RouteWrapperCollection collection2 = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 2, collection2.Count);
			AssertLeg("Only One Leg 'MostInteresting'", collection2["MostInteresting"], 2, "NZAKL", new ZDateTime(2006, 3, 4), "AUSYD", new ZDateTime(2006, 3, 5));
		}

		public void TestMostInterestingTransportWorksForExportDecs()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 5);
			declaration.JE_RL_NKFinalDestination = "AUMEL";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 6);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertEquals("Precondition: declaration.IsExport", true, declaration.IsExport);
			AssertEquals("Precondition: ", 1, declaration.Transports.Count);

			RouteWrapperCollection collection1 = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection1.Count);
			AssertLeg("Only One Leg 'MostInteresting'", collection1["MostInteresting"], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 5));

			Routing leg1 = declaration.Transports[0];
			Routing leg2 = declaration.Transports.AddNew();

			leg1.JW_ETD = new ZDateTime(2006, 3, 2);
			leg1.JW_RL_NKLoadPort = "USLAX";
			leg1.JW_ETA = new ZDateTime(2006, 3, 4);
			leg1.JW_RL_NKDiscPort = "NZAKL";

			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2006, 3, 4);
			leg2.JW_RL_NKLoadPort = "NZAKL";
			leg2.JW_ETA = new ZDateTime(2006, 3, 5);
			leg2.JW_RL_NKDiscPort = "AUSYD";

			RouteWrapperCollection collection2 = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 2, collection2.Count);
			AssertLeg("Only One Leg 'MostInteresting'", collection2["MostInteresting"], 1, "USLAX", new ZDateTime(2006, 3, 2), "NZAKL", new ZDateTime(2006, 3, 4));
		}

		public void TestRoutesDontShowUnlessYourDeclarationHasNoTransportLegsAndOriginAndDestinationsOfEachLegAreDifferent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USLAX";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			AssertLeg("One Leg", collection[0], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
		}

		public void TestLoadFromDeclarationAtConsolLevelWithMultipleTransports()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2009, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2009, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 3, 5);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2009, 3, 6);

			Routing leg1 = declaration.Transports[0];
			leg1.JW_ETA = new ZDateTime(2009, 3, 3);
			leg1.JW_RL_NKDiscPort = "NZAKL";

			//Add Second Leg first to stop back-updating linked Declaration.
			Routing leg2 = declaration.Transports.AddNew();
			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2009, 3, 4);
			leg2.JW_RL_NKLoadPort = "NZAKL";
			leg2.JW_ETA = new ZDateTime(2009, 3, 5);
			leg2.JW_RL_NKDiscPort = "AUSYD";

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertLeg("2 Legs", collection[0], 1, "USLAX", new ZDateTime(2009, 3, 2), "NZAKL", new ZDateTime(2009, 3, 3));
			AssertLeg("2 Legs", collection[1], 2, "NZAKL", new ZDateTime(2009, 3, 4), "AUSYD", new ZDateTime(2009, 3, 5));
		}

		public void TestLoadFromDeclarationAtShipmentLevelWithMultipleTransports()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2008, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2008, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2008, 3, 6);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2008, 3, 7);

			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);

			//Add Second Leg first to stop back-updating linked Declaration.
			Routing leg2 = declaration.Transports.AddNew();
			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2008, 3, 5);
			leg2.JW_RL_NKLoadPort = "NZCHC";
			leg2.JW_ETA = new ZDateTime(2008, 3, 6);
			leg2.JW_RL_NKDiscPort = "AUSYD";

			Routing leg1 = declaration.Transports[0];
			leg1.JW_ETA = new ZDateTime(2008, 3, 4);
			leg1.JW_RL_NKDiscPort = "NZCHC";

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
			AssertLeg("2 Legs", collection[0], 1, "USLAX", new ZDateTime(2008, 3, 2), "NZCHC", new ZDateTime(2008, 3, 4));
			AssertLeg("2 Legs", collection[1], 2, "NZCHC", new ZDateTime(2008, 3, 5), "AUSYD", new ZDateTime(2008, 3, 6));
		}

		public void TestLoadFromDeclarationAtConsolLevelWithDefaultTransport()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("1 Leg", collection[0], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
		}

		public void TestLoadFromDeclarationAtShipmentLevelWithDefaultTransport()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "AA001";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("1 Leg", collection[0], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
		}

		public void TestLoadFromDeclarationAtConsolLevelWithNoTransports()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("1 Leg", collection[0], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
		}

		public void TestLoadFromDeclarationAtShipmentLevelWithNoTransports()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertLeg("3 Legs", collection[0], 1, "USDNV", new ZDateTime(2006, 3, 1), "USLAX", new ZDateTime(2006, 3, 2));
			AssertLeg("3 Legs", collection[1], 2, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
			AssertLeg("3 Legs", collection[2], 3, "AUSYD", new ZDateTime(2006, 3, 3), "AUPER", new ZDateTime(2006, 3, 4));
		}

		public void TestImportExportIndexer()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();

				Routing transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "SGSIN";
				transport1.JW_LegOrder = 1;

				Routing transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "INBOM";
				transport2.JW_LegOrder = 2;

				Routing transport3 = consol.Transports.AddNew();
				transport3.JW_RL_NKLoadPort = "INBOM";
				transport3.JW_RL_NKDiscPort = "AUSYD";
				transport3.JW_LegOrder = 3;

				RouteWrapperCollection routeWrapperCollection = new RouteWrapperCollection(consol.Shipments.AddNew(), consol, Factory);

				AssertEquals("Import indexer", transport3.JW_LegOrder, routeWrapperCollection["Import"].LegNo);
				AssertEquals("Export indexer", transport1.JW_LegOrder, routeWrapperCollection["exPorT"].LegNo);
			}
		}

		public void TestMostInterestingIndexerFromShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.Transports.AddNew().JW_LegOrder = 1;
			consol.Transports.AddNew().JW_LegOrder = 2;
			consol.Transports.AddNew().JW_LegOrder = 3;
			AssertNotNull("MostInterestingTransport is not null", consol.Transports.MostInterestingTransport);

			RouteWrapperCollection collection = new RouteWrapperCollection(consol.Shipments.AddNew(), consol, Factory);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer", consol.Transports.MostInterestingTransport.JW_LegOrder, collection["MostInteresting"].LegNo);
		}

		public void TestMostInterestingIndexerFromAgencyShipment()
		{
			ZDateTime today = ZDateTime.Today;

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Admiral Freight";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "Voyage";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";
			origin.JA_E_DEP = today;
			origin.JA_ReceivalCommences = today.AddDays(10);
			origin.JA_CutOff = today.AddDays(11);
			origin.JA_DGReceivalCommences = today.AddDays(20);
			origin.JA_DGCutOff = today.AddDays(21);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = today.AddDays(1);

			JobSailing sailing = voyage.Sailings[0];

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			Routing transport = shipment.Transports[0];
			transport.JW_JX = sailing.PK;
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			RouteWrapperCollection collection = new RouteWrapperCollection(shipment, Factory);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer - VoyageNo", "Voyage", collection["MostInteresting"].Transport.VoyageNo);
			AssertEquals("MostInteresting indexer - VesselName", "Admiral Freight", collection["MostInteresting"].Transport.VesselName);
			AssertEquals("MostInteresting indexer - DateOfDeparture", today, collection["MostInteresting"].DateOfDeparture);
			AssertEquals("MostInteresting indexer - DateOfArrival", today.AddDays(1), collection["MostInteresting"].DateOfArrival);
			AssertEquals("MostInteresting indexer - Origin.UNLOCO", "HKHKG", collection["MostInteresting"].Origin.UNLOCO);
			AssertEquals("MostInteresting indexer - Destination.UNLOCO", "AUBNE", collection["MostInteresting"].Destination.UNLOCO);
			AssertEquals("MostInteresting indexer - FCLReceivalCommences", today.AddDays(10), collection["MostInteresting"].FCLReceivalCommences);
			AssertEquals("MostInteresting indexer - FCLCutOff", today.AddDays(11), collection["MostInteresting"].FCLCutOff);
			AssertEquals("MostInteresting indexer - DGFCLReceivalCommences", today.AddDays(20), collection["MostInteresting"].DGFCLReceivalCommences);
			AssertEquals("MostInteresting indexer - DGFCLCutOff", today.AddDays(21), collection["MostInteresting"].DGFCLCutOff);
		}

		public void TestMostInterestingIndexerFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.Transports.AddNew().JW_LegOrder = 1;
			consol.Transports.AddNew().JW_LegOrder = 2;
			consol.Transports.AddNew().JW_LegOrder = 3;
			AssertNotNull("MostInterestingTransport is not null", consol.Transports.MostInterestingTransport);

			RouteWrapperCollection collection = new RouteWrapperCollection(consol, Factory);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer", consol.Transports.MostInterestingTransport.JW_LegOrder, collection["MostInteresting"].LegNo);
		}

		public void TestMostInterestingIndexerFromDeclarationAtConsolLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer - LegNo", 1, collection["MostInteresting"].LegNo);
			AssertEquals("MostInteresting indexer - Origin.UNLOCO", "USLAX", collection["MostInteresting"].Origin.UNLOCO);
			AssertEquals("MostInteresting indexer - DateOfDeparture", new ZDateTime(2006, 3, 2), collection["MostInteresting"].DateOfDeparture);
			AssertEquals("MostInteresting indexer - ActualDeparture", new ZDateTime(2006, 3, 2), collection["MostInteresting"].ActualDeparture);
			AssertEquals("MostInteresting indexer - Destination.UNLOCO", "AUSYD", collection["MostInteresting"].Destination.UNLOCO);
			AssertEquals("MostInteresting indexer - DateOfArrival", new ZDateTime(2006, 3, 3), collection["MostInteresting"].DateOfArrival);
			AssertEquals("MostInteresting indexer - ActualArrival", new ZDateTime(2006, 3, 3), collection["MostInteresting"].ActualArrival);
		}

		public void TestMostInterestingIndexerFromDeclarationAtShipmentLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUPER";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollection collection = new RouteWrapperCollection(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);

			AssertNotNull("MostInteresting indexer", collection["MostInteresting"]);
			AssertEquals("MostInteresting indexer - LegNo", 2, collection["MostInteresting"].LegNo);
			AssertEquals("MostInteresting indexer - Origin.UNLOCO", "USLAX", collection["MostInteresting"].Origin.UNLOCO);
			AssertEquals("MostInteresting indexer - DateOfDeparture", new ZDateTime(2006, 3, 2), collection["MostInteresting"].DateOfDeparture);
			AssertEquals("MostInteresting indexer - DateOfDeparture", new ZDateTime(2006, 3, 2), collection["MostInteresting"].ActualDeparture);
			AssertEquals("MostInteresting indexer - Destination.UNLOCO", "AUSYD", collection["MostInteresting"].Destination.UNLOCO);
			AssertEquals("MostInteresting indexer - DateOfArrival", new ZDateTime(2006, 3, 3), collection["MostInteresting"].DateOfArrival);
			AssertEquals("MostInteresting indexer - DateOfDeparture", new ZDateTime(2006, 3, 3), collection["MostInteresting"].ActualArrival);
		}

		public void TestLoadFromVoyage()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "TITANIC";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "55555";

			OrgHeader line = Factory.New<OrgHeader>();
			OrgAddress orgAddress = line.Addresses.AddNew();
			orgAddress.OA_Address1 = "742 EVERGREEN TERRACE";
			orgAddress.OA_RN_NKCountryCode = "AU";
			voyage.JV_OH_Line = line.PK;

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_E_DEP = new ZDateTime(2011, 1, 1);
			origin1.JA_A_DEP = new ZDateTime(2011, 1, 2);
			origin1.JA_ReceivalCommences = new ZDateTime(2011, 1, 18);
			origin1.JA_DGReceivalCommences = new ZDateTime(2011, 1, 19);
			origin1.JA_CutOff = new ZDateTime(2011, 1, 23);
			origin1.JA_DGCutOff = new ZDateTime(2011, 1, 24);
			origin1.JA_RL_NKPortOfLoading = "AUMEL";
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_E_DEP = new ZDateTime(2011, 2, 3);
			origin2.JA_RL_NKPortOfLoading = "MYBAG";
			VoyageOrigin origin3 = voyage.Origins.AddNew();
			origin3.JA_E_DEP = new ZDateTime(2011, 1, 16);
			origin3.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_E_ARV = new ZDateTime(2011, 2, 18);
			destination1.JB_RL_NKPortOfDischarge = "HKHKG";
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_E_ARV = new ZDateTime(2011, 3, 12);
			destination2.JB_RL_NKPortOfDischarge = "NLAMS";
			VoyageDestination destination3 = voyage.Destinations.AddNew();
			destination3.JB_E_ARV = new ZDateTime(2011, 1, 20);
			destination3.JB_RL_NKPortOfDischarge = "AUBNE";
			VoyageDestination destination4 = voyage.Destinations.AddNew();
			destination4.JB_E_ARV = new ZDateTime(2011, 1, 15);
			destination4.JB_A_ARV = new ZDateTime(2011, 1, 14);
			destination4.JB_AvailabilityDate = new ZDateTime(2011, 1, 17);
			destination4.JB_StorageDate = new ZDateTime(2011, 1, 27);
			destination4.JB_RL_NKPortOfDischarge = "AUSYD";

			RouteWrapperCollection routing = new RouteWrapperCollection(voyage, Factory);

			AssertEquals("routing.Count", 5, routing.Count);
			AssertEquals("routing[0]", "AUMEL", routing[0].Origin.UNLOCO);
			AssertEquals("routing[0]", "AUSYD", routing[0].Destination.UNLOCO);
			AssertEquals("routing[1]", "AUSYD", routing[1].Origin.UNLOCO);
			AssertEquals("routing[1]", "AUBNE", routing[1].Destination.UNLOCO);
			AssertEquals("routing[2]", "AUBNE", routing[2].Origin.UNLOCO);
			AssertEquals("routing[2]", "MYBAG", routing[2].Destination.UNLOCO);
			AssertEquals("routing[3]", "MYBAG", routing[3].Origin.UNLOCO);
			AssertEquals("routing[3]", "HKHKG", routing[3].Destination.UNLOCO);
			AssertEquals("routing[4]", "HKHKG", routing[4].Origin.UNLOCO);
			AssertEquals("routing[4]", "NLAMS", routing[4].Destination.UNLOCO);

			AssertEquals("routing[0].LegNo", 1, routing[0].LegNo);
			AssertEquals("routing[0].Vessel", "TITANIC", routing[0].Transport.VesselName);
			AssertEquals("routing[0].Voyage", "55555", routing[0].Transport.VoyageNo);
			AssertEquals("routing[0].EstimatedDeparture", new ZDateTime(2011, 1, 1), routing[0].EstimatedDeparture);
			AssertEquals("routing[0].EstimatedArrival", new ZDateTime(2011, 1, 15), routing[0].EstimatedArrival);
			AssertEquals("routing[0].ActualDeparture", new ZDateTime(2011, 1, 2), routing[0].ActualDeparture);
			AssertEquals("routing[0].ActualArrival", new ZDateTime(2011, 1, 14), routing[0].ActualArrival);
			AssertEquals("routing[0].AvailabilityDate", new ZDateTime(2011, 1, 17), routing[0].AvailabilityDate);
			AssertEquals("routing[0].FCLReceivalCommences", new ZDateTime(2011, 1, 18), routing[0].FCLReceivalCommences);
			AssertEquals("routing[0].DGFCLReceivalCommences", new ZDateTime(2011, 1, 19), routing[0].DGFCLReceivalCommences);
			AssertEquals("routing[0].FCLCutOff", new ZDateTime(2011, 1, 23), routing[0].FCLCutOff);
			AssertEquals("routing[0].DGFCLCutOff", new ZDateTime(2011, 1, 24), routing[0].DGFCLCutOff);
			AssertEquals("routing[0].FCLStorageCommences", new ZDateTime(2011, 1, 27), routing[0].FCLStorageCommences);
			AssertEquals("routing[0].Carrier", "742 EVERGREEN TERRACE\nAUSTRALIA", routing[0].Carrier.Addresses[1].Address);

			AssertEquals("routing[0].OriginAddress", ZString.Empty, routing[0].OriginAddress.Address);
			AssertEquals("routing[0].DestinationAddress", ZString.Empty, routing[0].DestinationAddress.Address);
			AssertEquals("routing[0].LCLReceivalCommences", ZDateTime.Empty, routing[0].LCLReceivalCommences);
			AssertEquals("routing[0].LCLCutOff", ZDateTime.Empty, routing[0].LCLCutOff);
			AssertEquals("routing[0].LCLAvailabilityDate", ZDateTime.Empty, routing[0].LCLAvailabilityDate);
			AssertEquals("routing[0].LCLStorageCommences", ZDateTime.Empty, routing[0].LCLStorageCommences);
			AssertEquals("routing[0].CarriersReference", ZString.Empty, routing[0].CarriersReference);
		}

		public void TestLoadFromShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			Routing transport1 = consol.Transports.Count == 1 ? consol.Transports[0] : consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			Routing transport2 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 2;

			RouteWrapperCollection collection = new RouteWrapperCollection(shipment, consol, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestLoadFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			Routing transport1 = consol.Transports.Count == 1 ? consol.Transports[0] : consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			Routing transport2 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 2;

			RouteWrapperCollection collection = new RouteWrapperCollection(consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
		}

		public void TestLoadFromDispatchLoadList()
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();

			var transport1 = dispatchLoadList.Transports.Count == 1 ? dispatchLoadList.Transports[0] : dispatchLoadList.Transports.AddNew();
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var transport2 = dispatchLoadList.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var collection = new RouteWrapperCollection(dispatchLoadList, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestLoadFromReceiveConsignment()
		{
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();

			var transport1 = receiveConsignment.Transports.Count == 1 ? receiveConsignment.Transports[0] : receiveConsignment.Transports.AddNew();
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var transport2 = receiveConsignment.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var collection = new RouteWrapperCollection(receiveConsignment, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestLoadFromReceiveASN()
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();

			var transport1 = receiveASN.Transports.Count == 1 ? receiveASN.Transports[0] : receiveASN.Transports.AddNew();
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var transport2 = receiveASN.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			var collection = new RouteWrapperCollection(receiveASN, Factory);
			AssertEquals("collection.Count", 2, collection.Count);
		}

		public void TestConsolLoadFromCartageForSea()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage.Vessel = "CARTAGE VESSEL WOW";
			cartage.VoyageFlight = "S258";
			cartage.PortOfLoading = "AUSYD";
			cartage.PortOfDischarge = "AUDRW";
			cartage.E_DEP = new ZDateTime(2008, 4, 26);
			cartage.E_ARV = new ZDateTime(2008, 6, 13);

			RouteWrapperCollection collection = new RouteWrapperCollection(cartage, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			RouteWrapper wrapper = collection[0];
			AssertEquals("wrapper.Transport.VesselName", "CARTAGE VESSEL WOW", wrapper.Transport.VesselName);
			AssertEquals("wrapper.Transport.VoyageNo", "S258", wrapper.Transport.VoyageNo);
			AssertEquals("wrapper.Origin.UNLOCO", "AUSYD", wrapper.Origin.UNLOCO);
			AssertEquals("wrapper.Destination.UNLOCO", "AUDRW", wrapper.Destination.UNLOCO);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2008, 4, 26), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2008, 6, 13), wrapper.EstimatedArrival);
		}

		public void TestConsolLoadFromJobCartageForAir()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			cartage.VoyageFlight = "QF12";
			cartage.PortOfLoading = "AUMEL";
			cartage.PortOfDischarge = "NZAKL";
			cartage.E_DEP = new ZDateTime(2008, 9, 12);
			cartage.E_ARV = new ZDateTime(2008, 9, 14);

			RouteWrapperCollection collection = new RouteWrapperCollection(cartage, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			RouteWrapper wrapper = collection[0];
			AssertEquals("wrapper.Transport.FlightNo", "QF12", wrapper.Transport.FlightNo);
			AssertEquals("wrapper.Origin.UNLOCO", "AUMEL", wrapper.Origin.UNLOCO);
			AssertEquals("wrapper.Destination.UNLOCO", "NZAKL", wrapper.Destination.UNLOCO);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2008, 9, 12), wrapper.EstimatedDeparture);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2008, 9, 14), wrapper.EstimatedArrival);
		}

		[TestDate(2007, 1, 1)]
		public void TestShipmentLoadFromJobCartageWithTwoAddresses()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_EstimatedPickup = new ZDateTime(2007, 3, 12, 17, 23, 0);
			cartage.JJ_EstimatedDelivery = new ZDateTime(2007, 3, 13, 8, 23, 0);

			OrgHeader orgHeader = OrgTestHelper.GetNewOrganisation("SMFBMK2", "SCOTTS MASSIVE FLOATING BROTHEL MKII", "TEST 1 ADDRESS TO NOWHERE", "SECOND WASTED LINE",
				"C1", "CITY", "2034", "STATE", "PORT", "1900 MCLOVIN", "FAX", "EMAIL", "MOBILE", "", Factory);
			OrgContact contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "SUPER SCOTT";
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address = orgHeader.Addresses[0];
			address.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(5).AddMinutes(30);
			address.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(8).AddMinutes(40);
			address.OA_DoNotAttendFrom = ZDateTime.Now.AddMinutes(30);
			address.OA_DoNotAttendTo = ZDateTime.Now.AddHours(3).AddMinutes(45);
			cartage.FirstDocAddress.OrganisationPK = orgHeader.PK;

			OrgHeader orgHeader2 = OrgTestHelper.GetNewOrganisation("SAWALI", "SCOTTS ALCOHOL WAREHOUSE", "TEST 2 ADDRESS TO LISMORE", "BYRON BAY",
				"C2", "CITY2", "2039", "STATE2", "PORT2", "0434358997", "FAX2", "EMAIL2", "MOBILE2", "", Factory);
			OrgContact contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_ContactName = "ALLISON";
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address2 = orgHeader2.Addresses[0];
			address2.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(16).AddMinutes(30);
			address2.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(20).AddMinutes(15);
			address2.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(9).AddMinutes(30);
			address2.OA_DoNotAttendTo = ZDateTime.Now.AddHours(14).AddMinutes(45);
			cartage.SecondDocAddress.OrganisationPK = orgHeader2.PK;

			RouteWrapperCollection collection = new RouteWrapperCollection(cartage, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			RouteWrapper wrapper = collection[0];
			AssertEquals("wrapper.OriginAddress.CompanyNameAndAddress", "SCOTTS MASSIVE FLOATING BROTHEL MKII\nTEST 1 ADDRESS TO NOWHERE\nSECOND WASTED LINE\nCITY STATE 2034", wrapper.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.OriginAddress.ContactName", "SUPER SCOTT", wrapper.OriginAddress.ContactName);
			AssertEquals("wrapper.OriginAddress.Phone", "1900 MCLOVIN", wrapper.OriginAddress.Phone);
			AssertEquals("wrapper.OriginAddress.PickupFromTime", "5:30", wrapper.OriginAddress.PickupFromTime);
			AssertEquals("wrapper.OriginAddress.PickupToTime", "8:40", wrapper.OriginAddress.PickupToTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendFrom", "0:30", wrapper.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendTo", "3:45", wrapper.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 3, 12, 17, 23, 0), wrapper.EstimatedDeparture);

			AssertEquals("wrapper.DestinationAddress.CompanyNameAndAddress", "SCOTTS ALCOHOL WAREHOUSE\nTEST 2 ADDRESS TO LISMORE\nBYRON BAY\nCITY2 STATE2 2039", wrapper.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.DestinationAddress.ContactName", "ALLISON", wrapper.DestinationAddress.ContactName);
			AssertEquals("wrapper.DestinationAddress.Phone", "0434358997", wrapper.DestinationAddress.Phone);
			AssertEquals("wrapper.DestinationAddress.DeliverFromTime", "16:30", wrapper.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper.DestinationAddress.DeliverToTime", "20:15", wrapper.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendFrom", "9:30", wrapper.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendTo", "14:45", wrapper.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedArrival", new ZDateTime(2007, 3, 13, 8, 23, 0), wrapper.EstimatedArrival);
		}

		[TestDate(2007, 1, 1)]
		public void TestShipmentLoadFromJobCartageWithThreeAddresses()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_EstimatedPickup = new ZDateTime(2007, 4, 10, 8, 25, 0);
			cartage.JJ_EstimatedDelivery = new ZDateTime(2007, 4, 10, 12, 48, 0);

			OrgHeader orgHeader = OrgTestHelper.GetNewOrganisation("KJHSD34", "FIRST ORGANISATION", "FIRST ORGANISATION ADDRESS LINE 1", "FIRST ORGANISATION ADDRESS LINE 2",
				"C1", "CITY", "POSTCODE", "STATE", "PORT", "PHONE", "FAX", "EMAIL", "MOBILE", "", Factory);
			OrgContact contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "FIRST ORG CONTACT";
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address = orgHeader.Addresses[0];
			address.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(3).AddMinutes(30);
			address.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(6).AddMinutes(40);
			address.OA_DoNotAttendFrom = ZDateTime.Now.AddMinutes(45);
			address.OA_DoNotAttendTo = ZDateTime.Now.AddHours(2).AddMinutes(30);
			cartage.FirstDocAddress.OrganisationPK = orgHeader.PK;

			OrgHeader orgHeader2 = OrgTestHelper.GetNewOrganisation("JKHJSD3", "SECOND ORGANISATION", "SECOND ORGANISATION ADDRESS LINE 1", "SECOND ORGANISATION ADDRESS LINE 2",
				"C2", "CITY2", "POSTCODE2", "STATE2", "PORT2", "PHONE2", "FAX2", "EMAIL2", "MOBILE2", "", Factory);
			OrgContact contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_ContactName = "SECOND ORG CONTACT";
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address2 = orgHeader2.Addresses[0];
			address2.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(4).AddMinutes(30);
			address2.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(10).AddMinutes(40);
			address2.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(16).AddMinutes(30);
			address2.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(20).AddMinutes(15);
			address2.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(9).AddMinutes(30);
			address2.OA_DoNotAttendTo = ZDateTime.Now.AddHours(14).AddMinutes(45);
			cartage.SecondDocAddress.OrganisationPK = orgHeader2.PK;

			OrgHeader orgHeader3 = OrgTestHelper.GetNewOrganisation("83HJUKS", "THIRD ORGANISATION", "THIRD ORGANISATION ADDRESS LINE 1", "THIRD ORGANISATION ADDRESS LINE 2",
				"C3", "CITY3", "POSTCODE3", "STATE3", "PORT3", "PHONE3", "FAX3", "EMAIL3", "MOBILE3", "", Factory);
			OrgContact contact3 = orgHeader3.Contacts.AddNew();
			contact3.OC_ContactName = "THIRD ORG CONTACT";
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address3 = orgHeader3.Addresses[0];
			address3.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(1).AddMinutes(20);
			address3.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(3).AddMinutes(50);
			address3.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(5).AddMinutes(30);
			address3.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(9).AddMinutes(15);
			address3.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(10).AddMinutes(35);
			address3.OA_DoNotAttendTo = ZDateTime.Now.AddHours(13).AddMinutes(45);
			cartage.ThirdDocAddress.OrganisationPK = orgHeader3.PK;

			RouteWrapperCollection collection = new RouteWrapperCollection(cartage, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 2, collection.Count);

			RouteWrapper wrapper = collection[0];
			AssertEquals("wrapper.OriginAddress.CompanyNameAndAddress", "FIRST ORGANISATION\nFIRST ORGANISATION ADDRESS LINE 1\nFIRST ORGANISATION ADDRESS LINE 2\nCITY STATE POSTCODE", wrapper.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.OriginAddress.ContactName", "FIRST ORG CONTACT", wrapper.OriginAddress.ContactName);
			AssertEquals("wrapper.OriginAddress.Phone", "PHONE", wrapper.OriginAddress.Phone);
			AssertEquals("wrapper.OriginAddress.PickupFromTime", "3:30", wrapper.OriginAddress.PickupFromTime);
			AssertEquals("wrapper.OriginAddress.PickupToTime", "6:40", wrapper.OriginAddress.PickupToTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendFrom", "0:45", wrapper.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendTo", "2:30", wrapper.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 4, 10, 8, 25, 0), wrapper.EstimatedDeparture);

			AssertEquals("wrapper.DestinationAddress.CompanyNameAndAddress", "SECOND ORGANISATION\nSECOND ORGANISATION ADDRESS LINE 1\nSECOND ORGANISATION ADDRESS LINE 2\nCITY2 STATE2 POSTCODE2", wrapper.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.DestinationAddress.ContactName", "SECOND ORG CONTACT", wrapper.DestinationAddress.ContactName);
			AssertEquals("wrapper.DestinationAddress.Phone", "PHONE2", wrapper.DestinationAddress.Phone);
			AssertEquals("wrapper.DestinationAddress.DeliverFromTime", "16:30", wrapper.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper.DestinationAddress.DeliverToTime", "20:15", wrapper.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendFrom", "9:30", wrapper.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendTo", "14:45", wrapper.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedArrival", ZDateTime.Empty, wrapper.EstimatedArrival);

			RouteWrapper wrapper2 = collection[1];
			AssertEquals("wrapper2.OriginAddress.CompanyNameAndAddress", "SECOND ORGANISATION\nSECOND ORGANISATION ADDRESS LINE 1\nSECOND ORGANISATION ADDRESS LINE 2\nCITY2 STATE2 POSTCODE2", wrapper2.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper2.OriginAddress.ContactName", "SECOND ORG CONTACT", wrapper2.OriginAddress.ContactName);
			AssertEquals("wrapper2.OriginAddress.Phone", "PHONE2", wrapper2.OriginAddress.Phone);
			AssertEquals("wrapper2.OriginAddress.PickupFromTime", "4:30", wrapper2.OriginAddress.PickupFromTime);
			AssertEquals("wrapper2.OriginAddress.PickupToTime", "10:40", wrapper2.OriginAddress.PickupToTime);
			AssertEquals("wrapper2.OriginAddress.DoNotAttendFrom", "9:30", wrapper2.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper2.OriginAddress.DoNotAttendTo", "14:45", wrapper2.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper2.EstimatedDeparture", ZDateTime.Empty, wrapper2.EstimatedDeparture);

			AssertEquals("wrapper2.DestinationAddress.CompanyNameAndAddress", "THIRD ORGANISATION\nTHIRD ORGANISATION ADDRESS LINE 1\nTHIRD ORGANISATION ADDRESS LINE 2\nCITY3 STATE3 POSTCODE3", wrapper2.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper2.DestinationAddress.ContactName", "THIRD ORG CONTACT", wrapper2.DestinationAddress.ContactName);
			AssertEquals("wrapper2.DestinationAddress.Phone", "PHONE3", wrapper2.DestinationAddress.Phone);
			AssertEquals("wrapper2.DestinationAddress.DeliverFromTime", "5:30", wrapper2.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper2.DestinationAddress.DeliverToTime", "9:15", wrapper2.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper2.DestinationAddress.DoNotAttendFrom", "10:35", wrapper2.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper2.DestinationAddress.DoNotAttendTo", "13:45", wrapper2.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper2.EstimatedArrival", new ZDateTime(2007, 4, 10, 12, 48, 0), wrapper2.EstimatedArrival);
		}

		[TestDate(2007, 1, 1)]
		public void TestShipmentLoadFromJobCartageWithFourAddresses()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			cartage.JJ_EstimatedPickup = new ZDateTime(2007, 4, 10, 8, 25, 0);
			cartage.JJ_EstimatedDelivery = new ZDateTime(2007, 4, 10, 12, 48, 0);

			OrgHeader orgHeader = OrgTestHelper.GetNewOrganisation("KJHSD34", "FIRST ORGANISATION", "FIRST ORGANISATION ADDRESS LINE 1", "FIRST ORGANISATION ADDRESS LINE 2",
				"C1", "CITY", "POSTCODE", "STATE", "PORT", "PHONE", "FAX", "EMAIL", "MOBILE", "", Factory);
			OrgContact contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "FIRST ORG CONTACT";
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address = orgHeader.Addresses[0];
			address.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(3).AddMinutes(30);
			address.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(6).AddMinutes(40);
			address.OA_DoNotAttendFrom = ZDateTime.Now.AddMinutes(45);
			address.OA_DoNotAttendTo = ZDateTime.Now.AddHours(2).AddMinutes(30);
			cartage.FirstDocAddress.OrganisationPK = orgHeader.PK;

			OrgHeader orgHeader2 = OrgTestHelper.GetNewOrganisation("JKHJSD3", "SECOND ORGANISATION", "SECOND ORGANISATION ADDRESS LINE 1", "SECOND ORGANISATION ADDRESS LINE 2",
				"C2", "CITY2", "POSTCODE2", "STATE2", "PORT2", "PHONE2", "FAX2", "EMAIL2", "MOBILE2", "", Factory);
			OrgContact contact2 = orgHeader2.Contacts.AddNew();
			contact2.OC_ContactName = "SECOND ORG CONTACT";
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address2 = orgHeader2.Addresses[0];
			address2.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(4).AddMinutes(30);
			address2.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(10).AddMinutes(40);
			address2.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(16).AddMinutes(30);
			address2.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(20).AddMinutes(15);
			address2.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(9).AddMinutes(30);
			address2.OA_DoNotAttendTo = ZDateTime.Now.AddHours(14).AddMinutes(45);
			cartage.SecondDocAddress.OrganisationPK = orgHeader2.PK;

			OrgHeader orgHeader3 = OrgTestHelper.GetNewOrganisation("83HJUKS", "THIRD ORGANISATION", "THIRD ORGANISATION ADDRESS LINE 1", "THIRD ORGANISATION ADDRESS LINE 2",
				"C3", "CITY3", "POSTCODE3", "STATE3", "PORT3", "PHONE3", "FAX3", "EMAIL3", "MOBILE3", "", Factory);
			OrgContact contact3 = orgHeader3.Contacts.AddNew();
			contact3.OC_ContactName = "THIRD ORG CONTACT";
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address3 = orgHeader3.Addresses[0];
			address3.OA_PickupFromTimeOnly = ZDateTime.Now.AddHours(1).AddMinutes(20);
			address3.OA_PickupToTimeOnly = ZDateTime.Now.AddHours(3).AddMinutes(50);
			address3.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(5).AddMinutes(30);
			address3.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(9).AddMinutes(15);
			address3.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(10).AddMinutes(35);
			address3.OA_DoNotAttendTo = ZDateTime.Now.AddHours(13).AddMinutes(45);
			cartage.ThirdDocAddress.OrganisationPK = orgHeader3.PK;

			OrgHeader orgHeader4 = OrgTestHelper.GetNewOrganisation("SDJ45KJ", "FOURTH ORGANISATION", "FOURTH ORGANISATION ADDRESS LINE 1", "FOURTH ORGANISATION ADDRESS LINE 2",
				"C4", "CITY4", "POSTCODE4", "STATE4", "PORT4", "PHONE4", "FAX4", "EMAIL4", "MOBILE4", "", Factory);
			OrgContact contact4 = orgHeader4.Contacts.AddNew();
			contact4.OC_ContactName = "FOURTH ORG CONTACT";
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgAddress address4 = orgHeader4.Addresses[0];
			address4.OA_DeliverFromTimeOnly = ZDateTime.Now.AddHours(2).AddMinutes(33);
			address4.OA_DeliverToTimeOnly = ZDateTime.Now.AddHours(7).AddMinutes(19);
			address4.OA_DoNotAttendFrom = ZDateTime.Now.AddHours(8).AddMinutes(45);
			address4.OA_DoNotAttendTo = ZDateTime.Now.AddHours(19).AddMinutes(45);
			cartage.FourthDocAddress.OrganisationPK = orgHeader4.PK;

			RouteWrapperCollection collection = new RouteWrapperCollection(cartage, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);

			RouteWrapper wrapper = collection[0];
			AssertEquals("wrapper.OriginAddress.CompanyNameAndAddress", "FIRST ORGANISATION\nFIRST ORGANISATION ADDRESS LINE 1\nFIRST ORGANISATION ADDRESS LINE 2\nCITY STATE POSTCODE", wrapper.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.OriginAddress.ContactName", "FIRST ORG CONTACT", wrapper.OriginAddress.ContactName);
			AssertEquals("wrapper.OriginAddress.Phone", "PHONE", wrapper.OriginAddress.Phone);
			AssertEquals("wrapper.OriginAddress.PickupFromTime", "3:30", wrapper.OriginAddress.PickupFromTime);
			AssertEquals("wrapper.OriginAddress.PickupToTime", "6:40", wrapper.OriginAddress.PickupToTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendFrom", "0:45", wrapper.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.OriginAddress.DoNotAttendTo", "2:30", wrapper.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedDeparture", new ZDateTime(2007, 4, 10, 8, 25, 0), wrapper.EstimatedDeparture);

			AssertEquals("wrapper.DestinationAddress.CompanyNameAndAddress", "SECOND ORGANISATION\nSECOND ORGANISATION ADDRESS LINE 1\nSECOND ORGANISATION ADDRESS LINE 2\nCITY2 STATE2 POSTCODE2", wrapper.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper.DestinationAddress.ContactName", "SECOND ORG CONTACT", wrapper.DestinationAddress.ContactName);
			AssertEquals("wrapper.DestinationAddress.Phone", "PHONE2", wrapper.DestinationAddress.Phone);
			AssertEquals("wrapper.DestinationAddress.DeliverFromTime", "16:30", wrapper.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper.DestinationAddress.DeliverToTime", "20:15", wrapper.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendFrom", "9:30", wrapper.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper.DestinationAddress.DoNotAttendTo", "14:45", wrapper.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper.EstimatedArrival", ZDateTime.Empty, wrapper.EstimatedArrival);

			RouteWrapper wrapper2 = collection[1];
			AssertEquals("wrapper2.OriginAddress.CompanyNameAndAddress", "SECOND ORGANISATION\nSECOND ORGANISATION ADDRESS LINE 1\nSECOND ORGANISATION ADDRESS LINE 2\nCITY2 STATE2 POSTCODE2", wrapper2.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper2.OriginAddress.ContactName", "SECOND ORG CONTACT", wrapper2.OriginAddress.ContactName);
			AssertEquals("wrapper2.OriginAddress.Phone", "PHONE2", wrapper2.OriginAddress.Phone);
			AssertEquals("wrapper2.OriginAddress.PickupFromTime", "4:30", wrapper2.OriginAddress.PickupFromTime);
			AssertEquals("wrapper2.OriginAddress.PickupToTime", "10:40", wrapper2.OriginAddress.PickupToTime);
			AssertEquals("wrapper2.OriginAddress.DoNotAttendFrom", "9:30", wrapper2.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper2.OriginAddress.DoNotAttendTo", "14:45", wrapper2.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper2.EstimatedDeparture", ZDateTime.Empty, wrapper2.EstimatedDeparture);

			AssertEquals("wrapper2.DestinationAddress.CompanyNameAndAddress", "THIRD ORGANISATION\nTHIRD ORGANISATION ADDRESS LINE 1\nTHIRD ORGANISATION ADDRESS LINE 2\nCITY3 STATE3 POSTCODE3", wrapper2.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper2.DestinationAddress.ContactName", "THIRD ORG CONTACT", wrapper2.DestinationAddress.ContactName);
			AssertEquals("wrapper2.DestinationAddress.Phone", "PHONE3", wrapper2.DestinationAddress.Phone);
			AssertEquals("wrapper2.DestinationAddress.DeliverFromTime", "5:30", wrapper2.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper2.DestinationAddress.DeliverToTime", "9:15", wrapper2.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper2.DestinationAddress.DoNotAttendFrom", "10:35", wrapper2.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper2.DestinationAddress.DoNotAttendTo", "13:45", wrapper2.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper2.EstimatedArrival", ZDateTime.Empty, wrapper2.EstimatedArrival);

			RouteWrapper wrapper3 = collection[2];
			AssertEquals("wrapper3.OriginAddress.CompanyNameAndAddress", "THIRD ORGANISATION\nTHIRD ORGANISATION ADDRESS LINE 1\nTHIRD ORGANISATION ADDRESS LINE 2\nCITY3 STATE3 POSTCODE3", wrapper3.OriginAddress.CompanyNameAndAddress);
			AssertEquals("wrapper3.OriginAddress.ContactName", "THIRD ORG CONTACT", wrapper3.OriginAddress.ContactName);
			AssertEquals("wrapper3.OriginAddress.Phone", "PHONE3", wrapper3.OriginAddress.Phone);
			AssertEquals("wrapper3.OriginAddress.PickupFromTime", "1:20", wrapper3.OriginAddress.PickupFromTime);
			AssertEquals("wrapper3.OriginAddress.PickupToTime", "3:50", wrapper3.OriginAddress.PickupToTime);
			AssertEquals("wrapper3.OriginAddress.DoNotAttendFrom", "10:35", wrapper3.OriginAddress.DoNotAttendFromTime);
			AssertEquals("wrapper3.OriginAddress.DoNotAttendTo", "13:45", wrapper3.OriginAddress.DoNotAttendToTime);
			AssertEquals("wrapper3.EstimatedDeparture", ZDateTime.Empty, wrapper3.EstimatedDeparture);

			AssertEquals("wrapper3.DestinationAddress.CompanyNameAndAddress", "FOURTH ORGANISATION\nFOURTH ORGANISATION ADDRESS LINE 1\nFOURTH ORGANISATION ADDRESS LINE 2\nCITY4 STATE4 POSTCODE4", wrapper3.DestinationAddress.CompanyNameAndAddress);
			AssertEquals("wrapper3.DestinationAddress.ContactName", "FOURTH ORG CONTACT", wrapper3.DestinationAddress.ContactName);
			AssertEquals("wrapper3.DestinationAddress.Phone", "PHONE4", wrapper3.DestinationAddress.Phone);
			AssertEquals("wrapper3.DestinationAddress.DeliverFromTime", "2:33", wrapper3.DestinationAddress.DeliverFromTime);
			AssertEquals("wrapper3.DestinationAddress.DeliverToTime", "7:19", wrapper3.DestinationAddress.DeliverToTime);
			AssertEquals("wrapper3.DestinationAddress.DoNotAttendFrom", "8:45", wrapper3.DestinationAddress.DoNotAttendFromTime);
			AssertEquals("wrapper3.DestinationAddress.DoNotAttendTo", "19:45", wrapper3.DestinationAddress.DoNotAttendToTime);
			AssertEquals("wrapper3.EstimatedArrival", new ZDateTime(2007, 4, 10, 12, 48, 0), wrapper3.EstimatedArrival);
		}

		public void TestSynthesizeRoutesWithImportShipmentLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "USPHX";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUADL";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollectionHelper collection = new RouteWrapperCollectionHelper(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertLeg("Leg 1", collection[0], 1, "USPHX", new ZDateTime(2006, 3, 1), "USLAX", new ZDateTime(2006, 3, 2));
			AssertLeg("Leg 2", collection[1], 2, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
			AssertLeg("Leg 3", collection[2], 3, "AUSYD", new ZDateTime(2006, 3, 3), "AUADL", new ZDateTime(2006, 3, 4));
			AssertEquals("Loading route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("loading"))).PK);
			AssertEquals("Discharge route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("discharge"))).PK);
			AssertEquals("Import route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("import"))).PK);
			AssertNull("Export route null", collection.GetRouteForIndex("export"));
		}

		public void TestSynthesizeRoutesWithExportShipmentLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUADL";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "USPHX";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollectionHelper collection = new RouteWrapperCollectionHelper(declaration, RoutingLevel.Shipment, Factory);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertLeg("Leg 1", collection[0], 1, "AUADL", new ZDateTime(2006, 3, 1), "AUSYD", new ZDateTime(2006, 3, 2));
			AssertLeg("Leg 2", collection[1], 2, "AUSYD", new ZDateTime(2006, 3, 2), "USLAX", new ZDateTime(2006, 3, 3));
			AssertLeg("Leg 3", collection[2], 3, "USLAX", new ZDateTime(2006, 3, 3), "USPHX", new ZDateTime(2006, 3, 4));
			AssertEquals("Loading route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("loading"))).PK);
			AssertEquals("Discharge route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("discharge"))).PK);
			AssertEquals("Export route set", collection[1].PK, ((RouteWrapper)(collection.GetRouteForIndex("export"))).PK);
			AssertNull("Import route null", collection.GetRouteForIndex("import"));
		}

		public void TestSynthesizeRoutesWithImportConsolLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "USPHX";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "AUADL";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollectionHelper collection = new RouteWrapperCollectionHelper(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("Leg", collection[0], 1, "USLAX", new ZDateTime(2006, 3, 2), "AUSYD", new ZDateTime(2006, 3, 3));
			AssertEquals("Loading route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("loading"))).PK);
			AssertEquals("Discharge route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("discharge"))).PK);
			AssertEquals("Import route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("import"))).PK);
			AssertNull("Export route null", collection.GetRouteForIndex("export"));
		}

		public void TestSynthesizeRoutesWithExportConsolLevel()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUADL";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 3, 1);
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_ExportDate = new ZDateTime(2006, 3, 2);
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 3, 3);
			declaration.JE_RL_NKFinalDestination = "USPHX";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 3, 4);
			declaration.Transports.RemoveAndDeleteAll();

			RouteWrapperCollectionHelper collection = new RouteWrapperCollectionHelper(declaration, RoutingLevel.Consol, Factory);
			AssertEquals("collection.Count", 1, collection.Count);
			AssertLeg("Leg", collection[0], 1, "AUSYD", new ZDateTime(2006, 3, 2), "USLAX", new ZDateTime(2006, 3, 3));
			AssertEquals("Loading route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("loading"))).PK);
			AssertEquals("Discharge route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("discharge"))).PK);
			AssertEquals("Export route set", collection[0].PK, ((RouteWrapper)(collection.GetRouteForIndex("export"))).PK);
			AssertNull("Import route null", collection.GetRouteForIndex("import"));
		}

		public void TestImportIndexes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.AirSea;

				Routing transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport1.JW_RL_NKLoadPort = "USPHX";
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_LegOrder = 1;

				Routing transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "HKHKG";
				transport2.JW_LegOrder = 2;

				Routing transport3 = consol.Transports.AddNew();
				transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport3.JW_RL_NKLoadPort = "HKHKG";
				transport3.JW_RL_NKDiscPort = "SGSIN";
				transport3.JW_LegOrder = 3;

				Routing transport4 = consol.Transports.AddNew();
				transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport4.JW_RL_NKLoadPort = "SGSIN";
				transport4.JW_RL_NKDiscPort = "AUSYD";
				transport4.JW_LegOrder = 4;

				Routing transport5 = consol.Transports.AddNew();
				transport5.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport5.JW_RL_NKLoadPort = "AUSYD";
				transport5.JW_RL_NKDiscPort = "AUADL";
				transport5.JW_LegOrder = 5;

				RouteWrapperCollection routeWrapperCollection = new RouteWrapperCollection(consol.Shipments.AddNew(), consol, Factory);

				AssertEquals("Import indexer", transport4.JW_LegOrder, routeWrapperCollection["Import"].LegNo);
				AssertNull("Export indexer", routeWrapperCollection["exPorT"]);
				AssertEquals("Loading indexer", transport2.JW_LegOrder, routeWrapperCollection["loading"].LegNo);
				AssertEquals("Discharge indexer", transport4.JW_LegOrder, routeWrapperCollection["discharge"].LegNo);
			}
		}

		public void TestPopulateOriginAndDestinationPortsFromPlannedPortsWhenNoConsolOrTransportOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZCHC";
			shipment.JS_RL_NKDestination = "USDAN";
			shipment.JS_RL_NKLoadPort = "NZABY";
			shipment.JS_RL_NKDischargePort = "USCHI";

			var routeWrapperCollection = new RouteWrapperCollection(shipment, Factory.GetNull<ForwardingConsol>(), Factory);
			AssertEquals(1, routeWrapperCollection.Count);
			AssertEquals("Default 1 for LegNo", 1, routeWrapperCollection[0].LegNo);
			AssertEquals("Default empty for EstimatedDeparture", ZDateTime.Empty, routeWrapperCollection[0].EstimatedDeparture);
			AssertEquals("Default empty for EstimatedArrival", ZDateTime.Empty, routeWrapperCollection[0].EstimatedArrival);
			AssertEquals("From JS_RL_NKLoadPort when no consols or transports defined", "NZABY", routeWrapperCollection[0].Origin.UNLOCO);
			AssertEquals("From JS_RL_NKDischargePort when no consols or transports defined", "USCHI", routeWrapperCollection[0].Destination.UNLOCO);
		}

		public void TestExportIndexes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.SeaAir;

				Routing transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport1.JW_RL_NKLoadPort = "USPHX";
				transport1.JW_RL_NKDiscPort = "USLAX";
				transport1.JW_LegOrder = 1;

				Routing transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "HKHKG";
				transport2.JW_LegOrder = 2;

				Routing transport3 = consol.Transports.AddNew();
				transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport3.JW_RL_NKLoadPort = "HKHKG";
				transport3.JW_RL_NKDiscPort = "SGSIN";
				transport3.JW_LegOrder = 3;

				Routing transport4 = consol.Transports.AddNew();
				transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport4.JW_RL_NKLoadPort = "SGSIN";
				transport4.JW_RL_NKDiscPort = "AUSYD";
				transport4.JW_LegOrder = 4;

				Routing transport5 = consol.Transports.AddNew();
				transport5.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport5.JW_RL_NKLoadPort = "AUSYD";
				transport5.JW_RL_NKDiscPort = "AUADL";
				transport5.JW_LegOrder = 5;

				RouteWrapperCollection routeWrapperCollection = new RouteWrapperCollection(consol.Shipments.AddNew(), consol, Factory);

				AssertEquals("Export indexer", transport2.JW_LegOrder, routeWrapperCollection["Export"].LegNo);
				AssertNull("Import indexer", routeWrapperCollection["import"]);
				AssertEquals("Loading indexer", transport2.JW_LegOrder, routeWrapperCollection["loading"].LegNo);
				AssertEquals("Discharge indexer", transport4.JW_LegOrder, routeWrapperCollection["discharge"].LegNo);
			}
		}

		#region Implementation

		OrgHeader GetOrgHeader(string fullName)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = fullName;
			return result;
		}

		protected override RouteWrapperCollection GetNewDocumentWrapperCollection()
		{
			BaseJobDeclaration declaration = null;
			return new RouteWrapperCollection(declaration, RoutingLevel.Consol, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new RouteWrapper(0, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZDateTime.Empty, ZDateTime.Empty, ZString.Empty, null,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
				ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, null, null, ZString.Empty, null, Factory);
		}

		static void AssertLeg(ZString iterationDescription, RouteWrapper wrapper, ZInt legNo, ZString origin, ZDateTime atd, ZString destination, ZDateTime ata)
		{
			iterationDescription += ", Leg " + legNo.ToString();
			CombineAssertions(delegate
			{
				AssertEquals(iterationDescription + ", wrapper.LegNo", legNo, wrapper.LegNo);
				AssertEquals(iterationDescription + ", wrapper.Origin.UNLOCO", origin, wrapper.Origin.UNLOCO);
				AssertEquals(iterationDescription + ", wrapper.DateOfDeparture", atd, wrapper.DateOfDeparture);
				AssertEquals(iterationDescription + ", wrapper.Destination.UNLOCO", destination, wrapper.Destination.UNLOCO);
				AssertEquals(iterationDescription + ", wrapper.DateOfArrival", ata, wrapper.DateOfArrival);
			});
		}

		public class RouteWrapperCollectionHelper : RouteWrapperCollection
		{
			public RouteWrapperCollectionHelper(BaseJobDeclaration declarationBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
				: base(declarationBO, routingLevel, factory) { }

			public IBODocDataProvider GetRouteForIndex(ZString index)
			{
				return GetRow(index);
			}
		}

		#endregion
	}
}
