using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class RefundHeaderLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = RefundRequestControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var ruler = layout.CreateRuler(160);
			layout.Include(ruler, controlBag.CustomsDisbursementBillTextBox);
			layout.Include(ruler, controlBag.VersionNoCalcEdit);
			layout.Include(ruler, controlBag.RefundAmountOfValueForVATCalcEdit);
			layout.Include(ruler, controlBag.RefundAmountOfVATExemptionValueCalcEdit);

			return layout;
		}
	}
}
