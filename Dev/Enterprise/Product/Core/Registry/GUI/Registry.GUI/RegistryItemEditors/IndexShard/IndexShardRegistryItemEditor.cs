using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class IndexShardRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public IndexShardRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new IndexShardRegistryControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
