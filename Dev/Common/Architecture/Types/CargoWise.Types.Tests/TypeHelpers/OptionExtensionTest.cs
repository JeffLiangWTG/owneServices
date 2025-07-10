using System;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using WTG.Foundation.FrameworkExtensions.Functional;

namespace CargoWise.Types.Tests
{
	class OptionExtensionTest : TestCase
	{
		public void TestSelect_WithZDateOverload()
		{
			var defaultDate = new DateTime(2023, 05, 04);
			var someDate = new DateTime(2006, 1, 1);
			var optionNone = Option.None<ZDate>();
			var optionSome = Option.Some((ZDate)someDate);
			var optionNotValid = Option.Some(ZDate.Invalid);

			AssertEquals("MapTo() on None should use default", defaultDate, optionNone.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on Some should use value", someDate, optionSome.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on !IsValid should use default", defaultDate, optionNotValid.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
		}

		public void TestSelect_WithZDateTimeOverload()
		{
			var defaultDate = new DateTime(2023, 05, 04, 16, 55, 22);
			var someDate = new DateTime(2006, 1, 1, 10, 30, 0);
			var optionNone = Option.None<ZDateTime>();
			var optionSome = Option.Some((ZDateTime)someDate);
			var optionNotValid = Option.Some(ZDateTime.Invalid);

			AssertEquals("MapTo() on None should use default", defaultDate, optionNone.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on Some should use value", someDate, optionSome.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on !IsValid should use default", defaultDate, optionNotValid.MapTo(dt => dt.ToDateTime()).SomeOrDefault(defaultDate));
		}

		public void TestSelect_WithZDateTimeOffsetOverload()
		{
			var defaultDate = new DateTimeOffset(2023, 05, 04, 16, 55, 22, TimeSpan.FromHours(5));
			var someDate = new DateTimeOffset(2006, 1, 1, 10, 30, 0, TimeSpan.FromHours(-3));
			var optionNone = Option.None<ZDateTimeOffset>();
			var optionSome = Option.Some((ZDateTimeOffset)someDate);
			var optionNotValid = Option.Some(ZDateTimeOffset.Invalid);

			AssertEquals("MapTo() on None should use default", defaultDate, optionNone.MapTo(dt => dt.ToDateTimeOffset()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on Some should use value", someDate, optionSome.MapTo(dt => dt.ToDateTimeOffset()).SomeOrDefault(defaultDate));
			AssertEquals("MapTo() on !IsValid should use default", defaultDate, optionNotValid.MapTo(dt => dt.ToDateTimeOffset()).SomeOrDefault(defaultDate));
		}

		public void TestSelect_WithZGuidOverload()
		{
			var defaultGuid = new Guid("49b04690-494b-4863-9645-000000000000");
			var someGuid = new Guid("49b04690-494b-4863-9645-ffffffffffff");
			var optionNone = Option.None<ZGuid>();
			var optionSome = Option.Some((ZGuid)someGuid);
			var optionNotValid = Option.Some(ZGuid.Invalid);

			AssertEquals("MapTo() on None should use default", defaultGuid, optionNone.MapTo(g => g.ToGuid()).SomeOrDefault(defaultGuid));
			AssertEquals("MapTo() on Some should use value", someGuid, optionSome.MapTo(g => g.ToGuid()).SomeOrDefault(defaultGuid));
			AssertEquals("MapTo() on !IsValid should use default", defaultGuid, optionNotValid.MapTo(g => g.ToGuid()).SomeOrDefault(defaultGuid));
		}

		public void TestSelect_WithZGeographyOverload()
		{
			var defaultGeography = SqlGeography.STPointFromText(new SqlChars("POINT(1 2)"), ZGeography.SridGps);
			var someGeography = SqlGeography.STPointFromText(new SqlChars("POINT(3 4)"), ZGeography.SridGps);
			var optionNone = Option.None<ZGeography>();
			var optionSome = Option.Some((ZGeography)someGeography);
			var optionNotValid = Option.Some(ZGeography.Invalid);

			AssertEquals("MapTo() on None should use default", defaultGeography, optionNone.MapTo(x => x.XmlSerializedValue).SomeOrDefault(defaultGeography));
			AssertEquals("MapTo() on Some should use value", someGeography, optionSome.MapTo(x => x.XmlSerializedValue).SomeOrDefault(defaultGeography));
			AssertEquals("MapTo() on !IsValid should use default", defaultGeography, optionNotValid.MapTo(x => x.XmlSerializedValue).SomeOrDefault(defaultGeography));
		}
	}
}
