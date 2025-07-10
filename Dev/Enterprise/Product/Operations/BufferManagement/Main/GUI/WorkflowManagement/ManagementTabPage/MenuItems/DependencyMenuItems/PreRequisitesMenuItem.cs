using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	class PreRequisitesMenuItem : DependencyMenuItemBase
	{
		internal PreRequisitesMenuItem(ZGrid parentGrid)
			: base(parentGrid, new PreRequisitesMenuItemStrategy { RefreshOpenPrerequisitesAfterEdits = true })
		{
		}

		internal PreRequisitesMenuItem(ProcessHeader workflow, bool saveAfterActions)
			: base(workflow, new PreRequisitesMenuItemStrategy { SaveAfterActions = saveAfterActions })
		{
		}
	}
}
