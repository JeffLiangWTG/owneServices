using System;

namespace CargoWise.Common
{
	public static class ZTypeConstants
	{
		public static readonly int MinSmallDateTimeYearValue = 1900;
		public static readonly Guid InvalidGuid = new Guid("0000ffff-ffff-ffff-ffff-ffffffffffff");
		public static readonly Guid MissingGuid = new Guid("1111ffff-ffff-ffff-ffff-ffffffffffff");
		public static readonly string MissingGuidName = "Missing";
		public static readonly string InvalidGuidName = "Invalid";
	}

	public static class ZTypeUtils
	{
		public static decimal TruncateDecimal(decimal value, int decimals)
		{
			if (decimals < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(decimals));
			}

			decimal integerPart = decimal.Truncate(value);
			decimal nonIntegerPart = value - integerPart;
			decimal scale = (decimal)Math.Pow(10, decimals);

			return integerPart + (decimal.Truncate(scale * nonIntegerPart) / scale);
		}
	}
}
