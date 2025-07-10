using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business.Test
{
	public sealed class ReleaseGateTestCoordinator
	{
		public Action PreWorkflowReloadAction { get; set; }

		public Func<ViewProcessHeader, bool> IsWorkflowReleasableFunc { get; set; }

		public IList<ViewProcessHeader> WorkflowsInOrderOfProcessing => processedWorkflows.ToArray();

		readonly List<ViewProcessHeader> processedWorkflows = new List<ViewProcessHeader>();

		public bool SaveCapacityCacheForAllStaff { get; set; } = true;

		internal void AddProcessedWorkflow(ViewProcessHeader workflow)
		{
			processedWorkflows.Add(workflow);
		}

		public bool SortBatchedWorkflows { get; set; }
	}
}
