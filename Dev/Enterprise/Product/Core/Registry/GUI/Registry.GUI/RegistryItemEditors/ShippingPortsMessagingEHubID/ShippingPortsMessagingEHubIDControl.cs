namespace Enterprise.Registry.GUI
{
	public partial class ShippingPortsMessagingEHubIDControl : RegistryZUserControl
	{
		public ShippingPortsMessagingEHubIDControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ShippingPortsMessagingEHubIDGrid.ReadOnly = readOnly;
		}
	}
}
