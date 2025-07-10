using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class WarehouseLayout : IPanelLayoutProvider
	{
		public WarehouseLayout()
		{
			Layout = CreateGoodsItemSupplyChainActorLayout();
		}

		public PanelLayout Layout { get; }

		static PanelLayout CreateGoodsItemSupplyChainActorLayout()
		{
			var builder = new WarehouseLayoutBuilder<NctsDepartureCargoDesc>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PartGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.WarehouseEntryNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WarehouseEntryLineNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
