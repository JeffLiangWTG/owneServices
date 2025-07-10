using System.Collections.Generic;
using System.Drawing;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NodeColors
	{
		public IEnumerable<ColorOffset> colors;
		public double angle;
		public double opacity;

		public NodeColors(IEnumerable<ColorOffset> colors, double angle, double opacity = 1)
		{
			this.colors = colors;
			this.angle = angle;
			this.opacity = opacity;
		}
	}

	public class ColorOffset
	{
		public Color color;
		public double offset;

		public ColorOffset(Color color, double offset)
		{
			this.color = color;
			this.offset = offset;
		}
	}
}
