using System;
using System.Linq;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ExportSupplierHeaderUserControl : EU.GUI.EUExportSupplierHeaderUserControl
{
	public ExportSupplierHeaderUserControl()
	{
		InitializeComponent();
		ChargesGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("BEExportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae4", "[UCC 4/9] Invoice Charges");
		BaseGroupChargesGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("BEExportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae6", "[UCC 4/9] Group Charges (All Invoices)");
	}

	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => Enterprise.Customs.BE.GUI.Res.GetData("08B19745-0325-43A3-A986-4B848D0EFDCC", "[44] Additional Documents");
	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => EU.GUI.CaptionProvider.SupportingDocuments;

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumns();

		var currencyBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
		var exchangeRateBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
		var currencyCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BA", "[UCC 4/11] Curr.");
		var exchangeRateCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BC", "Exch. Rate", "Exchange Rate", "[UCC 4/15] Exchange Rate");

		currencyBoxColumnStyleInfo.CaptionResourceString = currencyCaption;
		exchangeRateBoxColumnStyleInfo.CaptionResourceString = exchangeRateCaption;

		var exchangeRateColumn = InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate);
		exchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var currencyColumn = InvoiceChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency);
		currencyColumn.CaptionResourceString = currencyCaption;

		var apportionedExchangeRateColumn = ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate);
		apportionedExchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var apportionedCurrencyColumn = ApportionedChargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_RX_NKCurrency);
		apportionedCurrencyColumn.CaptionResourceString = currencyCaption;

		var baseGroupExchangeRateColumn = BaseGroupChargesGrid.GetColumnStyle(BaseGroupInvoiceCharge.Schema.J7_ExchangeRate);
		baseGroupExchangeRateColumn.CaptionResourceString = exchangeRateCaption;
		var baseGroupCurrencyColumn = BaseGroupChargesGrid.GetColumnStyle(BaseGroupInvoiceCharge.Schema.J7_RX_NKCurrency);
		baseGroupCurrencyColumn.CaptionResourceString = currencyCaption;
	}

	void AddColumns()
	{
		JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = JobComInvoiceHeader.Schema.JZ_UCR,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
		});
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceHeaderSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(SupplierHeaderPreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);
}
