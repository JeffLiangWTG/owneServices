using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Module.Schedule
{
	public class ArchiveScheduleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddFiltersForTranslatableText("Description", StmScheduleTaskSchema.S5_ScheduleDescription, typeof(ArchiveScheduleTask), ResString.GetMultilingualString("ArchiveSchedule|ArchiveScheduleFilter|Description", "Description"));
			return filters;
		}
	}
}
