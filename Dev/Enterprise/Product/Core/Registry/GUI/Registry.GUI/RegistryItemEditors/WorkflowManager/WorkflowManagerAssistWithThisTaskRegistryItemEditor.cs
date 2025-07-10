using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class WorkflowManagerAssistWithThisTaskRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WorkflowManagerAssistWithThisTaskRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new WorkflowManagerAssistWithThisTaskControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override IRegistryBusiness PrepareDataForBinding(IRegistryBusiness data)
		{
			var originalData = (CategorisedAssistWithThisTaskSettingCollection)data;
			originalData.SynchroniseWithWorkflowDescriptorList();

			return base.PrepareDataForBinding(data);
		}
	}
}
