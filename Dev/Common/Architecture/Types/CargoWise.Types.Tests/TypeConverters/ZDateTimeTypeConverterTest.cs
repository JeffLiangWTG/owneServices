using System;

namespace CargoWise.Types.Tests
{
	using System.Globalization;

	class ZDateTimeTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZDateTime);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[]
			{
				new ValueMapping("", ZDateTime.Empty),
				new ValueMapping("<Invalid>", ZDateTime.Invalid),
				new ValueMapping(new ZDateTime(2006, 1, 2, 3, 4, 5).ToDateTime().ToString(CultureInfo.InvariantCulture), new ZDateTime(2006, 1, 2, 3, 4, 5)),
				new ValueMapping(new ZString(new ZDateTime(2006, 1, 2, 3, 4, 5).ToDateTime().ToString(CultureInfo.InvariantCulture)), new ZDateTime(2006, 1, 2, 3, 4, 5)),
				new ValueMapping(DBNull.Value, ZDateTime.Empty),
				new ValueMapping(ZDateTime.Empty, ZDateTime.Empty),
				new ValueMapping(ZDateTime.Invalid, ZDateTime.Invalid),
				new ValueMapping(ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday),
				new ValueMapping(ZDate.Empty, ZDateTime.Empty),
				new ValueMapping(ZDate.Invalid, ZDateTime.Invalid),
				new ValueMapping(new ZDate(1971, 9, 18), new ZDateTime(1971, 9, 18)),
				new ValueMapping(new DateTime(1971, 9, 18), new ZDateTime(1971, 9, 18))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[]
			{
				new ValueMapping(ZDateTime.Empty, ""),
				new ValueMapping(ZDateTime.Invalid, "<Invalid>"),
				new ValueMapping(new ZDateTime(2006, 1, 2, 3, 4, 5), new ZDateTime(2006, 1, 2, 3, 4, 5).ToDateTime().ToString(CultureInfo.InvariantCulture)),
				new ValueMapping(ZDateTime.BrettsBirthday, new DateTime(1971, 9, 18)),
				new ValueMapping(ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday),
				new ValueMapping(ZDateTime.BrettsBirthday, ZDate.BrettsBirthday),
				new ValueMapping(new ZDateTime(2014, 6, 23, 23, 59, 59), new ZDate(2014, 6, 23)),
				new ValueMapping(ZDateTime.Invalid, ZDate.Invalid),
				new ValueMapping(ZDateTime.Empty, ZDate.Empty),
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
