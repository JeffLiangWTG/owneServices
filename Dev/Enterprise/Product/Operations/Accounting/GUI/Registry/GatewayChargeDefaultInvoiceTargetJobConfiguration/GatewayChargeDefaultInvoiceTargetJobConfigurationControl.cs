using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class GatewayChargeDefaultInvoiceTargetJobConfigurationControl : RegistryZUserControl
	{
		public GatewayChargeDefaultInvoiceTargetJobConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

