using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobProfitLossReasonCodeControl : RegistryZUserControl
	{
		public JobProfitLossReasonCodeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			JobProfitLossReasonCodeGrid.ReadOnly = readOnly;
		}
	}
}

