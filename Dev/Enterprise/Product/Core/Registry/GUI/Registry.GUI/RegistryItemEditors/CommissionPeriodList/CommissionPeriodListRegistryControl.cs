namespace Enterprise.Registry.GUI
{
	public partial class CommissionPeriodListRegistryControl : RegistryZUserControl
	{
		public CommissionPeriodListRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}
	}
}
