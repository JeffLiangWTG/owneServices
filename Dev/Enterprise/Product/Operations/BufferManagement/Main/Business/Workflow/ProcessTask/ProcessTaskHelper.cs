using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	class ProcessTaskHelper : IProcessTaskHelper
	{
		public bool IsTaskStartable(IProcessTask task) => task.IsStartable();

		[ThreadSafe]
		static readonly HashSet<string> serviceTaskCodesNotAllowedToDeleteTasks = new HashSet<string>()
		{
			CapabilityTaskAutoAssignmentServiceTask.Code,
			ReleaseGateRunnerServiceTask.Code,
			SchematicTransferLoopMonitorServiceTask.Code,
			StaggeredReleaseDelayCalculatorTask.Code,
			TagMonitorServiceTask.Code,
			TagServiceTask.Code,
			TransferRuleRunnerServiceTask.Code,
			"PVE", // BI PAVE Subscribers
			"TAS", // ACTION Scheduler
		};

		public bool CanDelete(IProcessTask task)
		{
			var bo = (task as BusinessObject);

			if (bo == null || !bo.IsInDatabase)
			{
				return true;
			}

			var serviceTaskCodeService = bo.Factory.ServiceContainer.GetService<ServiceTaskCodeService>();

			if (serviceTaskCodeService == null)
			{
				return true;
			}

			return !serviceTaskCodesNotAllowedToDeleteTasks.Contains(serviceTaskCodeService.ServiceTaskCode);
		}
	}
}
