using System;
using System.ComponentModel;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class EditDateScheduleControl : ZUserControl
	{
		public EditDateScheduleControl()
		{
			InitializeComponent();
		}

		DateSchedule DateSchedule
		{
			get { return DataSource as DateSchedule; }
		}

		DateSchedule oldDataSource;
		string oldDataMember;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != oldDataSource || dataMember != oldDataMember)
			{
				UnBindEvents();

				base.SetDataBinding(dataSource, dataMember);

				DateSchedule dateSchedule = DateSchedule;
				if (dateSchedule != null)
				{
					Period = dateSchedule.Period;
					UpdatePeriodNumberVisibility();

					dateSchedule.PeriodInfo.ValueChanged += new EventHandler(PeriodInfo_ValueChanged);
					dateSchedule.PeriodScopeInfo.ValueChanged += new EventHandler(PeriodScopeInfo_ValueChanged);
				}

				oldDataSource = dateSchedule;
				oldDataMember = dataMember;
			}
		}

		void PeriodScopeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePeriodNumberVisibility();
		}

		void UpdatePeriodNumberVisibility()
		{
			DayNumericUpDown.Visible = !DateSchedule.PeriodCountInfo.ReadOnly;
			WeekNumericUpDown.Visible = !DateSchedule.PeriodCountInfo.ReadOnly;
			MonthNumericUpDown.Visible = !DateSchedule.PeriodCountInfo.ReadOnly;
			YearNumericUpDown.Visible = !DateSchedule.PeriodCountInfo.ReadOnly;
		}

		void PeriodInfo_ValueChanged(object sender, EventArgs e)
		{
			Period = DateSchedule.Period;
		}

		string period;
		[Description("Sets the period type."), Category("Misc"), DefaultValue(ScheduleRecurrenceType.Daily), Browsable(true)]
		public string Period
		{
			get { return period; }
			set
			{
				if (value != period)
				{
					period = value;

					switch (period)
					{
						case ScheduleRecurrenceType.Daily:
							DayPanel.Visible = true;
							WeekPanel.Visible = false;
							MonthPanel.Visible = false;
							YearPanel.Visible = false;
							HourMinutePanel.Visible = false;
							break;

						case ScheduleRecurrenceType.Weekly:
							DayPanel.Visible = false;
							WeekPanel.Visible = true;
							MonthPanel.Visible = false;
							YearPanel.Visible = false;
							HourMinutePanel.Visible = false;
							break;

						case ScheduleRecurrenceType.Monthly:
							DayPanel.Visible = false;
							WeekPanel.Visible = false;
							MonthPanel.Visible = true;
							YearPanel.Visible = false;
							HourMinutePanel.Visible = false;
							MonthDayNumericUpDown.BindTo = "DayNumber";
							YearDayNumericUpDown.BindTo = "";
							break;

						case ScheduleRecurrenceType.Yearly:
							DayPanel.Visible = false;
							WeekPanel.Visible = false;
							MonthPanel.Visible = false;
							YearPanel.Visible = true;
							HourMinutePanel.Visible = false;
							MonthDayNumericUpDown.BindTo = "";
							YearDayNumericUpDown.BindTo = "DayNumber";
							break;

						case ScheduleRecurrenceType.HourAndMinute:
							DayPanel.Visible = false;
							WeekPanel.Visible = false;
							MonthPanel.Visible = false;
							YearPanel.Visible = false;
							HourMinutePanel.Visible = true;

							if (DateSchedule.PeriodScope == PeriodScopeList.Codes.This)
							{
								DateSchedule.PeriodScope = string.Empty;
							}

							break;

						default:
							DayPanel.Visible = false;
							WeekPanel.Visible = false;
							MonthPanel.Visible = false;
							YearPanel.Visible = false;
							HourMinutePanel.Visible = false;
							break;
					}
				}
			}
		}

		void PeriodNumberUpDown_ValueChanged(object sender, EventArgs e)
		{
			var numericUpDown = sender as ZNumericUpDown;
			if (numericUpDown != null)
			{
				DateSchedule.PeriodCount = Convert.ToInt32(numericUpDown.Value);
			}
		}

		void DayNumberUpDown_ValueChanged(object sender, EventArgs e)
		{
			var numericUpDown = sender as ZNumericUpDown;
			if (numericUpDown != null)
			{
				DateSchedule.DayNumber = Convert.ToInt16(numericUpDown.Value);
			}
		}

		void HourNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			var numericUpDown = sender as ZNumericUpDown;
			if (numericUpDown != null)
			{
				DateSchedule.Hour = Convert.ToInt16(numericUpDown.Value);
			}
		}

		void MinuteNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			var numericUpDown = sender as ZNumericUpDown;
			if (numericUpDown != null)
			{
				DateSchedule.MinuteOfHour = Convert.ToInt16(numericUpDown.Value);
			}
		}

		void UnBindEvents()
		{
			if (oldDataSource != null)
			{
				oldDataSource.PeriodInfo.ValueChanged -= new EventHandler(PeriodInfo_ValueChanged);
				oldDataSource.PeriodScopeInfo.ValueChanged -= new EventHandler(PeriodScopeInfo_ValueChanged);
			}
		}
	}
}
