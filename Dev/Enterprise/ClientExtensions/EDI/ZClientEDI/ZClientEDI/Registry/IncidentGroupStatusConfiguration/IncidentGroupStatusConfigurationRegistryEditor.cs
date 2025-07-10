using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class IncidentGroupStatusConfigurationRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public IncidentGroupStatusConfigurationRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			EditorInfo = dataType as IncidentGroupStatusConfigurationRegistryEditorInfo;
		}

		public IncidentGroupStatusConfigurationRegistryEditorInfo EditorInfo;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new IncidentGroupStatusConfigurationControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
