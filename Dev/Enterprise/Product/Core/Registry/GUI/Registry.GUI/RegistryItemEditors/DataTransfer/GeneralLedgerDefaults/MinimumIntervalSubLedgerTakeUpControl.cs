namespace Enterprise.Registry.GUI
{
	public partial class MinimumIntervalSubLedgerTakeUpControl : RegistryZUserControl
	{
		public MinimumIntervalSubLedgerTakeUpControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			Enabled = !readOnly;
		}
	}
}
