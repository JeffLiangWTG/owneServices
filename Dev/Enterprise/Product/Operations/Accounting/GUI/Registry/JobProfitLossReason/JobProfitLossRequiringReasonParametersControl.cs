using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobProfitLossRequiringReasonParametersControl : RegistryZUserControl
	{
		public JobProfitLossRequiringReasonParametersControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MarginThresholdPositiveProfitCalcEdit.ReadOnly = readOnly;
			MarginThresholdNegativeProfitCalcEdit.ReadOnly = readOnly;
			JobStatusControl.ReadOnly = readOnly;
		}
	}
}

