namespace Enterprise.Registry.GUI
{
	public partial class AddressListControl : RegistryZUserControl
	{
		public AddressListControl()
		{
			InitializeComponent();
		}

		#region RegistryZUserControl Overrides

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AddressListGrid.ReadOnly = ReadOnly;
		}

		#endregion
	}
}
