using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ExchangeRateToleranceControl : RegistryZUserControl
	{
		public ExchangeRateToleranceControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ExchangeRateToleranceGrid.ReadOnly = readOnly;
		}
	}
}
