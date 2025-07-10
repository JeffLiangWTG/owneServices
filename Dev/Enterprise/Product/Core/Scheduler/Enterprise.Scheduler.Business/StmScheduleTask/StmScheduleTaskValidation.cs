using CargoWise.EntityFramework;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskValidation : AutoStmScheduleTaskValidation
	{
		public StmScheduleTaskValidation(AutoStmScheduleTask parent)
			: base(parent)
		{
		}

		new StmScheduleTask Parent
		{
			get { return (StmScheduleTask)base.Parent; }
		}

		protected override void CheckS5_DayList()
		{
			base.CheckS5_DayList();
			if (Parent.Recurrence.WeeklyRange && !Parent.S5_DayList.Contains('Y'))
			{
				Parent.S5_DayListInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected override void CheckS5_ScheduleDescription()
		{
			base.CheckS5_ScheduleDescription();
			if (Parent.Recurrence.WeeklyRange && !Parent.S5_DayList.Contains('Y'))
			{
				Parent.S5_ScheduleDescriptionInfo.AddError(Res.GetString("09c6f3c8-79c6-4dd4-86f5-a89a52ca3fe3", "Please select at least one day."));
			}
		}

		protected override void CheckS5_TaskPeriodCount()
		{
			base.CheckS5_TaskPeriodCount();
			switch (Parent.S5_TaskPeriod)
			{
				case ScheduleRecurrenceType.Daily:
					if (Parent.Recurrence.DailyDay)
					{
						MandatoryValidation.CheckEntered(Parent.S5_TaskPeriodCountInfo);
					}
					break;

				case ScheduleRecurrenceType.Weekly:
					MandatoryValidation.CheckEntered(Parent.S5_TaskPeriodCountInfo);
					break;

				case ScheduleRecurrenceType.Monthly:
					if (Parent.Recurrence.MonthlyDay)
					{
						MandatoryValidation.CheckEntered(Parent.S5_TaskPeriodCountInfo);
					}
					break;

				case ScheduleRecurrenceType.AccountingPeriod:
					if (Parent.Recurrence.AccountingDay)
					{
						MandatoryValidation.CheckEntered(Parent.S5_TaskPeriodCountInfo);
					}
					break;
			}
		}

		protected override void CheckS5_DailyStartTimeIsValidZDateTimeRange()
		{
			// Don't validate as it can legitimatly be more than 10 years old
		}

		protected override void CheckS5_DailyEndTimeIsValidZDateTimeRange()
		{
			// Don't validate as it can legitimatly be more than 10 years old
		}
	}
}
