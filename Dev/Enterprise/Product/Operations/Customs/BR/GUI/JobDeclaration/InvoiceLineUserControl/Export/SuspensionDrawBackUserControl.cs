using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class SuspensionDrawbackUserControl : ZUserControl
	{
		public SuspensionDrawbackUserControl()
		{
			InitializeComponent();
			InitializeGrid();
		}

		void InitializeGrid()
		{
			if (SuspensionDrawbackGrid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Tariff) is Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo)
			{
				tariffColumnStyleInfo.GetCountryCode = () => Core.Constants.CountryCodes.Brazil;
				tariffColumnStyleInfo.GetDataGrouping = () => Core.Constants.CountryCodes.Brazil;
				tariffColumnStyleInfo.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
				tariffColumnStyleInfo.GetEffectiveDate = GetEffectiveDate;
			}
		}

		public ZDateTime GetEffectiveDate() => CurrentDataItem is JobComInvoiceLine invoiceLine ? invoiceLine.EffectiveAssessmentDate : ZDateTime.Today;
	}
}
