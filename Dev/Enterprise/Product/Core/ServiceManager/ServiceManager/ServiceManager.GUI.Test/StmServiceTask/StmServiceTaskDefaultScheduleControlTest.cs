using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	class StmServiceTaskDefaultScheduleControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return StmServiceTaskFormTest.GetFormToBashWithExcludedCaptions(Factory.New<StmServiceTask>());
		}

		[RequiresSTA]
		public void TestDefaultScheduleIsDisplayedCorrectly_Seconds()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "30seconds",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleEndAtUtc = "19hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("SecondPanel", true)[0];
				var everySecondControl = panelControl.Controls.Find("EverySecond", true)[0];
				var startTimeControl = panelControl.Controls.Find("StartTimeSecond", true)[0];
				var endTimeControl = panelControl.Controls.Find("EndTimeSecond", true)[0];

				AssertEquals("30", everySecondControl.Text);
				AssertEquals("09:00", startTimeControl.Text);
				AssertEquals("19:00", endTimeControl.Text);
			}
		}

		public void TestDefaultScheduleIsDisplayedCorrectly_Minutes()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "30minutes",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleEndAtUtc = "19hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("MinutePanel", true)[0];
				var everyMinuteControl = panelControl.Controls.Find("EveryMinute", true)[0];
				var startTimeControl = panelControl.Controls.Find("StartTimeMinute", true)[0];
				var endTimeControl = panelControl.Controls.Find("EndTimeMinute", true)[0];

				AssertEquals("30", everyMinuteControl.Text);
				AssertEquals("09:00", startTimeControl.Text);
				AssertEquals("19:00", endTimeControl.Text);
			}
		}

		public void TestDefaultScheduleIsDisplayedCorrectly_Hours()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3hours",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleEndAtUtc = "19hours",
						DefaultScheduleRandomStartOffset = "2hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("HourlyPanel", true)[0];
				var everyHourControl = panelControl.Controls.Find("EveryHour", true)[0];
				var startTimeControl = panelControl.Controls.Find("StartTimeHour", true)[0];
				var endTimeControl = panelControl.Controls.Find("EndTimeHour", true)[0];
				var randomStartOffset = panelControl.Controls.Find("HourlyRandomStartOffset", true)[0];

				AssertEquals("3", everyHourControl.Text);
				AssertEquals("09:00", startTimeControl.Text);
				AssertEquals("19:00", endTimeControl.Text);
				AssertEquals("02:00", randomStartOffset.Text);
			}
		}

		public void TestDefaultScheduleIsDisplayedCorrectly_Days()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3days",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleRandomStartOffset = "2hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("DailyPanel", true)[0];
				var everyDayControl = panelControl.Controls.Find("DailyDaysNumber", true)[0];
				var startTimeControl = panelControl.Controls.Find("DailyRecurringStartTimeEdit", true)[0];
				var randomStartOffset = panelControl.Controls.Find("DailyRandomStartOffset", true)[0];

				AssertEquals("3", everyDayControl.Text);
				AssertEquals("09:00", startTimeControl.Text);
				AssertEquals("02:00", randomStartOffset.Text);
			}
		}

		[RequiresSTA]
		public void TestDefaultScheduleIsDisplayedCorrectly_Weeks()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3weeks",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleRandomStartOffset = "2hours",
						DefaultScheduleDaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("WeeklyPanel", true)[0];
				var everyDayControl = panelControl.Controls.Find("RecurEveryWeekTextBox", true)[0];
				var sundayControl = (ZCheckBox)panelControl.Controls.Find("SundayCheckBox", true)[0];
				var mondayControl = (ZCheckBox)panelControl.Controls.Find("MondayCheckBox", true)[0];
				var tuesdayControl = (ZCheckBox)panelControl.Controls.Find("TuesdayCheckBox", true)[0];
				var wednesdayControl = (ZCheckBox)panelControl.Controls.Find("WednesdayCheckBox", true)[0];
				var thursdayControl = (ZCheckBox)panelControl.Controls.Find("ThursdayCheckBox", true)[0];
				var fridayControl = (ZCheckBox)panelControl.Controls.Find("FridayCheckBox", true)[0];
				var saturdayControl = (ZCheckBox)panelControl.Controls.Find("SaturdayCheckBox", true)[0];
				var startTimeControl = panelControl.Controls.Find("WeeklyRecurringStartTimeEdit", true)[0];

				AssertEquals("3", everyDayControl.Text);
				AssertEquals("09:00", startTimeControl.Text);
				Assert(mondayControl.Checked);
				Assert(wednesdayControl.Checked);
				Assert(fridayControl.Checked);
				Assert(!tuesdayControl.Checked);
				Assert(!thursdayControl.Checked);
				Assert(!saturdayControl.Checked);
				Assert(!sundayControl.Checked);
			}
		}

		public void TestDefaultScheduleIsDisplayedCorrectly_Months()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3months",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleEndAtUtc = "19hours",
						DefaultScheduleRandomStartOffset = "2hours",
						DefaultScheduleDayOfMonth = 15
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("MonthlyPanel", true)[0];
				var everyMonthControl = panelControl.Controls.Find("MonthlyNumMonth", true)[0];
				var dayOfMonthControl = panelControl.Controls.Find("MonthlyDate", true)[0];

				AssertEquals("3", everyMonthControl.Text);
				AssertEquals("15", dayOfMonthControl.Text);
			}
		}

		public void TestDefaultScheduleIsDisplayedCorrectly_Years()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3years",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleRandomStartOffset = "2hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				// Assert
				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var panelControl = defaultScheduleControl.Controls.Find("YearlyPanel", true)[0];
				var startTimeControl = panelControl.Controls.Find("YearlyRecurringStartTimeEdit", true)[0];

				AssertEquals("09:00", startTimeControl.Text);
			}
		}

		public void TestDefaultScheduleConfigurationBoxNotOverflow()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(
					new HostedServiceAttribute("ABC", "#DESC#", "TST", typeof(object))
					{
						DefaultScheduleRunEvery = "3years",
						DefaultScheduleStartAtUtc = "9hours",
						DefaultScheduleRandomStartOffset = "2hours"
					});

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "ABC";

				// Act
				using var form = new StmServiceTaskForm(taskSchedule);
				form.Show();

				var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
				var mainGroupBoxControl = defaultScheduleControl.Controls.Find("MainGroupBox", true)[0];
				var periodPanelControl = defaultScheduleControl.Controls.Find("PeriodPanel", true)[0];
				string[] panelArray =
				{
					"MonthlyPanel", "SecondPanel", "MinutePanel", "HourlyPanel", "WeeklyPanel", "DailyPanel",
					"YearlyPanel",
				};

				var result = true;
				foreach (var panelName in panelArray)
				{
					var panelControl = defaultScheduleControl.Controls.Find(panelName, true)[0];
					result &= mainGroupBoxControl.Width > panelControl.Width + periodPanelControl.Width;
				}

				// Assert
				AssertEquals(true, result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.DisposeSubstitutions();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes") &&
				a.Code == "ABC" &&
				a.Description == "ABC Description");
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable?.Dispose();
			base.TearDown();
		}

		IDisposable hostedServiceAttributeProviderDisposable;
	}
}
