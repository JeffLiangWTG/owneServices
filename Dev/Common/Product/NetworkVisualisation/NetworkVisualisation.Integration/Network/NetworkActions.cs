using System;

namespace CargoWise.NetworkVisualisation.Integration
{
	[Flags]
	public enum NetworkActions
	{
		None = 1 << 0,
		Show = 1 << 1,
		Hide = 1 << 2,
		StyleDiagram = 1 << 3,
		EditEntity = 1 << 4,
		AddChildEntities = 1 << 5,
		GenericActions = 1 << 6,
		Affinities = 1 << 7,
	}
}
