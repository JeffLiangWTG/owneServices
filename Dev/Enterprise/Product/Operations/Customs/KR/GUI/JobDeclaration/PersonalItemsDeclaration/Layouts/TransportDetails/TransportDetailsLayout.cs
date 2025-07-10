using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class TransportDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = TransportDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(100);
			var ruler2 = layout.CreateRuler(285);

			layout.Include(ruler1, common.HouseBillTextBox);
			layout.Include(ruler1, common.FreightCalcEdit);
			layout.Include(ruler1, common.StartDateDateEdit, ruler2, common.ArrivalDateDateEdit);
			layout.Include(ruler1, common.PortofLoadingCodeFindBox);
			layout.Include(ruler1, common.ForeignCityCodeFindBox);

			return layout;
		}
	}
}
