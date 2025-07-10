using System;
using System.Globalization;
using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

public class BooleanToVisibilityConverterTest : TestCase
{
	BooleanToVisibilityConverter converter;

	protected override void SetUp()
	{
		converter = new BooleanToVisibilityConverter();
	}

	public void TestConvert()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Visibility Visible", Visibility.Visible, converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertEquals("Visibility Hidden", Visibility.Hidden, converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture));

			converter.IsCollapsed = true;
			AssertEquals("Visibility Collapsed", Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvert_WhenValueIsNotBoolean()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when Number", "The source must be a bool", () => converter.Convert(5, typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when String", "The source must be a bool", () => converter.Convert("blah", typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when Null",   "The source must be a bool", () => converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvert_WhenTargetIsNotVisibilityType()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when object", "The target must be a Visibility", () => converter.Convert(true, typeof(object), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when string", "The target must be a Visibility", () => converter.Convert(false, typeof(string), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvertBack()
	{
		CombineAssertions(() =>
		{
			Assert("when Visible", (bool)converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.CurrentCulture));
			Assert("when Hidden", !(bool)converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.CurrentCulture));
			Assert("when Collapsed", !(bool)converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvertBack_WhenValueIsNotVisibility()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when Object", "The source must be a Visibility", () => converter.ConvertBack(new object(), typeof(bool), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when Number", "The source must be a Visibility", () => converter.ConvertBack(5, typeof(bool), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when Null", "The source must be a Visibility", () => converter.ConvertBack(null, typeof(bool), null, CultureInfo.CurrentCulture));
		});
	}

	public void TestConvertBack_WhenTargetTypeNotBoolean()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<InvalidOperationException>("when Object", "The target must be a bool", () => converter.ConvertBack(Visibility.Visible, typeof(object), null, CultureInfo.CurrentCulture));
			AssertExceptionThrown<InvalidOperationException>("when Visibility", "The target must be a bool", () => converter.ConvertBack(Visibility.Visible, typeof(Visibility), null, CultureInfo.CurrentCulture));
		});
	}
}
