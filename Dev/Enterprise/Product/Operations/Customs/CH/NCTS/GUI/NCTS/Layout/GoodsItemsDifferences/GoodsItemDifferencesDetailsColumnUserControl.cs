using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class GoodsItemDifferencesDetailsColumnUserControl : ZUserControl
{
	public GoodsItemDifferencesDetailsColumnUserControl()
	{
		InitializeComponent();
		DeclaredCommodityCodeCodeFindBox.GetEffectiveDate = () => GetNctsArrivalCargoDesc(DeclaredCommodityCodeCodeFindBox)?.ValuationDate ?? ZDateTime.Today;
		DeclaredCommodityCodeCodeFindBox.GetShouldShowExactDescription = () => false;
		UnloadedCommodityCodeCodeFindBox.GetEffectiveDate = () => GetNctsUnloadedCargoDesc(UnloadedCommodityCodeCodeFindBox)?.ValuationDate ?? ZDateTime.Today;
		UnloadedCommodityCodeCodeFindBox.GetShouldShowExactDescription = () => false;
	}

	NctsArrivalCargoDesc GetNctsArrivalCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as NctsArrivalCargoDesc;
	NctsUnloadedCargoDesc GetNctsUnloadedCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => GetNctsArrivalCargoDesc(tariffFindBox)?.UnloadedGoodsItem;
}
