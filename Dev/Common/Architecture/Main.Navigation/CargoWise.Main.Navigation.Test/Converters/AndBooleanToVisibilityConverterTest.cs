using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class AndBooleanToVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new AndBooleanToVisibilityConverter();
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { true, true }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { false, false }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { false, true }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { true, false }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { null }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { "blah", "blah" }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new AndBooleanToVisibilityConverter();
		var unsetValues = new[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack(Visibility.Hidden, new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack(Visibility.Visible, new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack(Visibility.Collapsed, new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack(null, new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack("blah", new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
		AssertArrayEqualsByElements("Unset value", unsetValues, converter.ConvertBack(1, new[] { typeof(bool), typeof(bool) }, null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
