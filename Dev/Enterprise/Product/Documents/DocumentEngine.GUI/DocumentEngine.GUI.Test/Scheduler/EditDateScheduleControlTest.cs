using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	sealed class EditDateScheduleControlTest : TestCaseWithFactory
	{
		public void TestPeriodPanelVisibility()
		{
			using (var form = new ZForm(new DateSchedule()))
			{
				using (var control = new EditDateScheduleControl())
				{
					form.Controls.Add(control);
					form.Show();

					var dateSchedule = form.BusinessEntity as DateSchedule;
					dateSchedule.Period = ZString.Empty;

					AssertEquals("DayPanel.Visible", false, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", false, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", false, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", false, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", false, control.HourMinutePanel.Visible);

					dateSchedule.Period = ScheduleRecurrenceType.Daily;
					AssertEquals("DayPanel.Visible", true, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", false, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", false, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", false, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", false, control.HourMinutePanel.Visible);

					dateSchedule.Period = ScheduleRecurrenceType.Weekly;
					AssertEquals("DayPanel.Visible", false, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", true, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", false, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", false, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", false, control.HourMinutePanel.Visible);

					dateSchedule.Period = ScheduleRecurrenceType.Monthly;
					AssertEquals("DayPanel.Visible", false, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", false, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", true, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", false, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", false, control.HourMinutePanel.Visible);

					dateSchedule.Period = ScheduleRecurrenceType.Yearly;
					AssertEquals("DayPanel.Visible", false, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", false, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", false, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", true, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", false, control.HourMinutePanel.Visible);

					dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
					AssertEquals("DayPanel.Visible", false, control.DayPanel.Visible);
					AssertEquals("WeekPanel.Visible", false, control.WeekPanel.Visible);
					AssertEquals("MonthPanel.Visible", false, control.MonthPanel.Visible);
					AssertEquals("YearPanel.Visible", false, control.YearPanel.Visible);
					AssertEquals("HourMinutePanel.Visible", true, control.HourMinutePanel.Visible);
				}
			}
		}

		public void TestDayNumberBindToControl()
		{
			using (var form = new ZForm(new DateSchedule()))
			{
				using (var control = new EditDateScheduleControl())
				{
					form.Controls.Add(control);
					form.Show();
					var dateSchedule = form.BusinessEntity as DateSchedule;

					dateSchedule.Period = ScheduleRecurrenceType.Monthly;
					AssertEquals("DayNumber", control.MonthDayNumericUpDown.BindTo);
					AssertEquals("", control.YearDayNumericUpDown.BindTo);

					dateSchedule.Period = ScheduleRecurrenceType.Yearly;
					AssertEquals("", control.MonthDayNumericUpDown.BindTo);
					AssertEquals("DayNumber", control.YearDayNumericUpDown.BindTo);
				}
			}
		}

		public void TestHourMinuteBindToControl()
		{
			using (var form = new ZForm(new DateSchedule()))
			{
				using (var control = new EditDateScheduleControl())
				{
					form.Controls.Add(control);
					form.Show();
					var dateSchedule = form.BusinessEntity as DateSchedule;

					dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
					AssertEquals("Hour", control.HourNumericUpDown.BindTo);
					AssertEquals("MinuteOfHour", control.MinuteNumericUpDown.BindTo);
				}
			}
		}

		public void TestEnteringInvalidStuffInHourAndMinutePeriodScopeCausesNoAdverseConsequences()
		{
			using (var form = new ZForm(new DateSchedule()))
			{
				using (var control = new EditDateScheduleControl())
				{
					form.Controls.Add(control);
					form.Show();
					var dateSchedule = form.BusinessEntity as DateSchedule;

					dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
					control.HourMinuteScopeDropEdit.Text = "This";
					Assert("Changing the hour minute scope to this (which is invalid in this context) should not make the hour numeric updown invisible", control.HourNumericUpDown.Visible);
					AssertEquals("(no date selected)", dateSchedule.Description);

					control.HourMinuteScopeDropEdit.Text = "qwer";
					Assert("Changing the hour minute scope to garbage (which is invalid) should not make the hour numeric updown invisible", control.HourNumericUpDown.Visible);
					AssertEquals("(no date selected)", dateSchedule.Description);
				}
			}
		}

		public void TestSwitchingToHourMinuteWithScopeThis()
		{
			using (var form = new ZForm(new DateSchedule()))
			{
				using (var control = new EditDateScheduleControl())
				{
					form.Controls.Add(control);
					form.Show();
					var dateSchedule = form.BusinessEntity as DateSchedule;
					dateSchedule.Period = ScheduleRecurrenceType.Daily;
					control.HourMinuteScopeDropEdit.Text = PeriodScopeList.Codes.This;
					CombineAssertions("Preconditions", () =>
					{
						AssertEquals("This", control.HourMinuteScopeDropEdit.Text);
						AssertEquals("This", dateSchedule.PeriodScope);
					});

					dateSchedule.Period = ScheduleRecurrenceType.HourAndMinute;
					CombineAssertions("Switching to hour and minute should clean out a period of type \"this\"", () =>
					{
						AssertEquals(string.Empty, dateSchedule.PeriodScope);
						AssertEquals(string.Empty, control.HourMinuteScopeDropEdit.Text);
					});
					Assert("Since empty is not a valid scope we should get an error", dateSchedule.HasErrors);
				}
			}
		}
	}
}
