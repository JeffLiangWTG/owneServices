using System;

namespace CargoWise.Types.Tests
{
	class ZLongTypeConverterTest : TypeConverterTest
	{
		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping((long)0, (ZLong)0),
				new ValueMapping(3, (ZLong)3),
				new ValueMapping((ZInt)2, (ZLong)2),
				new ValueMapping((short)3, (ZLong)3),
				new ValueMapping((ZShort)1, (ZLong)1),
				new ValueMapping(new ZByte(2), (ZLong)2),
				new ValueMapping("3", (ZLong)3),
				new ValueMapping(new ZString("3"), (ZLong)3),
				new ValueMapping(DBNull.Value, ZLong.Zero),
				new ValueMapping(123456, (ZLong)123456)
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping((ZLong)0, (ZLong)0),
				new ValueMapping((ZLong)0, (long)0),
				new ValueMapping((ZLong)10, (ZDecimal)10.0m),
				new ValueMapping((ZLong)10, 10.0m)
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

		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZLong);
		}
	}
}
