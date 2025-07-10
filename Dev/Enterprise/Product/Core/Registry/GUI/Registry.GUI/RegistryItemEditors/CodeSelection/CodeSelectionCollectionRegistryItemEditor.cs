using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class CodeSelectionCollectionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeSelectionCollectionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CodeSelectionCollectionControl();
		}
	}
}
