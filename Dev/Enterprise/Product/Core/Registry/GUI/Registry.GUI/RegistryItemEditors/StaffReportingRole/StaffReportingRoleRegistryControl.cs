namespace Enterprise.Registry.GUI
{
	public partial class StaffReportingRoleRegistryControl : RegistryZUserControl
	{
		public StaffReportingRoleRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			StaffReportingRoleGrid.ReadOnly = readOnly;
		}
	}
}
