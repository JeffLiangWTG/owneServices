using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ManifestGroupNotificationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ManifestGroupNotificationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected sealed override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ManifestGroupNotificationControl();
		}

		protected sealed override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
