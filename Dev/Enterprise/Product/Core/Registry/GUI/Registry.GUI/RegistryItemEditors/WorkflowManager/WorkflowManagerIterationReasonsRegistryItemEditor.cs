using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class WorkflowManagerIterationReasonsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WorkflowManagerIterationReasonsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new WorkflowManagerIterationReasonsControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override IRegistryBusiness PrepareDataForBinding(IRegistryBusiness data)
		{
			var originalData = (CategorisedWorkflowIterationReasonsCollection)data;
			originalData.SynchroniseWithWorkflowDescriptorList();

			return base.PrepareDataForBinding(data);
		}
	}
}
