using System;
using CargoWise.Database.Abstractions;
using NUnit.Framework;

namespace CargoWise.Types.Tests.Misc
{
	public class ZDbValueConversionTests : TestCase
	{
		public void TestUnwrapsZBoolTrue()
		{
			Assert(conversion.TryUnwrapSimpleValue(ZBool.True, out var unwrappedTrue));
			AssertType<bool>(unwrappedTrue);
			Assert((bool)unwrappedTrue);
		}

		public void TestCachesBoxedTrueValues()
		{
			Assert(conversion.TryUnwrapSimpleValue(ZBool.True, out var unwrappedTrue1));
			Assert(conversion.TryUnwrapSimpleValue(ZBool.True, out var unwrappedTrue2));
			AssertSame(unwrappedTrue1, unwrappedTrue2);
		}

		public void TestUnwrapsZBoolFalse()
		{
			Assert(conversion.TryUnwrapSimpleValue(ZBool.False, out var unwrappedFalse));
			AssertType<bool>(unwrappedFalse);
			Assert(!((bool)unwrappedFalse));
		}

		public void TestCachesBoxedFalseValues()
		{
			Assert(conversion.TryUnwrapSimpleValue(ZBool.False, out var unwrappedFalse1));
			Assert(conversion.TryUnwrapSimpleValue(ZBool.False, out var unwrappedFalse2));
			AssertSame(unwrappedFalse1, unwrappedFalse2);
		}

		public void TestGetStandardTypeConverter()
		{
			AssertType<ZBlobTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZBlob) }));
			AssertType<ZBoolTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZBool) }));
			AssertType<ZByteTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZByte) }));
			AssertType<ZDateTimeOffsetTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZDateTimeOffset) }));
			AssertType<ZDateTimeTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZDateTime) }));
			AssertType<ZDateTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZDate) }));
			AssertType<ZDecimalTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZDecimal) }));
			AssertType<ZGeographyTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZGeography) }));
			AssertType<ZIntTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZInt) }));
			AssertType<ZLongTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZLong) }));
			AssertType<ZShortTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZShort) }));
			AssertType<ZStringTypeConverter>(conversion.TryGetConverterForValues(new[] { default(ZString) }));
		}

		public void TestGetTypeConverterForZGuidCanConvertInvalidZGuid()
		{
			var converter = conversion.TryGetConverterForValues(new[] { default(ZGuid) });
			AssertNotNull(converter);

			var value = converter.ConvertTo(ZGuid.Invalid, typeof(Guid));
			AssertEquals("Result should be the special value ZGuid.Invalid.", new ZGuid(value), ZGuid.Invalid);
		}

		IDbValueConversion conversion;

		protected override void SetUp()
		{
			base.SetUp();

			conversion = new ZDbValueConversion();
		}
	}
}
