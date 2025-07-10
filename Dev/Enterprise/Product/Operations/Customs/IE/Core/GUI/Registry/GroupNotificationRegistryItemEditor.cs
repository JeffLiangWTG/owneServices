using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.GUI
{
	public class GroupNotificationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public GroupNotificationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new GroupNotificationControl();

		protected sealed override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;
	}
}
