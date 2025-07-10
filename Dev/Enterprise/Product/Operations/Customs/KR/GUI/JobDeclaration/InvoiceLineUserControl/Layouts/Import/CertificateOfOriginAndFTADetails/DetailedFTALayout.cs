using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DetailedFTALayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = COOandFTAControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(165);

			layout.Include(ruler1, common.SequenceNoCalcEdit);
			layout.Include(ruler1, common.UsedQuantityCalcEdit, common.UsedUQDropEdit);

			return layout;
		}
	}
}
