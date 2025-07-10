namespace Enterprise.Registry.GUI
{
	public partial class WorkflowManagerAssistWithThisTaskControl : RegistryZUserControl
	{
		public WorkflowManagerAssistWithThisTaskControl()
		{
			InitializeComponent();
		}

		#region Overrides of RegistryZUserControl

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ParentGrid.ReadOnly = true;
			WorkflowTypeBox.ReadOnly = true;
			TaskTypeBox.ReadOnly = readOnly;
			LowEstimateMinutesBox.ReadOnly = readOnly;
			VariationFactorBox.ReadOnly = readOnly;
		}

		#endregion
	}
}
