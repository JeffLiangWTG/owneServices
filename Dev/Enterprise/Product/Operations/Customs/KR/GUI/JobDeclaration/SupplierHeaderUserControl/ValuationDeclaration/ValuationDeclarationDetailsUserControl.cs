using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDeclarationDetailsUserControl : ZUserControl
	{
		public ValuationDeclarationDetailsUserControl()
		{
			InitializeComponent();

			DetailsPanel.UpdateLayout(new ValuationDeclarationDetailsLayout());
			ProvisionalPricePanel.UpdateLayout(new ProvisionalPriceLayout());
			ProvisionalPricingReasonsPanel.UpdateLayout(new ProvisionalPricingReasonsLayout());
		}
	}
}
