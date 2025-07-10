using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopyModuleService : IScheduleTaskModuleService
	{
		public ZString ScheduleTaskModuleName { get; set; }
	}
}
