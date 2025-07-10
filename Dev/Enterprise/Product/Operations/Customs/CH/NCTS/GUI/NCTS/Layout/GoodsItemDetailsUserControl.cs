using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class GoodsItemDetailsUserControl : ZUserControl
{
	public GoodsItemDetailsUserControl()
	{
		InitializeComponent();
		InitializeNctsTariffFindBox();
	}

	void InitializeNctsTariffFindBox()
	{
		HarmonisedTariffFindBox.GetShouldShowExactDescription = () => false;
		HarmonisedTariffFindBox.GetEffectiveDate = () => ((NctsDepartureCargoDesc)((ZUserControl)HarmonisedTariffFindBox.Parent).CurrentDataItem)?.ValuationDate ?? ZDateTime.Today;
	}
}
