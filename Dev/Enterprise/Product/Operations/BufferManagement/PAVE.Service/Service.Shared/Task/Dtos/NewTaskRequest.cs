using System;

namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos
{
	public class NewTaskRequest
	{
		public Guid WorkflowId { get; set; }

		public string Description { get; set; }

		public string Type { get; set; }

		public int? EstimateFactor { get; set; }

		public int? LowEstimate { get; set; }

		public int? ActualDuration { get; set; }

		public string CapabilityCode { get; set; }

		public Guid? RelatedTaskId { get; set; }

		public Guid? StaffId { get; set; }

		public string Notes { get; set; }
	}
}
