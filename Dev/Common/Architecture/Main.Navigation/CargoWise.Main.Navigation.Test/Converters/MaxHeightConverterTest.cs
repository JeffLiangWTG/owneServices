using System;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Converters.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class MaxHeightConverterTest : TestCase
{
	public void TestConvert()
	{
		var converter = new MaxHeightConverter();
		AssertEquals("Max Height Returned", 180.0, converter.Convert(new object[] { 2, 200.0 }, typeof(object), "10", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - 0 Returned", 0.0, converter.Convert(new object[] { "2" }, typeof(object), "10", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - 0 Returned", 0.0, converter.Convert(new object[] { "2", 200.0 }, typeof(object), "10", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - 0 Returned", 0.0, converter.Convert(new object[] { 2, "200.0" }, typeof(object), "10", System.Globalization.CultureInfo.CurrentCulture));
		AssertEquals("Bad Parameters - 0 Returned", 0.0, converter.Convert(new object[] { 2, 200.0 }, typeof(object), 10, System.Globalization.CultureInfo.CurrentCulture));
	}

	public void TestConvertBack()
	{
		var converter = new MaxHeightConverter();
		AssertExceptionThrown(typeof(NotImplementedException), () => converter.ConvertBack(null, new Type[] { typeof(object) }, null, System.Globalization.CultureInfo.CurrentCulture));
	}
}
