using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class DummyChannel : IDiagramChannel
	{
		public DummyChannel(string name, int? height = null, Color? color = null)
		{
			Name = name;
			Height = height ?? 600;
			Color = color ?? Color.HotPink;
		}

		public string Name { get; }
		public int Height { get; }
		public Color Color { get; }

		public static IEnumerable<DummyChannel> Create(params string[] names)
		{
			return names.Select(n => new DummyChannel(n)).ToArray();
		}
	}
}
