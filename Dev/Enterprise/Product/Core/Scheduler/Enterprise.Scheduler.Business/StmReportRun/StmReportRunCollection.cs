using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	public class StmReportRunCollection : ActiveBusinessObjectCollection<StmReportRun>
	{
		public StmReportRunCollection(StmScheduleTask master)
			: base(master.Factory, master, null, StmReportRunSchema.RRI_S5_Schedule)
		{
			SetReadOnlyIncludingChildren(true);
		}

		public StmReportRunCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
