using System;

namespace CargoWise.Types.Tests
{
	class ZByteTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZByte);
		}
		protected override ValueMapping[] GetConvertibleFromValues()
		{
			Guid guid = Guid.NewGuid();
			return new ValueMapping[] {
				new ValueMapping((byte)2, new ZByte(2)),
				new ValueMapping(DBNull.Value, ZByte.Zero),
				new ValueMapping("6", new ZByte(Convert.ToByte("6"))),
				new ValueMapping(new ZByte(2), new ZByte(2))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			Guid guid = Guid.NewGuid();
			return new ValueMapping[] {
				new ValueMapping(new ZByte(1), (byte)1),
				new ValueMapping(new ZByte(2), (ZByte)2),
				new ValueMapping(new ZByte(3), 3),
				new ValueMapping(new ZByte(4), (short)4),
				new ValueMapping(new ZByte(5), (decimal)5),
				new ValueMapping(new ZByte(6), (ZInt)6),
				new ValueMapping(new ZByte(7), (ZShort)7),
				new ValueMapping(new ZByte(8), (ZDecimal)8)
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(int) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(DateTime) };
		}
	}
}
