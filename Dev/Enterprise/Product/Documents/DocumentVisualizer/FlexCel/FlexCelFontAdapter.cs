using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[Immutable]
	public class FlexCelFontAdapter : IFont
	{
		public FlexCelFontAdapter(IFlexCelPalette palette, TFlxFont flxFont, byte flxRotation)
		{
			Argument.NotNull(palette, "palette");
			Argument.NotNull(flxFont, "flxFont");

			name = flxFont.Name;
			size = flxFont.Size20 / 20f;
			color = flxFont.Color.ToColor(palette);
			style = flxFont.Style.ToFontStyle();
			rotation = flxRotation;
		}

		public string Name
		{
			get { return name; }
		}

		readonly string name;

		public float Size
		{
			get { return size; }
		}

		readonly float size;

		public Color Color
		{
			get { return color; }
		}

		readonly Color color;

		public FontStyle Style
		{
			get { return style; }
		}

		readonly FontStyle style;

		public byte Rotation
		{
			get { return rotation; }
		}

		readonly byte rotation;
	}
}
