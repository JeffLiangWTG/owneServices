using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemDifferencesDetailsColumnUserControl : ZUserControl
	{
		public Phase5GoodsItemDifferencesDetailsColumnUserControl()
		{
			InitializeComponent();
			InitializeDeclaredCommodityCodeCodeFindBox();
			InitializeUnloadedCommodityCodeCodeFindBox();
		}

		void InitializeDeclaredCommodityCodeCodeFindBox()
		{
			DeclaredCommodityCodeCodeFindBox.GetEffectiveDate = () => GetNctsArrivalCargoDesc(DeclaredCommodityCodeCodeFindBox)?.ValuationDate ?? ZDateTime.Today;
			DeclaredCommodityCodeCodeFindBox.GetTariffType = () => GetNctsArrivalCargoDesc(DeclaredCommodityCodeCodeFindBox)?.TariffType ?? ZString.Empty;
			DeclaredCommodityCodeCodeFindBox.GetDataGrouping = () => GetNctsArrivalCargoDesc(DeclaredCommodityCodeCodeFindBox)?.DataGroupingCode ?? ZString.Empty;
		}

		NctsArrivalCargoDesc GetNctsArrivalCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as NctsArrivalCargoDesc;

		void InitializeUnloadedCommodityCodeCodeFindBox()
		{
			UnloadedCommodityCodeCodeFindBox.GetEffectiveDate = () => GetNctsUnloadedCargoDesc(UnloadedCommodityCodeCodeFindBox)?.ValuationDate ?? ZDateTime.Today;
			UnloadedCommodityCodeCodeFindBox.GetTariffType = () => GetNctsUnloadedCargoDesc(UnloadedCommodityCodeCodeFindBox)?.TariffType ?? ZString.Empty;
			UnloadedCommodityCodeCodeFindBox.GetDataGrouping = () => GetNctsUnloadedCargoDesc(UnloadedCommodityCodeCodeFindBox)?.DataGroupingCode ?? ZString.Empty;
		}

		NctsUnloadedCargoDesc GetNctsUnloadedCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => GetNctsArrivalCargoDesc(tariffFindBox)?.UnloadedGoodsItem;
	}
}
