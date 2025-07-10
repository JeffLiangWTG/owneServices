using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class BoleroEBLForOrganisationConfigurationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public BoleroEBLForOrganisationConfigurationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new BoleroEBLForOrganisationConfigurationControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
