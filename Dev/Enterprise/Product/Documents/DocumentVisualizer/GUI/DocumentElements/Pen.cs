using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.GUI
{
	public class Pen : IPen
	{
		public Pen()
		{
			DashStyle = DashStyle.Solid;
		}

		public Color Color { get; set; }
		public int Thickness { get; set; }
		public IReadOnlyCollection<float> DashPattern { get; set; }
		public DashStyle DashStyle { get; set; }
	}
}