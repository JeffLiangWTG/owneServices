namespace Enterprise.Registry.GUI
{
	public partial class CalculateDeliveryDueDateOptionsRegistryItemControl : RegistryZUserControl
	{
		public CalculateDeliveryDueDateOptionsRegistryItemControl()
		{
			InitializeComponent();
			zPanel1.Visible = yesRadioButton.Checked;
			this.yesRadioButton.CheckedChanged += YesRadioButton_CheckedChanged;
		}

		void YesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			zPanel1.Visible = yesRadioButton.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			yesRadioButton.Enabled = !readOnly;
			noRadioButton.Enabled = !readOnly;
		}
	}
}
