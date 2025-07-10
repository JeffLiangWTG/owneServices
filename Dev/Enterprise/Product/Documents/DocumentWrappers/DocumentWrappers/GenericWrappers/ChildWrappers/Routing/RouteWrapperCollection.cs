using System;
using System.Collections.Generic;
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
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transit.Business;
using IRoutingSupport = Enterprise.Freight.Business.IRoutingSupport;
using Routing = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public enum RoutingLevel { Consol = 1, Shipment = 2 }

	public class RouteWrapperCollection : GenericWrapperCollection<RouteWrapper>
	{
		public RouteWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RouteWrapperCollection(CommonConsol consolBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consolBO != null)
			{
				LoadRoutings(consolBO, consolBO.Transports.MostInterestingTransport);
			}
		}

		public RouteWrapperCollection(CommonShipment shipmentBO, CommonConsol consolBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				LoadRoutings(shipmentBO, consolBO.Transports.MostInterestingTransport);

				if (Count == 0)
				{
					AddFromPlannedPortsForShipment(shipmentBO);
				}
			}
		}

		public RouteWrapperCollection(AgencyShipment agencyShipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (agencyShipmentBO != null)
			{
				LoadRoutings(agencyShipmentBO, agencyShipmentBO.MostInterestingTransport);
			}
		}

		public RouteWrapperCollection(QuotedBooking bookingBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (bookingBO?.Booking != null)
			{
				if (bookingBO.Booking.MostInterestingTransport != null)
				{
					LoadRoutings(bookingBO.Booking, bookingBO.Booking.MostInterestingTransport);
				}
				else
				{
					var sailing = bookingBO.Booking.Sailing;
					if (sailing != null)
					{
						var transportMode = sailing.JX_TransportMode;
						mostInterestingTransport = new RouteWrapper(1, transportMode, ZString.Empty,
							sailing.JX_JV_NKVessel, sailing.JX_JV_VoyageFlight,
							sailing.JX_JA_RL_NKPortOfLoading, null, sailing.JX_JA_E_DEP, sailing.JX_JA_A_DEP,
							sailing.JX_JB_RL_NKPortOfDischarge, null, sailing.JX_JB_E_ARV, sailing.JX_JB_A_ARV,
							sailing.JX_JB_CTOAvailabilityDate,
							sailing.JX_JA_CTOReceivalCommences, sailing.JX_DepotReceivalCommences, sailing.JX_JA_DGFCLReceivalCommences,
							sailing.JX_JA_CTOCutOff, sailing.JX_DepotCutOff, sailing.JX_JA_DGFCLCutOff,
							sailing.JX_DepotAvailabilityDate,
							sailing.JX_JB_CTOStorageDate, sailing.JX_DepotStorageDate,
							bookingBO.Booking.BookedShippingLine, null, ZString.Empty, sailing.Voyage,
							factory);

						Add(mostInterestingTransport);
					}
					else if (!bookingBO.Booking.JS_RL_NKLoadPort.IsEmpty ||
						!bookingBO.Booking.JS_RL_NKDischargePort.IsEmpty)
					{
						var transport = new RouteWrapper(
							1,
							bookingBO.Booking.JS_RL_NKLoadPort,
							ZDateTime.Empty,
							bookingBO.Booking.JS_RL_NKDischargePort,
							ZDateTime.Empty,
							factory);

						Add(transport);
					}
				}
			}
		}

		public RouteWrapperCollection(BaseJobDeclaration declarationBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declarationBO != null)
			{
				if (routingLevel == RoutingLevel.Consol)
				{
					AddFromTransportCollectionOrSynthesizeIfNoTransports(declarationBO, delegate
					{
						RouteWrapper mainRoute = new RouteWrapper(1, declarationBO, factory);
						Add(mainRoute);
						loadingRoute = mainRoute;
						dischargeRoute = mainRoute;
						if (declarationBO.IsImport)
						{
							importRoute = mainRoute;
						}
						else if (declarationBO.IsExport)
						{
							exportRoute = mainRoute;
						}
					});
				}
				else if (routingLevel == RoutingLevel.Shipment)
				{
					AddFromTransportCollectionOrSynthesizeIfNoTransports(declarationBO, delegate
					{
						int index = 1;
						if (declarationBO.JE_RL_NKOrigin != declarationBO.JE_RL_NKPortOfLoading)
						{
							Add(new RouteWrapper(index++,
						declarationBO.JE_RL_NKOrigin, declarationBO.JE_DateAtOrigin,
						declarationBO.JE_RL_NKPortOfLoading, declarationBO.JE_ExportDate, factory));
						}

						RouteWrapper mainRoute = new RouteWrapper(index++, declarationBO, factory);
						Add(mainRoute);
						loadingRoute = mainRoute;
						dischargeRoute = mainRoute;
						if (declarationBO.IsImport)
						{
							importRoute = mainRoute;
						}
						else if (declarationBO.IsExport)
						{
							exportRoute = mainRoute;
						}

						if (declarationBO.JE_RL_NKPortOfArrival != declarationBO.JE_RL_NKFinalDestination)
						{
							Add(new RouteWrapper(index++,
						declarationBO.JE_RL_NKPortOfArrival, declarationBO.JE_DateOfArrival,
						declarationBO.JE_RL_NKFinalDestination, declarationBO.JE_DateAtFinalDestination, factory));
						}
					});
				}

				if (mostInterestingTransport == null && this.Count == 1)
				{
					mostInterestingTransport = this[0];
				}

				if (mostInterestingTransport == null)
				{
					foreach (RouteWrapper route in this)
					{
						bool isMostInteresting = declarationBO.IsExport
							? route.Origin.UNLOCO == declarationBO.JE_RL_NKPortOfLoading
							: route.Destination.UNLOCO == declarationBO.JE_RL_NKFinalDestination;

						if (isMostInteresting)
						{
							mostInterestingTransport = route;
							break;
						}
					}
				}

				if (mostInterestingTransport == null)
				{
					foreach (RouteWrapper route in this)
					{
						if (route.IsInternational)
						{
							mostInterestingTransport = route;
							if (declarationBO.IsExport)
							{
								break;
							}
						}
					}
				}

				var customsClearanceLegProvider = declarationBO.MostInterestingLegProvider;
				var clearanceLoadingRoute = (RouteWrapper)customsClearanceLegProvider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(this));
				if (clearanceLoadingRoute != null)
				{
					loadingRoute = clearanceLoadingRoute;
				}

				var clearanceDischargeRoute = (RouteWrapper)customsClearanceLegProvider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(this));
				if (clearanceDischargeRoute != null)
				{
					dischargeRoute = clearanceDischargeRoute;
				}
			}
		}

		public RouteWrapperCollection(RateOneOffShipment oneOffShipment, BusinessObjectFactory factory)
			: base(factory)
		{
			if (oneOffShipment != null)
			{
				ZString origin = oneOffShipment.TT_RL_NKReceivalLocation;
				ZString destination = oneOffShipment.TT_RL_NKDeliveryLocation;
				ZString via = oneOffShipment.TT_RL_NKViaLocation;

				if (!via.IsEmpty)
				{
					Add(new RouteWrapper(0, origin, ZDateTime.Empty, via, ZDateTime.Empty, factory));
					Add(new RouteWrapper(1, via, ZDateTime.Empty, destination, ZDateTime.Empty, factory));
				}
				else if (!origin.IsEmpty || !destination.IsEmpty)
				{
					Add(new RouteWrapper(0, origin, ZDateTime.Empty, destination, ZDateTime.Empty, factory));
				}
			}
		}

		public RouteWrapperCollection(JobVoyage voyage, BusinessObjectFactory factory)
			: base(factory)
		{
			if (voyage != null)
			{
				Dictionary<string, Tuple> ports = new Dictionary<string, Tuple>();

				foreach (VoyageOrigin origin in voyage.Origins)
				{
					if (!origin.JA_E_DEP.IsValid)
					{
						continue;
					}

					Tuple t;

					if (!ports.TryGetValue(origin.JA_RL_NKPortOfLoading, out t))
					{
						t = new Tuple();
						t.Port = origin.JA_RL_NKPortOfLoading;
						t.SeqDateTime = origin.JA_E_DEP;
						ports.Add(origin.JA_RL_NKPortOfLoading, t);
					}

					t.Origin = origin;
				}

				foreach (VoyageDestination destination in voyage.Destinations)
				{
					if (!destination.JB_E_ARV.IsValid)
					{
						continue;
					}

					Tuple t;

					if (!ports.TryGetValue(destination.JB_RL_NKPortOfDischarge, out t))
					{
						t = new Tuple();
						t.Port = destination.JB_RL_NKPortOfDischarge;
						t.SeqDateTime = destination.JB_E_ARV;
						ports.Add(destination.JB_RL_NKPortOfDischarge, t);
					}

					t.Destination = destination;
				}

				List<Tuple> portsList = new List<Tuple>(ports.Values);

				portsList.Sort((t1, t2) => t1.SeqDateTime.CompareTo(t2.SeqDateTime));

				for (int i = 1; i < portsList.Count; i++)
				{
					Tuple previous = portsList[i - 1];
					Tuple current = portsList[i];

					Add(new RouteWrapper(
						i, voyage.JV_AirSeaRoad, ZString.Empty, voyage.JV_RV_NKVessel, voyage.JV_VoyageFlight, previous.Port, null,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_E_DEP,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_A_DEP,
						current.Port, null,
						current.Destination == null ? ZDateTime.Empty : current.Destination.JB_E_ARV,
						current.Destination == null ? ZDateTime.Empty : current.Destination.JB_A_ARV,
						current.Destination == null ? ZDateTime.Empty : current.Destination.JB_AvailabilityDate,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_ReceivalCommences,
						ZDateTime.Empty,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_DGReceivalCommences,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_CutOff,
						ZDateTime.Empty,
						previous.Origin == null ? ZDateTime.Empty : previous.Origin.JA_DGCutOff,
						ZDateTime.Empty,
						current.Destination == null ? ZDateTime.Empty : current.Destination.JB_StorageDate,
						ZDateTime.Empty,
						voyage.Line, null, ZString.Empty, voyage, factory
						));
				}
			}
		}

		public RouteWrapperCollection(WhsItemDispatchLoadList dispatchLoadListBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (dispatchLoadListBO != null)
			{
				LoadRoutings(dispatchLoadListBO, (l) => l.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			}
		}

		public RouteWrapperCollection(WhsItemReceiveConsignment receiveConsignmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (receiveConsignmentBO != null)
			{
				LoadRoutings(receiveConsignmentBO, (l) => l.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			}
		}

		public RouteWrapperCollection(WhsItemReceiveASN receiveASNBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (receiveASNBO != null)
			{
				LoadRoutings(receiveASNBO, (l) => l.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			}
		}

		delegate void RouteSynthesizer();
		void AddFromTransportCollectionOrSynthesizeIfNoTransports(BaseJobDeclaration declaration, RouteSynthesizer synthesizeRoutes)
		{
			LoadRoutings(declaration, o => false);
			if (Count == 0)
			{
				synthesizeRoutes();
			}
		}

		void AddFromPlannedPortsForShipment(CommonShipment shipmentBO)
		{
			if (!shipmentBO.JS_RL_NKLoadPort.IsEmpty || !shipmentBO.JS_RL_NKDischargePort.IsEmpty)
			{
				Add(new RouteWrapper(1, shipmentBO.JS_RL_NKLoadPort, ZDateTime.Empty, shipmentBO.JS_RL_NKDischargePort, ZDate.Empty, Factory));
			}
		}

		#region Order Constructor

		public RouteWrapperCollection(Order orderBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: base(factory)
		{
			if (orderBO != null)
			{
				if (orderBO.Shipment != null)
				{
					ForwardingConsol consol = FreightWrapperFromShipment.New(orderBO.Shipment, Factory)[0].Consol;
					if (consol != null)
					{
						if (routingLevel == RoutingLevel.Consol)
						{
							LoadRoutings(consol, consol.Transports.MostInterestingTransport);
						}
						else if (routingLevel == RoutingLevel.Shipment)
						{
							LoadRoutings(orderBO.Shipment, consol.Transports.MostInterestingTransport);
						}
					}
				}
				else if (orderBO.Declaration != null)
				{
					FreightWrapper declarationWrapper = FreightWrapperFromDeclaration.New(orderBO.Declaration, Factory)[0];
					AddRange(routingLevel == RoutingLevel.Consol ?
						declarationWrapper.ConsolRoutes :
						declarationWrapper.ShipmentRoutes);
				}
				else
				{
					OrderConsolRouteDataCollection consolRoutes = new OrderConsolRouteDataCollection();
					consolRoutes.AddRemovingDuplicates(orderBO.JD_RV_NKDepartureVessel, orderBO.JD_DepartureVoyage, orderBO.JD_Milestone_E_DEP, orderBO.JD_E_ARV_1stIntermediate);
					consolRoutes.AddRemovingDuplicates(orderBO.JD_RV_NKIntermediateVessel, orderBO.JD_IntermediateVoyage, orderBO.JD_E_DEP_2, orderBO.JD_E_ARV_2ndIntermediate);
					consolRoutes.AddRemovingDuplicates(orderBO.JD_RV_NKArrivalVessel, orderBO.JD_ArrivalVoyage, orderBO.JD_E_DEP_3, orderBO.JD_Milestone_E_ARV);
					consolRoutes.SetOuterDatesAndPorts(orderBO.JD_RL_NKPortOfLoading, orderBO.JD_RL_NKPortOfDischarge, orderBO.JD_Milestone_E_ARV);

					ZString transportMode = GetOrderTransportMode(orderBO);
					OrgHeader carrier = Factory.Load<OrgHeader>(orderBO.JD_OH_Carrier);

					if (consolRoutes.Count > 0)
					{
						int currentRoute = 0;
						foreach (OrderConsolRouteData route in consolRoutes)
						{
							Add(new RouteWrapper(++currentRoute, transportMode, ZString.Empty, route.VesselName, route.Voyage, route.DeparturePort, null,
								route.DepartureDate, ZDateTime.Empty, route.ArrivalPort, null, route.ArrivalDate, ZDateTime.Empty,
								ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty,
								ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, carrier, null, ZString.Empty, null, factory));
						}
					}
					else
					{
						Add(new RouteWrapper(1, transportMode, ZString.Empty, ZString.Empty, orderBO.JD_RL_NKPortOfLoading,
							orderBO.JD_RL_NKPortOfDischarge, orderBO.JD_Milestone_E_DEP, orderBO.JD_Milestone_E_ARV, factory));
					}

					mostInterestingTransport = Count > 0 ? this[0] : null;
				}
			}
		}

		ZString GetOrderTransportMode(Order order)
		{
			if (order.IsSeaTransport)
			{
				return Constants.TransportModes.Sea;
			}
			else
			{
				return order.JD_TransportMode;
			}
		}

		class OrderConsolRouteDataCollection : List<OrderConsolRouteData>
		{
			public void AddRemovingDuplicates(ZString vesselName, ZString voyage, ZDateTime departureDate, ZDateTime arrivalDate)
			{
				if (!(vesselName.IsEmpty && voyage.IsEmpty) && this.Find(delegate(OrderConsolRouteData data)
				{ return data.VesselName == vesselName && data.Voyage == voyage; }) == null)
				{
					Add(new OrderConsolRouteData(vesselName, voyage, departureDate, arrivalDate));
				}
			}

			public void SetOuterDatesAndPorts(ZString loadPort, ZString dischargePort, ZDateTime dischargeDate)
			{
				if (Count != 0)
				{
					this[0].DeparturePort = loadPort;
					this[Count - 1].ArrivalPort = dischargePort;
					this[Count - 1].ArrivalDate = dischargeDate;
				}
			}
		}

		class OrderConsolRouteData
		{
			public OrderConsolRouteData(ZString vesselName, ZString voyage, ZDateTime departureDate, ZDateTime arrivalDate)
			{
				VesselName = vesselName;
				Voyage = voyage;
				DepartureDate = departureDate;
				ArrivalDate = arrivalDate;
			}

			public readonly ZString VesselName;
			public readonly ZString Voyage;
			public readonly ZDateTime DepartureDate;
			public ZDateTime ArrivalDate;
			public ZString DeparturePort;
			public ZString ArrivalPort;
		}

		#endregion

		#region Cartage Constructor

		public RouteWrapperCollection(CommonCartage cartageBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: base(factory)
		{
			if (cartageBO != null)
			{
				if (routingLevel == RoutingLevel.Consol)
				{
					if (!cartageBO.Vessel.IsEmpty || !cartageBO.VoyageFlight.IsEmpty
						|| !cartageBO.PortOfLoading.IsEmpty || !cartageBO.PortOfDischarge.IsEmpty)
					{
						ZString transportMode = GetCartageTransportMode(cartageBO);
						mostInterestingTransport = new RouteWrapper(1, transportMode, cartageBO.Vessel,
							cartageBO.VoyageFlight, cartageBO.PortOfLoading, cartageBO.PortOfDischarge,
							cartageBO.E_DEP, cartageBO.E_ARV, factory);
						Add(mostInterestingTransport);
					}
				}
				else if (routingLevel == RoutingLevel.Shipment)
				{
					int currentRoute = 0;
					CartageShipmentRouteDataCollection shipmentRoutes = new CartageShipmentRouteDataCollection();
					shipmentRoutes.Add(cartageBO.FirstDocAddress, cartageBO.SecondDocAddress);
					shipmentRoutes.Add(cartageBO.SecondDocAddress, cartageBO.ThirdDocAddress);
					shipmentRoutes.Add(cartageBO.ThirdDocAddress, cartageBO.FourthDocAddress);
					shipmentRoutes.SetOuterDates(cartageBO.JJ_EstimatedPickup, cartageBO.JJ_EstimatedDelivery);

					foreach (CartageShipmentRouteData route in shipmentRoutes)
					{
						Add(new RouteWrapper(++currentRoute, route.LoadAddress, route.DestAddress, route.DepartureDate, route.ArrivalDate, factory));
					}
				}
			}
		}

		class CartageShipmentRouteDataCollection : List<CartageShipmentRouteData>
		{
			public void Add(JobDocAddress loadAddress, JobDocAddress destAddress)
			{
				if (loadAddress != null && destAddress != null && !loadAddress.IsEmpty && !destAddress.IsEmpty)
				{
					Add(new CartageShipmentRouteData(loadAddress, destAddress, ZDateTime.Empty, ZDateTime.Empty));
				}
			}

			public void SetOuterDates(ZDateTime departureDate, ZDateTime arrivalDate)
			{
				if (Count != 0)
				{
					this[0].DepartureDate = departureDate;
					this[Count - 1].ArrivalDate = arrivalDate;
				}
			}
		}

		class CartageShipmentRouteData
		{
			public CartageShipmentRouteData(JobDocAddress loadAddress, JobDocAddress destAddress, ZDateTime departureDate, ZDateTime arrivalDate)
			{
				LoadAddress = loadAddress;
				DestAddress = destAddress;
				DepartureDate = departureDate;
				ArrivalDate = arrivalDate;
			}

			public readonly JobDocAddress LoadAddress;
			public readonly JobDocAddress DestAddress;
			public ZDateTime DepartureDate;
			public ZDateTime ArrivalDate;
		}

		ZString GetCartageTransportMode(CommonCartage cartage)
		{
			var result = ZString.Empty;

			if (cartage == null)
			{
				return result;
			}

			if (cartage.IsSea)
			{
				result = Constants.TransportModes.Sea;
			}
			else if (cartage.IsAir)
			{
				result = Constants.TransportModes.Air;
			}
			else if (cartage.IsRail)
			{
				result = Constants.TransportModes.Rail;
			}
			else if (cartage.IsRoad)
			{
				result = Constants.TransportModes.Road;
			}

			return result;
		}

		#endregion

		#region Container Constructor

		public RouteWrapperCollection(CommonContainer containerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			JobSailing sailing = containerBO.Sailing;

			if (sailing != null)
			{
				ZString transportMode = containerBO.Consol != null ? containerBO.Consol.TransportMode : sailing.JX_TransportMode;
				mostInterestingTransport = new RouteWrapper(1, transportMode, ZString.Empty,
					sailing.JX_JV_NKVessel, sailing.JX_JV_VoyageFlight,
					sailing.JX_JA_RL_NKPortOfLoading, null, sailing.JX_JA_E_DEP, sailing.JX_JA_A_DEP,
					sailing.JX_JB_RL_NKPortOfDischarge, null, sailing.JX_JB_E_ARV, sailing.JX_JB_A_ARV,
					sailing.JX_JB_CTOAvailabilityDate,
					sailing.JX_JA_CTOReceivalCommences, sailing.JX_DepotReceivalCommences, sailing.JX_JA_DGFCLReceivalCommences,
					sailing.JX_JA_CTOCutOff, sailing.JX_DepotCutOff, sailing.JX_JA_DGFCLCutOff,
					sailing.JX_DepotAvailabilityDate,
					sailing.JX_JB_CTOStorageDate, sailing.JX_DepotStorageDate,
					null, null, ZString.Empty, sailing.Voyage,
					factory);

				Add(mostInterestingTransport);
			}
		}

		#endregion

		void LoadRoutings(IRoutingSupport routingSupporter, Routing mostInterestingLeg)
		{
			LoadRoutings(routingSupporter, (l) => l == mostInterestingLeg);
		}

		protected void LoadRoutings(IRoutingSupport routingSupporter, Predicate<Routing> mostInterestingPredicate)
		{
			TransportOrderHelper routings = new TransportOrderHelper(routingSupporter.TransportsIncludingRelated);
			Routing routingsImportLeg = routings.ImportLeg;
			Routing routingsExportLeg = routings.ExportLeg;
			Routing routingsLoadingLeg = routings.FirstLegWithTransportMode(EffectiveFirstMode(routingSupporter.TransportMode));
			Routing routingsDischargeLeg = routings.LastLegWithTransportMode(EffectiveLastMode(routingSupporter.TransportMode), null, ZString.Empty);
			foreach (Routing routing in routings)
			{
				RouteWrapper routeWrapper = new RouteWrapper(routing, Factory);
				Add(routeWrapper);

				if (routing == routingsImportLeg)
				{
					importRoute = routeWrapper;
				}
				else if (routing == routingsExportLeg)
				{
					exportRoute = routeWrapper;
				}
				if (routing == routingsLoadingLeg)
				{
					loadingRoute = routeWrapper;
				}
				if (routing == routingsDischargeLeg)
				{
					dischargeRoute = routeWrapper;
				}
				if (mostInterestingTransport == null && mostInterestingPredicate(routing))
				{
					mostInterestingTransport = routeWrapper;
				}
			}
		}

		ZString EffectiveFirstMode(ZString mode)
		{
			if (mode == Core.Constants.TransportModes.AirSea)
			{
				return Core.Constants.TransportModes.Air;
			}
			else if (mode == Core.Constants.TransportModes.SeaAir)
			{
				return Core.Constants.TransportModes.Sea;
			}
			else
			{
				return mode;
			}
		}

		ZString EffectiveLastMode(ZString mode)
		{
			if (mode == Core.Constants.TransportModes.AirSea)
			{
				return Core.Constants.TransportModes.Sea;
			}
			else if (mode == Core.Constants.TransportModes.SeaAir)
			{
				return Core.Constants.TransportModes.Air;
			}
			else
			{
				return mode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is a Development Keyword")]
		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (index.EqualsIgnoringCase("mostinteresting"))
			{
				return mostInterestingTransport;
			}
			else if (index.EqualsIgnoringCase("import"))
			{
				return importRoute;
			}
			else if (index.EqualsIgnoringCase("export"))
			{
				return exportRoute;
			}
			else if (index.EqualsIgnoringCase("loading") || index.EqualsIgnoringCase("customs loading"))
			{
				return loadingRoute;
			}
			else if (index.EqualsIgnoringCase("discharge") || index.EqualsIgnoringCase("customs discharge"))
			{
				return dischargeRoute;
			}

			return base.GetRow(index);
		}

		class Tuple
		{
			public string Port { get; set; }
			public ZDateTime SeqDateTime { get; set; }
			public VoyageOrigin Origin { get; set; }
			public VoyageDestination Destination { get; set; }
		}

		RouteWrapper importRoute;
		RouteWrapper exportRoute;
		RouteWrapper mostInterestingTransport;
		RouteWrapper loadingRoute;
		RouteWrapper dischargeRoute;
	}
}
