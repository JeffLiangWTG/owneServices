using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(MockSchedule))]
	sealed class ScheduleTest : ScheduleTestCase<MockSchedule>
	{
		public override void TestDescription()
		{
			AssertEquals("IsValid", false, Schedule.IsValid);
			AssertEquals("Description", "", Schedule.Description);

			Schedule.PeriodScope = "a";
			AssertEquals("IsValid", false, Schedule.IsValid);
			AssertEquals("Description", "", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;
			AssertEquals("IsValid", true, Schedule.IsValid);
			AssertEquals("Description", "The xxx prior to when the report is run.", Schedule.Description);

			Schedule.PeriodCount = 2;
			AssertEquals("Description", "2 xxxs prior to when the report is run.", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertEquals("Description", "2 xxxs after when the report is run.", Schedule.Description);

			Schedule.DescriptionStart = "ooo";
			AssertEquals("Description", "ooo 2 xxxs after when the report is run.", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("Description", "ooo the xxx of when the report is run.", Schedule.Description);
		}

		public override void TestToStorageValue()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 3;
			AssertEquals("ToStorageValue()", new DateTime(1910, 4, 1, 0, 0, 30), Schedule.ToStorageValue());
		}

		public override void TestIsValid()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			base.TestIsValid();
		}
	}
}
