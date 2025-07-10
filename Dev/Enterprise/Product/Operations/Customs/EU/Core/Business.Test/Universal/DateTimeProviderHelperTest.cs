using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class DateTimeProviderHelperTest : TestCaseWithFactory
	{
		public void TestConvertToUnspecifiedDateTimeKindIfPossible() => CombineAssertions(() =>
		{
			AssertEquals("Invalid", DateTime.MinValue, DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Invalid));
			AssertEquals("Empty", DateTime.MinValue, DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.Empty));
			AssertEquals("Valid", new DateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.BrettsBirthday));
			var datetime = new ZDateTimeOffset(2022, 05, 15, 14, 45, 36, 345, TimeSpan.Zero).ToZDateTime();
			AssertEquals("Remove milliseconds", new DateTime(2022, 05, 15, 14, 45, 36), DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(datetime, true));
			AssertEquals("Keep milliseconds", new DateTime(2022, 05, 15, 14, 45, 36, 345), DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(datetime, false));
		});

		public void TestConvertAisUcc5DateStringToZDateTime() => CombineAssertions(() =>
		{
			AssertEquals("Empty string", ZDateTime.Empty, DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(string.Empty));
			AssertEquals("Valid Format", new ZDateTime(2021, 02, 15), DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime("20210215"));
			AssertEquals("Invalid Format", ZDateTime.Empty, DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime("2024-03-12 14:07:33.090"));
		});

		public void TestConvertStringToNullableInt() => CombineAssertions(() =>
		{
			AssertNull("Empty", DateTimeProviderHelper.ConvertStringToNullableInt(ZString.Empty));
			AssertEquals("Valid", 1, DateTimeProviderHelper.ConvertStringToNullableInt("1"));
			AssertNull("Invalid", DateTimeProviderHelper.ConvertStringToNullableInt("X"));
		});
	}
}
