using Enterprise.Registry.GUI;

namespace Enterprise.Dash.GUI
{
	public partial class WorkflowConfigurationStepCollectionControl : RegistryZUserControl
	{
		public WorkflowConfigurationStepCollectionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			workflowConfigurationStepGrid.ReadOnly = readOnly;
		}
	}
}
