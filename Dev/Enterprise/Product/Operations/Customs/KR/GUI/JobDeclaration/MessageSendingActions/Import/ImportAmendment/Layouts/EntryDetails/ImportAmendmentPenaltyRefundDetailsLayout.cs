using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportAmendmentPenaltyRefundDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var bag = ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(220);

			layout.Include(0, ruler1, bag.PenaltyExemptReqDropEdit);
			layout.Include(0, ruler1, bag.PenaltyExemptSequenceCalcEdit);
			layout.Include(0, ruler1, bag.DTYPenaltyReducedYNDropEdit);
			layout.Include(0, ruler1, bag.DTYPenaltyTypeDropEdit);
			layout.Include(0, ruler1, bag.DomesticTaxPenaltyTypeDropEdit);

			layout.AddColumn();
			layout.Include(1, ruler2, bag.PenaltyExemptReasonCodeDropEdit);
			layout.Include(1, ruler2, bag.PenaltyExemptReasonMultiLineTextBox);
			layout.Include(1, ruler2, bag.PenaltyExemptAmountCalcEdit);
			layout.Include(1, ruler2, bag.RefundRequestYNDropEdit);

			return layout;
		}
	}
}
