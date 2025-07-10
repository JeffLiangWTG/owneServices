using System;

namespace CargoWise.Types.Tests
{
	class ZShortTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZShort);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping((byte)1, new ZShort((short)1)),
				new ValueMapping((short)4, new ZShort((short)4)),
				new ValueMapping((ZByte)2, new ZShort((short)2)),
				new ValueMapping(DBNull.Value, ZShort.Zero),
				new ValueMapping((ZShort)7, new ZShort((short)7)),
				new ValueMapping((ZString)"123", new ZShort((short)123)),
				new ValueMapping("123", new ZShort((short)123))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			Guid guid = Guid.NewGuid();
			return new ValueMapping[] {
				new ValueMapping(new ZShort((short)4), (short)4),
				new ValueMapping(new ZShort((short)4), 4),
				new ValueMapping(new ZShort((short)7), (ZShort)7)
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(ZInt) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(ZBlob) };
		}

		public void TestConvertToThrowInvalidCastException()
		{
			ZShortTypeConverter zshortConvert = ZShortTypeConverter.Instance;
			int value = 34;
#if NETFRAMEWORK
			var expectedExceptionMessage = "Specified cast is not valid. (in ConvertTo, value = '34'; value.GetType() = 'System.Int32'; destinationType = 'CargoWise.Types.ZShort')";
#else
			var expectedExceptionMessage = "Unable to cast object of type 'System.Int32' to type 'CargoWise.Types.ZShort'. (in ConvertTo, value = '34'; value.GetType() = 'System.Int32'; destinationType = 'CargoWise.Types.ZShort')";
#endif

			AssertExceptionThrown(typeof(InvalidCastException), expectedExceptionMessage, () =>
			{
				zshortConvert.ConvertTo(null, null, value, typeof(string));
			});
		}
	}
}
