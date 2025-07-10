using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkflowManagementTabPageProvider
	{
		IWorkflowManagementTabPage GetTabPage(IWorkflowProviderCore provider);
	}
}
