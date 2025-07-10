using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	class WorkflowRelatedDiagramsTabPageProvider : IWorkflowRelatedDiagramsTabPageProvider
	{
		public IWorkflowRelatedDiagramsTabPage GetTabPage(IProcessHeader workflow)
		{
			return new WorkflowRelatedDiagramsTabPage(workflow);
		}
	}
}
