using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkflow : IIdentified
	{
		Guid PK { get; }
		Guid ReleaseGroupPK { get; }
		Guid CurrentComponentPK { get; set; }
		Guid JobLevelWorkflowPK { get; }

		Guid ParentId { get; }
		string ParentTableCode { get; }

		int PlannedDurationMinutes { get; set; }

		string Description { get; }
		string JobDescription { get; }
		string WorkflowType { get; }
		string Status { get; }

		DateTime EarliestStartDateUtc { get; }
		DateTime JobEarliestStartDateUtc { get; }
		DateTime LastTransferDateUtc { get; set; }

		string LastTransferType { get; set; }

		IEnumerable<IWorkflowTask> Tasks { get; }

		bool IsCcpmScheduleReleasable { get; }
	}
}
