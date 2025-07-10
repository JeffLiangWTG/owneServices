using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
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
			layout.Include(ruler1, common.ProductCodeFindBox, ruler2, common.LotNumberTextBox);
			layout.Include(ruler1, common.TariffFindBox);
			layout.Include(ruler1, common.GoodsDescriptionLongTextControl);
			layout.Include(ruler1, common.BrandCodeFindBox, ruler2, common.BrandNameTextBox);
			layout.Include(ruler1, common.ModelTradeNameTextBox);
			layout.Include(ruler1, common.IngredientLongTextControl);
			return layout;
		}
	}
}
