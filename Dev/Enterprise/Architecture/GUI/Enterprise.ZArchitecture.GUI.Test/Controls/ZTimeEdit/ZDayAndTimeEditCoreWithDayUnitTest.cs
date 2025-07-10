using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZDayAndTimeEditCoreWithDayUnitTest : ZTimeEditCoreTest
	{
		public override void TestGetTimeFromText()
		{
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 1), "0");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 2), "1");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 1, 31), "30");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 2, 1), "31");

			//leap year shenanigans (if there's an extra day in feb, we reach 255 days in the year one day sooner)
			var leapYearAdjustment = DateTime.IsLeapYear(ZDateTime.Now.Year) ? -1 : 0;
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 9, 12 + leapYearAdjustment), "254");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 9, 13 + leapYearAdjustment), "255");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 9, 13 + leapYearAdjustment), "256");
			AssertGetTimeFromText(new ZDateTime(ZDateTime.Now.Year, 9, 13 + leapYearAdjustment), "2000000000");

			AssertGetTimeFromText(ZDateTime.Empty, "");
			AssertGetTimeFromText(ZDateTime.Invalid, "a");
			AssertGetTimeFromText(ZDateTime.Invalid, "0:01");
			AssertGetTimeFromText(ZDateTime.Invalid, "-1");
		}

		public void TestGetTextFromTime()
		{
			AssertGetTextFromTime("0", new ZDateTime(2006, 1, 1, 10, 30, 0));
			AssertGetTextFromTime("1", new ZDateTime(2006, 1, 2, 10, 30, 0));
			AssertGetTextFromTime("30", new ZDateTime(2006, 1, 31, 10, 5, 0));
			AssertGetTextFromTime("31", new ZDateTime(2006, 2, 1, 1, 13, 0));

			//more leap year shenanigans (2020 is a leap year, 2006 is now)
			AssertGetTextFromTime("254", new ZDateTime(2006, 9, 12, 0, 0, 0));
			AssertGetTextFromTime("255", new ZDateTime(2006, 9, 13, 0, 0, 0));
			AssertGetTextFromTime("255", new ZDateTime(2006, 9, 14, 0, 0, 0));

			AssertGetTextFromTime("254", new ZDateTime(2020, 9, 11, 0, 0, 0));
			AssertGetTextFromTime("255", new ZDateTime(2020, 9, 12, 0, 0, 0));
			AssertGetTextFromTime("255", new ZDateTime(2020, 9, 13, 0, 0, 0));

			AssertGetTextFromTime("255", new ZDateTime(2006, 12, 31, 0, 0, 0));
			AssertGetTextFromTime("255", new ZDateTime(2020, 12, 31, 0, 0, 0));
		}

		public void TestGetTextFromEmptyOrInvalidTime()
		{
			AssertEquals("GetTextFromTime(ZDateTime.Empty)", "0", Core.GetTextFromTime(ZDateTime.Empty, false));
			AssertEquals("GetTextFromTime(ZDateTime.Invalid)", "<Invalid>", Core.GetTextFromTime(ZDateTime.Invalid, false));
		}

		#region Implementation

		void AssertGetTextFromTime(string expectedText, ZDateTime time)
		{
			AssertEquals(expectedText, Core.GetTextFromTime(time, false));
		}

		protected override ZTimeEditCore NewCore()
		{
			return new ZDayAndTimeEditCore(ContainerPenaltyTimeUnit.Codes.Days, Control);
		}

		protected override ZTimeEdit NewEditor()
		{
			return new ZDayAndTimeEdit();
		}

		new ZDayAndTimeEdit Control
		{
			get { return (ZDayAndTimeEdit)base.Control; }
		}

		#endregion
	}
}
