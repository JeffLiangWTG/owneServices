using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public class McpIslCredentialsSettingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public McpIslCredentialsSettingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new McpIslCredentialsSettingControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
