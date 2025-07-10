using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Import5FEMessageRefundRequestUserControl : ZUserControl
	{
		public Import5FEMessageRefundRequestUserControl()
		{
			InitializeComponent();

			DeclarationPanel.UpdateLayout(new RefundDeclarationLayout());
			RefundHeaderPanel.UpdateLayout(new RefundHeaderLayout());
			PaidAndRefundOfTaxAndPenaltyPanel.UpdateLayout(new MessageSendingRefundDetailsLayout());
		}
	}
}
