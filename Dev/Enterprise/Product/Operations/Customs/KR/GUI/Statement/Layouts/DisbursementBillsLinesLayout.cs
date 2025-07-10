using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DisbursementBillsLinesLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var krBag = StatementLineControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(140);

			layout.Include(0, captionRuler, krBag.DutyAmountCalcEdit);
			layout.Include(0, captionRuler, krBag.LiquorTaxCalcEdit);
			layout.Include(0, captionRuler, krBag.AgricultureTaxCalcEdit);
			layout.AddColumn();
			layout.Include(1, captionRuler, krBag.TransportationTaxCalcEdit);
			layout.Include(1, captionRuler, krBag.EducationTaxCalcEdit);
			layout.Include(1, captionRuler, krBag.InterestCalcEdit);
			layout.AddColumn();
			layout.Include(2, captionRuler, krBag.SpecialConsumptionTaxCalcEdit);
			layout.Include(2, captionRuler, krBag.VATCalcEdit);
			layout.Include(2, captionRuler, krBag.DeclarationPenaltyCalcEdit);
			return layout;
		}
	}
}
