namespace Enterprise.Registry.GUI
{
	partial class PurgeSettingsControl : RegistryZUserControl
	{
		public PurgeSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(ReadOnly);
			InterchangesGrid.ReadOnly = readOnly;
			MessageTypesGrid.ReadOnly = readOnly;
			ApplicationCodesGrid.ReadOnly = readOnly;
			PurgeMessageBatchSizeCalcEdit.ReadOnly = readOnly;
		}
	}
}
