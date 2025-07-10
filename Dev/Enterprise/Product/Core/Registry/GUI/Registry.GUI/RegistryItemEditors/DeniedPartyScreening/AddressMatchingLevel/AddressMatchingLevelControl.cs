namespace Enterprise.Registry.GUI
{
	public partial class AddressMatchingLevelControl : RegistryZUserControl
	{
		public AddressMatchingLevelControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			StrictRadioButton.Enabled = !readOnly;
			BalancedRadioButton.Enabled = !readOnly;
			ComprehensiveRadioButton.Enabled = !readOnly;
		}
	}
}
