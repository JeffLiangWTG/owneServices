using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ImportQuantityAndWeightLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = ImportInvoiceLineDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(100);
			var ruler2 = layout.CreateRuler(430);
			layout.Include(ruler1, common.CustomsQtyCalcDropEdit, ruler2, common.CustomsUnitPriceCalcEdit);
			layout.Include(ruler1, common.CustomsSecondQuantityCalcDropEdit, ruler2, common.CustomsThirdQuantityCalcDropEdit);
			layout.Include(ruler1, common.CustomsFourthQuantityCalcDropEdit, ruler2, common.DrawBackQtyCalcDropEdit);
			layout.Include(ruler1, common.GrossWeightCalcDropEdit, ruler2, common.NetWeightCalcDropEdit);
			layout.Include(ruler1, common.InvoiceQtyCalcDropEdit, ruler2, common.UnitPriceCalcEdit);
			layout.Include(ruler1, common.PriceCalcFindBox, ruler2, common.InstallationCostCalcEdit);
			return layout;
		}
	}
}
