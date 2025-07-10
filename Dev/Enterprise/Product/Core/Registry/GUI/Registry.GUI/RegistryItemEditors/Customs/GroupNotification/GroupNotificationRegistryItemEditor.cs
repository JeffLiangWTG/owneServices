using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class GroupNotificationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public GroupNotificationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected sealed override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new GroupNotificationControl();
		}

		protected sealed override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
