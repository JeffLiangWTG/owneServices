using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ValuationDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public ValuationDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var bag = ValuationDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(bag);

			var captionWidth = (int)ColumnLayoutBuilderCaptionWidthSize.Long;

			var captionRuler = layout.CreateRuler(captionWidth);
			var mediumWidthRuler = layout.CreateRightRuler(captionWidth + 200);
			var longWidthRuler = layout.CreateRightRuler(captionWidth + 600);
			var columnWidthRuler = layout.CreateRuler(captionWidth + 100);

			layout.Include(0, captionRuler, bag.ProductCodeCodeFindBox, mediumWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, bag.TariffFindBox, longWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, bag.DescriptionLongTextControl, longWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, bag.ModelTradeNameTextBox, longWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, bag.BrandNameTextBox, longWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, bag.IngredientLongTextControl, longWidthRuler, columnWidthRuler);
			return layout;
		}
	}
}
