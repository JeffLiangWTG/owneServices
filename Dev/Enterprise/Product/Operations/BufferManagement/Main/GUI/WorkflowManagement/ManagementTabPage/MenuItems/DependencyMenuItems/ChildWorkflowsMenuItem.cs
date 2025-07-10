using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	class ChildWorkflowsMenuItem : DependencyMenuItemBase
	{
		internal ChildWorkflowsMenuItem(ZGrid parentGrid)
			: base(parentGrid, new ChildWorkflowsMenuItemStrategy())
		{
		}

		internal ChildWorkflowsMenuItem(ProcessHeader workflow, bool saveAfterActions)
			: base(workflow, new ChildWorkflowsMenuItemStrategy { SaveAfterActions = saveAfterActions })
		{
		}
	}
}
