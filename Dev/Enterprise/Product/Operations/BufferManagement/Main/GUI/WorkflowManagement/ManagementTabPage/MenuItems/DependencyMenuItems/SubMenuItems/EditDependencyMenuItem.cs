using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	class EditDependencyMenuItem : LabelledDependencySubMenuItemBase
	{
		internal EditDependencyMenuItem(LinkedProcessHeader dependency, ProcessHeader sourceWorkflow, DependencyMenuItemStrategy strategy)
			: base(dependency, sourceWorkflow, strategy)
		{
		}

		protected override void OnMenuItemClicked()
		{
			MainThreadEntityOpener.OpenEntityEditFormOnMainThread(Dependency.LinkToProcessHeader, ControllerIDs.ProcessHeaderLink);
		}
	}
}
