using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZCalcEditPropertyTypeConverterTest : TestCase
	{
		public void TestConvertFromEnum()
		{
			AssertEquals("Convert(ZCalcEditBindToType.Byte)", typeof(ZByte), ZCalcEditPropertyTypeConverter.Convert(ZCalcEditPropertyType.Byte));
			AssertEquals("Convert(ZCalcEditBindToType.Decimal)", typeof(ZDecimal), ZCalcEditPropertyTypeConverter.Convert(ZCalcEditPropertyType.Decimal));
			AssertEquals("Convert(ZCalcEditBindToType.Int)", typeof(ZInt), ZCalcEditPropertyTypeConverter.Convert(ZCalcEditPropertyType.Int));
			AssertEquals("Convert(ZCalcEditBindToType.Short)", typeof(ZShort), ZCalcEditPropertyTypeConverter.Convert(ZCalcEditPropertyType.Short));
			AssertEquals("Convert(ZCalcEditBindToType.Long)", typeof(ZLong), ZCalcEditPropertyTypeConverter.Convert(ZCalcEditPropertyType.Long));
		}

		public void TestConvertFromType()
		{
			AssertEquals("Convert(typeof(ZByte))", ZCalcEditPropertyType.Byte, ZCalcEditPropertyTypeConverter.Convert(typeof(ZByte)));
			AssertEquals("Convert(typeof(ZDecimal))", ZCalcEditPropertyType.Decimal, ZCalcEditPropertyTypeConverter.Convert(typeof(ZDecimal)));
			AssertEquals("Convert(typeof(ZInt))", ZCalcEditPropertyType.Int, ZCalcEditPropertyTypeConverter.Convert(typeof(ZInt)));
			AssertEquals("Convert(typeof(ZShort))", ZCalcEditPropertyType.Short, ZCalcEditPropertyTypeConverter.Convert(typeof(ZShort)));
			AssertEquals("Convert(typeof(ZLong))", ZCalcEditPropertyType.Long, ZCalcEditPropertyTypeConverter.Convert(typeof(ZLong)));
		}
	}
}
