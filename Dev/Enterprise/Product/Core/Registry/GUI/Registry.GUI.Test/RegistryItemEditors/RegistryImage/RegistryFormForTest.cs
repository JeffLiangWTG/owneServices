using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryFormForTest : ZChildForm, IRegistryForm
	{
		public void UpdateHasChanges()
		{
			IsUpdateHasChangesTriggered = true;
		}

		public bool IsUpdateHasChangesTriggered { get; private set; }
	}
}
