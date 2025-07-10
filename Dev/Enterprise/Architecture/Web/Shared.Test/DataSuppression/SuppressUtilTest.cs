using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	sealed class SuppressUtilTest : TestCase
	{
		public void TestGetSuppressedValue()
		{
			var testFlight = "QF123";

			AssertEquals(testFlight, SuppressUtil.GetSuppressedValue(testFlight, false));
			AssertEquals(SuppressUtil.SuppressedText, SuppressUtil.GetSuppressedValue(testFlight, true));

			var testDate = ZDateTime.Now.AddDays(1);

			AssertEquals(testDate, SuppressUtil.GetSuppressedValue(testDate, false));
			AssertEquals(SuppressUtil.SuppressedDateTime, SuppressUtil.GetSuppressedValue(testDate, true));

			//suppress by by value type
			AssertEquals(SuppressUtil.SuppressedText, SuppressUtil.GetSuppressedValue((ZString)"text"));
			AssertEquals(SuppressUtil.SuppressedDateTime, SuppressUtil.GetSuppressedValue(new ZDateTime(2009, 09, 09)));
			Assert((ZBool)SuppressUtil.GetSuppressedValue((ZBool)true));
		}

		public void TestGetFormattedDate()
		{
			AssertEquals(string.Empty, SuppressUtil.GetFormattedDate(ZDateTime.Empty, ZDateTimePickerFormat.Long));
			AssertEquals(SuppressUtil.SuppressedText, SuppressUtil.GetFormattedDate(SuppressUtil.SuppressedDateTime, ZDateTimePickerFormat.Long));

			var date = new ZDateTime(2009, 2, 2, 2, 2, 2);

			AssertEquals("02-Feb-09 02:02", SuppressUtil.GetFormattedDate(date, ZDateTimePickerFormat.Long));
			AssertEquals("02:02", SuppressUtil.GetFormattedDate(date, ZDateTimePickerFormat.Time));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(date, ZDateTimePickerFormat.Short));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(date, ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(date, ZDateTimePickerFormat.Custom));
		}

		public void TestGetFormattedDate_ZDateTimeOffset()
		{
			AssertEquals(string.Empty, SuppressUtil.GetFormattedDate(ZDateTime.Empty, ZDateTimePickerFormat.Long));
			AssertEquals(SuppressUtil.SuppressedText, SuppressUtil.GetFormattedDate(SuppressUtil.SuppressedDateTime, ZDateTimePickerFormat.Long));

			var dateTimeOffset = new ZDateTimeOffset(new ZDateTime(2009, 2, 2, 2, 2, 2));

			AssertEquals("02-Feb-09 02:02", SuppressUtil.GetFormattedDate(dateTimeOffset, ZDateTimePickerFormat.Long));
			AssertEquals("02:02", SuppressUtil.GetFormattedDate(dateTimeOffset, ZDateTimePickerFormat.Time));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(dateTimeOffset, ZDateTimePickerFormat.Short));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(dateTimeOffset, ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes));
			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(dateTimeOffset, ZDateTimePickerFormat.Custom));
		}

		public void TestGetFormattedDate_ZDate()
		{
			AssertEquals(string.Empty, SuppressUtil.GetFormattedDate(ZDateTime.Empty, ZDateTimePickerFormat.Long));
			AssertEquals(SuppressUtil.SuppressedText, SuppressUtil.GetFormattedDate(SuppressUtil.SuppressedDateTime, ZDateTimePickerFormat.Long));

			var date = new ZDate(new ZDateTime(2009, 2, 2, 2, 2, 2));

			AssertEquals("02-Feb-09", SuppressUtil.GetFormattedDate(date));
		}
	}
}
