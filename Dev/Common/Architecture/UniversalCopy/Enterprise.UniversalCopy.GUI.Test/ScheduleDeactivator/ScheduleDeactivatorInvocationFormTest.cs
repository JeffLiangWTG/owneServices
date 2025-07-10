using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Scheduler.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	[TestedType(typeof(ScheduleDeactivatorInvocationForm))]
	class ScheduleDeactivatorInvocationFormTest : ZFormBasherTest
	{
		public void TestFormDisplaysScheduleInformation()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();

				var textBox = (ZTextBox)form.Controls.Find("schedulesDisplayTextbox", true)[0];

				AssertNotNull(viewModel.SchedulesText);
				AssertEquals(viewModel.SchedulesText, textBox.Text);
			}
		}

		public void TestCloseWithoutChoosingOption_ShouldUseCancelAndDoNotDeactivate()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();

				AssertNull(viewModel.Response);
				form.Close();

				AssertEquals(ScheduleDeactivatorResponse.DoNotCancelOrDeactivate, viewModel.Response);
			}
		}

		public void TestFormCaptionWithMultipleSchedules()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();

				var formCaption = (ZLabel)form.Controls.Find("formDescriptionLabel", true)[0];
				var expectedCaption = @"Multiple copy schedules are attached.
Deactivate all related copy schedules before canceling?";

				AssertEquals(expectedCaption, formCaption.Text);
			}
		}

		public void TestFormCaptionWithSingleSchedule()
		{
			var copyTasks = new List<StmUniversalCopyScheduleTask>();
			var task = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			copyTasks.Add(task);
			viewModel = new ScheduleDeactivatorViewModel(copyTasks.AsEnumerable());

			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();

				var formCaption = (ZLabel)form.Controls.Find("formDescriptionLabel", true)[0];
				var expectedCaption = @"A copy schedule is attached.
Deactivate this related copy schedule before canceling?";

				AssertEquals(expectedCaption, formCaption.Text);
			}
		}

		#region Select Option

		public void TestSelectOption_CancelAndDeactivate()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();
				AssertNull(viewModel.Response);

				var button = (ZButton)form.Controls.Find("Cancel and deactivate", true)[0];
				button.PerformClick();

				AssertEquals(true, form.IsDisposed);
				AssertEquals(ScheduleDeactivatorResponse.CancelAndDeactivate, viewModel.Response);
			}
		}

		public void TestSelectOption_CancelAndDoNotDeactivate()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();
				AssertNull(viewModel.Response);

				var button = (ZButton)form.Controls.Find("Cancel and do not deactivate", true)[0];
				button.PerformClick();

				AssertEquals(true, form.IsDisposed);
				AssertEquals(ScheduleDeactivatorResponse.CancelAndDoNotDeactivate, viewModel.Response);
			}
		}

		public void TestSelectOption_DoNotCancelOrDeactivate()
		{
			using (var form = new ScheduleDeactivatorInvocationForm(viewModel))
			{
				form.Show();
				AssertNull(viewModel.Response);

				var button = (ZButton)form.Controls.Find("Do not cancel or deactivate", true)[0];
				button.PerformClick();

				AssertEquals(true, form.IsDisposed);
				AssertEquals(ScheduleDeactivatorResponse.DoNotCancelOrDeactivate, viewModel.Response);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ScheduleDeactivatorInvocationForm(viewModel);
		}

		#endregion

		ScheduleDeactivatorViewModel viewModel;
		IEnumerable<StmUniversalCopyScheduleTask> scheduledTasks;

		protected override void SetUp()
		{
			base.SetUp();

			var copyTasks = new List<StmUniversalCopyScheduleTask>();
			var task = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var task2 = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();

			task.S5_ScheduleDescription = "Task 1 description";
			task.S5_TaskPeriod = ScheduleRecurrenceType.Daily;
			task2.S5_ScheduleDescription = "Task 2 description";
			task2.S5_TaskPeriod = ScheduleRecurrenceType.Monthly;

			copyTasks.Add(task);
			copyTasks.Add(task2);
			scheduledTasks = copyTasks.AsEnumerable();

			viewModel = new ScheduleDeactivatorViewModel(scheduledTasks);
		}
	}
}
