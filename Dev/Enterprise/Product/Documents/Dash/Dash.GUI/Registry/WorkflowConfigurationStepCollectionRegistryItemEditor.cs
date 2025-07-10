using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Dash.GUI
{
	public class WorkflowConfigurationStepCollectionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WorkflowConfigurationStepCollectionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new WorkflowConfigurationStepCollectionControl();
		}
	}
}
