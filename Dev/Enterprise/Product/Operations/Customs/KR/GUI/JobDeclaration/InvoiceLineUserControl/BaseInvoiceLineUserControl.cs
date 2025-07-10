using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
			ChangeColumnsInGrid();
		}
		protected override ResourceStringData GetJI_Calc_GSTConvertToLocalCurrencyControlCaption() => Res.GetData("CB2A7F29-4816-4D0B-B596-AD7DE2F8CF16", "VAT Amount");

		void ChangeColumnsInGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ContainerMode));
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CC));
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr).IsVisible = true;

			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_InvoiceQuantity).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			InvoiceLineCharges.ChargesGrid.ColumnStyles.Remove(InvoiceLineCharges.ChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
			InvoiceLineCharges.ApportionedChargesGrid.ColumnStyles.Remove(InvoiceLineCharges.ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
		}

		protected override ZString TariffColumnNameCore => BaseJobComInvoiceLine.Schema.JI_FormattedTariff;
		protected override bool UseUniversalTariff => true;

		public string ColumnTitleWhenExportForDutiable => Res.GetString("40F39196-17D7-48F9-BD90-87D1EBAD65B0", "Add to FOB?");
		public string ColumnTitleWhenExportForIncludedInInvoiceAmount => Res.GetString("E16981A1-1583-4099-AB45-52BFD9488366", "Included In Invoice");
		public string ColumnTitleExchangeRate => Res.GetString("20BD4BD9-ECAB-460E-88F3-FADD5EC89CF3", "Exchange Rate");
	}
}
