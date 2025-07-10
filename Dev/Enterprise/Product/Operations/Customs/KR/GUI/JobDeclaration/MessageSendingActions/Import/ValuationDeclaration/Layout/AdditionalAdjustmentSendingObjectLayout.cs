using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AdditionalAdjustmentSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public AdditionalAdjustmentSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodTwoToThreeControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(210);
			var ruler2 = layout.CreateRuler(500);
			layout.Include(ruler1, bag.AdditionalAdjustmentQuantityDiscountCalcEdit, ruler2, bag.AdditionalAdjustmentCommercialAmountCalcEdit);
			layout.Include(ruler1, bag.AdditionalAdjustmentTransportationCostCalcEdit, ruler2, bag.AdditionalAdjustmentShippingPortCostCalcEdit);
			layout.Include(ruler1, bag.AdditionalAdjustmentInsuranceCalcEdit, ruler2, bag.TotalAdditionalAdjustmentAmountCalcEdit);

			return layout;
		}
	}
}
