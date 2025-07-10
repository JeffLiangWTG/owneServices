using System;
using System.Linq;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ImportSupplierHeaderUserControl : EU.GUI.EUImportSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();
		ReorderTabPages();
		ChargesGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("BEImportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae3", "[UCC 4/9] Invoice Charges");
		BaseGroupChargesGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("BEImportSupplierHeaderUserControl|52ef45c3-f115-4258-b47e-a8cb8ffc5ae5", "[UCC 4/9] Group Charges (All Invoices)");
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumns();

		var currencyBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCodeFindBoxColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
		var exchangeRateBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().Single(x => x.ColumnName == Business.Declaration.JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
		var currencyCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BW", "[UCC 4/11] Curr.");
		var exchangeRateCaption = Res.GetData("BD76BB97-A618-4966-A9AC-AD49F21513BV", "Exch. Rate", "Exchange Rate", "[UCC 4/15] Exchange Rate");

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

		var intracommunityReceiverFindBoxColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZOrganisationFindBoxColumnStyleInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.ConsigneeOrgPK);
		var intracommunityReceiverDropEditColumnStyleInfo = JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGuidDropEditColumnStyleInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);

		intracommunityReceiverFindBoxColumnStyleInfo.CaptionResourceString = Res.GetData("571C8D72-ED5C-416E-B555-2B91CD47A5E5", "Intra-community Receiver");
		intracommunityReceiverFindBoxColumnStyleInfo.GroupName = Res.GetData("37BA5EE1-8C5C-4302-9812-76D87D08800A", "Intra-community Receiver");
		intracommunityReceiverFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		intracommunityReceiverFindBoxColumnStyleInfo.IsVisible = true;

		intracommunityReceiverDropEditColumnStyleInfo.CaptionResourceString = Res.GetData("19A7A05A-9B3A-4BF2-A2C3-7026F220E14B", "Intra-community Receiver Address");
		intracommunityReceiverDropEditColumnStyleInfo.ToolTip = Res.GetString("8D564A4E-043E-4370-A0A4-5D3A20195C53", "Invoice\'s Intra-community Receiver Address");
		intracommunityReceiverDropEditColumnStyleInfo.GroupName = Res.GetData("37BA5EE1-8C5C-4302-9812-76D87D08800A", "Intra-community Receiver");
		intracommunityReceiverDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
		intracommunityReceiverDropEditColumnStyleInfo.IsVisible = true;
	}

	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => Enterprise.Customs.BE.GUI.Res.GetData("826B58F4-3E32-444A-9D7B-889BDA554790", "[44] Additional Documents");
	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => EU.GUI.CaptionProvider.SupportingDocuments;

	void ReorderTabPages()
	{
		InvoiceTabControl.TabPages.Clear();
		InvoiceTabControl.TabPages.Remove(ComInvoiceDetailsTabPage);
		InvoiceTabControl.TabPages.Insert(ComInvoiceDetailsTabPage, 0);

		InvoiceTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		InvoiceTabControl.TabPages.Insert(SupportingDocumentsTabPage, 1);

		InvoiceTabControl.TabPages.Remove(AdditionalInfoTabPage);
		InvoiceTabControl.TabPages.Insert(AdditionalInfoTabPage, 2);

		InvoiceTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		InvoiceTabControl.TabPages.Insert(PreviousDocumentsTabPage, 3);

		InvoiceTabControl.TabPages.Remove(ValueIndicatorsTabPage);
		InvoiceTabControl.TabPages.Insert(ValueIndicatorsTabPage, 4);

		InvoiceTabControl.TabPages.Remove(CustomFieldsTabPage);
		InvoiceTabControl.TabPages.Insert(CustomFieldsTabPage, 5);
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

	protected override Type GetAdditionalInfosUserControlType() => typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid);
}
