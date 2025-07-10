namespace Enterprise.Registry.GUI
{
	public partial class AWBRoundingRegistryControl : ChargeableWeightRoundingRegistryControl
	{
		public AWBRoundingRegistryControl()
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
