using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service
{
	public class CapabilityTaskAutoAssignmentService : PAVEService, ICapabilityTaskAutoAssignmentService
	{
		public void AutoAssignCapabilityTasks(IEnumerable<Guid> workflowPKs, ILogger logger)
		{
			Process(workflowPKs, logger);
		}

		protected override void ProcessCore(IEnumerable<Guid> workflowPKs, ILogger logger)
		{
			if (BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				logger.Log(LogType.Information, "Capability Task Auto-Assignment not run because the [Disable Capacity Calculations] registry item is enabled.");
				return;
			}
			var assigner = ObjectFactory.Get<IWorkflowCapabilityAssigner>();
			assigner.AutoAssignWorkflowsImmediatelyOrDelayed(null, workflowPKs, logger);
		}
	}
}
