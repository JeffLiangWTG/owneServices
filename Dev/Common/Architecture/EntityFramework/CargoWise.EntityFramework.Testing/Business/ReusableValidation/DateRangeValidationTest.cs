using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DateRangeValidationTest : TestCaseWithDummyForValidationTesting
	{
		public void TestStandardCheckValidZDateTimeOffsetRange()
		{
			DateRangeValidation stdDateRangeValidation = new DateRangeValidation();

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateTimeOffsetInfo.HasNotifications());
			stdDateRangeValidation.Validate(Dummy.Z0_DateTimeOffsetInfo);
			AssertEquals("within ZDateTime Range", false, Dummy.Z0_DateTimeOffsetInfo.HasNotifications());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-11));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-9));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(6));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(4));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-11));
			((INeedRow)Dummy).Row.AcceptChanges();
			Assert("Pre-Condition", Dummy.Z0_DateTimeOffsetInfo.OriginalValue.Equals(Dummy.Z0_DateTimeOffsetInfo.Value));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert("Date has not changed so there should be no errors", !Dummy.Z0_DateTimeOffsetInfo.HasErrors());
		}

		public void TestStandardCheckValidZDateTimeRange()
		{
			DateRangeValidation stdDateRangeValidation = new DateRangeValidation();

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateInfo.HasNotifications());
			stdDateRangeValidation.Validate(Dummy.Z0_DateInfo);
			AssertEquals("within ZDateTime Range", false, Dummy.Z0_DateInfo.HasNotifications());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-11));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-9));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(Dummy.Z0_DateInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(6));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(4));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(Dummy.Z0_DateInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-11));
			((INeedRow)Dummy).Row.AcceptChanges();
			Assert("Pre-Condition", Dummy.Z0_DateInfo.OriginalValue.Equals(Dummy.Z0_DateInfo.Value));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert("Date has not changed so there should be no errors", !Dummy.Z0_DateInfo.HasErrors());
		}

		public void TestOverridenDateRangeValidation()
		{
			DateRangeValidation overriddenDateRangeValidation = new DateRangeValidation(new TypeValidationLimits()
			{ FutureYearsBeforeWarning = 2, FutureYearsBeforeError = 25, PastYearsBeforeError = 13, PastYearsBeforeWarning = 2 });

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateInfo.HasNotifications());
			overriddenDateRangeValidation.Validate(Dummy.Z0_DateInfo);
			AssertEquals("within ZDateTime Range", false, Dummy.Z0_DateInfo.HasNotifications());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-14));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-5));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(Dummy.Z0_DateInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(26));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(4));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(Dummy.Z0_DateInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(-1));
			((INeedRow)Dummy).Row.AcceptChanges();
			Assert("Pre-Condition", Dummy.Z0_DateInfo.OriginalValue.Equals(Dummy.Z0_DateInfo.Value));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo);
			Assert("Date has not changed so there should be no errors", !Dummy.Z0_DateInfo.HasErrors());
		}

		void SetDateTimeAndClearNotifications(ZPropertyInfo info, ZDateTime value)
		{
			info.Value = value;
			info.ClearAllNotifications();
		}

		void SetDateTimeAndClearNotifications(ZPropertyInfo info, ZDateTimeOffset value)
		{
			info.Value = value;
			info.ClearAllNotifications();
		}

		[TestDate(2010, 4, 1)]
		public void TestMaximumFutureYears()
		{
			AssertEquals("Future Limit", 70, DateRangeValidation.MaximumFutureYears);
		}

		[TestDate(2015, 05, 27)]
		public void TestMaximumPastYears()
		{
			AssertEquals("Past Limit", 116, DateRangeValidation.MaximumPastYears);
		}

		public void TestValidateDateValueHasChanged()
		{
			var dateInfo = Dummy.GetZPropertyInfo(DummyBizoSchema.Constants.Z0_DateOnly);
			var dateTimeInfo = Dummy.GetZPropertyInfo(DummyBizoSchema.Constants.Z0_Date);
			var varCharInfo = Dummy.GetZPropertyInfo(DummyBizoSchema.Constants.Z0_NVarChar);

			var stdDateRangeValidation = new DateRangeValidation();
			AssertNoExceptionThrown(() => stdDateRangeValidation.ValidateDateValueHasChanged(dateInfo));
			AssertNoExceptionThrown(() => stdDateRangeValidation.ValidateDateValueHasChanged(dateTimeInfo));
			AssertExceptionThrown(typeof(InvalidCastException), () => stdDateRangeValidation.ValidateDateValueHasChanged(varCharInfo));

			dateTimeInfo.Value = ZDateTime.Now.AddYears(6);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			dateTimeInfo.ClearAllNotifications();
			stdDateRangeValidation.ValidateDateValueHasChanged(dateTimeInfo, false);
			Assert(!Dummy.Z0_DateInfo.HasErrors());

			dateTimeInfo.ClearAllNotifications();
			dateTimeInfo.Value = ZDateTime.Now.AddYears(4);
			Assert(Dummy.Z0_DateInfo.HasWarnings());
			dateTimeInfo.ClearAllNotifications();
			stdDateRangeValidation.ValidateDateValueHasChanged(dateTimeInfo, false);
			Assert(!Dummy.Z0_DateInfo.HasWarnings());
		}

		[TestDate(2019, 6, 4)]
		public void TestWarningForPastYear()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PastYearsBeforeWarning is 0", "The date '04-Jun-2019' should be in future.", DateRangeValidation.WarningForPastYear(ZDateTime.Now.ToString(DATE_FORMAT), 0));
				AssertEquals("PastYearsBeforeWarning is 1", "The date '04-Jun-2019' is more than 1 year old.", DateRangeValidation.WarningForPastYear(ZDateTime.Now.ToString(DATE_FORMAT), 1));
				AssertEquals("PastYearsBeforeWarning is greater than 1", "The date '04-Jun-2019' is more than 20 years old.", DateRangeValidation.WarningForPastYear(ZDateTime.Now.ToString(DATE_FORMAT), 20));
			});
		}

		[TestDate(2019, 6, 4)]
		public void TestWarningForFutureYear()
		{
			CombineAssertions(() =>
			{
				AssertEquals("FutureYearsBeforeWarning isn't greater than 1", "The date '04-Jun-2019' is more than 1 year from now.", DateRangeValidation.WarningForFutureYear(ZDateTime.Now.ToString(DATE_FORMAT), 1));
				AssertEquals("FutureYearsBeforeWarning is greater than 1", "The date '04-Jun-2019' is more than 20 years from now.", DateRangeValidation.WarningForFutureYear(ZDateTime.Now.ToString(DATE_FORMAT), 20));
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		const string DATE_FORMAT = "dd-MMM-yyyy";
	}
}
