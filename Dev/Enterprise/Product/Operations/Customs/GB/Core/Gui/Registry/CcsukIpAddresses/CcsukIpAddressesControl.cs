using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class CcsukIpAddressesControl : RegistryZUserControl
	{
		public CcsukIpAddressesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AddressesGrid.ReadOnly = readOnly;
		}
	}
}
