using CargoWise.Types;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public CAInvoiceLineUserControl()
		{
			InitializeComponent();

			InvoiceLineUserControlHelper.SetTariffRelated(this.CustomsInvoiceLinesBoundGrid, this.Name, this.JI_TariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Canada;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Canada;
	}
}
