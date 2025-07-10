namespace Enterprise.Registry.GUI
{
	public partial class InternetAddressListRegistryControl : RegistryZUserControl
	{
		public InternetAddressListRegistryControl()
		{
			InitializeComponent();
			Grid.DisableImportDataMenuItem = true;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}
	}
}
