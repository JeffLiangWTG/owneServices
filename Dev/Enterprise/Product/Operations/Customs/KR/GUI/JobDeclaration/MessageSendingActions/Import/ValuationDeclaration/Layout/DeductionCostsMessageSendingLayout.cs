using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeductionCostsMessageSendingLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = PriceControlBag.InstanceForMessageSendingObject;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(580);
			var ruler3 = layout.CreateRuler(1050);

			layout.Include(ruler1, common.LocalTransportationCostCalcEdit, ruler2, common.TechnicalCostCalcEdit);
			layout.Include(ruler1, common.OtherCostsCalcEdit, ruler2, common.DiscountAmountCalcEdit, ruler3, common.TotalDeductionAmountCalcEdit);

			return layout;
		}
	}
}
