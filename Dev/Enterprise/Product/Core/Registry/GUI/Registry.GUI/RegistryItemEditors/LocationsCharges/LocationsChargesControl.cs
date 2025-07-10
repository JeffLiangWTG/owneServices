namespace Enterprise.Registry.GUI
{
	public partial class LocationsChargesControl : RegistryZUserControl
	{
		public LocationsChargesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LocationsGrid.ReadOnly = readOnly;
			ChargesGrid.ReadOnly = readOnly;
		}
	}
}
