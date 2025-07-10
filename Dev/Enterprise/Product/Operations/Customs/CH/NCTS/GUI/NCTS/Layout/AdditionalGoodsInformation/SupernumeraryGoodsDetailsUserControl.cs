using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed partial class SupernumeraryGoodsDetailsUserControl : ZUserControl
{
	public SupernumeraryGoodsDetailsUserControl()
	{
		InitializeComponent();
		InitializeTariffFindBox();
	}

	void InitializeTariffFindBox()
	{
		TariffFindBox.GetCountryCode = () => (CurrentDataItem as SupernumeraryGoods)?.Parent?.Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		TariffFindBox.GetDataGrouping = () => SupernumeraryGoods.TariffDataGrouping;
		TariffFindBox.TariffType = SupernumeraryGoods.TariffType;
		TariffFindBox.GetEffectiveDate = () => (DataSource as NctsHeader)?.ArrivalMovementHeader.ValuationDate ?? ZDateTime.Today;
	}
}
