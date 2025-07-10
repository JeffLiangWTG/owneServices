using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ADPasswordSettingsRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ADPasswordSettingsRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new ADPasswordSettingsRegistryControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
