using System;
using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]

public class EqualityToCollapsedConverterTest : TestCase
{
	EqualityToCollapsedConverter converter;

	protected override void SetUp()
	{
		converter = new EqualityToCollapsedConverter();
	}

	public void TestConvert_WhenBadParametersSupplied_ReturnsVisible()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Single value passed", Visibility.Visible,
				converter.Convert(new object[] { null }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("Null passed as values array", Visibility.Visible,
				converter.Convert(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("boolean value passed", Visibility.Visible,
				converter.Convert(new object[] { true }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
		});
	}

	public void TestConvert_WhenValuesEqual_ReturnsCollapsed()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Both Null", Visibility.Collapsed,
				converter.Convert(new object[] { null, null }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("Case Insensitive", Visibility.Collapsed,
				converter.Convert(new object[] { "Text", "text" }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("Same texts", Visibility.Collapsed,
				converter.Convert(new object[] { "text", "text" }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
		});
	}
	public void TestConvert_WhenValuesUnequal_ReturnsVisible()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Only First Value Null", Visibility.Visible,
				converter.Convert(new object[] { null, "text2" }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("Only Second Value Null", Visibility.Visible,
				converter.Convert(new object[] { "text1", null }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
			AssertEquals("Different Texts", Visibility.Visible,
				converter.Convert(new object[] { "text1", "text2" }, typeof(object), null,
					System.Globalization.CultureInfo.CurrentCulture));
		});
	}

	public void TestConvertBack()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown(typeof(NotImplementedException),
				() => converter.ConvertBack(null, new Type[] { typeof(object) }, null,
					System.Globalization.CultureInfo.CurrentCulture));
		});
	}
}
