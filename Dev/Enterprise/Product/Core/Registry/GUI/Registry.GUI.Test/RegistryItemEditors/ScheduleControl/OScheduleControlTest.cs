using System;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class OScheduleControlTest : TestCase
	{
		public void TestAllowableChars()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				AssertEquals("AllowableChars", "FDL", testControl.AllowableChars);
			}
		}

		public void TestGetDayText()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.MondayText = "    *"; //2am
				AssertEquals("Day has text", "    *", testControl.GetDayText(DayOfWeek.Monday));
				AssertEquals("Day is empty", "", testControl.GetDayText(DayOfWeek.Sunday));
			}
		}

		public void TestGetTimeChar()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.MondayText = "    *  A"; //2am & 3:30am
				AssertEquals("GetTimeChar: Time is marked", '*', testControl.GetTimeChar(DayOfWeek.Monday, new DateTime(2000, 1, 1, 2, 10, 0)));
				AssertEquals("GetTimeChar: half-hour", 'A', testControl.GetTimeChar(DayOfWeek.Monday, new DateTime(2000, 1, 1, 3, 30, 0)));
				AssertEquals("GetTimeChar: beyond end of text", ' ', testControl.GetTimeChar(DayOfWeek.Monday, new DateTime(2000, 1, 1, 23, 0, 0)));
			}
		}

		public void TestDayTextLongerThan48CharIsTruncated()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.MondayText = "*".PadRight(49);
				AssertEquals("Length should be 48", 48, testControl.MondayText.Length);

				testControl.SetDayText(DayOfWeek.Tuesday, "*".PadRight(49));
				AssertEquals("Length should be 48", 48, testControl.TuesdayText.Length);
			}
		}

		public void TestFullWeekText()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				string testMonday = "    *";
				string testThursday = "  * *";
				string testWeek =
					new string(testControl.DayScheduleSeparator[0], 1) + testMonday +
					new string(testControl.DayScheduleSeparator[0], 3) + testThursday +
					new string(testControl.DayScheduleSeparator[0], 2);

				testControl.MondayText = testMonday;
				testControl.ThursdayText = testThursday;
				AssertEquals("FullWeekText 01", testWeek, testControl.FullWeekText);

				testControl.FullWeekText = "";
				AssertEquals("MondayText", "", testControl.MondayText);
				AssertEquals("MondayText", "", testControl.ThursdayText);

				testControl.FullWeekText = testWeek;
				AssertEquals("MondayText", testMonday, testControl.MondayText);
				AssertEquals("MondayText", testThursday, testControl.ThursdayText);
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestSettingAllowableCharContainingDaySeparatorThrowsException()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.AllowableChars = "whatever" + testControl.DayScheduleSeparator + "morestuff";
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestSettingDaySeparatorLongerThan1CharThrowsException()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.DayScheduleSeparator = ", ";
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestSettingDaySeparatorAsOneOfTheAllowableCharThrowsException()
		{
			using (OScheduleControl testControl = new OScheduleControl())
			{
				testControl.DayScheduleSeparator = testControl.AllowableChars.Substring(0, 1);
			}
		}
	}
}
