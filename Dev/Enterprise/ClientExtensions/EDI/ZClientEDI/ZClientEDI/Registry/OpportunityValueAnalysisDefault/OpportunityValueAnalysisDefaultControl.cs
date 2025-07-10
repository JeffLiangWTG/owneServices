using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class OpportunityValueAnalysisDefaultControl : RegistryZUserControl
	{
		public OpportunityValueAnalysisDefaultControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			gridRates.ReadOnly = readOnly;
		}
	}
}
