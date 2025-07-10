using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	class StmServiceTaskRecurrenceControlTest : ZFormBasherTest
	{
		public void TestPanelVisibility()
		{
			var scheduleTask = Factory.New<StmServiceTask>();
			scheduleTask.Recurrence.DaysRange = true;

			using (var form = new MockZForm(scheduleTask))
			{
				form.Show();
				AssertPanelVisibility(form, form.RecurrenceControl.DailyPanel);

				scheduleTask.Recurrence.WeeksRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.WeeklyPanel);

				scheduleTask.Recurrence.MonthsRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.MonthlyPanel);

				scheduleTask.Recurrence.YearsRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.YearlyPanel);

				scheduleTask.Recurrence.HoursRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.HourlyPanel);

				scheduleTask.Recurrence.MinutesRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.MinutePanel);

				scheduleTask.Recurrence.SecondsRange = true;
				AssertPanelVisibility(form, form.RecurrenceControl.SecondPanel);
			}
		}

		public void TestScheduleIsDisplayedCorrectly_Period()
		{
			CombineAssertions(() =>
			{
				Test(new NextRunTimeCalculatorSeconds { Period = 30 }, "SecondPanel", "EverySecond", "30");
				Test(new NextRunTimeCalculatorMinutes { Period = 30 }, "MinutePanel", "EveryMinute", "30");
				Test(new NextRunTimeCalculatorHours { Period = 5 }, "HourlyPanel", "EveryHour", "5");
				Test(new NextRunTimeCalculatorDays { Period = 5 }, "DailyPanel", "DailyDaysNumber", "5");
				Test(new NextRunTimeCalculatorWeeks { Period = 3 }, "WeeklyPanel", "RecurEveryWeekTextBox", "3");
				Test(new NextRunTimeCalculatorMonthsByDate { Period = 4 }, "MonthlyPanel", "MonthlyNumMonth", "4");
			});

			void Test(INextRunTimeCalculator calculator, string panelName, string periodControlName, string expectedValue)
			{
				// Arrange
				var scheduleTask = Factory.New<StmServiceTask>();
				scheduleTask.NextRunTimeCalculator = calculator;

				using var form = new StmServiceTaskForm(scheduleTask);

				// Act
				form.Show();

				Assert("Unique Recurrence control should be found.", TryGetChildControl(form, "RecurrenceControl", out var recurrenceControl));
				Assert("Unique Panel control should be found", TryGetChildControl(recurrenceControl, panelName, out var panelControl));
				Assert("Unique Period control should be found.", TryGetChildControl(panelControl, periodControlName, out var periodControl));

				// Assert
				AssertEquals(expectedValue, periodControl.Text);
			}
		}

		[TestDate(2024, 6, 1, 12, 30, 0)]
		public void TestNextRunTimeIsDisplayedCorrectly()
		{
			CombineAssertions(() =>
			{
				Test(new NextRunTimeCalculatorSeconds { Period = 30 });
				Test(new NextRunTimeCalculatorMinutes { Period = 30 });
				Test(new NextRunTimeCalculatorHours { Period = 5 });
				Test(new NextRunTimeCalculatorDays { Period = 5, ScheduledRunTime = TimeSpan.Zero });
				Test(new NextRunTimeCalculatorWeeks { Period = 3, ScheduledRunTime = TimeSpan.Zero, DaysOfOccurrence = new[] { DayOfWeek.Sunday } });
				Test(new NextRunTimeCalculatorMonthsByDate { Period = 4, ScheduledRunTime = TimeSpan.Zero, DayOfOccurrence = 1 });
				Test(new NextRunTimeCalculatorYearsByDate { Month = 1, Day = 1 , ScheduledRunTime = TimeSpan.Zero });
			});

			void Test(INextRunTimeCalculator calculator)
			{
				// Arrange
				var scheduleTask = Factory.New<StmServiceTask>();
				scheduleTask.NextRunTimeCalculator = calculator;

				using var form = new StmServiceTaskForm(scheduleTask);

				scheduleTask.SST_NextRunTime = calculator.CalculateNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset());
				var expectedDateUtc = scheduleTask.NextRunTime;
				var expectedDateLocal = scheduleTask.NextRunTimeLocal;

				// Act
				form.Show();

				var nextRunTimeUtcControl = form.Controls.Find("NextRunTimeDateEdit", true)[0];
				var nextRunTimeLocalControl = form.Controls.Find("LocalNextRunTimeDateEdit", true)[0];

				// Assert
				AssertEquals($"{calculator.GetType().Name}", expectedDateUtc.ToString("dd-MMM-yy HH:mm").ToUpper(), nextRunTimeUtcControl.Text);
				AssertEquals($"{calculator.GetType().Name}", expectedDateLocal.ToString("dd-MMM-yy HH:mm").ToUpper(), nextRunTimeLocalControl.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			return StmServiceTaskFormTest.GetFormToBashWithExcludedCaptions(taskSchedule);
		}

		void AssertPanelVisibility(MockZForm form, ZPanel visiblePanel)
		{
			ZPanel[] panels =
				{
					form.RecurrenceControl.DailyPanel,
					form.RecurrenceControl.WeeklyPanel,
					form.RecurrenceControl.MonthlyPanel,
					form.RecurrenceControl.YearlyPanel,
					form.RecurrenceControl.HourlyPanel,
					form.RecurrenceControl.MinutePanel,
					form.RecurrenceControl.SecondPanel,
				};

			foreach (var panel in panels)
			{
				AssertEquals(panel.Name + ".Visible", (panel == visiblePanel), panel.Visible);
			}
		}

		bool TryGetChildControl(Control parentControl, string childControlName, out Control childControl)
		{
			var controls = parentControl.Controls.Find(childControlName, searchAllChildren: true);
			childControl = controls.Length == 1 ? controls[0] : null;

			return childControl is not null;
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

		#region Test Classes

		class MockZForm : ZForm
		{
			public MockZForm(StmServiceTask businessEntity)
				: base(businessEntity)
			{
				ControllerID = ControllerIDs.StmServiceTask;
				CaptionRenderingEnabled = true;
			}

			protected override void InitializeComponent()
			{
				RecurrenceControl = new TestStmServiceTaskRecurrenceControl
				{
					BindTo = ""
				};
				Controls.Add(RecurrenceControl);
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 600, true);
			}

			public TestStmServiceTaskRecurrenceControl RecurrenceControl { get; private set; }
		}

		class TestStmServiceTaskRecurrenceControl : StmServiceTaskRecurrenceControl
		{
			public new ZPanel SecondPanel => base.SecondPanel;
			public new ZPanel MinutePanel => base.MinutePanel;
			public new ZPanel HourlyPanel => base.HourlyPanel;
			public new ZPanel DailyPanel => base.DailyPanel;
			public new ZPanel WeeklyPanel => base.WeeklyPanel;
			public new ZPanel MonthlyPanel => base.MonthlyPanel;
			public new ZPanel YearlyPanel => base.YearlyPanel;
		}

		#endregion
	}
}
