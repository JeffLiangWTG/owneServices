using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class GatewayChargeDefaultDebtorConfigurationControl : RegistryZUserControl
	{
		public GatewayChargeDefaultDebtorConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GatewayChargeDefaultDebtorConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

