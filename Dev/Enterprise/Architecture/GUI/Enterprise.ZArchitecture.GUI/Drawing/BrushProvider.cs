using System;
using System.Collections.Generic;
using System.Drawing;

namespace Enterprise.ZArchitecture.GUI
{
	public static class BrushProvider
	{
		public static Brush FromColor(Color color)
		{
			if (color.IsSystemColor)
			{
				return SystemBrushes.FromSystemColor(color);
			}
			else
			{
				if (!Brushes.TryGetValue(color, out var brush))
				{
					brush = new SolidBrush(color);
					Brushes[color] = brush;
				}
				return brush;
			}
		}

		static Dictionary<Color, Brush> Brushes { get { return brushes ?? (brushes = new Dictionary<Color, Brush>(16)); } }
		[ThreadStatic]
		static Dictionary<Color, Brush> brushes;
	}
}
