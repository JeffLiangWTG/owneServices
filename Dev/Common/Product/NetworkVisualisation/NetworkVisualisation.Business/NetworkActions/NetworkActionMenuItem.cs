using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	[DebuggerDisplay("Name: {Name}")]
	public class NetworkActionMenuItem
	{
		public NetworkActionMenuItem(
			string name,
			string tooltip,
			bool enabled,
			bool ticked,
			IEnumerable<NetworkActionMenuItem> items,
			INetworkAction action = null)
		{
			Name = name;
			Tooltip = tooltip;
			Enabled = enabled;
			Ticked = ticked;
			Items = items;
			Action = action;
		}

		public INetworkAction Action { get; }

		public string Name { get; }
		public string Tooltip { get; }
		public bool Enabled { get; }
		public bool Ticked { get; }

		public IEnumerable<NetworkActionMenuItem> Items { get; }
	}
}
