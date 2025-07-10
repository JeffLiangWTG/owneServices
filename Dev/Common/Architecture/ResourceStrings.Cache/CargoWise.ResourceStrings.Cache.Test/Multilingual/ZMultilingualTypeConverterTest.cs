using System;
using CargoWise.Types;
using CargoWise.Types.Tests;

namespace Enterprise.ZArchitecture.Core
{
	class ZMultilingualTypeConverterTest : TypeConverterTest
	{
		protected override Type GetZTypeImplementingTypeConverter()
		{
			return typeof(NoResString);
		}

		protected override ValueMapping[] GetConvertibleFromValues()
		{
			return new ValueMapping[] {
				new ValueMapping("Hello", (NoResString)"Hello"),
				new ValueMapping('H', (NoResString)"H"),
				new ValueMapping((NoResString)"hi", (NoResString)"hi"),
				new ValueMapping(DBNull.Value, (NoResString)"")
			};
		}

		protected override ValueMapping[] GetConvertibleToValues()
		{
			return new ValueMapping[] {
				new ValueMapping((NoResString)"Hello", (ZString)"Hello"),
				new ValueMapping((NoResString)"Hello", "Hello"),
				new ValueMapping((NoResString)"hi", (NoResString)"hi"),
				new ValueMapping(null, typeof(NoResString), null, typeof(string))
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

		public override void TestTypeCanBeConstructedUsingItself()
		{
			Assert(true); // tested in ConvertFrom() and ConvertTo()
		}
	}
}
