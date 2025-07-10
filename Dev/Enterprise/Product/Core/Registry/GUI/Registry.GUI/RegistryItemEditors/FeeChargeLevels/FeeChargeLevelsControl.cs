namespace Enterprise.Registry.GUI
{
	public partial class FeeChargeLevelsControl : RegistryZUserControl
	{
		public FeeChargeLevelsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			FeeChargeTypesGrid.ReadOnly = readOnly;
			FeeChargeLevelsGrid.ReadOnly = readOnly;
		}
	}
}
