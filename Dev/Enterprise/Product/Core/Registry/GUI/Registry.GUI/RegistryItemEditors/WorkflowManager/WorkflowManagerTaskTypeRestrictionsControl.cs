namespace Enterprise.Registry.GUI
{
	public partial class WorkflowManagerTaskTypeRestrictionsControl : RegistryZUserControl
	{
		public WorkflowManagerTaskTypeRestrictionsControl()
		{
			InitializeComponent();
		}

		#region RegistryZUserControl Overrides

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RestrictionsGrid.ReadOnly = TaskTypesGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}
