using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EXPProductsCustomDetailsCommonLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;
		static PanelLayout CreateLayout()
		{
			var builder = new EXPProductsCustomDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.BrandNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ModelTradeNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.IngredientTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.UsageCommentTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ClassificationDescriptionTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
