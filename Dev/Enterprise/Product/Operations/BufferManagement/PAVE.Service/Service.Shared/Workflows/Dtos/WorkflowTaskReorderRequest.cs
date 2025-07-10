using System;

namespace Enterprise.BufferManagement.Service.Shared.Workflows.Dtos
{
	public class WorkflowTaskReorderRequest
	{
		#region Source

		public Guid? ActualPreviousTaskId { get; set; }

		public int ActualSequence { get; set; }

		public Guid? ActualNextTaskId { get; set; }

		#endregion

		#region Target

		public Guid? PreviousTaskId { get; set; }

		public int? PreviousTaskSequence { get; set; }

		public Guid? NextTaskId { get; set; }

		public int? NextTaskSequence { get; set; }

		#endregion

		public bool? IsUngrouping { get; set; }
	}
}
