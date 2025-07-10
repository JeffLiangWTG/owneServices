using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Drawing.Colors;

namespace Enterprise.ZArchitecture.GUI.Drawing
{
	public class ButtonTheme
	{
		public ButtonTheme(
			Color foreColour, Color backColour,
			Color mouseOverForeColour, Color mouseOverBackColour,
			Color mouseDownForeColour, Color mouseDownBackColour,
			int borderSize)
		{
			ForeColour = foreColour;
			BackColour = backColour;
			MouseOverForeColour = mouseOverForeColour;
			MouseOverBackColour = mouseOverBackColour;
			MouseDownForeColour = mouseDownForeColour;
			MouseDownBackColour = mouseDownBackColour;

			BorderSize = borderSize;
		}

		public ButtonTheme(Color foreColor, Color backColor, int borderSize = 0)
			: this(foreColor, backColor,
				  foreColor, ColourHelper.ShiftBrightness(backColor, 0.1f),
				  foreColor, ColourHelper.ShiftBrightness(backColor, -0.1f),
				  borderSize)
		{ }

		public Color ForeColour { get; }
		public Color BackColour { get; }
		public Color MouseOverForeColour { get; }
		public Color MouseOverBackColour { get; }
		public Color MouseDownForeColour { get; }
		public Color MouseDownBackColour { get; }
		public int BorderSize { get; }

		public ButtonTheme AsInactive()
		{
			return new ButtonTheme
			(
				ColourHelper.MakeGrayscale(ForeColour),
				ColourHelper.MakeGrayscale(BackColour),
				ColourHelper.MakeGrayscale(MouseOverForeColour),
				ColourHelper.MakeGrayscale(MouseOverBackColour),
				ColourHelper.MakeGrayscale(MouseDownForeColour),
				ColourHelper.MakeGrayscale(MouseDownBackColour),
				BorderSize
			);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not shown to user")]
		public override string ToString()
		{
			return $"ForeColour={ForeColour} BackColour={BackColour} MouseOverForeColour={MouseOverForeColour} MouseOverBackColour={MouseOverBackColour} MouseDownForeColour={MouseDownForeColour} MouseDownBackColour={MouseDownBackColour} BorderSize={BorderSize}"; // not shown to users
		}
	}

	public static class ButtonThemeExtensions
	{
		public static void ApplyTheme(this Button button, ButtonTheme theme)
		{
			button.ForeColor = theme.ForeColour;
			button.BackColor = theme.BackColour;
			button.FlatAppearance.MouseOverBackColor = theme.MouseOverBackColour;
			button.FlatAppearance.MouseDownBackColor = theme.MouseDownBackColour;
			button.FlatAppearance.BorderSize = theme.BorderSize;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, theme.BackColour); //transparent;

			// not available in FlatButtonAppearance and we can't subclass it so we have to do it manually
			button.MouseDown += (s, e) => { button.ForeColor = theme.MouseDownForeColour; };
			button.MouseUp += (s, e) => { button.ForeColor = theme.MouseOverForeColour; };
			button.MouseEnter += (s, e) => { button.ForeColor = theme.MouseOverForeColour; };
			button.MouseLeave += (s, e) => { button.ForeColor = theme.ForeColour; };
		}
	}
}

#if DEBUG

namespace Enterprise.ZArchitecture.GUI.Drawing
{
	using NUnit.Framework;

	public class ButtonThemeTestHelpers : TestCase
	{
		public static void AssertButtonThemesEqual(ButtonTheme a, ButtonTheme b)
		{
			AssertColorEquals("ForeColor was different", a.ForeColour, b.ForeColour);
			AssertColorEquals("BackColor was different", a.BackColour, b.BackColour);
			AssertColorEquals("MouseOverForeColour was different", a.MouseOverForeColour, b.MouseOverForeColour);
			AssertColorEquals("MouseOverBackColour was different", a.MouseOverBackColour, b.MouseOverBackColour);
			AssertColorEquals("MouseDownForeColour was different", a.MouseDownForeColour, b.MouseDownForeColour);
			AssertColorEquals("MouseDownBackColour was different", a.MouseDownBackColour, b.MouseDownBackColour);
			AssertEquals("BorderSize was different", a.BorderSize, b.BorderSize);
		}

		public static  void AssertButtonHasThemeApplied(Button button, ButtonTheme theme)
		{
			// assert
			AssertColorEquals("ForeColour ", theme.ForeColour, button.ForeColor);
			AssertColorEquals("BackColour was different", theme.BackColour, button.BackColor);
			AssertColorEquals("MouseOverBackColour was different", theme.MouseOverBackColour, button.FlatAppearance.MouseOverBackColor);
			AssertColorEquals("MouseDownBackColour was different", theme.MouseDownBackColour, button.FlatAppearance.MouseDownBackColor);
			// implicitly used from theme data, no need for user to set them (at least for now)
			AssertEquals("BorderSize was different", theme.BorderSize, button.FlatAppearance.BorderSize);
			AssertColorEquals("BackColour was different", theme.BackColour, button.FlatAppearance.BorderColor = theme.BackColour);

			// mouse over event - unsure how to test these
			//AssertColorEquals("MouseOverForeColour was different", theme.MouseOverForeColour, button.ForeColor);

			// mouse down event - unsure how to test these
			//AssertColorEquals("MouseDownForeColour was different", theme.MouseDownForeColour, button.ForeColor);
		}
	}
}

#endif
