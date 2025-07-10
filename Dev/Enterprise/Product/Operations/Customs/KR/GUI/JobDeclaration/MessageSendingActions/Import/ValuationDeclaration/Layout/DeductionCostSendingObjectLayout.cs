using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeductionCostSendingObjectLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public DeductionCostSendingObjectLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = MethodFourControlBag.InstanceForSendingObject;
			layout.RegisterControlBag(bag);
			var ruler1 = layout.CreateRuler(230);
			var ruler2 = layout.CreateRightRuler(340);
			var ruler3 = layout.CreateRuler(550);
			layout.Include(ruler1, bag.DeductionCostCustomsReferenceNumberTextBox, ruler3, bag.DeductionCostConsignmentSalesFeeCalcEdit);
			layout.Include(ruler1, bag.DeductionCostGeneralCostCalcEdit, ruler3, bag.DeductionCostCostRateCodeDropEdit);
			layout.Include(ruler1, bag.DeductionCostCostRateCalcEdit, ruler2, bag.PercentageLabel, ruler3, bag.DeductionCostTransportationCostCalcEdit);
			layout.Include(ruler1, bag.DeductionCosInsuranceCalcEdit, ruler3, bag.DeductionCostUnloadCostCalcEdit);
			layout.Include(ruler1, bag.DeductionCostOtherTransportationCostsCalcEdit, ruler3, bag.DeductionCostAdditionalCostCalcEdit);
			layout.Include(ruler1, bag.DeductionCosTaxCalcEdit, ruler3, bag.DeductionCostTotalDeductionAmountCalcEdit);

			return layout;
		}
	}
}
