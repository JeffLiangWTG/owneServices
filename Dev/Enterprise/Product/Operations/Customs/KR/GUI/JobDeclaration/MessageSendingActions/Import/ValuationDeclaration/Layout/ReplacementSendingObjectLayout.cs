using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ReplacementSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public ReplacementSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodTwoToThreeControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(260);
			var ruler2 = layout.CreateRuler(500);
			layout.Include(ruler1, bag.ReplacementAmountCalcFindBox, ruler2, bag.ReplacementExchangeRateCalcEdit);
			layout.Include(ruler1, bag.ReplacementAmountKRWCalcEdit);

			return layout;
		}
	}
}
