using System;
using System.Drawing;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IAffinity
	{
		Guid AffinityPK { get; set; }
		string Name { get; set; }
		Color Colour { get; set; }
		int AllowedConcurrency { get; set; }
	}
}
