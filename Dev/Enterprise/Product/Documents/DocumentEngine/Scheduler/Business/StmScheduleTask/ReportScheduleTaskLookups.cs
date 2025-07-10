using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskLookups : StmScheduleTaskLookups
	{
		public ReportScheduleTaskLookups(ReportScheduleTask parent)
			: base(parent)
		{
		}

		#region MenuItems

		public SchedulableReportCollection MenuItems
		{
			get
			{
				if (menuItems == null)
				{
					menuItems = new SchedulableReportCollection(Factory);
				}
				return menuItems;
			}
		}

		SchedulableReportCollection menuItems;

		#endregion

		#region AllStaff

		public GlbStaffCollection AllStaff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion
	}
}
