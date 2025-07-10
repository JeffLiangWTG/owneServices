namespace Enterprise.Registry.GUI
{
	public partial class DepotAddressColorSoundControl : RegistryZUserControl
	{
		public DepotAddressColorSoundControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			depotAddressColorSoundGrid.ReadOnly = readOnly;
		}
	}
}
