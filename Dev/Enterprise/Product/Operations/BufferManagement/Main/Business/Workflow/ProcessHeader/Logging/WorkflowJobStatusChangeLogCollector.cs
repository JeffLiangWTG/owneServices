using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	sealed class WorkflowJobStatusChangeLogCollector
	{
		readonly HashSet<ProcessHeader> workflowsNeedingStatusChangeLog = new HashSet<ProcessHeader>();

		internal void Collect(ProcessHeader workflow)
		{
			if (!workflow.IsTemplate)
			{
				workflowsNeedingStatusChangeLog.Add(workflow);
			}
		}

		internal void CommitAllLogs()
		{
			foreach (var processHeader in workflowsNeedingStatusChangeLog)
			{
				ProcessHeaderLogger.AddFetchHints(processHeader, processHeader.HasOpenStatus);
			}

			foreach (var processHeader in workflowsNeedingStatusChangeLog)
			{
				ProcessHeaderLogger.AddJobStatusChangeEvent(processHeader, processHeader.HasOpenStatus);
			}
		}
	}
}
