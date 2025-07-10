using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class LiabilityDetailsUserControl : ZUserControl
	{
		public LiabilityDetailsUserControl()
		{
			InitializeComponent();
			InitializeCommodityCodeCodeFindBox();
		}

		void InitializeCommodityCodeCodeFindBox()
		{
			CommodityCodeTariffFindBox.GetEffectiveDate = () => GetNctsArrivalCargoDesc(CommodityCodeTariffFindBox)?.ValuationDate ?? ZDateTime.Today;
			CommodityCodeTariffFindBox.GetTariffType = () => GetNctsArrivalCargoDesc(CommodityCodeTariffFindBox)?.TariffType ?? ZString.Empty;
			CommodityCodeTariffFindBox.GetDataGrouping = () => GetNctsArrivalCargoDesc(CommodityCodeTariffFindBox)?.DataGroupingCode ?? ZString.Empty;
		}

		NctsArrivalCargoDesc GetNctsArrivalCargoDesc(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as NctsArrivalCargoDesc;
	}
}
