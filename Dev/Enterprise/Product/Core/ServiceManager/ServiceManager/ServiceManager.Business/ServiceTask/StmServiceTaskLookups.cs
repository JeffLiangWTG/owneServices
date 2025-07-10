using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceTaskLookups : AutoStmServiceTaskLookups
	{
		public StmServiceTaskLookups(AutoStmServiceTask parent) : base(parent)
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
					months.AddPair("1", Res.GetString("A9CA780F-4F10-4241-A5F9-B08A1991C69C", "January"));
					months.AddPair("2", Res.GetString("E2821210-4895-4A49-B70C-24E6725A7415", "February"));
					months.AddPair("3", Res.GetString("2FC71654-3A69-48F9-981B-C0BAEE0E7564", "March"));
					months.AddPair("4", Res.GetString("D7EC1CD6-AAD3-42FD-AB3B-6736A78360C4", "April"));
					months.AddPair("5", Res.GetString("656DEF1B-87EB-46AE-87DB-E14A22E37256", "May"));
					months.AddPair("6", Res.GetString("249DCB8E-C94B-4635-8B91-0B6CD21395DE", "June"));
					months.AddPair("7", Res.GetString("4128EE23-332C-4C8E-A472-260061019929", "July"));
					months.AddPair("8", Res.GetString("989E0314-7A37-45BF-B8D9-A17B56B428BB", "August"));
					months.AddPair("9", Res.GetString("4F29D654-E7A8-4BA2-9AEB-1069D10BB43E", "September"));
					months.AddPair("10", Res.GetString("7552D301-76E9-431A-8E24-85A1885DCC5C", "October"));
					months.AddPair("11", Res.GetString("04D437A6-93D0-4BFF-AF3A-705B6B31C1D2", "November"));
					months.AddPair("12", Res.GetString("DF6AEECD-99A2-4217-A593-2FA734962959", "December"));
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
					weekCounts.AddPair("1", Res.GetString("37460420-184F-4598-90FA-C0E195C01964", "first"));
					weekCounts.AddPair("2", Res.GetString("0FFA152B-4322-4E7D-8EDA-F0ACAC6B9489", "second"));
					weekCounts.AddPair("3", Res.GetString("4E14F3FD-104D-4482-9DEE-078CC5B013E8", "third"));
					weekCounts.AddPair("4", Res.GetString("427799D2-2805-42F4-963A-B0C8AD539A40", "fourth"));
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
