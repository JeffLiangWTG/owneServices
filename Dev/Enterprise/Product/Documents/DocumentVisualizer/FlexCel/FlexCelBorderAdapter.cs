using System.Diagnostics;
using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("{Color.Name} {Style}")]
	[Immutable]
	public sealed class FlexCelBorderAdapter : IBorder
	{
		public FlexCelBorderAdapter(IFlexCelPalette palette, TFlxOneBorder flxBorder)
		{
			Argument.NotNull(palette, nameof(palette));
			Style = flxBorder.Style.ToBorder(); 
			Color = flxBorder.Color.ToColor(palette);
		}

		public BorderStyle Style { get; }
		public Color Color { get; }
	}
}