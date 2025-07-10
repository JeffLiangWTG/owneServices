using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GoodsItemDetailsUserControl : ZUserControl
	{
		public GoodsItemDetailsUserControl()
		{
			InitializeComponent();
			InitializeCommodityCodeTariffFindBox();
		}

		void InitializeCommodityCodeTariffFindBox()
		{
			CommodityCodeTariffFindBox.GetEffectiveDate = () => GetNctsDepartureCargoDesc(CommodityCodeTariffFindBox)?.ValuationDate ?? ZDateTime.Today;
			CommodityCodeTariffFindBox.GetTariffType = () => GetNctsDepartureCargoDesc(CommodityCodeTariffFindBox)?.TariffType ?? ZString.Empty;
			CommodityCodeTariffFindBox.GetDataGrouping = () => GetNctsDepartureCargoDesc(CommodityCodeTariffFindBox)?.DataGroupingCode ?? ZString.Empty;
		}

		NctsDepartureCargoDesc GetNctsDepartureCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => ((ZUserControl)tariffFindBox.Parent).CurrentDataItem as NctsDepartureCargoDesc;
	}
}
