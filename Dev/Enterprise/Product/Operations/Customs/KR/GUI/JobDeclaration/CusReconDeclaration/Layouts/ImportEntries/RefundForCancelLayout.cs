using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundForCancelLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public RefundForCancelLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundForCancelControlBag.Instance;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(130);
			var ruler2 = layout.CreateRuler(272);
			layout.Include(ruler1, bag.CancelReasonDropEdit);
			layout.Include(ruler1, bag.DisposalNumberTextBox);
			layout.Include(ruler1, bag.DisposalDateEdit);
			layout.Include(ruler1, bag.GoodsLocationDescriptionLongTextControl);
			layout.Include(ruler1, bag.ResidualSubstanceDescriptionLongTextControl);
			layout.Include(ruler1, bag.DamageSituationLongTextControl);
			layout.Include(ruler1, bag.ExportEntryNumberTextBox, ruler2, bag.ExportEntryLineNumberTextBox);

			return layout;
		}
	}
}
