using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	public interface IWorkflowCapabilityAssignerDataAccessor
	{
		IWorkflowCapabilityAssignerBatchProcessor GetWorkflowBatchProcessor(BMComponent component, string workflowBatchNameForLogging);

		IEnumerable<ProcessHeader> LoadWorkflowsByPKs(IEnumerable<Guid> workflowPKs);

		IWorkflowCapabilityAssignerRelatedDataLoader GetRelatedDataLoader();
	}
}
