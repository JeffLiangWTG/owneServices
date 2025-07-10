using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	public class JobRelatedWorkflowsControlsProvider : IJobRelatedWorkflowsControlsProvider
	{
		public IJobRelatedParentChildWorkflowsControl GetRelatedParentWorkflowsControl()
		{
			return new JobRelatedParentWorkflowsControl();
		}

		public IJobRelatedParentChildWorkflowsControl GetRelatedChildWorkflowsControl()
		{
			return new JobRelatedChildWorkflowsControl();
		}
	}
}
