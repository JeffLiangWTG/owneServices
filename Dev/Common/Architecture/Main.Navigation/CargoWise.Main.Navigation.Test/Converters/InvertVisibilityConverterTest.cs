using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class InvertVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new InvertVisibilityConverter();
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(Visibility.Visible, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(Visibility.Collapsed, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(Visibility.Hidden, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.Convert(5, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.Convert("blah", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.Convert(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new InvertVisibilityConverter();
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.ConvertBack(Visibility.Visible, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.ConvertBack(Visibility.Collapsed, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.ConvertBack(Visibility.Hidden, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.ConvertBack(5, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.ConvertBack("blah", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad parameters - Unset Value", DependencyProperty.UnsetValue, converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
