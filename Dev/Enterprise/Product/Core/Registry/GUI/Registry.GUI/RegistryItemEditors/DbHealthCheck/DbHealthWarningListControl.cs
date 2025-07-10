namespace Enterprise.Registry.GUI
{
	public partial class DbHealthWarningListControl : RegistryZUserControl
	{
		public DbHealthWarningListControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			warningZGrid.ReadOnly = readOnly;
		}
	}
}
