using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Common.Drawing.Colors;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	[Immutable]
	public struct DisplayItemTheme
	{
		public static DisplayItemTheme DefaultItem => new DisplayItemTheme(Color.Black, Color.White, Color.Black, Color.SkyBlue, FontStyle.Regular);
		public static DisplayItemTheme DefaultError => new DisplayItemTheme(Color.Red, Color.White, Color.Red, Color.White, FontStyle.Bold);
		public static DisplayItemTheme DefaultHeading => new DisplayItemTheme(
			DefaultItem.ForeColour, ColourHelper.ShiftBrightness(DefaultItem.BackColour, -0.20f),
			DefaultItem.ForeColour, ColourHelper.ShiftBrightness(DefaultItem.BackColour, -0.20f),
			FontStyle.Bold);

		public DisplayItemTheme(Color foreColour, Color backColour, Color selectedForeColour, Color selectedBackColour, FontStyle fontStyle)
		{
			ForeColour = foreColour;
			BackColour = backColour;
			SelectedForeColour = selectedForeColour;
			SelectedBackColour = selectedBackColour;
			FontStyle = fontStyle;
		}

		public Color ForeColour { get; }
		public Color BackColour { get; }
		public Color SelectedForeColour { get; }
		public Color SelectedBackColour { get; }
		public FontStyle FontStyle { get; }
	}
}
