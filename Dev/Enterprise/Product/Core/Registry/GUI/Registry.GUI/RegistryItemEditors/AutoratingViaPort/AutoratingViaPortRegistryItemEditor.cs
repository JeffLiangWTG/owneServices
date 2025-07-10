using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AutoratingViaPortRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AutoratingViaPortRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
			=> new AutoratingViaPortControl();

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
			=> EditorPaneAnchor.All;
	}
}
