namespace Enterprise.Registry.GUI
{
	public partial class AddressValidationDisabledCountryItemsControl : RegistryZUserControl
	{
		public AddressValidationDisabledCountryItemsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AddressValidationDisabledCountryItemsGrid.ReadOnly = readOnly;
		}
	}
}
