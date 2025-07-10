using System;
using System.Globalization;
using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

public class StringToVisibilityConverterTest : TestCase
{
	StringToVisibilityConverter converter;

	protected override void SetUp()
	{
		converter = new StringToVisibilityConverter();
	}

	public void TestConvert()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert("Not empty", typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertEquals("Visibility Hidden", Visibility.Hidden, converter.Convert("", typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertEquals("Visibility Hidden", Visibility.Hidden, converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture));

			converter.IsCollapsed = true;
			AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert("", typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvert_WhenValueIsNotString()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when Number", "The source must be a string", () => converter.Convert(5, typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when Boolean", "The source must be a string", () => converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvert_WhenTargetIsNotVisibilityType()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when object", "The target must be a Visibility", () => converter.Convert("Not empty", typeof(object), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when string", "The target must be a Visibility", () => converter.Convert("Not empty", typeof(string), null, CultureInfo.CurrentCulture));
		});
	}
}
