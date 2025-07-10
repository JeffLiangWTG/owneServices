using System;

namespace CargoWise.Types.Tests
{
	using System.Globalization;

	class ZDateTimeOffsetTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZDateTimeOffset);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[]
			{
				new ValueMapping("", ZDateTimeOffset.Empty),
				new ValueMapping(DBNull.Value, ZDateTimeOffset.Empty),
				new ValueMapping(ZDateTimeOffset.Empty, ZDateTimeOffset.Empty),
				new ValueMapping(ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid),
				new ValueMapping(ZDateTime.Empty, ZDateTimeOffset.Empty),
				new ValueMapping(ZDateTime.Invalid, ZDateTimeOffset.Invalid),
				new ValueMapping(ZDate.Empty, ZDateTimeOffset.Empty),
				new ValueMapping(ZDate.Invalid, ZDateTimeOffset.Invalid),
				//ZDateTimeOffset -> ZDateTimeOffset
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//ZDateTimeOffset -> String -> ZDateTimeOffset, Invariant Culture
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//ZDateTimeOffset -> ZString -> ZDateTimeOffset, Invariant Culture
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new ZString(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//DateTimeOffset -> ZDateTimeOffset
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//ZDateTime -> ZDateTimeOffset (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new ZDateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Utc), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Local), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Unspecified), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Utc), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Local), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZDateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Unspecified), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10))),
				//DateTime -> ZDateTimeOffset (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new DateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Utc), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new DateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Local), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new DateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Unspecified), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new DateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Utc), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new DateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Local), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new DateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Unspecified), new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10))),
				//ZDate -> ZDateTimeOffset (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new ZDate(2016, 04, 01), new ZDateTimeOffset(2016, 04, 01, 00, 00, 00, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDate(2016, 04, 05), new ZDateTimeOffset(2016, 04, 05, 00, 00, 00, TimeSpan.FromHours(10)))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[]
			{
				new ValueMapping(ZDateTimeOffset.Empty, ""),
				new ValueMapping(ZDateTimeOffset.Invalid, "<Invalid>"),
				new ValueMapping(ZDateTimeOffset.Empty, ZDateTimeOffset.Empty),
				new ValueMapping(ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid),
				new ValueMapping(ZDateTimeOffset.Empty, ZDateTime.Empty),
				new ValueMapping(ZDateTimeOffset.Invalid, ZDateTime.Invalid),
				new ValueMapping(ZDateTimeOffset.Empty, ZDate.Empty),
				new ValueMapping(ZDateTimeOffset.Invalid, ZDate.Invalid),
				//ZDateTimeOffset -> ZDateTimeOffset
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//ZDateTimeOffset -> String, Invariant Culture
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)), new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)).ToDateTimeOffset().ToString(CultureInfo.InvariantCulture)),
				//ZDateTimeOffset -> DateTimeOffset
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(-14))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(10))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(20))),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20)), new DateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromMinutes(-20))),
				//ZDateTimeOffset -> ZDateTime (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new ZDateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Utc)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Unspecified)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.Zero), new ZDateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Utc)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10)), new ZDateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Unspecified)),
				//ZDateTimeOffset -> DateTime (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.Zero), new DateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Utc)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new DateTime(2016, 04, 01, 02, 03, 04, DateTimeKind.Unspecified)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.Zero), new DateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Utc)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10)), new DateTime(2016, 04, 05, 02, 03, 04, DateTimeKind.Unspecified)),
				//ZDateTimeOffset -> ZDate (using UNLOCO "AUSYD" for time zone information)
				new ValueMapping(new ZDateTimeOffset(2016, 04, 01, 02, 03, 04, TimeSpan.FromHours(11)), new ZDate(2016, 04, 01)),
				new ValueMapping(new ZDateTimeOffset(2016, 04, 05, 02, 03, 04, TimeSpan.FromHours(10)), new ZDate(2016, 04, 05)),
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(ZInt) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(ZInt) };
		}
	}
}
