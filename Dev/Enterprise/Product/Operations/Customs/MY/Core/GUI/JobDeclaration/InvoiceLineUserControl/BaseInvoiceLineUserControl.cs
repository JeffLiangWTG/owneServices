using Enterprise.Customs.GUI;

namespace Enterprise.Customs.MY.GUI
{
	public partial class MYInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		Enterprise.Customs.Universal.GUI.TariffFindBox zTariffFindBox;

		public MYInvoiceLineUserControl()
		{
			InitializeComponent();
			InvoiceLineUserControlHelper.SetTariffRelated(CustomsInvoiceLinesBoundGrid, Name, zTariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Malaysia;
	}
}
