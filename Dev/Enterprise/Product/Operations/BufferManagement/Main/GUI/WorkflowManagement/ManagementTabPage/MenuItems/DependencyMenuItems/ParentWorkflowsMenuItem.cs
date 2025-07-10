using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	class ParentWorkflowsMenuItem : DependencyMenuItemBase
	{
		internal ParentWorkflowsMenuItem(ZGrid parentGrid)
			: base(parentGrid, new ParentWorkflowsMenuItemStrategy())
		{
		}

		internal ParentWorkflowsMenuItem(ProcessHeader workflow, bool saveAfterActions)
			: base(workflow, new ParentWorkflowsMenuItemStrategy { SaveAfterActions = saveAfterActions })
		{
		}
	}
}
