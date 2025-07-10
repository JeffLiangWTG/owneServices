using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CapabilityTaskAssignmentScheduleForWorkflow
	{
		public CapabilityTaskAssignmentScheduleForWorkflow()
		{
		}

		public CapabilityTaskAssignmentScheduleForWorkflow(ZDateTime targetTimeUtc, IEnumerable<ProcessTask> tasks, GlbCapability capability)
		{
			TargetTimeUtc = targetTimeUtc;
			EarliestTasks.Add(new TasksAndRequiredCapability(tasks, capability));
		}

		public ZDateTime TargetTimeUtc { get; }

		public bool IsSchedulingRequired => TargetTimeUtc != ZDateTime.Empty;

		public List<TasksAndRequiredCapability> EarliestTasks = new List<TasksAndRequiredCapability>();
	}
}
