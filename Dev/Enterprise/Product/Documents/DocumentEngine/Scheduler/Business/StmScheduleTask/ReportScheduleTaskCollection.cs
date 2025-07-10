
using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTaskCollection : StmScheduleTaskCollection
	{
		public ReportScheduleTaskCollection(BusinessObjectFactory factory)
			: base(factory, StmMenuItemSchema.Constants.Prefix)
		{
		}

		public new ReportScheduleTask AddNew()
		{
			return (ReportScheduleTask)base.AddNew();
		}

		public new ReportScheduleTask this[int i]
		{
			get { return (ReportScheduleTask)base[i]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(StmScheduleTask));
			filter.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, ReportScheduleTask.ScheduleType);
			result.AddToFilter(filter);
			return result;
		}
	}
}
