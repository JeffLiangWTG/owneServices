using System;

namespace CargoWise.Types.Tests
{
	using System.Globalization;

	class ZTimeTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZTime);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[]
			{
				new ValueMapping("", ZTime.Empty),
				new ValueMapping("<Invalid>", ZTime.Invalid),
				new ValueMapping(new ZTime(3, 4).ToDateTime().ToString(CultureInfo.InvariantCulture), new ZTime(3, 4)),
				new ValueMapping(new ZString(new ZTime(3, 4).ToDateTime().ToString(CultureInfo.InvariantCulture)), new ZTime(3, 4)),
				new ValueMapping(DBNull.Value, ZTime.Empty),
				new ValueMapping(ZTime.Empty, ZTime.Empty),
				new ValueMapping(ZTime.Invalid, ZTime.Invalid),
				new ValueMapping(new DateTime(1971, 9, 18), new ZTime(0, 0)),
				new ValueMapping(new DateTime(1971, 9, 18, 12, 34, 0), new ZTime(12, 34)),
				new ValueMapping(new ZLong(new TimeSpan(23, 59, 0).Ticks), new ZTime(23, 59)),
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			var today = ZDateTime.Today;
			return new ValueMapping[]
			{
				new ValueMapping(ZTime.Empty, ""),
				new ValueMapping(ZTime.Invalid, "<Invalid>"),
				new ValueMapping(new ZTime(3, 4), new ZTime(3, 4).ToDateTime().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZTime(23, 59), new ZDateTime(today.Year, today.Month, today.Day, 23, 59, 0)),
				new ValueMapping(ZTime.Invalid, ZTime.Invalid),
				new ValueMapping(ZTime.Empty, ZTime.Empty),
				new ValueMapping(new ZTime(23, 59), new ZLong(new TimeSpan(23, 59, 0).Ticks)),
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(ZGeography) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(ZGeography) };
		}
	}
}
