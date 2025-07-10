namespace Enterprise.Registry.GUI
{
	public partial class AutomaticContainerCreationControl : RegistryZUserControl
	{
		public AutomaticContainerCreationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			alwaysCreateRadioButton.Enabled = !readOnly;
			createUpToATDOrShippingInstructionRadioButton.Enabled = !readOnly;
			neverCreateRadioButton.Enabled = !readOnly;
		}
	}
}
