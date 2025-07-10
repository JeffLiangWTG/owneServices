namespace Enterprise.Registry.GUI
{
	public sealed partial class WorkflowValidationProcessTypeControl : RegistryZUserControl
	{
		public WorkflowValidationProcessTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			WorkflowValidationProcessTypesGrid.ReadOnly = readOnly;
		}
	}
}
