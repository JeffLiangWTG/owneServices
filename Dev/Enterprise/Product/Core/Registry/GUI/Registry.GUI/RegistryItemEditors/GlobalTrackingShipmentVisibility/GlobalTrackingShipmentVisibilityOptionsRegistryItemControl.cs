namespace Enterprise.Registry.GUI
{
	public partial class GlobalTrackingShipmentVisibilityOptionsRegistryItemControl : RegistryZUserControl
	{
		public GlobalTrackingShipmentVisibilityOptionsRegistryItemControl()
		{
			InitializeComponent();
			ContainerAutomationGroupBox.Visible = yesRadioButton.Checked;
			shipmentVisibilityServiceEhubIDsGrid.Enabled = overrideDefault.Checked;
			this.yesRadioButton.CheckedChanged += YesRadioButton_CheckedChanged;
			this.overrideDefault.CheckedChanged += OverrideDefault_CheckedChanged;
		}

		void YesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			ContainerAutomationGroupBox.Visible = yesRadioButton.Checked;
		}

		void OverrideDefault_CheckedChanged(object sender, System.EventArgs e)
		{
			shipmentVisibilityServiceEhubIDsGrid.Enabled = overrideDefault.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			yesRadioButton.Enabled = !readOnly;
			noRadioButton.Enabled = !readOnly;
		}
	}
}
