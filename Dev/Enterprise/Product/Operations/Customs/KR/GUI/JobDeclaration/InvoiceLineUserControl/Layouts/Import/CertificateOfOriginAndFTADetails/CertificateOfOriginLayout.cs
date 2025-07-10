using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CertificateOfOriginLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = COOandFTAControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(200);

			layout.Include(ruler1, common.GoodsOriginCodeFindBox);
			layout.Include(ruler1, common.CODeterminationRuleDropEdit);
			layout.Include(ruler1, common.COLabelLocationDropEdit);
			layout.Include(ruler1, common.COLabelTypeDropEdit);
			layout.Include(ruler1, common.COLabelExemptionReasonDropEdit);

			return layout;
		}
	}
}
