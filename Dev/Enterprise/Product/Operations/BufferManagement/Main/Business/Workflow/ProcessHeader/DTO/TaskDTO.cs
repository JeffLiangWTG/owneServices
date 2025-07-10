using System;
using System.Diagnostics;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("PK: {PK}, StaffCode: {StaffCode}, ResourceCapability: {RequiredCapability}")]
	public class TaskDTO : IEstimatable
	{
		public TaskDTO(Guid pk, WorkflowDTO workflow)
		{
			PK = pk;
			Workflow = Argument.NotNull(workflow, "workflow");
		}

		public WorkflowDTO Workflow { get; private set; }

		public Guid PK { get; private set; }
		public string TaskID { get; set; }

		public string StaffCode { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal EstimateHours { get; set; }
		public Guid RequiredCapability { get; set; }
		public Guid Group { get; set; }
	}
}
