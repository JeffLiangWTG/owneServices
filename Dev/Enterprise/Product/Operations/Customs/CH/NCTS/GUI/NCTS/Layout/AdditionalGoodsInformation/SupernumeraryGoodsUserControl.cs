using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed partial class SupernumeraryGoodsUserControl : ZUserControl
{
	public SupernumeraryGoodsUserControl()
	{
		InitializeComponent();
		InitializeTariffFindBox();
	}

	void InitializeTariffFindBox()
	{
		var columnStyleInfo = (TariffColumnStyleInfo)SupernumeraryGoodsGrid.GetColumnStyle(SupernumeraryGoods.Schema.CSI_Tariff);
		columnStyleInfo.TariffType = SupernumeraryGoods.TariffType;
		columnStyleInfo.GetCountryCode = () => (CurrentDataItem as NctsHeader)?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		columnStyleInfo.GetDataGrouping = () => SupernumeraryGoods.TariffDataGrouping;
		columnStyleInfo.GetEffectiveDate = () => (DataSource as NctsHeader)?.ArrivalMovementHeader.ValuationDate ?? ZDateTime.Today;
	}
}
