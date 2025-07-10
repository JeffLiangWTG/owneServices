namespace Enterprise.BufferManagement.Integration
{
	public interface IJobRelatedWorkflowsControlsProvider
	{
		IJobRelatedParentChildWorkflowsControl GetRelatedParentWorkflowsControl();
		IJobRelatedParentChildWorkflowsControl GetRelatedChildWorkflowsControl();
	}
}
