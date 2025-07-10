using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI.HotSpotPictureBox
{
	public class HotSpot
	{
		readonly string code;
		readonly Color color;

		public HotSpot(string code, Color color)
		{
			this.code = code;
			this.color = color;
		}

		public string Code
		{
			get { return code; }
		}

		public Color Color
		{
			get { return color; }
		}

		public override string ToString()
		{
			return string.Format((NoResString)"HotSpot {0} - Color: {1}, {2}, {3}", code, color.R, color.G, color.B);
		}
	}
}
