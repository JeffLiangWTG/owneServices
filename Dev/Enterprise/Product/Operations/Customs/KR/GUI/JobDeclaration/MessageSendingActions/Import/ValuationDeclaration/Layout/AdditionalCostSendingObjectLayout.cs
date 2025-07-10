using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AdditionalCostSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public AdditionalCostSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodFiveToSixControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(150);
			var ruler2 = layout.CreateRuler(438);
			layout.Include(ruler1, bag.AdditionalCostFreightToArrivalPortCalcEdit, ruler2, bag.AdditionalCostFreightToDeparturePortCalcEdit);
			layout.Include(ruler1, bag.AdditionalCostInsuranceCalcEdit, ruler2, bag.AdditionalCostTotalAdditionalAmountCalcEdit);

			return layout;
		}
	}
}
