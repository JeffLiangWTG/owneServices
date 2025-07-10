using System;
using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkActionAccessibility
	{
		IEnumerable<INetworkActionDenialReason> DenialReasons { get; }
		bool IsAllowed { get; }

		INetworkActionAccessibility Union(INetworkActionAccessibility second);
		INetworkActionAccessibility UnionIfAllowed(Func<INetworkActionAccessibility> second);
	}
}