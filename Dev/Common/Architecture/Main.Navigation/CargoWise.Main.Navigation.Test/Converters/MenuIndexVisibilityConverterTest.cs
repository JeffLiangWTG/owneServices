using System;
using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class MenuIndexVisibilityConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new MenuIndexVisibilityConverter();
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 1 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 1, 1 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { "1" }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 1, "1", 1 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 1, 1, 1 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 1, -1, 1 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		// Two columns. Ensure details row is open when any of the first row items are selected
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 0, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 1, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 2, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		// First row details are collapsed when 2nd row items are selected
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 3, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 4, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 5, 0, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		// 2nd row details are collapsed when 1st row items are selected
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 0, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 1, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(new object[] { 2, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		// 2nd row details are visible when 2nd row items are selected
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 3, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 4, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(new object[] { 5, 1, 2 }, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new MenuIndexVisibilityConverter();
		AssertExceptionThrown(typeof(NotImplementedException), () => converter.ConvertBack(null, new Type[] { typeof(object) }, null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
