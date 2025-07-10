using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportAmendmentEntryDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(250);
			var ruler3 = layout.CreateRuler(220);
			var ruler4 = layout.CreateRuler(350);

			layout.Include(0, ruler1, common.VersionNoCalcEdit, ruler2, common.AmendmentTypeTextBox, common.AmendmentTypeDescriptionTextBox);
			layout.Include(0, ruler1, common.ReasonCodeDropEdit);
			layout.Include(0, ruler1, common.AmendmentReasonTextBox);
			layout.Include(0, ruler1, common.FaultPartyDropEdit);
			layout.Include(0, ruler1, common.FaultReasonTextBox);
			layout.AddColumn();
			layout.Include(1, ruler3, common.PenaltyPaymentReasonCodeFindBox);
			layout.Include(1, ruler3, common.TotalAmendedCountItemCalcEdit, ruler4, common.TotalAmendedCountDutyTaxCalcEdit);
			layout.Include(1, ruler3, common.BeforeTotalDutyTaxCalcEdit, ruler4, common.AfterTotalDutyTaxCalcEdit);
			layout.Include(1, ruler3, common.DutyTaxDifferenceCalcEdit);
			layout.Include(1, ruler3, common.BeforeCustomsValueCalcEdit, ruler4, common.AfterCustomsValueCalcEdit);
			layout.Include(1, ruler3, common.CustomsValueDifferenceCalcEdit);

			return layout;
		}
	}
}
