using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TypeValidationTest : TestCaseWithDummyForValidationTesting
	{
		#region Valid ZDate Tests

		public void TestCheckValidZDateWithoutRange()
		{
			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Invalid);
			TypeValidation.CheckValidZDateWithoutRange(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today);
			TypeValidation.CheckValidZDateWithoutRange(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateOnlyInfo.HasWarnings());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Empty);
			TypeValidation.CheckValidZDateWithoutRange(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors() != Dummy.Z0_DateOnlyInfo.IsNullable);
		}

		public void TestCheckValidZDateRange()
		{
			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateOnlyInfo.HasNotifications());
			Assert("within ZDate Range", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_DateOnly));

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today.AddYears(-11));
			TypeValidation.CheckValidZDateRange(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert("Is not within ZDate Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_DateOnly));

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today.AddYears(-9));
			TypeValidation.CheckValidZDateRange(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(Dummy.Z0_DateOnlyInfo.HasWarnings());
			Assert("Is within ZDate Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_DateOnly));

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today.AddYears(6));
			TypeValidation.CheckValidZDateRange(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert("Is not within ZDate Range", !TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_DateOnly));

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today.AddYears(4));
			TypeValidation.CheckValidZDateRange(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(Dummy.Z0_DateOnlyInfo.HasWarnings());
			Assert("Is within ZDate Range without error", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_DateOnly));

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today.AddYears(-11));
			((INeedRow)Dummy).Row.AcceptChanges();
			Assert("Pre-Condition", Dummy.Z0_DateOnlyInfo.OriginalValue.Equals(Dummy.Z0_DateOnlyInfo.Value));
			TypeValidation.CheckValidZDateRange(Dummy.Z0_DateOnlyInfo);
			Assert("Date has not changed so there should be no errors", !Dummy.Z0_DateOnlyInfo.HasErrors());
		}

		public void TestCheckValidSmallDateTimeForDate()
		{
			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Invalid);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Today);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDate.Empty);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors() != Dummy.Z0_DateInfo.IsNullable);

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDateTime.MinSmallDateTimeValue.AddDays(1).Date);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateOnlyInfo.HasWarnings());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDateTime.MinSmallDateTimeValue.AddDays(-1).Date);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateOnlyInfo.HasWarnings());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDateTime.MaxSmallDateTimeValue.AddDays(-1).Date);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(!Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateOnlyInfo.HasWarnings());

			SetDateAndClearNotifications(Dummy.Z0_DateOnlyInfo, ZDateTime.MaxSmallDateTimeValue.AddDays(1).Date);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateOnlyInfo);
			Assert(Dummy.Z0_DateOnlyInfo.HasErrors());
			Assert(!Dummy.Z0_DateOnlyInfo.HasWarnings());
		}

		void SetDateAndClearNotifications(ZPropertyInfo info, ZDate value)
		{
			info.Value = value;
			info.ClearAllNotifications();
		}

		#endregion

		#region Valid ZDateTime Tests

		public void TestIsInSmallDateTimeRange()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, new ZDateTime(2080, 1, 1));
			Assert(!TypeValidation.IsInSmallDateTimeRange(Dummy.Z0_DateInfo));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, new ZDateTime(1899, 1, 1));
			Assert(!TypeValidation.IsInSmallDateTimeRange(Dummy.Z0_DateInfo));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, new ZDateTime(2014, 1, 1));
			Assert(TypeValidation.IsInSmallDateTimeRange(Dummy.Z0_DateInfo));
		}

		public void TestCheckValidZDateTimeWithoutRange()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Invalid);
			TypeValidation.CheckValidZDateTimeWithoutRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now);
			TypeValidation.CheckValidZDateTimeWithoutRange(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Empty);
			TypeValidation.CheckValidZDateTimeWithoutRange(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors() != Dummy.Z0_DateInfo.IsNullable);
		}

		public void TestCheckValidSmallDateTime()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Invalid);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Today);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Empty);
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors() != Dummy.Z0_DateInfo.IsNullable);

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.MinSmallDateTimeValue.AddDays(1));
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.MinSmallDateTimeValue.AddDays(-1));
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.MaxSmallDateTimeValue.AddDays(-1));
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.MaxSmallDateTimeValue.AddDays(1));
			TypeValidation.CheckValidSmallDateTime(Dummy.Z0_DateInfo);
			Assert(Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());
		}

		public void TestCheckValidZDateTimeRange()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateInfo.HasNotifications());
			Assert("within ZDateTime Range", TypeValidation.IsWithinValidZDateTimeRangeWithoutError(Dummy.Z0_Date));

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

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(6));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo, false);
			Assert(!Dummy.Z0_DateInfo.HasErrors());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateInfo, ZDateTime.Now.AddYears(4));
			TypeValidation.CheckValidZDateTimeRange(Dummy.Z0_DateInfo, false);
			Assert(!Dummy.Z0_DateInfo.HasErrors());
			Assert(!Dummy.Z0_DateInfo.HasWarnings());
		}

		public void TestCheckValidZDateTimeRange_WithDurationgAttributes()
		{
			var dummy = Factory.New<DummyBusinessObjectWithZDateTimeDurationValue>();

			dummy.Z0_DateInfo.Value = ZDateTime.DefaultDurationEpoch;
			TypeValidation.CheckValidZDateTimeRange(dummy.Z0_DateInfo);
			Assert(!dummy.Z0_DateInfo.HasErrors());
			Assert(!dummy.Z0_DateInfo.HasWarnings());
		}

		public void TestCheckValidZDateTimeRange_WithNegativeableDurationgAttributes()
		{
			var dummy = Factory.New<DummyBusinessObjectWithZDateTimeNegativeableDurationValue>();

			dummy.Z0_DateInfo.Value = ZDateTime.DefaultDurationEpoch.AddYears(-1);
			TypeValidation.CheckValidZDateTimeRange(dummy.Z0_DateInfo);
			Assert(!dummy.Z0_DateInfo.HasErrors());
			Assert(!dummy.Z0_DateInfo.HasWarnings());
		}

		class DummyBusinessObjectWithZDateTimeDurationValue : DummyBusinessObject
		{
			public DummyBusinessObjectWithZDateTimeDurationValue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ZDateTimeDurationValue]
			public override ZDateTime Z0_Date { get => base.Z0_Date; set => base.Z0_Date = value; }
		}

		class DummyBusinessObjectWithZDateTimeNegativeableDurationValue : DummyBusinessObject
		{
			public DummyBusinessObjectWithZDateTimeNegativeableDurationValue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ZDateTimeOffsetValueNegatable]
			public override ZDateTime Z0_Date { get => base.Z0_Date; set => base.Z0_Date = value; }
		}

		void SetDateTimeAndClearNotifications(ZPropertyInfo info, ZDateTime value)
		{
			info.Value = value;
			info.ClearAllNotifications();
		}

		#endregion

		#region Valid ZDateTimeOffset Tests

		public void TestCheckValidZDateTimeOffsetWithoutRange()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Invalid);
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now);
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasWarnings());

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Empty);
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors() != Dummy.Z0_DateTimeOffsetInfo.IsNullable);
		}

		public void TestCheckValidZDateTimeOffsetRange()
		{
			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Today);
			Assert("Precondition - there should be no notifications", !Dummy.Z0_DateTimeOffsetInfo.HasNotifications());
			Assert("within ZDateTime Range", TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-11));
			TypeValidation.CheckValidZDateTimeOffsetRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-9));
			TypeValidation.CheckValidZDateTimeOffsetRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(6));
			TypeValidation.CheckValidZDateTimeOffsetRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert("Is not within ZDateTime Range", !TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(4));
			TypeValidation.CheckValidZDateTimeOffsetRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert(!Dummy.Z0_DateTimeOffsetInfo.HasErrors());
			Assert(Dummy.Z0_DateTimeOffsetInfo.HasWarnings());
			Assert("Is within ZDateTime Range without error", TypeValidation.IsWithinValidZDateTimeOffsetRangeWithoutError(Dummy.Z0_DateTimeOffset));

			SetDateTimeAndClearNotifications(Dummy.Z0_DateTimeOffsetInfo, ZDateTimeOffset.Now.AddYears(-11));
			((INeedRow)Dummy).Row.AcceptChanges();
			Assert("Pre-Condition", Dummy.Z0_DateTimeOffsetInfo.OriginalValue.Equals(Dummy.Z0_DateTimeOffsetInfo.Value));
			TypeValidation.CheckValidZDateTimeOffsetRange(Dummy.Z0_DateTimeOffsetInfo);
			Assert("Date has not changed so there should be no errors", !Dummy.Z0_DateTimeOffsetInfo.HasErrors());
		}

		void SetDateTimeAndClearNotifications(ZPropertyInfo info, ZDateTimeOffset value)
		{
			info.Value = value;
			info.ClearAllNotifications();
		}

		#endregion

		#region Valid ZGeography Temporarily Suppress Tests

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestCheckValidGeography()
		{
			Dummy.Z0_Geography = new ZGeography("-121 48");
			AssertNoErrors("No error expected", Dummy.Z0_GeographyInfo);

			Dummy.Z0_Geography = null;
			AssertNoErrors("No error expected", Dummy.Z0_GeographyInfo);

			Dummy.Z0_Geography = ZGeography.Empty;
			AssertNoErrors("No error expected", Dummy.Z0_GeographyInfo);

			TypeValidation.CheckValidGeography(Dummy.Z0_GeographyInfo, "rty");
			AssertNoErrors("No error expected", Dummy.Z0_GeographyInfo);

			Dummy.Z0_Geography = new ZGeography(SqlGeography.STPolyFromText(new SqlChars("POLYGON ((0 0,1 1,0 1,1 0,0 0))"), 4326));
			AssertHasErrors("Error expected", Dummy.Z0_GeographyInfo);
		}

		#endregion

		#region Valid Money Tests

		public void TestErrorMessageForCheckValidMoney()
		{
			Dummy.Z0_Decimal = 1234567M;
			TypeValidation.CheckValidDecimal(Dummy.Z0_DecimalInfo, 9, 3);
			AssertHasError(Dummy.Z0_DecimalInfo, "The number 1,234,567 is too large, the maximum value allowed for Decimal is 999,999.999.");

			Dummy.Z0_Decimal = 999999999999999M;
			TypeValidation.CheckValidMoney(Dummy.Z0_DecimalInfo, 19, 4);
			var expectError =
				string.Format("The number 999,999,999,999,999 is too large, the value's range of Decimal is between {0} and {1}.",
					TypeValidation.MinMoney, TypeValidation.MaxMoney);
			AssertHasError(Dummy.Z0_DecimalInfo, expectError);
		}

		#endregion

		#region Valid Decimal Tests

		public void TestCheckValidDecimal()
		{
			AssertZDecimal(Dummy, 0M, 4, 2, true);
			AssertZDecimal(Dummy, 3M, 4, 2, true);
			AssertZDecimal(Dummy, 3M, 1, 0, true);
			AssertZDecimal(Dummy, 20.1M, 4, 2, true);
			AssertZDecimal(Dummy, 20.12M, 4, 2, true);
			AssertZDecimal(Dummy, 99.99M, 4, 2, true);

			AssertZDecimal(Dummy, 120M, 4, 2, false);
			AssertZDecimal(Dummy, 120.1M, 4, 2, false);
			AssertZDecimal(Dummy, 120.12M, 4, 2, false);

			AssertZDecimal(Dummy, 12M, 4, 0, true);
		}

		public void TestErrorMessageForCheckValidDecimal()
		{
			Dummy.Z0_Decimal = 1234567M;
			TypeValidation.CheckValidDecimal(Dummy.Z0_DecimalInfo, 9, 3);
			AssertHasError(Dummy.Z0_DecimalInfo, "The number 1,234,567 is too large, the maximum value allowed for Decimal is 999,999.999.");
		}

		void AssertZDecimal(DummyBusinessObject bizObj, decimal value, int databasePrecision, int databaseScale, bool shouldBeValid)
		{
			bizObj.Z0_AnotherDecimal = value;
			bizObj.Z0_AnotherDecimalInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(bizObj.Z0_AnotherDecimalInfo, databasePrecision, databaseScale);

			string assertMessage = string.Format(
				"Precision is <{0}>, Scale is <{1}>, Value {2} should be {3}valid.", databasePrecision, databaseScale, value, shouldBeValid ? "" : "in");

			Assert(assertMessage, bizObj.Z0_AnotherDecimalInfo.HasErrors() != shouldBeValid);
		}

		#endregion

		public void TestMissingGuidValidation()
		{
			Dummy.Z0_Guid = ZGuid.Missing;
			AssertEquals("Should only have 1 error", 1, Dummy.Z0_GuidInfo.GetErrors().Count());

			Dummy.Z0_GuidInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(Dummy.Z0_GuidInfo, "thunk");
			AssertHasError(Dummy.Z0_GuidInfo, "The selected thunk is no longer valid. Please choose a new thunk from the list.");
		}
	}
}
