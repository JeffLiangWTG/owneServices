using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	class DependentWorkflowsMenuItem : DependencyMenuItemBase
	{
		internal DependentWorkflowsMenuItem(ZGrid parentGrid)
			: base(parentGrid, new DependentWorkflowsMenuItemStrategy())
		{
		}

		internal DependentWorkflowsMenuItem(ProcessHeader workflow, bool saveAfterActions)
			: base(workflow, new DependentWorkflowsMenuItemStrategy { SaveAfterActions = saveAfterActions })
		{
		}
	}
}
