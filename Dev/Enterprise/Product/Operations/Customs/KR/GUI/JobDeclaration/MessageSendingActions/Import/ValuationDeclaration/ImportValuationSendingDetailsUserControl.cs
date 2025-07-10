using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportValuationSendingDetailsUserControl : ZUserControl
	{
		public ImportValuationSendingDetailsUserControl()
		{
			InitializeComponent();
			UpdateLayout();
		}

		void UpdateLayout()
		{
			DetailsDynamicLayoutPanel.UpdateLayout(new ImportValuationSendingDetailsLayout());
			ProvisionalPriceDynamicLayoutPanel.UpdateLayout(new ProvisionalPriceMessageSendingLayout());
			ProvisionalPricingReasonsDynamicLayoutPanel.UpdateLayout(new ProvisionalPricingReasonsMessageSendingLayout());
		}
	}
}
