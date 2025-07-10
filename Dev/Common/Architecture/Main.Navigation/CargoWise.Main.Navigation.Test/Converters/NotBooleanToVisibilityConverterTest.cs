using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class NotBooleanToVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new NotBooleanToVisibilityConverter();
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(true, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(false, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Visible", Visibility.Visible, converter.Convert(5, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Visible", Visibility.Visible, converter.Convert("blah", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Visibility Visible", Visibility.Visible, converter.Convert(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new NotBooleanToVisibilityConverter();
		Assert("True", (bool)converter.ConvertBack(Visibility.Collapsed, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		Assert("True", (bool)converter.ConvertBack(Visibility.Hidden, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		Assert("False", !(bool)converter.ConvertBack(Visibility.Visible, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		Assert("Bad parameters - False", !(bool)converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		Assert("Bad parameters - False", !(bool)converter.ConvertBack(5, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		Assert("Bad parameters - False", !(bool)converter.ConvertBack("blah", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
