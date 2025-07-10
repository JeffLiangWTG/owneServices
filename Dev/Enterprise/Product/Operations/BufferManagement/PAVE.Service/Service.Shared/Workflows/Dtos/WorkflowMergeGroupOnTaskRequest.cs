using System;

namespace Enterprise.BufferManagement.Service.Shared.Workflows.Dtos
{
	public class WorkflowMergeGroupOnTaskRequest
	{
		#region Source

		public Guid[] SourceGroupTasks { get; set; } = Array.Empty<Guid>();

		#endregion

		#region Target

		public int TargetTaskSequence { get; set; }

		#endregion
	}
}
