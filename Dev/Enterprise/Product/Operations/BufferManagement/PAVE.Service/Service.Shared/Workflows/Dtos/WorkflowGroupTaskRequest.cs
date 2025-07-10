using System;

namespace Enterprise.BufferManagement.Service.Shared.Workflows.Dtos
{
	public class WorkflowGroupTaskRequest
	{
		#region Source

		public Guid? ActualPreviousTaskId { get; set; }

		public int ActualSequence { get; set; }

		public Guid? ActualNextTaskId { get; set; }

		#endregion
	}
}
