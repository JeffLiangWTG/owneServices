using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Customs.EU.Manifest.ICS2.Business.SynchroniserHelper;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class RouteEntryCollectionSynchroniser : BusinessObjectCollectionSynchroniser
{
	public RouteEntryCollectionSynchroniser(ForwardingConsol source, AsycudaManifestHeader destination)
		: base(source, destination)
	{
	}

	protected new ForwardingConsol Source => (ForwardingConsol)base.Source;
	protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

	protected override void HookElementSynchronisers()
	{
		var hookedItineraries = new HashSet<RouteEntry>();

		var sequenceNumberCalculator = Destination.Itinerary.SequenceNumberCalculator;
		using (sequenceNumberCalculator.GetLineNumberSuspender())
		{
			if (Source.IsSea)
			{
				HookItinerariesFromShipmentOrigins((short)(hookedItineraries.Count + 1)).ForEach(i => hookedItineraries.Add(i));
				if (HookItineraryFromFirstNotEULoadPort((short)(hookedItineraries.Count + 1)) is { } itinerary)
				{
					hookedItineraries.Add(itinerary);
				}
			}

			HookItinerariesFromTransports((short)(hookedItineraries.Count + 1)).ForEach(i => hookedItineraries.Add(i));
			if (Source.IsSea)
			{
				HookItinerariesFromShipmentDestinations((short)(hookedItineraries.Count + 1)).ForEach(i => hookedItineraries.Add(i));
			}

			foreach (var itineraryToDelete in Destination.Itinerary.Except(hookedItineraries).ToArray())
			{
				Destination.Itinerary.RemoveAndDelete(itineraryToDelete);
			}
		}
		sequenceNumberCalculator.ReCalculateAll();
	}

	IEnumerable<RouteEntry> HookItinerariesFromShipmentDestinations(short orderSeed)
	{
		var existingTransports = GetExistingtransports();
		var destinationShipments = Source.Shipments.Cast<ForwardingShipment>()
			.Where(b => b.Destination is { } destination && !existingTransports.Contains(destination.Code));
		foreach (var destinationShipment in destinationShipments)
		{
			var synchroniser = ElementSynchronisers.FindMatchingSource<DestinationToRouteEntrySynchroniser>(destinationShipment);
			RouteEntry itinerary;
			if (synchroniser is null || synchroniser.Destination.IsDeleted)
			{
				itinerary = FindItineraryOrAddNew(orderSeed, destinationShipment.Destination.Code);
				synchroniser = new DestinationToRouteEntrySynchroniser(itinerary, destinationShipment);
				ElementSynchronisers.Add(synchroniser);
			}
			else
			{
				itinerary = synchroniser.Destination;
			}

			synchroniser.SetEnabled(IsEnabled, DetectEnabled);
			itinerary.CY_Order = orderSeed++;
			yield return itinerary;
		}
	}

	IEnumerable<RouteEntry> HookItinerariesFromTransports(short orderSeed)
	{
		foreach (var transport in Source.Transports.Cast<Transport>().OrderBy(t => t.JW_LegOrder))
		{
			var synchroniser = ElementSynchronisers.FindMatchingSource<DischargePortToRouteEntrySynchroniser>(transport);
			RouteEntry itinerary;
			if (synchroniser is null || synchroniser.Destination.IsDeleted)
			{
				itinerary = FindItineraryOrAddNew(orderSeed, transport.JW_RL_NKDiscPort);
				synchroniser = new DischargePortToRouteEntrySynchroniser(itinerary, transport);
				ElementSynchronisers.Add(synchroniser);
			}
			else
			{
				itinerary = synchroniser.Destination;
			}

			synchroniser.SetEnabled(IsEnabled, DetectEnabled);
			itinerary.CY_Order = orderSeed++;
			yield return itinerary;
		}
	}

	RouteEntry HookItineraryFromFirstNotEULoadPort(short orderSeed)
	{
		var loadPortToAddFirst = GetLoadPortToAddFirst();
		if (loadPortToAddFirst is not null)
		{
			var synchroniser = ElementSynchronisers.FindMatchingSource<LoadPortToRouteEntrySynchroniser>(loadPortToAddFirst);
			RouteEntry itinerary;
			if (synchroniser is null || synchroniser.Destination.IsDeleted)
			{
				itinerary = FindItineraryOrAddNew(orderSeed, loadPortToAddFirst.JW_RL_NKLoadPort);
				synchroniser = new LoadPortToRouteEntrySynchroniser(itinerary, loadPortToAddFirst);
				ElementSynchronisers.Add(synchroniser);
			}
			else
			{
				itinerary = synchroniser.Destination;
			}

			synchroniser.SetEnabled(IsEnabled, DetectEnabled);
			itinerary.CY_Order = orderSeed;
			return itinerary;
		}

		return null;
	}

	IEnumerable<RouteEntry> HookItinerariesFromShipmentOrigins(short orderSeed)
	{
		var existingTransports = GetExistingtransports();
		var originShipments = Source.Shipments.Cast<ForwardingShipment>()
			.Where(s => s.Origin is { } origin && !existingTransports.Contains(origin.Code));
		foreach (var originShipment in originShipments)
		{
			var synchroniser = ElementSynchronisers.FindMatchingSource<OriginToRouteEntrySynchroniser>(originShipment);
			RouteEntry itinerary;
			if (synchroniser is null || synchroniser.Destination.IsDeleted)
			{
				itinerary = FindItineraryOrAddNew(orderSeed, originShipment.Origin.Code);
				synchroniser = new OriginToRouteEntrySynchroniser(itinerary, originShipment);
				ElementSynchronisers.Add(synchroniser);
			}
			else
			{
				itinerary = synchroniser.Destination;
			}

			synchroniser.SetEnabled(IsEnabled, DetectEnabled);
			itinerary.CY_Order = orderSeed++;
			yield return itinerary;
		}
	}

	HashSet<ZString> GetExistingtransports()
	{
		var existingTransports = Source.Transports.Cast<Transport>().Select(t => t.JW_RL_NKDiscPort).ToHashSet();
		var loadPortToAddFirst = GetLoadPortToAddFirst();
		if (loadPortToAddFirst is not null)
		{
			existingTransports.Add(loadPortToAddFirst.JW_RL_NKLoadPort);
		}

		return existingTransports;
	}

	Transport GetLoadPortToAddFirst()
	{
		var loadPortRoute = GetFirstSeaTransportWithDischargePortInEuCountry(Source);
		if (loadPortRoute is not null && loadPortRoute.JW_LegOrder == 1)
		{
			var itineraryRecord = Source.Transports.Cast<Transport>().OrderBy(t => t.JW_LegOrder).FirstOrDefault(t => loadPortRoute.JW_RL_NKLoadPort == t.JW_RL_NKDiscPort);
			if (itineraryRecord is null)
			{
				return loadPortRoute;
			}
		}

		return null;
	}

	RouteEntry FindItineraryOrAddNew(short orderNumber, ZString code) => Destination.Itinerary.Cast<RouteEntry>().FirstOrDefault(i => !i.IsDeleted && i.CY_Order == orderNumber && i.CY_Code == code) ?? Destination.Itinerary.AddNew();

	protected override void HookEvents()
	{
		base.HookEvents();
		Source.JK_TransportModeInfo.ValueChanged -= Info_ValueChanged;
		Source.JK_TransportModeInfo.ValueChanged += Info_ValueChanged;
		HookSourceCollectionsMembersEvents();
	}

	protected override void UnHookEvents()
	{
		base.UnHookEvents();
		Source.JK_TransportModeInfo.ValueChanged -= Info_ValueChanged;
		foreach (Transport transport in Source.Transports)
		{
			transport.JW_LegOrderInfo.ValueChanged -= Info_ValueChanged;
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= Info_ValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged -= Info_ValueChanged;
		}
		foreach (ForwardingShipment shipment in Source.Shipments)
		{
			shipment.JS_RL_NKDestinationInfo.ValueChanged -= Info_ValueChanged;
			shipment.JS_RL_NKOriginInfo.ValueChanged -= Info_ValueChanged;
		}
	}

	protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
	{
		yield return Source.Shipments;
		yield return Source.Transports;
	}

	protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		base.Collection_CountChanged(sender, e);

		HookSourceCollectionsMembersEvents();
	}

	void Info_ValueChanged(object sender, EventArgs e)
	{
		Synchronise();
	}

	void HookSourceCollectionsMembersEvents()
	{
		foreach (Transport transport in Source.Transports)
		{
			transport.JW_LegOrderInfo.ValueChanged -= Info_ValueChanged;
			transport.JW_LegOrderInfo.ValueChanged += Info_ValueChanged;
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= Info_ValueChanged;
			transport.JW_RL_NKLoadPortInfo.ValueChanged += Info_ValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged -= Info_ValueChanged;
			transport.JW_RL_NKDiscPortInfo.ValueChanged += Info_ValueChanged;
		}

		foreach (ForwardingShipment shipment in Source.Shipments)
		{
			shipment.JS_RL_NKDestinationInfo.ValueChanged -= Info_ValueChanged;
			shipment.JS_RL_NKDestinationInfo.ValueChanged += Info_ValueChanged;
			shipment.JS_RL_NKOriginInfo.ValueChanged -= Info_ValueChanged;
			shipment.JS_RL_NKOriginInfo.ValueChanged += Info_ValueChanged;
		}
	}
}
