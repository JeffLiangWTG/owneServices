namespace Enterprise.Registry.GUI
{
	public partial class LandedCostingPreferencesControl : RegistryZUserControl
	{
		public LandedCostingPreferencesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LandedCostingGroupsGrid.ReadOnly = readOnly;
			ChargeGroupsAndChargeCodesGrid.ReadOnly = readOnly;
		}
	}
}
