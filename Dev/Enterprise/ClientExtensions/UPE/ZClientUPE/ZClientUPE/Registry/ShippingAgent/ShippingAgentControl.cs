using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class ShippingAgentControl : RegistryZUserControl
	{
		public ShippingAgentControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ShippingAgentZAddressControl.Enabled = !readOnly;
			ShippingAgentZAddressControl.ReadOnly = readOnly;
		}
	}
}
