using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class InvLineFTADetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = COOandFTAControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(195);

			layout.Include(ruler1, common.ProductTypeDropEdit);
			layout.Include(ruler1, common.CountryInvIssuedDropEdit);
			layout.Include(ruler1, common.CountryCodeFindBox);
			layout.Include(ruler1, common.CoveredByCOOExporterSystemCheckBox);
			layout.Include(ruler1, common.ExporterNumberTextBox);
			layout.Include(ruler1, common.SplitOrderCalcEdit);
			layout.Include(ruler1, common.SupportingDocTypeDropEdit);
			layout.Include(ruler1, common.IssuerTypeDropEdit);
			layout.Include(ruler1, common.TotalNetWeightCalcEdit, common.UQDropEdit);

			return layout;
		}
	}
}
