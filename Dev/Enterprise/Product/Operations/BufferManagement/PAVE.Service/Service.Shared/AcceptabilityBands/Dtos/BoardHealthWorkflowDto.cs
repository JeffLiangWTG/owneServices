using System;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class BoardHealthWorkflowDto
	{
		public Guid WorkflowId { get; set; }
		public string WorkflowTitle { get; set; }
		public Guid JobId { get; set; }
		public string JobCode { get; set; }
		public string JobType { get; set; }
		public string JobTitle { get; set; }
	}
}
