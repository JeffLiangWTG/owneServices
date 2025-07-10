using System;

namespace CargoWise.Types.Tests
{
	class ZBoolTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(ZBool);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping("true", ZBool.True),
				new ValueMapping("True", ZBool.True),
				new ValueMapping("TRUE", ZBool.True),
				new ValueMapping("false", ZBool.False),
				new ValueMapping("False", ZBool.False),
				new ValueMapping("FALSE", ZBool.False),
				new ValueMapping("yes", ZBool.True),
				new ValueMapping("Yes", ZBool.True),
				new ValueMapping("YES", ZBool.True),
				new ValueMapping("no", ZBool.False),
				new ValueMapping("No", ZBool.False),
				new ValueMapping("NO", ZBool.False),
				new ValueMapping("Y", ZBool.True),
				new ValueMapping("y", ZBool.True),
				new ValueMapping("N", ZBool.False),
				new ValueMapping("n", ZBool.False),
				new ValueMapping('Y', ZBool.True),
				new ValueMapping('y', ZBool.True),
				new ValueMapping('N', ZBool.False),
				new ValueMapping('n', ZBool.False),
				new ValueMapping(ZBool.True, ZBool.True),
				new ValueMapping(ZBool.False, ZBool.False),
				new ValueMapping(true, ZBool.True),
				new ValueMapping(false, ZBool.False),
				new ValueMapping(DBNull.Value, ZBool.False),
				new ValueMapping(1, ZBool.True),
				new ValueMapping(0, ZBool.False)
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping(ZBool.True, 'Y'),
				new ValueMapping(ZBool.False, 'N'),
				new ValueMapping(ZBool.True, true),
				new ValueMapping(ZBool.False, false),
				new ValueMapping(ZBool.True, ZBool.True),
				new ValueMapping(ZBool.False, ZBool.False),
				new ValueMapping(ZBool.True, 1),
				new ValueMapping(ZBool.False, 0)
			};
		}

		protected override Type[] GetNonConvertibleFromTypes()
		{
			return new Type[] { typeof(DateTime) };
		}

		protected override Type[] GetNonConvertibleToTypes()
		{
			return new Type[] { typeof(DateTime) };
		}
	}
}
