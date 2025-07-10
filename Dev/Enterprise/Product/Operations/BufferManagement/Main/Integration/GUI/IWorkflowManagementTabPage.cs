using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkflowManagementTabPage
	{
		void NavigateToWorkflowItem(IProcessTask task);
		void NavigateToWorkflowItem(IProcessHeader workflow);
	}
}
