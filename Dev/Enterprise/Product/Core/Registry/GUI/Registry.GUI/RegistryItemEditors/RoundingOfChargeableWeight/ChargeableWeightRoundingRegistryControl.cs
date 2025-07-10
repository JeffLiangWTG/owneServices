namespace Enterprise.Registry.GUI
{
	public partial class ChargeableWeightRoundingRegistryControl : RegistryZUserControl
	{
		public ChargeableWeightRoundingRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChargeableWeightRoundingGrid.ReadOnly = readOnly;
		}
	}
}
