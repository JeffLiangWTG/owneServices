using System.Drawing;
using NUnit.Framework;

namespace CargoWise.Common.Drawing.Colors
{
	public class ColourHelperTest : TestCase
	{
		public void TestMakeGray()
		{
			var initial = Color.Green;
			var actual = ColourHelper.MakeGrayscale(initial);
			AssertColorEquals(Color.FromArgb(64, 64, 64), actual);
		}

		public void TestMakeGrayExistingGray()
		{
			var initial = Color.Gray;
			var actual = ColourHelper.MakeGrayscale(initial);
			AssertColorEquals(initial, actual);
		}

		public void TestShiftBrightnessDarker()
		{
			var initial = Color.Green;
			var actual = ColourHelper.ShiftBrightness(initial, -0.1f);
			AssertColorEquals(Color.FromArgb(255, 0, 77, 0), actual);
		}

		public void TestShiftBrightnessDarkerBlack()
		{
			var initial = Color.Black;
			var actual = ColourHelper.ShiftBrightness(initial, -0.1f);
			AssertColorEquals(initial, actual);
		}

		public void TestShiftBrightnessLighter()
		{
			var initial = Color.Green;
			var actual = ColourHelper.ShiftBrightness(initial, 0.1f);
			AssertColorEquals(Color.FromArgb(255, 0, 179, 0), actual);
		}

		public void TestShiftBrightnessLighterWhite()
		{
			var initial = Color.White;
			var actual = ColourHelper.ShiftBrightness(initial, 0.1f);
			AssertColorEquals(initial, actual);
		}
	}
}