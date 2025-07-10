namespace Enterprise.Registry.GUI
{
	public partial class CreditReportsRegistryControl : RegistryZUserControl
	{
		public CreditReportsRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CreditReportRegistryGrid.ReadOnly = readOnly;
		}
	}
}
