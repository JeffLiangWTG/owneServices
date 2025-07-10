using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	class ScheduleDeactivatorViewModelTest : TestCaseWithFactory
	{
		public void TestMultipleSchedulesTextIsCorrect()
		{
			var scheduleTextList = new List<ZString>();
			scheduleTextList.Add("Schedule Description: Task 1 description Recurrence: (D) Daily");
			scheduleTextList.Add("Schedule Description: Task 2 description Recurrence: (M) Monthly");
			var scheduleTextZString = ZString.Join(System.Environment.NewLine, scheduleTextList.ToArray());

			var scheduleTasksList = new List<StmUniversalCopyScheduleTask>();
			var schedule = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var secondSchedule = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();

			schedule.S5_TaskPeriod = ScheduleRecurrenceType.Daily;
			schedule.S5_ScheduleDescription = "Task 1 description";
			secondSchedule.S5_TaskPeriod = ScheduleRecurrenceType.Monthly;
			secondSchedule.S5_ScheduleDescription = "Task 2 description";

			scheduleTasksList.Add(schedule);
			scheduleTasksList.Add(secondSchedule);

			var viewModel = new ScheduleDeactivatorViewModel(scheduleTasksList.AsEnumerable());

			AssertEquals(scheduleTextZString, viewModel.SchedulesText);
		}

		public void TestSingleScheduleTextIsCorrect()
		{
			var viewModel = GetViewModel();

			ZString scheduleTextToDisplay = ("Schedule Description: Test schedule description Recurrence: (D) Daily");

			AssertEquals(scheduleTextToDisplay, viewModel.SchedulesText);
		}

		public ScheduleDeactivatorViewModel GetViewModel()
		{
			var scheduleTasksList = new List<StmUniversalCopyScheduleTask>();
			var schedule = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();

			schedule.S5_TaskPeriod = ScheduleRecurrenceType.Daily;
			schedule.S5_ScheduleDescription = "Test schedule description";
			scheduleTasksList.Add(schedule);

			var viewModel = new ScheduleDeactivatorViewModel(scheduleTasksList);

			return viewModel;
		}
	}

	[TestedType(typeof(ScheduleDeactivatorViewModel))]
	class ScheduleDeactivatorViewModelNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var copyTasks = new List<StmUniversalCopyScheduleTask>();
			var task = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();

			task.S5_ScheduleDescription = "Task 1 description";
			task.S5_TaskPeriod = ScheduleRecurrenceType.Daily;

			return new ScheduleDeactivatorViewModel(copyTasks.AsEnumerable());
		}
	}
}
