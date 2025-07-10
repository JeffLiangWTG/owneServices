using System.Drawing;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ColorHelperTest : TestCaseWithFactory
	{
		public void TestHasCorrectFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("valid color with 2 separators", true, ColorHelper.HasCorrectFormat("111,111,111"));
				AssertEquals("2 separators but invalid color value", true, ColorHelper.HasCorrectFormat("111,11,1111"));
				AssertEquals("No separator", true, !ColorHelper.HasCorrectFormat("111111111"));
				AssertEquals("more than 2 separators", true, !ColorHelper.HasCorrectFormat("111,111,1,1"));
			});
		}

		public void TestIsValidColor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("valid color with 2 separators", true, ColorHelper.IsValidColor("111,111,111"));
				AssertEquals("2 separators but invalid color value", true, !ColorHelper.IsValidColor("111,11,1111"));
				AssertEquals("No separator", true, !ColorHelper.IsValidColor("111111111"));
				AssertEquals("more than 2 separators", true, !ColorHelper.IsValidColor("111,111,1,1"));
			});
		}

		public void TestValidColorValue()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Non Numeric", false, ColorHelper.ValidColorValue("A"));
				AssertEquals("<0", false, ColorHelper.ValidColorValue("-1"));
				AssertEquals("=0", true, ColorHelper.ValidColorValue("0"));
				AssertEquals("(0,255)", true, ColorHelper.ValidColorValue("100"));
				AssertEquals("=255", true, ColorHelper.ValidColorValue("255"));
				AssertEquals(">255", false, ColorHelper.ValidColorValue("256"));
			});
		}

		public void TestGetRGBbyColor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Red", "255000000", ColorHelper.GetRGBbyColor(Color.Red));
				AssertEquals("LightSalmon", "255160122", ColorHelper.GetRGBbyColor(Color.LightSalmon));
				AssertEquals("LightYellow", "255255224", ColorHelper.GetRGBbyColor(Color.LightYellow));
			});
		}

		public void TestGetStringByByte()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Red.R", "255", ColorHelper.GetStringByByte(Color.Red.R));
				AssertEquals("Red.G", "000", ColorHelper.GetStringByByte(Color.Red.G));
			});
		}

		public void TestFormatColorValue()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Length < 9", "12345678", ColorHelper.FormatColorValue("12345678"));
				AssertEquals("Valid", "123,456,789", ColorHelper.FormatColorValue("123456789"));
				AssertEquals("Length > 9", "1234567891", ColorHelper.FormatColorValue("1234567891"));
				AssertEquals("Length = 9 but contains separator", "123,45678", ColorHelper.FormatColorValue("123,45678"));
			});
		}
	}
}
