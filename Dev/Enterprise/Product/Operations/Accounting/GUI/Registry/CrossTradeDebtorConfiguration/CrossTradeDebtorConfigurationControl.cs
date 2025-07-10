using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CrossTradeDebtorConfigurationControl : RegistryZUserControl
	{
		public CrossTradeDebtorConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CrossTradeDebtorConfigGrid.ReadOnly = readOnly;
		}
	}
}
