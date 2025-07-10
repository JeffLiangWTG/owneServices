using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundDeclarationDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public RefundDeclarationDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundDeclarationControlBag.Instance;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(180);
			layout.Include(ruler1, bag.EntryNumberTextBox);
			layout.Include(ruler1, bag.TotalRefundAmountCalcEdit);
			layout.Include(ruler1, bag.MessageStatusDropEdit);
			layout.Include(ruler1, bag.EntryStatusDropEdit);
			layout.Include(ruler1, bag.AcceptedDateEdit);
			layout.Include(ruler1, bag.RefundApprovalDateEdit);
			layout.Include(ruler1, bag.RefundApprovalNumberTextBox);
			layout.Include(ruler1, bag.ProvisionDateEdit);
			layout.Include(ruler1, bag.ProvisionNumberTextBox);
			return layout;
		}
	}
}
