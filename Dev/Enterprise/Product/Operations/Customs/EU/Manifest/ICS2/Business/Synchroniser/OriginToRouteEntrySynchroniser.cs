using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class OriginToRouteEntrySynchroniser : BusinessObjectSynchroniser
{
	public OriginToRouteEntrySynchroniser(RouteEntry destination, ForwardingShipment source)
		: base(destination, source)
	{
	}

	protected new ForwardingShipment Source { get => (ForwardingShipment)base.Source; }

	public new RouteEntry Destination { get => (RouteEntry)base.Destination; }

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();

		Synchronisers.Add(new FieldSynchroniser(Destination.CY_CodeInfo, Source.Origin.CodeInfo));
		Destination.ReadOnly = true;
	}

	protected override void UnHookSynchronisers()
	{
		base.UnHookSynchronisers();
		Destination.ReadOnly = false;
	}
}
