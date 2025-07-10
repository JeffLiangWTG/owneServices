using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(AccPeriodSchedule))]
	sealed class AccPeriodScheduleTest : ScheduleTestCase<AccPeriodSchedule>
	{
		public void TestClear_FauxIntegrationTest()
		{
			AccPeriodTestDataCreator.Create(Factory);
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 6, 29);
			Schedule.ScheduleTask = scheduleTask;
			AssertEquals("IsValid", true, Schedule.IsValid);

			//ScheduleForm.cs Clear()
			Schedule.Clear();
			Schedule.ClearDate();
			AssertEquals(false, Schedule.IsValid);
			//Schedule.OnValueChanged -> SingleAccountingPeriodField.UpdateSinglePeriodFromSchedule -> AccPeriodSchedule.GetSchedulePeriod
			AssertEquals(0, Schedule.GetSchedulePeriod());

			//ScheduleForm.cs HandleOKButton()
			Schedule.PeriodScope = "This";
			AssertEquals(true, Schedule.IsValid);
			//Schedule.OnValueChanged -> SingleAccountingPeriodField.UpdateSinglePeriodFromSchedule -> AccPeriodSchedule.GetSchedulePeriod
			var schedulePeriod = Schedule.GetSchedulePeriod();
			AssertNotEquals(0, schedulePeriod);
			AssertEquals(schedulePeriod, Schedule.GetSchedulePeriod());
		}

		public void TestDefaults()
		{
			AssertEquals("Schedule.PeriodScope", PeriodScopeList.Codes.This, Schedule.PeriodScope);
		}

		public void TestGetSchedulePeriod()
		{
			AccPeriodTestDataCreator.Create(Factory);

			AssertEquals("IsValid", true, Schedule.IsValid);
			AssertEquals("GetSchedulePeriod()", 0, Schedule.GetSchedulePeriod());

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 2;
			AssertEquals("IsValid", true, Schedule.IsValid);
			AssertEquals("GetSchedulePeriod()", 0, Schedule.GetSchedulePeriod());

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2006, 6, 29);
			Schedule.ScheduleTask = scheduleTask;
			AssertEquals("GetSchedulePeriod()", 200610, Schedule.GetSchedulePeriod());
		}

		[TestUtcOffset(-8, 0, 0)]//US time
		public void TestGetSchedulePeriodUseLocalTime()
		{
			AccPeriodTestDataCreator.Create(Factory);

			AssertEquals("IsValid", true, Schedule.IsValid);
			AssertEquals("GetSchedulePeriod()", 0, Schedule.GetSchedulePeriod());

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			Schedule.ScheduleTask = scheduleTask;

			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("GetSchedulePeriod()", 200604, Schedule.GetSchedulePeriod());

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 2;
			AssertEquals("GetSchedulePeriod()", 200610, Schedule.GetSchedulePeriod());

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;
			AssertEquals("GetSchedulePeriod()", 200601, Schedule.GetSchedulePeriod());
		}

		public override void TestDescription()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("Description", "The accounting period of when the report is run.", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 1;
			AssertEquals("Description", "The accounting period prior to when the report is run.", Schedule.Description);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 2;
			AssertEquals("Description", "2 accounting periods after when the report is run.", Schedule.Description);
		}

		public override void TestToStorageValue()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("Schedule.ToStorageValue()", new DateTime(1910, 1, 4), Schedule.ToStorageValue());

			Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			Schedule.PeriodCount = 3;
			AssertEquals("Schedule.ToStorageValue()", new DateTime(1909, 10, 4), Schedule.ToStorageValue());

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 6;
			AssertEquals("Schedule.ToStorageValue()", new DateTime(1910, 7, 4), Schedule.ToStorageValue());
		}

		public void TestTryParse()
		{
			AccPeriodSchedule schedule;

			AssertEquals("TryParse()", false, AccPeriodSchedule.TryParse(DateTime.MinValue, out schedule));
			AssertNull("schedule", schedule);

			AssertEquals("TryParse()", false, AccPeriodSchedule.TryParse(new DateTime(1910, 1, 5), out schedule));
			AssertNull("schedule", schedule);

			AssertEquals("TryParse()", true, AccPeriodSchedule.TryParse(new DateTime(1910, 1, 4), out schedule));
			AssertEquals("PeriodScope", PeriodScopeList.Codes.This, schedule.PeriodScope);
			AssertEquals("PeriodNumber", ZByte.Zero, schedule.PeriodCount);

			AssertEquals("TryParse()", true, AccPeriodSchedule.TryParse(new DateTime(1909, 10, 4), out schedule));
			AssertEquals("PeriodScope", PeriodScopeList.Codes.Previous, schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZByte)3, schedule.PeriodCount);

			AssertEquals("TryParse()", true, AccPeriodSchedule.TryParse(new DateTime(1910, 7, 4), out schedule));
			AssertEquals("PeriodScope", PeriodScopeList.Codes.Next, schedule.PeriodScope);
			AssertEquals("PeriodNumber", (ZByte)6, schedule.PeriodCount);
		}

		public void TestFillData()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			var schedule = new AccPeriodSchedule();
			schedule.ScheduleTask = scheduleTask;

			var scheduleData = new AccPeriodScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.Next,
				PeriodCount = 2,
			};

			schedule.FillData(scheduleData);

			AssertEquals("PeriodCount", schedule.PeriodCount, scheduleData.PeriodCount);
			AssertEquals("PeriodScope", schedule.PeriodScope, scheduleData.PeriodScope);
		}

		public void TestExtractData()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.CalcNextRunTimeLocal = new ZDateTime(2006, 4, 1, 2, 45, 0);
			var schedule = new AccPeriodSchedule();
			schedule.ScheduleTask = scheduleTask;

			var scheduleData = new AccPeriodScheduleData()
			{
				PeriodScope = PeriodScopeList.Codes.Next,
				PeriodCount = 2,
			};

			schedule.FillData(scheduleData);

			var expectedData = schedule.ExtractData();
			AssertEquals("PeriodScope", expectedData.PeriodScope, schedule.PeriodScope);
			AssertEquals("PeriodCount", expectedData.PeriodCount, schedule.PeriodCount);
			AssertEquals("CalculatedResult", expectedData.CalculatedResult, schedule.Description);
			AssertEquals("StorageValue", expectedData.StorageValue, schedule.ToStorageValue());
			AssertEquals("ScheduleDate", expectedData.SchedulePeriod, schedule.GetSchedulePeriod());

			scheduleData = new AccPeriodScheduleData();
			schedule.FillData(scheduleData);
			expectedData = schedule.ExtractData();
			Assert("invalid schedule", !schedule.IsValid);
			AssertNull(expectedData);
		}
	}
}
