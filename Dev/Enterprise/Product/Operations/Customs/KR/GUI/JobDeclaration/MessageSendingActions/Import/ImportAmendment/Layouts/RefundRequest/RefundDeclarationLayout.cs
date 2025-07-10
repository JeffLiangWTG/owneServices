using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class RefundDeclarationLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = RefundRequestControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var ruler = layout.CreateRuler(160);
			layout.Include(ruler, controlBag.RefundTypeDropEdit);
			layout.Include(ruler, controlBag.RefundCauseDropEdit);
			layout.Include(ruler, controlBag.RefundReasonDropEdit);
			layout.Include(ruler, controlBag.RefundSentWith5FEDropEdit);
			layout.Include(ruler, controlBag.TaxOfficeCodeFindBox);

			return layout;
		}
	}
}
