using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AdditionalCostsMessageSendingLayout : IPanelLayoutProvider
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

			layout.Include(ruler1, common.PurchaseCostCalcEdit, ruler2, common.BrokerageFeeCalcEdit, ruler3, common.ContainerPackagingCostCalcEdit);
			layout.Include(ruler1, common.GoodsCostCalcEdit, ruler2, common.ProductToolCostsCalcEdit, ruler3, common.CommodityUsageCostsCalcEdit);
			layout.Include(ruler1, common.ProductDevCostsCalcEdit, ruler2, common.RoyaltyCalcEdit, ruler3, common.ProfitAmountCalcEdit);
			layout.Include(ruler3, common.ExcludingTransportationCostsCalcEdit);
			layout.Include(ruler1, common.FreightCalcEdit, ruler2, common.UnloadCostCalcEdit, ruler3, common.InsuranceCalcEdit);
			layout.Include(ruler3, common.TransportationCostCalcEdit);
			layout.Include(ruler3, common.TotalAdditionalAmountCalcEdit);

			return layout;
		}
	}
}
