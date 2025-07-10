namespace Enterprise.Registry.GUI
{
	public partial class DpsMatchingConfigurationControl : RegistryZUserControl
	{
		public DpsMatchingConfigurationControl()
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
