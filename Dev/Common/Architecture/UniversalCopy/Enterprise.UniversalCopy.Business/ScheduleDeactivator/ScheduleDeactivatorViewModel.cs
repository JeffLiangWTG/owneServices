using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Scheduler.Business;

namespace Enterprise.UniversalCopy.Business
{
	public class ScheduleDeactivatorViewModel : NonPersistentBusinessObject
	{
		public ScheduleDeactivatorViewModel(IEnumerable<StmUniversalCopyScheduleTask> tasks)
		{
			relatedCopySchedules = tasks;
		}

		readonly IEnumerable<StmUniversalCopyScheduleTask> relatedCopySchedules;

		public ScheduleDeactivatorResponse? Response { get; set; }

		public ZString SchedulesText
		{
			get
			{
				var scheduleText = new List<ZString>();
				foreach (StmUniversalCopyScheduleTask schedule in relatedCopySchedules)
				{
					string taskPeriod = "";
					switch (schedule.S5_TaskPeriod)
					{
						case ScheduleRecurrenceType.Daily:
							taskPeriod = Res.GetString("3c0ba5a8-d92e-4b98-ad80-959688818a39", "Daily");
							break;
						case ScheduleRecurrenceType.Weekly:
							taskPeriod = Res.GetString("bbc11f36-04dd-4ddf-bd8c-3d0d5076ce41", "Weekly");
							break;
						case ScheduleRecurrenceType.Monthly:
							taskPeriod = Res.GetString("3f2905df-4be0-4ecf-b11a-73fe73916af7", "Monthly");
							break;
						case ScheduleRecurrenceType.AccountingPeriod:
							taskPeriod = Res.GetString("5909d85f-3717-48f2-9b3e-11986573fdb7", "Accounting Period");
							break;
						case ScheduleRecurrenceType.Yearly:
							taskPeriod = Res.GetString("a76e0f73-4784-46fe-952b-cbdf873767af", "Yearly");
							break;
					}

					scheduleText.Add(Res.GetString("93842ee6-76fb-4e79-a7ce-632c492f6729", "Schedule Description: {0} Recurrence: ({1}) {2}", schedule.S5_ScheduleDescription, schedule.S5_TaskPeriod, taskPeriod));
				}

				return ZString.Join(System.Environment.NewLine, scheduleText.ToArray());
			}
		}

		public ZString FormCaption
		{
			get
			{
				var countOfSchedules = 0;
				foreach (StmUniversalCopyScheduleTask schedule in relatedCopySchedules)
				{
					countOfSchedules += 1;
				}
				if (countOfSchedules > 1)
				{
					return Res.GetString("3c8d21b9-0c68-4820-9aa1-12dc46228183", @"Multiple copy schedules are attached.
Deactivate all related copy schedules before canceling?");
				}
				else
				{
					return Res.GetString("1416597d-48b5-4cdd-AA72-5d73edfc734f", @"A copy schedule is attached.
Deactivate this related copy schedule before canceling?");
				}
			}
		}
	}
}
