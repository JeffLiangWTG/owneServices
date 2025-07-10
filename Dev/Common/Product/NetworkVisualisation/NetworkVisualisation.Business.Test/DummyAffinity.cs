using System;
using System.Drawing;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class DummyAffinity : IAffinity
	{
		public DummyAffinity(Guid pK, string name, Color colour)
		{
			this.AffinityPK = pK;
			this.Name = name;
			this.Colour = colour;
		}

		public Guid AffinityPK { get; set; }

		public string Name { get; set; }

		public Color Colour { get; set; }

		public int AllowedConcurrency { get; set; }
	}
}
