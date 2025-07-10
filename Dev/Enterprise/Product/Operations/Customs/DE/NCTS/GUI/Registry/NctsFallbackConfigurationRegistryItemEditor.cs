using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class NctsFallbackConfigurationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NctsFallbackConfigurationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new NctsFallbackConfigurationRegistryItemControl();
	}
}
