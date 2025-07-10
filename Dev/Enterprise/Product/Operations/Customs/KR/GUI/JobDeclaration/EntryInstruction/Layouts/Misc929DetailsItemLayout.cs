using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class Misc929DetailsItemLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = EntryInstructionLayoutControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler((int)ColumnLayoutBuilderCaptionWidthSize.Medium);

			layout.Include(ruler1, common.TotalPackagesCalcEdit, common.TotalPackagesUQDropEdit);
			layout.Include(ruler1, common.AgreedRateDropEdit);

			return layout;
		}
	}
}
