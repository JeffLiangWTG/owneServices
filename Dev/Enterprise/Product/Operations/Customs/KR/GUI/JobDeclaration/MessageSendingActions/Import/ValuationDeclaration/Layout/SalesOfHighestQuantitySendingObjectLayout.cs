using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SalesOfHighestQuantitySendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public SalesOfHighestQuantitySendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodFourControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(200);
			var ruler2 = layout.CreateRuler(550);
			layout.Include(ruler1, bag.SalesOfHighestQuantityAmountCalcFindBox, ruler2, bag.SalesOfHighestQuantityExchangeRateCalcEdit);
			layout.Include(ruler1, bag.SalesOfHighestQuantityAmountKRWCalcEdit);
			return layout;
		}
	}
}
