using System;

namespace CargoWise.Types.Tests
{
	using System.Globalization;
	using CargoWise.Types;

	class ZIntTypeConverterTest : TypeConverterTest
	{
		public void TestConvertTo_NormalInt()
		{
			var converter = ZIntTypeConverter.Instance;
			AssertExceptionThrown<InvalidCastException>("Specified cast is not valid.Type: System.Int32.Value: 5", (() => converter.ConvertTo(5, typeof(decimal))));
		}

		public void TestConvertTo_ZInt()
		{
			var converter = ZIntTypeConverter.Instance;
			AssertEquals((decimal)5, converter.ConvertTo((ZInt)5, typeof(decimal)));
		}

		public void TestConvertFromValueSafe()
		{
			var converter = ZIntTypeConverter.Instance;
			AssertEquals("Decimal can be handled.", 20, converter.ConvertFrom(null, null, "20.0"));
			AssertEquals("Variety of culture has been taken into account.", 5355, converter.ConvertFrom(null, new CultureInfo("bn-BD"), "5,355.789"));
			AssertEquals("Variety of culture has been taken into account.", 5355, converter.ConvertFrom(null, new CultureInfo("da-DK"), "5.355,789"));
			AssertExceptionThrown<FormatException>("Please check whether your input is of a valid format!", () => converter.ConvertFrom(null, null, "1123a.dd34354"));
		}
		public void TestConvertsNullToZero()
		{
			var converter = ZIntTypeConverter.Instance;
			AssertEquals("Null can be converted into zero.", "0", converter.ConvertTo(null, typeof(string)));
			AssertEquals("Null can be converted into zero.", 0, converter.ConvertTo(null, typeof(int)));
			AssertEquals("Null can be converted into zero.", 0, converter.ConvertTo(null, typeof(ZInt)));
			AssertEquals("Null can be converted into zero.", 0.0m, converter.ConvertTo(null, typeof(ZDecimal)));
			AssertEquals("Null can be converted into zero.", 0.0m, converter.ConvertTo(null, typeof(decimal)));
		}

		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZInt);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping(0, (ZInt)0),
				new ValueMapping(new ZShort((short)1), (ZInt)1),
				new ValueMapping(new ZByte(2), (ZInt)2),
				new ValueMapping((short)3, (ZInt)3),
				new ValueMapping("3", (ZInt)3),
				new ValueMapping(new ZString("3"), (ZInt)3),
				new ValueMapping(DBNull.Value, ZInt.Zero),
				new ValueMapping(123456, (ZInt)123456)
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping((ZInt)0, (ZInt)0),
				new ValueMapping((ZInt)0, 0),
				new ValueMapping((ZInt)10, (ZDecimal)10.0m),
				new ValueMapping((ZInt)10, 10.0m)
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(decimal), typeof(DateTime) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(ZString), typeof(DateTime) };
		}
	}
}
