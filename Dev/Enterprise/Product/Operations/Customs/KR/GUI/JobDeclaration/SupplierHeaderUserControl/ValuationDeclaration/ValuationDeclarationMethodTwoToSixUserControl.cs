using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDeclarationMethodTwoToSixUserControl : ZUserControl
	{
		public ValuationDeclarationMethodTwoToSixUserControl()
		{
			InitializeComponent();
			DynamicDocumentLayoutPanel.UpdateLayout(new MethodTwoToSixDocumentLayout());
			DynamicItemUseCodeLayoutPanel.UpdateLayout(new MethodTwoToSixItemUseCodeLayout());
			DynamicGoodsPricingBasisLayoutPanel.UpdateLayout(new MethodTwoToSixGoodsPricingBasisLayout());
		}

		public void ChangeBindingToMessageSendingObject()
		{
			DynamicDocumentLayoutPanel.UpdateLayout(new MethodTwoToSixDocumentSendingObjectLayout());
			DynamicItemUseCodeLayoutPanel.UpdateLayout(new MethodTwoToSixItemUseCodeSendingObjectLayout());
			DynamicGoodsPricingBasisLayoutPanel.UpdateLayout(new MethodTwoToSixGoodsPricingBasisSendingObjectLayout());
		}
	}
}
