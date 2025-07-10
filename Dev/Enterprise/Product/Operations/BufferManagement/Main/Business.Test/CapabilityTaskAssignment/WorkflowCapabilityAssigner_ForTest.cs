using System;

namespace Enterprise.BufferManagement.Business.Test
{
	public class WorkflowCapabilityAssigner_ForTest : WorkflowCapabilityAssigner
	{
		public WorkflowCapabilityAssigner_ForTest(Action<ProcessHeader[]> preAssignmentAction)
		{
			this.preAssignmentAction = preAssignmentAction;
		}

		readonly Action<ProcessHeader[]> preAssignmentAction;

		protected override void PerformPreAssignmentAction_ForTest(ProcessHeader[] workflowBatch)
		{
			preAssignmentAction?.Invoke(workflowBatch);
		}

		protected override IDisposable SwitchBranchAndDepartment(IDisposable context, Guid branchPK, Guid departmentPK)
		{
			contextSwitchCount++;
			return base.SwitchBranchAndDepartment(context, branchPK, departmentPK);
		}

		public int ContextSwitchCount
		{
			get { return contextSwitchCount; }
		}

		int contextSwitchCount;
	}
}
