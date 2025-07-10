using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class CashFlowActivityConfigurationControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid CashFlowActivityConfigurationGrid;

		#endregion

		public CashFlowActivityConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CashFlowActivityConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

