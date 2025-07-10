using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportInvoiceLineInwardProcessingUserControl : ZUserControl
	{
		public ImportInvoiceLineInwardProcessingUserControl()
		{
			InitializeComponent();
			if (ProcessedProductGrid.GetColumnStyle(nameof(InwardProcessingProduct.FormattedTariff)) is Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo)
			{
				tariffColumnStyleInfo.GetCountryCode = () => Core.Constants.CountryCodes.Germany;
				tariffColumnStyleInfo.GetDataGrouping = () => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			}
		}
	}
}
