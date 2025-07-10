using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	class OpenDependencyMenuItem : LabelledDependencySubMenuItemBase
	{
		internal OpenDependencyMenuItem(LinkedProcessHeader dependency, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy)
			: base(dependency, sourceWorkflow, strategy)
		{
		}

		protected override void OnMenuItemClicked()
		{
			MainThreadEntityOpener.OpenEntityEditFormOnMainThread(Dependency.ProcessHeader, ControllerIDs.ProcessHeader);
		}
	}
}
