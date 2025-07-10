using System;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class ShortcutToolTipConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new ShortcutToolTipConverter();
		var conversion = converter.Convert(3, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture).ToString();
		Assert("returned string did not contain the correct key combination: Ctrl+F3.", conversion.Contains("Ctrl+F3"));
		Assert("returned string did not contain the correct key combination: Ctrl+Shift+F3.", conversion.Contains("Ctrl+Shift+F3"));
		conversion = converter.Convert(7, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture).ToString();
		Assert("returned string did not contain the correct key combination: Ctrl+F7.", conversion.Contains("Ctrl+F7"));
		Assert("returned string did not contain the correct key combination: Ctrl+Shift+F7.", conversion.Contains("Ctrl+Shift+F7"));
		AssertNull(converter.Convert("3", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
		AssertNull(converter.Convert("Bob", typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertExceed()
	{
		var converter = new ShortcutToolTipConverter();
		var conversion = converter.Convert(13, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture).ToString();
		Assert("returned string shouldn't contain : Ctrl+F13.", !conversion.Contains("Ctrl+F13"));
	}

	public void TestConvertBack()
	{
		var converter = new ShortcutToolTipConverter();
		AssertExceptionThrown(typeof(NotImplementedException), () => converter.ConvertBack(null, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
