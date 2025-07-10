using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class LoadPortToRouteEntrySynchroniser : BusinessObjectSynchroniser
{
	public LoadPortToRouteEntrySynchroniser(RouteEntry destination, Transport source)
		: base(destination, source)
	{
	}

	protected new Transport Source { get => (Transport)base.Source; }

	public new RouteEntry Destination { get => (RouteEntry)base.Destination; }

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();

		Synchronisers.Add(new FieldSynchroniser(Destination.CY_CodeInfo, Source.JW_RL_NKLoadPortInfo));
		Destination.ReadOnly = true;
	}

	protected override void UnHookSynchronisers()
	{
		base.UnHookSynchronisers();
		Destination.ReadOnly = false;
	}
}
