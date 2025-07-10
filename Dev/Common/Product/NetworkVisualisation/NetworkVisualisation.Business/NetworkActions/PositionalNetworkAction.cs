using System;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class PositionalNetworkAction : StaticNetworkAction
	{
		public PositionalNetworkAction(Action<Location> action = null, ResourceString name = null, ResourceString description = null, int group = 0, int groupIndex = 0)
			: base(name, description, group: group, groupIndex: groupIndex)
		{
			this.action = action;
		}

		readonly Action<Location> action;

		public void Execute(Location location)
		{
			action?.Invoke(location);
		}
	}
}
