using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	internal class DbHealthWarningListRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DbHealthWarningListRegistryItemEditor(IRegistryDataType dataType, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			: base(dataType, currentFallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DbHealthWarningListControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
