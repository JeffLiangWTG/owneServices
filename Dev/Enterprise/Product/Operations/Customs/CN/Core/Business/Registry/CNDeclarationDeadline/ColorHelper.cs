using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class ColorHelper
	{
		const char Separator = ',';
		public static bool HasCorrectFormat(ZString rgb)
		{
			return rgb.ToString().Count(x => x == Separator) == 2;
		}

		public static bool IsValidColor(ZString rgb)
		{
			var result = false;
			var formatCorrect = HasCorrectFormat(rgb);
			if (formatCorrect)
			{
				var rgbArray = rgb.Split(new[] { Separator });
				var r = rgbArray[0];
				var g = rgbArray[1];
				var b = rgbArray[2];
				result = ValidColorValue(r) && ValidColorValue(g) && ValidColorValue(b);
			}

			return result;
		}

		public static bool ValidColorValue(string value)
		{
			var result = false;
			if (int.TryParse(value, out var i))
			{
				result = i >= 0 && i <= 255;
			}
			return result;
		}

		public static string GetRGBbyColor(Color color)
		{
			return GetStringByByte(color.R) + GetStringByByte(color.G) + GetStringByByte(color.B);
		}

		public static string GetStringByByte(byte rgb)
		{
			var result = rgb.ToString(CultureInfo.InvariantCulture);
			result = result.PadLeft(3, '0');
			return result;
		}

		public static ZString FormatColorValue(string value)
		{
			if (value.Length == 9 && !value.Contains(Separator))
			{
				value = value.Insert(3, Separator.ToString()).Insert(7, Separator.ToString());
			}

			return value;
		}
	}
}
