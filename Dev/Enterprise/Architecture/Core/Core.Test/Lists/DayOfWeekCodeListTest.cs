using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DayOfWeekCodeListTest : TestCase
	{
		public void TestDescriptionsAndDayOfWeek()
		{
			AssertCodeDescriptionPair("SUN", "Sunday", DayOfWeek.Sunday);
			AssertCodeDescriptionPair("MON", "Monday", DayOfWeek.Monday);
			AssertCodeDescriptionPair("TUE", "Tuesday", DayOfWeek.Tuesday);
			AssertCodeDescriptionPair("WED", "Wednesday", DayOfWeek.Wednesday);
			AssertCodeDescriptionPair("THU", "Thursday", DayOfWeek.Thursday);
			AssertCodeDescriptionPair("FRI", "Friday", DayOfWeek.Friday);
			AssertCodeDescriptionPair("SAT", "Saturday", DayOfWeek.Saturday);
		}

		void AssertCodeDescriptionPair(string code, string description, DayOfWeek dayOfWeek)
		{
			DayOfWeekCodeList list = new DayOfWeekCodeList();
			AssertEquals("Description for code '" + code + "'", description, list.GetDescriptionFromCode(code));
			AssertEquals("DayOfWeek for code '" + code + "'", dayOfWeek, list.GetDayOfWeek(code));
		}
	}
}
