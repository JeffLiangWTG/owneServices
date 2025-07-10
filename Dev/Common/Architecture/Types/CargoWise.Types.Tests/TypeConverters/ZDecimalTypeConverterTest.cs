using System;

namespace CargoWise.Types.Tests
{
	class ZDecimalTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZDecimal);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new[] {
				new ValueMapping((byte)1, new ZDecimal(1)),
				new ValueMapping((ZByte)2, new ZDecimal(2)),
				new ValueMapping(3, new ZDecimal(3)),
				new ValueMapping((short)3, new ZDecimal(3)),
				new ValueMapping((long)3, new ZDecimal(3)),
				new ValueMapping((long)3, new ZDecimal(3)),
				new ValueMapping((short)4, new ZDecimal(4)),
				new ValueMapping((decimal)5, new ZDecimal(5)),
				new ValueMapping((float)5, new ZDecimal(5)),
				new ValueMapping((double)5, new ZDecimal(5)),
				new ValueMapping((ZInt)6, new ZDecimal(6)),
				new ValueMapping((ZShort)7, new ZDecimal(7)),
				new ValueMapping("3.2", (ZDecimal)3.2m),
				new ValueMapping(new ZString("5.6"), (ZDecimal)5.6m),
				new ValueMapping((ZDecimal)8, new ZDecimal(8)),
				new ValueMapping(DBNull.Value, ZDecimal.Zero),
				new ValueMapping((ZLong)3, new ZDecimal(3))
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new[] {
				new ValueMapping(new ZDecimal(1), (ZDecimal)1),
				new ValueMapping(new ZDecimal(2), (decimal)2)
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new[] { typeof(ZBlob) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new[] { typeof(ZInt), typeof(int), typeof(ZLong), typeof(long), typeof(byte), typeof(ZByte) };
		}

		public void TestConvertToThrowInvalidCastException()
		{
			var zdecimalConvert = ZDecimalTypeConverter.Instance;
			const double value = 34.5d;
#if NETFRAMEWORK
			const string expectedExceptionMessage = "Specified cast is not valid. (in ConvertTo, value = '34.5'; value.GetType() = 'System.Double'; destinationType = 'CargoWise.Types.ZDecimal')";
#else
			const string expectedExceptionMessage = "Unable to cast object of type 'System.Double' to type 'CargoWise.Types.ZDecimal'. (in ConvertTo, value = '34.5'; value.GetType() = 'System.Double'; destinationType = 'CargoWise.Types.ZDecimal')";
#endif
			AssertExceptionThrown(typeof(InvalidCastException), expectedExceptionMessage, () =>
			{
				zdecimalConvert.ConvertTo(null, null, value, typeof(string));
			});
		}

		public void TestConvertTo_FromValueIsNull()
		{
			var zdecimalConvert = ZDecimalTypeConverter.Instance;
			AssertEquals(ZDecimal.Zero, zdecimalConvert.ConvertTo(null, null, null, typeof(ZDecimal)));
			AssertEquals(decimal.Zero, zdecimalConvert.ConvertTo(null, null, null, typeof(decimal)));
		}
	}
}
