using System;

namespace Enterprise.BufferManagement.Service.Shared.Workflows.Dtos
{
	public class WorkflowGroupMergeRequest
	{
		#region Source

		public Guid[] SourceGroupTasks { get; set; } = Array.Empty<Guid>();

		#endregion

		#region Target

		public Guid[] TargetGroupTasks { get; set; } = Array.Empty<Guid>();

		#endregion
	}
}
