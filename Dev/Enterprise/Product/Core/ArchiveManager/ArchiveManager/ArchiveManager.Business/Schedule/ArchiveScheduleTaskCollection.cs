using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Scheduler.Business;

namespace Enterprise.ArchiveManager.Business.Schedule
{
	public class ArchiveScheduleTaskCollection : StmScheduleTaskCollection
	{
		public ArchiveScheduleTaskCollection(BusinessObjectFactory factory)
			: base(factory, Constants.ArchiveManager.ParentTableCode)
		{ }

		public new ArchiveScheduleTask AddNew()
			=> (ArchiveScheduleTask)base.AddNew();

		public new ArchiveScheduleTask this[int i]
			=> (ArchiveScheduleTask)base[i];
	}
}
