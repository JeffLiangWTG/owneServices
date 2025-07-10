using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class NotificationEmailTemplateRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NotificationEmailTemplateRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new NotificationEmailTemplateRegistryControl();
		}
	}
}
