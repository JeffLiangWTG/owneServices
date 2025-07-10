using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EXPProductsCustomDetailsCertificateOfOriginLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;
		static PanelLayout CreateLayout()
		{
			var builder = new EXPProductsCustomDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.COOLabelLocationDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
