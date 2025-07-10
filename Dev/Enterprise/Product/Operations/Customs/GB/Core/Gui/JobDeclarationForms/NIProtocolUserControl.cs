using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class NIProtocolUserControl : ZUserControl
	{
		public NIProtocolUserControl()
		{
			InitializeComponent();
			JE_IsGvmsPortCheckBox.AllowOverlap(JE_ClaimEuSubsidyCheckBox);
			JE_ClaimEuSubsidyCheckBox.AllowOverlap(JE_NiGoodsAtRiskOfMovingToROICheckBox);
		}
	}
}
