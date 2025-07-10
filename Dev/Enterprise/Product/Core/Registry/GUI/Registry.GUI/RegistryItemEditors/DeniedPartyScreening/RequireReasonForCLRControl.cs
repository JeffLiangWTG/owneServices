namespace Enterprise.Registry.GUI
{
	public partial class RequireReasonForCLRControl : RegistryZUserControl
	{
		public RequireReasonForCLRControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			Enabled = !readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			RequireReasonForCLRGrid.ReadOnly = NoRadioButton.Checked;
		}

		internal void YesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			RequireReasonForCLRGrid.ReadOnly = !YesRadioButton.Checked;
		}
	}
}

