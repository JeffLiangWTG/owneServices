namespace Enterprise.Registry.GUI
{
	public partial class CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl : RegistryZUserControl
	{
		public CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.MaximumDurationCalcEdit.ReadOnly = readOnly;
			this.UnlimitedDurationCheckBox.ReadOnly = readOnly;
		}
	}
}
