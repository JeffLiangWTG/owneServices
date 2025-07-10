using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecurrenceLookups : ZLookups
	{
		public StmScheduleTaskRecurrenceLookups(StmScheduleTaskRecurrence parent)
			: base(parent)
		{
		}

		#region Months

		public CodeDescriptionPairList Months
		{
			get
			{
				if (months == null)
				{
					months = new CodeDescriptionPairList();
					months.AddPair("1", Res.GetString("f927b362-c144-4593-a2d6-05bde8058e33", "January"));
					months.AddPair("2", Res.GetString("6f2bf42a-0386-43ea-8ea6-73ba70aadacb", "February"));
					months.AddPair("3", Res.GetString("c7fa1166-b84d-48fc-bc76-bb9f3fe74ef4", "March"));
					months.AddPair("4", Res.GetString("4e40ebb5-d0cd-4158-b03c-6d2cf77f1a89", "April"));
					months.AddPair("5", Res.GetString("0075eae6-5b98-4099-bc3f-637c2f14305c", "May"));
					months.AddPair("6", Res.GetString("ea8ed1dd-6045-43ad-83ba-ec527e7579a9", "June"));
					months.AddPair("7", Res.GetString("60268eb6-6afe-42f2-aac4-123568b09609", "July"));
					months.AddPair("8", Res.GetString("bcee5d95-9edf-47d1-a69f-f448f25b4ee6", "August"));
					months.AddPair("9", Res.GetString("97741014-f05e-4f3d-8a20-c49cb8595e96", "September"));
					months.AddPair("10", Res.GetString("33c823c1-2173-492b-a439-88fe06833cea", "October"));
					months.AddPair("11", Res.GetString("33f37a49-8ea6-418d-aaaf-55da6ab8c719", "November"));
					months.AddPair("12", Res.GetString("0a2dbc22-cf6b-47c1-8aab-3bbfa5e181d6", "December"));
				}
				return months;
			}
		}

		CodeDescriptionPairList months;

		#endregion

		#region Week Counts

		public CodeDescriptionPairList WeekCounts
		{
			get
			{
				if (weekCounts == null)
				{
					weekCounts = new CodeDescriptionPairList();
					weekCounts.AddPair("1", Res.GetString("faf59b1f-9c2e-40ae-bbbe-05249b7d992c", "first"));
					weekCounts.AddPair("2", Res.GetString("947d450e-d516-4ba7-950b-4017c65a038e", "second"));
					weekCounts.AddPair("3", Res.GetString("34dfc376-bd2f-4aa1-ac5d-b7b76b2972b8", "third"));
					weekCounts.AddPair("4", Res.GetString("12cddb6c-6d6f-4a8a-8a63-ade54011ef22", "fourth"));
				}
				return weekCounts;
			}
		}

		CodeDescriptionPairList weekCounts;

		#endregion

		#region Week Days

		public WeekDayList WeekDays
		{
			get
			{
				if (weekDays == null)
				{
					weekDays = new WeekDayList();
				}
				return weekDays;
			}
		}
		WeekDayList weekDays;

		#endregion
	}
}
