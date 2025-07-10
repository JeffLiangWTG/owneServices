using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class NumericUtil
	{
		public static bool IsNumeric(Type type)
		{
			return
				type == typeof(byte) ||
				type == typeof(sbyte) ||
				type == typeof(short) ||
				type == typeof(ushort) ||
				type == typeof(int) ||
				type == typeof(uint) ||
				type == typeof(long) ||
				type == typeof(ulong) ||
				type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(decimal);
		}

		public static string ToStringWithDecimalPlaces(object number, int decimalPlaces)
		{
			Argument.NotNull(number, nameof(number)); // Suggested By ReviewBot 
			if (decimalPlaces != -1)
			{
				return ((IFormattable)number).ToString("0." + new string('0', decimalPlaces), null);
			}
			else
			{
				return number.ToString();
			}
		}

		public static object ChangeDecimalPlaces(object value, int decimalPlaces)
		{
			if (!(decimalPlaces >= 0 && decimalPlaces <= 15))
			{
				throw new ArgumentException("Invalid argument.", nameof(decimalPlaces));
			}

			object result = value;

			if (value is float)
			{
				result = (float)Math.Round((float)value, decimalPlaces); // Round is ok for our purposes
			}
			else if (value is double)
			{
				result = Math.Round((double)value, decimalPlaces); // Round is ok for our purposes
			}
			else if (value is decimal)
			{
				result = Math.Round((decimal)value, decimalPlaces); // Round is ok for our purposes
			}

			return result;
		}

		public static void IncreaseIndexSafe(ref byte index)
		{
			if (index < byte.MaxValue)
			{
				index++;
			}
			else
			{
				ErrorReporter.ReportOnce("IncreaseIndexExceededValueRange", "IncreaseIndex exceeded value range.");
			}
		}

		public static void DecreaseIndexSafe(ref byte index)
		{
			if (index > 0)
			{
				index--;
			}
			else
			{
				ErrorReporter.ReportOnce("DecreaseIndexExceededValueRange", "DecreaseIndex exceeded value range.");
			}
		}
	}
}
