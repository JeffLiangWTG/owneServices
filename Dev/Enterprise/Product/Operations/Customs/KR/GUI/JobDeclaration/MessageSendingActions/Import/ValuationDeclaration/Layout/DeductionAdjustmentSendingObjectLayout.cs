using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeductionAdjustmentSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public DeductionAdjustmentSendingObjectLayout()
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
			layout.Include(ruler1, bag.DeductionAdjustmentQuantityDiscountCalcEdit, ruler2, bag.DeductionAdjustmentCommercialAmountCalcEdit);
			layout.Include(ruler1, bag.DeductionAdjustmentTransportationCostCalcEdit, ruler2, bag.DeductionAdjustmentShippingPortCostCalcEdit);
			layout.Include(ruler1, bag.DeductionAdjustmentInsuranceCalcEdit, ruler2, bag.TotalDeductionAdjustmentAmountCalcEdit);

			return layout;
		}
	}
}
