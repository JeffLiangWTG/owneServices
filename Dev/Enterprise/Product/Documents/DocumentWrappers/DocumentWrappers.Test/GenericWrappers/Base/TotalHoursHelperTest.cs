using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	sealed class TotalHoursHelperTest : TestCase
	{
		public void TestGetTextFromTime()
		{
			AssertGetTextFromTime("10:30", new ZDateTime(2006, 1, 1, 10, 30, 0));
			AssertGetTextFromTime("10:03", new ZDateTime(2006, 1, 1, 10, 3, 0));
			AssertGetTextFromTime("1:03", new ZDateTime(2006, 1, 1, 1, 3, 0));
			AssertGetTextFromTime("0:01", new ZDateTime(2006, 1, 1, 0, 1, 0));
			AssertGetTextFromTime(ZString.Empty, new ZDateTime(2006, 1, 1, 0, 0, 0));
			AssertGetTextFromTime("23:59", new ZDateTime(2006, 1, 1, 23, 59, 0));
			AssertGetTextFromTime("-0:01", new ZDateTime(2006, 1, 1).AddMinutes(-1));
			AssertGetTextFromTime("-23:59", new ZDateTime(2006, 1, 1).AddHours(-23).AddMinutes(-59));
		}

		public void TestGetTextFromTimeSpan()
		{
			AssertGetTextFromTimeSpan("10:30", new TimeSpan(10, 30, 0));
			AssertGetTextFromTimeSpan("10:03", new TimeSpan(10, 3, 0));
			AssertGetTextFromTimeSpan("1:03", new TimeSpan(1, 3, 0));
			AssertGetTextFromTimeSpan("0:01", new TimeSpan(0, 1, 0));
			AssertGetTextFromTimeSpan(ZString.Empty, new TimeSpan(0, 0, 0));
			AssertGetTextFromTimeSpan("23:59", new TimeSpan(23, 59, 0));
			AssertGetTextFromTimeSpan("-0:01", new TimeSpan(0, -1, 0));
			AssertGetTextFromTimeSpan("-23:59", new TimeSpan(-23, -59, 0));
		}

		public void TestGetTextFromEmptyOrInvalidTime()
		{
			AssertEquals("GetTextFromTime(ZDateTime.Empty)", ZDateTime.Empty.ToString(), TimeHelper.GetTextFromTime(ZDateTime.Empty));
			AssertEquals("GetTextFromTime(ZDateTime.Invalid)", ZDateTime.Empty.ToString(), TimeHelper.GetTextFromTime(ZDateTime.Invalid));
		}

		TotalHoursHelper TimeHelper
		{
			get { return new TotalHoursHelper(); }
		}

		void AssertGetTextFromTime(string expectedText, ZDateTime time)
		{
			AssertEquals(expectedText, TimeHelper.GetTextFromTime(time));
		}

		void AssertGetTextFromTimeSpan(string expectedText, TimeSpan timeSpan)
		{
			AssertEquals(expectedText, TimeHelper.GetTextFromTimeSpan(timeSpan));
		}
	}
}
