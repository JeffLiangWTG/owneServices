using System;
using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CompareValidationTest : TestCaseWithDummy
	{
		#region Helpers

		static ZDateTime GetAsDate(ZPropertyInfo info) => info.Value is ZDateTime d ? d : info.Value is ZDate zd ? zd : ((ZDateTimeOffset)info.Value).ToZDateTime();

		static string GetFormat(ZPropertyInfo info) => GetAsDate(info).FormatDateTime();

		static void SetDate(ZPropertyInfo info, ZDateTime val)
		{
			if (info.Value is ZDateTime)
			{
				info.Value = val;
			}
			else if (info.Value is ZDate)
			{
				info.Value = val.Date;
			}
			else
			{
				info.Value = new ZDateTimeOffset(val);
			}
		}

		#endregion

		public void TestCheckNumberGreaterThanZero()
		{
			Assert(Dummy.Z0_Number == 0);
			AssertNoNotifications(Dummy);

			Dummy.Z0_Number = 5;
			CompareValidation.CheckNumberGreaterThanZero(Dummy.Z0_NumberInfo);
			AssertNoNotifications(Dummy);

			Dummy.Z0_Number = 0;
			CompareValidation.CheckNumberGreaterThanZero(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than 0.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestMessageErrorIfLessThanOrEqualToZero()
		{
			Assert(Dummy.Z0_Number == 0);
			AssertNoNotifications(Dummy);

			Dummy.Z0_Number = 5;
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Dummy.Z0_NumberInfo);
			AssertNoNotifications(Dummy);

			Dummy.Z0_Number = 0;
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Dummy.Z0_NumberInfo);
			AssertHasMessageError(Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than 0.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestCheckNumberNotNegative()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = 5;
			CompareValidation.CheckNumberNotNegative(Dummy.Z0_NumberInfo);
			AssertNoNotifications("Should be no Notifications when Z0_Number == 5", Dummy);

			Dummy.Z0_Number = 0;
			CompareValidation.CheckNumberNotNegative(Dummy.Z0_NumberInfo);
			AssertNoNotifications("Should be no Notifications when Z0_Number == 0", Dummy);

			Dummy.Z0_Number = -1;
			CompareValidation.CheckNumberNotNegative(Dummy.Z0_NumberInfo);
			AssertHasError("Should be Notifications when Z0_Number == -1",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than or equal to 0.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestCheckNumberGreaterThanOrEqualTo()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = -2;
			CompareValidation.CheckGreaterThanOrEqualTo(Dummy.Z0_NumberInfo, -5);
			AssertNoNotifications("Should be no Notifications when negative number is greater than other negative number", Dummy);

			Dummy.Z0_Number = 5;
			CompareValidation.CheckGreaterThanOrEqualTo(Dummy.Z0_NumberInfo, 7);
			AssertHasError("Should be Notifications when number out of range",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than or equal to 7.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestMessageErrorIfLessThan()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = -2;
			CompareValidation.MessageErrorIfLessThan(Dummy.Z0_NumberInfo, -5);
			AssertNoNotifications("Should be no Notifications when negative number is greater than other negative number", Dummy);

			Dummy.Z0_Number = 5;
			CompareValidation.MessageErrorIfLessThan(Dummy.Z0_NumberInfo, 7);
			AssertHasMessageError("Should be Notifications when number out of range",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than or equal to 7.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestMessageErrorIfNumberNegative()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = 5;
			CompareValidation.MessageErrorIfNumberNegative(Dummy.Z0_NumberInfo);
			AssertNoNotifications("Should be no Notifications when Z0_Number == 5", Dummy);

			Dummy.Z0_Number = 0;
			CompareValidation.MessageErrorIfNumberNegative(Dummy.Z0_NumberInfo);
			AssertNoNotifications("Should be no Notifications when Z0_Number == 0", Dummy);

			Dummy.Z0_Number = -1;
			CompareValidation.MessageErrorIfNumberNegative(Dummy.Z0_NumberInfo);
			AssertHasMessageError("Should be Notifications when Z0_Number == -1",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' greater than or equal to 0.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestCheckNumberLessThanOrEqualTo()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = -5;
			CompareValidation.CheckLessThanOrEqualTo(Dummy.Z0_NumberInfo, -2);
			AssertNoNotifications("Should be no Notifications when negative number is less than other negative number", Dummy);

			Dummy.Z0_Number = 7;
			CompareValidation.CheckLessThanOrEqualTo(Dummy.Z0_NumberInfo, 5);
			AssertHasError("Should be Notifications when number out of range",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' less than or equal to 5.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestCheckWithinRange()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = 0;
			CompareValidation.CheckWithinRange(Dummy.Z0_NumberInfo, 1, 10);
			AssertHasError("Less than Minimum allowed value, error expected",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' within the range 1 to 10.",
				Dummy.Z0_NumberInfo.HumanReadableName));

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 7;
			CompareValidation.CheckWithinRange(Dummy.Z0_NumberInfo, 1, 10);
			AssertNoErrors("Value is within range, no error expected", Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 10;
			CompareValidation.CheckWithinRange(Dummy.Z0_NumberInfo, 1, 10);
			AssertNoErrors("Value is within range, no error expected", Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 11;
			CompareValidation.CheckWithinRange(Dummy.Z0_NumberInfo, 1, 10);
			AssertHasError("Greater than Maximum range, error expected",
				Dummy.Z0_NumberInfo,
				string.Format("Please enter a '{0}' within the range 1 to 10.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestCheckWithinSet()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = 0;
			CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo, 1, 3, 5, 7, 9);
			AssertHasError(Dummy.Z0_NumberInfo, string.Format("Please enter a '{0}' within the set [1, 3, 5, 7, 9].", Dummy.Z0_NumberInfo.HumanReadableName));

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 2;
			CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo, 1, 3, 5, 7, 9);
			AssertHasError(Dummy.Z0_NumberInfo, string.Format("Please enter a '{0}' within the set [1, 3, 5, 7, 9].", Dummy.Z0_NumberInfo.HumanReadableName));

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			Dummy.Z0_Number = 7;
			CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo, 1, 3, 5, 7, 9);
			AssertNoErrors("Value is within the set, no error expected", Dummy.Z0_NumberInfo);

			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo, Array.Empty<decimal>()); });
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo, null); });
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ CompareValidation.CheckWithinSet(Dummy.Z0_NumberInfo); });
		}

		public void TestWarnIfNumberGreaterThanValue()
		{
			AssertEquals("Initial value of Z0_Number", new ZInt(0), Dummy.Z0_Number);
			AssertEquals("Initial Notifications", false, Dummy.HasNotifications());

			Dummy.Z0_Number = 5;
			CompareValidation.WarnIfGreaterThanValue(Dummy.Z0_NumberInfo, 10);
			AssertNoNotifications("Should be no notifications when number less than value", Dummy);

			Dummy.Z0_Number = 11;
			CompareValidation.WarnIfGreaterThanValue(Dummy.Z0_NumberInfo, 10);
			AssertHasWarning("Should be notifications when number less than value",
				Dummy.Z0_NumberInfo,
				string.Format("The '{0}' is greater than 10.",
				Dummy.Z0_NumberInfo.HumanReadableName));
		}

		public void TestWarnIfDateIsNotAfterAnotherDate()
		{
			AsserTestWarnIfDateIsNotAfterAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestWarnIfDateIsNotAfterAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AsserTestWarnIfDateIsNotAfterAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		void AsserTestWarnIfDateIsNotAfterAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDateTime.Now.AddDays(1));
			SetDate(anotherDateInfo, ZDateTime.Now);
			CompareValidation.WarnIfDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoWarnings("Should be no notifications when date is after", Dummy);

			SetDate(dateInfo, ZDateTime.Now);
			SetDate(anotherDateInfo, ZDateTime.Now.AddDays(1));
			CompareValidation.WarnIfDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasWarning("Should be a notifications now that the date is before",
				dateInfo,
				string.Format("The '{0}' should be after the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
		}

		public void TestWarnIfDateIsNotBeforeAnotherDate()
		{
			AssertTestWarnIfDateIsNotBeforeAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestWarnIfDateIsNotBeforeAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertTestWarnIfDateIsNotBeforeAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		void AssertTestWarnIfDateIsNotBeforeAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDateTime.Now);
			SetDate(anotherDateInfo, ZDateTime.Now.AddDays(1));
			CompareValidation.WarnIfDateIsNotBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoWarnings("Should be no notifications when date is before", Dummy);

			SetDate(dateInfo, ZDateTime.Now.AddDays(1));
			SetDate(anotherDateInfo, ZDateTime.Now);
			CompareValidation.WarnIfDateIsNotBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertHasWarning("Should be a notifications now that the date is after",
				dateInfo,
				string.Format("The '{0}' should be before the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
		}

		public void TestCheckEqual()
		{
			Assert(Dummy.Z0_Number == 0);
			Assert(Dummy.Z0_AnotherNumber == 0);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			CompareValidation.CheckEqual(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 4;
			CompareValidation.CheckEqual(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} and {1} must be the same.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckNotEqual()
		{
			Assert(Dummy.Z0_Number == 0);
			Assert(Dummy.Z0_AnotherNumber == 0);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 4;
			CompareValidation.CheckNotEqual(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_AnotherNumber = 4;
			CompareValidation.CheckNotEqual(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} and {1} cannot be the same.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckNumberGreaterThanOtherNumber()
		{
			Dummy.Z0_Number = 4;
			Dummy.Z0_AnotherNumber = 2;
			AssertNoErrors(Dummy.Z0_NumberInfo);

			CompareValidation.CheckNumberGreaterThanOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 2;
			CompareValidation.CheckNumberGreaterThanOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} must be greater than the {1}.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckNumberGreaterThanOrEqualToOtherNumber()
		{
			Dummy.Z0_Number = 4;
			Dummy.Z0_AnotherNumber = 2;
			AssertNoErrors(Dummy.Z0_NumberInfo);

			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 2;
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 1;
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} must be greater than or equal to the {1}.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckNumberLessThanOtherNumber()
		{
			Dummy.Z0_Number = 2;
			Dummy.Z0_AnotherNumber = 4;
			AssertNoErrors(Dummy.Z0_NumberInfo);

			CompareValidation.CheckNumberLessThanOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 4;
			CompareValidation.CheckNumberLessThanOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} must be less than the {1}.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckNumberLessThanOrEqualToOtherNumber()
		{
			Dummy.Z0_Number = 2;
			Dummy.Z0_AnotherNumber = 4;
			AssertNoErrors(Dummy.Z0_NumberInfo);

			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 4;
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertNoErrors(Dummy.Z0_NumberInfo);

			Dummy.Z0_Number = 5;
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The {0} must be less than or equal to the {1}.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
		}

		public void TestCheckValueIsNotDuplicated_ZInt()
		{
			ZPropertyInfo[] propertyInfos = new ZPropertyInfo[] { Dummy.Z0_NumberInfo, Dummy.Z0_AnotherNumberInfo };

			Dummy.Z0_Number = 12;
			Dummy.Z0_AnotherNumber = 12;

			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_NumberInfo, propertyInfos);
			AssertHasError(Dummy.Z0_NumberInfo,
				string.Format("The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.",
				Dummy.Z0_NumberInfo.HumanReadableName, Dummy.Z0_AnotherNumberInfo.HumanReadableName));
			AssertNoErrors(Dummy.Z0_AnotherNumberInfo);

			Dummy.Z0_NumberInfo.ClearAllNotifications();
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_AnotherNumberInfo, propertyInfos);
			AssertNoErrors(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_AnotherNumberInfo,
				string.Format("The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.",
				Dummy.Z0_AnotherNumberInfo.HumanReadableName, Dummy.Z0_NumberInfo.HumanReadableName));

			Dummy.Z0_Number = 11;
			Dummy.Z0_AnotherNumber = 12;

			Dummy.Z0_AnotherNumberInfo.ClearAllNotifications();
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_NumberInfo, propertyInfos);
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_AnotherNumberInfo, propertyInfos);
			AssertNoErrors(Dummy.Z0_NumberInfo);
			AssertNoErrors(Dummy.Z0_AnotherNumberInfo);
		}

		public void TestCheckValueIsNotDuplicated_ZString()
		{
			ZPropertyInfo[] propertyInfos = new ZPropertyInfo[] { Dummy.Z0_CodeInfo, Dummy.Z0_DescriptionInfo };

			Dummy.Z0_Code = "Apple";
			Dummy.Z0_Description = "Apple";

			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_CodeInfo, propertyInfos);
			AssertHasError(Dummy.Z0_CodeInfo,
				string.Format("The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.",
				Dummy.Z0_CodeInfo.HumanReadableName, Dummy.Z0_DescriptionInfo.HumanReadableName));
			AssertNoErrors(Dummy.Z0_DescriptionInfo);

			Dummy.Z0_CodeInfo.ClearAllNotifications();
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_DescriptionInfo, propertyInfos);
			AssertNoErrors(Dummy.Z0_CodeInfo);
			AssertHasError(Dummy.Z0_DescriptionInfo,
				string.Format("The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.",
				Dummy.Z0_DescriptionInfo.HumanReadableName, Dummy.Z0_CodeInfo.HumanReadableName));

			Dummy.Z0_Code = "Apple";
			Dummy.Z0_Description = "Orange";

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_CodeInfo, propertyInfos);
			CompareValidation.CheckValueIsNotDuplicated(Dummy.Z0_DescriptionInfo, propertyInfos);
			AssertNoErrors(Dummy.Z0_CodeInfo);
			AssertNoErrors(Dummy.Z0_DescriptionInfo);
		}

		public void TestCheckDateIsAfterAnotherDate()
		{
			AssertDateIsAfterAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckDateIsAfterAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertDateIsAfterAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		public void TestCheckDateIsAfterAnotherDate_ZDateOffset_ToDate()
		{
			AssertDateIsAfterAnotherDate(Dummy.Z0_DateTimeOffsetInfo, Dummy.Z0_ZDateInfo);
		}

		public void TestCheckZDateIsAfterAnotherDate()
		{
			AssertZDateIsAFterAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckDateIsNotBeforeAnotherDate()
		{
			AssertDateIsNotBeforeAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckDateIsNotBeforeAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertDateIsNotBeforeAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		public void TestCheckZDateIsNotBeforeAnotherDate()
		{
			AssertZDateIsNotBeforeAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckZDateIsNotBeforeAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertZDateIsNotBeforeAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		public void TestCheckDateIsBeforeAnotherDate()
		{
			AssertDateIsBeforeAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckZDateIsBeforeAnotherDate()
		{
			AssertZDateIsBeforeAnotherDate(Dummy.Z0_ZDateInfo, Dummy.Z0_AnotherZDateInfo);
		}

		public void TestCheckZDateIsBeforeAnotherDate_ZDateOffset()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertZDateIsBeforeAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		public void TestCheckZDateIsBeforeAnotherDate_ZDateOffset_ToZDate()
		{
			AssertZDateIsBeforeAnotherDate(Dummy.Z0_DateTimeOffsetInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckDateIsNotAfterAnotherDate()
		{
			AssertDateIsNotAfterAnotherDate(Dummy.Z0_DateInfo, Dummy.Z0_AnotherDateInfo);
		}

		public void TestCheckDateIsNotAfterAnotherDate_ZDateOffsete()
		{
			var otherDummy = Factory.New<DummyBusinessObject>();
			AssertDateIsNotAfterAnotherDate(Dummy.Z0_DateTimeOffsetInfo, otherDummy.Z0_DateTimeOffsetInfo);
		}

		public void TestCheckZDateIsNotAfterAnotherZDate()
		{
			AssertZDateIsNotAfterAnotherDate(Dummy.Z0_ZDateInfo, Dummy.Z0_AnotherZDateInfo);
		}

		void AssertDateIsAfterAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			ZDateTime dt = ZDateTime.Now;

			Assert(GetAsDate(dateInfo).IsEmpty);
			Assert(GetAsDate(anotherDateInfo).IsEmpty);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Two empty dates", dateInfo);

			SetDate(dateInfo, dt);
			SetDate(anotherDateInfo, new ZDateTime(dt.Date));
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Date is after AnotherDate", dateInfo);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is after AnotherDate (ZDateTime overload)", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is before AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			AssertNoErrors("Error NOT expected: Notifications cleared up", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt.Date);

			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is same as AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is same as AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));
		}

		void AssertZDateIsAFterAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			ZDateTime dt = ZDateTime.Now;

			Assert(GetAsDate(dateInfo).IsEmpty);
			Assert(GetAsDate(anotherDateInfo).IsEmpty);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Two empty dates", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is before AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			AssertNoErrors("Error NOT expected: Notifications cleared up", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt.Date);
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is same as AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsAfterAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is same as AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));
		}

		void AssertDateIsNotBeforeAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			ZDateTime dt = ZDateTime.Now;

			SetDate(dateInfo, dt);
			SetDate(anotherDateInfo, dt.Date);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is not before AnotherDate", dateInfo);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is not before AnotherDate (ZDateTime overload)", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetAsDate(anotherDateInfo).FormatDateTime()));

			dateInfo.ClearAllNotifications();
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetAsDate(anotherDateInfo).FormatDateTime()));

			dateInfo.ClearAllNotifications();
			AssertNoErrors("Error NOT expected: Notifications cleared up", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt.Date);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is same as AnotherDate", dateInfo);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is same as AnotherDate (ZDateTime overload)", dateInfo);
		}

		void AssertZDateIsNotBeforeAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			ZDateTime dt = ZDateTime.Now;

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is before AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be after '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			AssertNoErrors("Error NOT expected: Notifications cleared up", dateInfo);

			SetDate(dateInfo, dt.Date);
			SetDate(anotherDateInfo, dt.Date);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is same as AnotherDate", dateInfo);
			CompareValidation.CheckDateIsNotBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is same as AnotherDate (ZDateTime overload)", dateInfo);
		}

		void AssertDateIsBeforeAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDateTime.Empty);
			SetDate(anotherDateInfo, ZDateTime.Empty);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Two empty dates", dateInfo);

			SetDate(dateInfo, ZDateTime.Now);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: One date empty", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Same date and time",
				dateInfo,
				string.Format("The '{0}' must be before the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Same date and time (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be before '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			SetDate(dateInfo, GetAsDate(dateInfo).AddHours(-2));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Date is before AnotherDate", dateInfo);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is before AnotherDate (ZDateTime overload)", dateInfo);

			SetDate(dateInfo, GetAsDate(dateInfo).AddHours(12));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is after AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be before the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is after AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be before '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));
		}

		void AssertZDateIsBeforeAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDate.Empty);
			SetDate(anotherDateInfo, ZDateTime.Empty);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Two empty dates", dateInfo);

			SetDate(dateInfo, ZDate.Today);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: One date empty", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Same date and time",
				dateInfo,
				string.Format("The '{0}' must be before the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Same date and time (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be before '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));

			dateInfo.ClearAllNotifications();
			SetDate(dateInfo, GetAsDate(dateInfo).AddDays(-2));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Error NOT expected: Date is before AnotherDate", dateInfo);
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertNoErrors("Error NOT expected: Date is before AnotherDate (ZDateTime overload)", dateInfo);

			SetDate(dateInfo, GetAsDate(dateInfo).AddDays(12));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Error expected: Date is after AnotherDate",
				dateInfo,
				string.Format("The '{0}' must be before the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));
			CompareValidation.CheckDateIsBeforeAnotherDate(dateInfo, GetAsDate(anotherDateInfo));
			AssertHasError("Error expected: Date is after AnotherDate (ZDateTime overload)",
				dateInfo,
				string.Format("The '{0}' must be before '{1}'.",
				dateInfo.HumanReadableName, GetFormat(anotherDateInfo)));
		}

		void AssertDateIsNotAfterAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDateTime.Empty);
			SetDate(anotherDateInfo, ZDateTime.Empty);
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Two empty dates", dateInfo);

			SetDate(dateInfo, ZDateTime.Now);
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("One date empty", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Same date and time", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo).AddHours(-1));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Date1 is after Date2",
				dateInfo,
				string.Format("The '{0}' must be before or the same as the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));

			dateInfo.ClearAllNotifications();
			SetDate(anotherDateInfo, GetAsDate(dateInfo).AddHours(1));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Date1 is before Date2", dateInfo);
		}

		void AssertZDateIsNotAfterAnotherDate(ZPropertyInfo dateInfo, ZPropertyInfo anotherDateInfo)
		{
			SetDate(dateInfo, ZDate.Empty);
			SetDate(anotherDateInfo, ZDate.Empty);
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Two empty dates", dateInfo);

			SetDate(dateInfo, ZDate.Today);
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("One date empty", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Same date and time", dateInfo);

			SetDate(anotherDateInfo, GetAsDate(dateInfo).AddDays(-1));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertHasError("Date1 is after Date2",
				dateInfo,
				string.Format("The '{0}' must be before or the same as the '{1}'.",
				dateInfo.HumanReadableName, anotherDateInfo.HumanReadableName));

			dateInfo.ClearAllNotifications();
			SetDate(anotherDateInfo, GetAsDate(dateInfo).AddDays(1));
			CompareValidation.CheckDateIsNotAfterAnotherDate(dateInfo, anotherDateInfo);
			AssertNoErrors("Date1 is before Date2", dateInfo);
		}

		#region Implementation

		protected override void RunTest()
		{
			using (Dummy.SuspendValidationTesting())
			{
				base.RunTest();
			}
		}

		new DummyBizOWithZDate Dummy
		{
			get { return (DummyBizOWithZDate)base.Dummy; }
		}

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyBizOWithZDate); }
		}

		public class DummyBizOWithZDate : DummyBusinessObject
		{
			public DummyBizOWithZDate(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new abstract class Schema : DummyBaseBusinessObject.Schema
			{
				public const string Z0_AnotherZDate = "Z0_AnotherZDate";
				public const string Z0_ZDate = "Z0_ZDate";
			}

			public ZDate Z0_AnotherZDate
			{
				get { return z0_AnotherDate; }
				set { SetNonPersistentPropertyValue(Z0_AnotherZDateInfo, ref z0_AnotherDate, value); }
			}
			ZDate z0_AnotherDate;

			public ZPropertyInfo Z0_AnotherZDateInfo
			{
				get { return GetZPropertyInfo(Schema.Z0_AnotherZDate); }
			}

			public ZDate Z0_ZDate
			{
				get { return z0_Date; }
				set { SetNonPersistentPropertyValue(Z0_ZDateInfo, ref z0_Date, value); }
			}
			ZDate z0_Date;

			public ZPropertyInfo Z0_ZDateInfo
			{
				get { return GetZPropertyInfo(Schema.Z0_ZDate); }
			}
		}

		#endregion
	}
}
