using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PriceGroupBoxUserControl : ZUserControl
	{
		public PriceGroupBoxUserControl()
		{
			InitializeComponent();

			BasisForCalculationPanel.UpdateLayout(new BasisForCalculationLayout());
			AdditionalCostsPanel.UpdateLayout(new AdditionalCostsLayout());
			DeductionCostsPanel.UpdateLayout(new DeductionCostsLayout());
		}
		public void ChangeBindingToMessageSendingObject()
		{
			BasisForCalculationPanel.UpdateLayout(new BasisForCalculationMessageSendingLayout());
			AdditionalCostsPanel.UpdateLayout(new AdditionalCostsMessageSendingLayout());
			DeductionCostsPanel.UpdateLayout(new DeductionCostsMessageSendingLayout());
		}
	}
}
