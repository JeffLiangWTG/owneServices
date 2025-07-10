namespace Enterprise.Registry.GUI
{
	public partial class HVLVPurgePeriodRegistryControl : RegistryZUserControl
	{
		public HVLVPurgePeriodRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			enablePurgingCheckBox.ReadOnly = readOnly;
		}
	}
}
