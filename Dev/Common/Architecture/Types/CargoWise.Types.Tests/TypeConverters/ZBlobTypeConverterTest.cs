namespace CargoWise.Types.Tests
{
	using System;

	class ZBlobTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZBlob);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping(DBNull.Value, ZBlob.Empty),
				new ValueMapping(new byte[] { byte.MinValue, byte.MaxValue }, new ZBlob(new byte[] { byte.MinValue, byte.MaxValue })),
				new ValueMapping(new ZBlob(new byte[] { byte.MinValue, byte.MaxValue }), new ZBlob(new byte[] { byte.MinValue, byte.MaxValue }))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping(new ZBlob(new byte[] { byte.MinValue, byte.MaxValue }), new byte[] { byte.MinValue, byte.MaxValue })
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
