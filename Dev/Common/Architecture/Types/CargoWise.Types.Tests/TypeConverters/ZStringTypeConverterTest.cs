using System;

namespace CargoWise.Types.Tests
{
	class ZStringTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZString);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping("Hello", (ZString)"Hello"),
				new ValueMapping('H', (ZString)"H"),
				new ValueMapping(DBNull.Value, ZString.Empty),
				new ValueMapping(new ZGuid("00000000-0000-0000-0000-000000000000"), (ZString)"00000000-0000-0000-0000-000000000000"),
				new ValueMapping(new Guid("00000000-0000-0000-0000-000000000000"), (ZString)"00000000-0000-0000-0000-000000000000")
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping((ZString)"Hello", (ZString)"Hello"),
				new ValueMapping((ZString)"Hello", "Hello")
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(int), typeof(DateTime) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(int), typeof(DateTime) };
		}
	}
}
