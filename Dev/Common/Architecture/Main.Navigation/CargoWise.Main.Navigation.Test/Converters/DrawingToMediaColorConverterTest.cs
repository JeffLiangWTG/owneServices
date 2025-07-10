using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class DrawingToMediaColorConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new DrawingToMediaColorConverter();
		AssertEquals("Invalid value passed", System.Windows.Media.Colors.White, converter.Convert("blah", typeof(object), null, CultureInfo.CurrentCulture));
		AssertEquals("Null value passed", System.Windows.Media.Colors.White, converter.Convert(null, typeof(object), null, CultureInfo.CurrentCulture));
		AssertColorsAreTheSame((System.Windows.Media.Color)converter.Convert(System.Drawing.Color.FromArgb(10, 20, 30, 40), typeof(object), null, CultureInfo.CurrentCulture), System.Drawing.Color.FromArgb(10, 20, 30, 40));
	}

	public void TestConvertBack()
	{
		var converter = new DrawingToMediaColorConverter();
		AssertEquals("Invalid value passed", System.Drawing.Color.White, converter.ConvertBack("blah", typeof(object), null, CultureInfo.CurrentCulture));
		AssertEquals("Null value passed", System.Drawing.Color.White, converter.ConvertBack(null, typeof(object), null, CultureInfo.CurrentCulture));
		AssertColorsAreTheSame(System.Windows.Media.Color.FromArgb(10, 20, 30, 40), (System.Drawing.Color)converter.ConvertBack(System.Windows.Media.Color.FromArgb(10, 20, 30, 40), typeof(object), null, CultureInfo.CurrentCulture));
	}

	void AssertColorsAreTheSame(System.Windows.Media.Color color1, System.Drawing.Color color2)
	{
		AssertEquals("Alpha", color1.A, color2.A);
		AssertEquals("Red", color1.R, color2.R);
		AssertEquals("Green", color1.G, color2.G);
		AssertEquals("Blue", color1.B, color2.B);
	}
}
