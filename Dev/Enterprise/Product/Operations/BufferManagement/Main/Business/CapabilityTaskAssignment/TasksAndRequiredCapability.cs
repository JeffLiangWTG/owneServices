using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class TasksAndRequiredCapability
	{
		public TasksAndRequiredCapability(IEnumerable<ProcessTask> tasks, GlbCapability capability)
		{
			Tasks = new List<ProcessTask>(tasks);
			RequiredCapability = capability;
		}

		public List<ProcessTask> Tasks { get; }

		public GlbCapability RequiredCapability { get; }
	}
}
