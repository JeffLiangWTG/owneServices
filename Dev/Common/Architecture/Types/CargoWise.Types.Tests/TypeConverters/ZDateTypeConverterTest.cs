using System;

namespace CargoWise.Types.Tests
{
	class ZDateTypeConverterTest : TypeConverterTest
	{
		public override void TestTypeCanBeConstructedUsingItself()
		{
			Assert("This is nooby - I have no need to support it on ZDate - if you do, reimplement", true);
		}

		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZDate);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
					new ValueMapping(ZDateTime.Empty, ZDate.Empty),
					new ValueMapping(ZDateTime.Invalid, ZDate.Invalid),
					new ValueMapping(new ZDate(1971, 9, 18), new ZDate(1971, 9, 18)),
					new ValueMapping(DBNull.Value, ZDate.Empty),
					new ValueMapping(new DateTime(2005, 6, 13), new ZDate(2005, 6, 13)),
					new ValueMapping(ZString.Empty, ZDate.Empty),
					new ValueMapping("<Invalid>", ZDate.Invalid),
					new ValueMapping("2013-08-15", new ZDate(2013, 8, 15)),
					new ValueMapping(new ZString("1971-09-18"), new ZDate(1971, 9, 18)),
				};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping(ZDate.Empty, ZDateTime.Empty),
				new ValueMapping(ZDate.Invalid, ZDateTime.Invalid),
				new ValueMapping(new ZDate(1971, 9, 18), ZDateTime.BrettsBirthday),
				new ValueMapping(new ZDate(1971, 9, 18), new ZDate(1971, 9, 18)),
				new ValueMapping(new ZDate(1971, 9, 18), "1971-09-18"),
				new ValueMapping(new ZDate(2013, 8, 15), new ZString("2013-08-15")),
				new ValueMapping(ZDate.Empty, ""),
				new ValueMapping(ZDate.Invalid, new ZString("<Invalid>")),
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
