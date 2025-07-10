namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkflowRelatedDiagramsTabPageProvider
	{
		IWorkflowRelatedDiagramsTabPage GetTabPage(IProcessHeader workflow);
	}
}
