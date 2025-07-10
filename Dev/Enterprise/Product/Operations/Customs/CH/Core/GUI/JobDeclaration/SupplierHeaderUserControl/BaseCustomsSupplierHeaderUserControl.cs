using System;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class BaseCustomsSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
{
	public BaseCustomsSupplierHeaderUserControl()
	{
		InitializeComponent();
		InvoiceTabControl.ReorderTabPages(TabPagesInOrder);
		HideCalculatedInvoiceTotals();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumnsOnChargeGrids();
		ReorderColumnsOnChargeGrids();
	}

	protected virtual ZTabPage[] TabPagesInOrder => new ZTabPage[]
	{
			ComInvoiceDetailsTabPage,
			PreviousDocumentsTabPage,
			SupportingDocumentsTabPage,
			SpecialMentionsTabPage,
			CustomFieldsTabPage
	};

	protected virtual void AddColumnsOnChargeGrids()
	{
	}

	void ReorderColumnsOnChargeGrids()
	{
		InvoiceChargesGrid.ReOrderColumns(InvoiceChargesGridColumnOrder);
		BaseGroupChargesGrid.ReOrderColumns(BaseGroupChargesGridColumnOrder);
	}

	protected virtual string[] InvoiceChargesGridColumnOrder { get; }

	protected virtual string[] BaseGroupChargesGridColumnOrder { get; }

	void HideCalculatedInvoiceTotals()
	{
		this.JZ_FOBAmountBoundCurrencyControl.Visible = false;
		this.JZ_CIFAmountBoundCurrencyControl.Visible = false;
		this.JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;
	}

	void InvoiceChargesCalculateFreightButton_Click(object sender, EventArgs e)
	{
		var currentInvoice = GetCurrentInvoice();
		if (currentInvoice != null)
		{
			ShowCalculateFreightForm(currentInvoice.Charges);
		}
	}

	void GroupChargesCalculateFreightButton_Click(object sender, EventArgs e)
	{
		var topGroupInvoice = JobDeclaration?.TopGroupInvoice;
		if (topGroupInvoice != null)
		{
			ShowCalculateFreightForm(topGroupInvoice.Charges);
		}
	}

	JobComInvoiceHeader GetCurrentInvoice()
	{
		var invoiceHeader = (JobComInvoiceHeader)InvoiceHeadersBoundGrid.ListManager?.GetCurrent();
		if (invoiceHeader != null && invoiceHeader.IsDeleted)
		{
			invoiceHeader = null;
		}
		return invoiceHeader;
	}

	void ShowCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges)
	{
		var bizObj = new CalculateFreightBizObj(charges);
		ZFormModaliser.Show(new CalculateFreightForm(bizObj), FindForm());
	}

	internal static void AddIsStatisticalValueApplicableColumn(ZGrid grid)
	{
		grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo()
		{
			CaptionResourceString = Res.GetData("C67AE998-02C4-4E54-A5A9-65078C061858", "Statistical Value Applicable"),
			ColumnName = InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
			ToolTip = Res.GetString("24B4C0FB-8548-4603-8D3E-0BDABA29480E", "Statistical Value Applicable"),
			IsVisible = true,
			IsReadOnly = false,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152)
		});
	}

	internal static void AddExchangeRatesColumns(ZGrid grid)
	{
		var exchangeRatesGroupName = Res.GetData("F908E249-E1AD-4596-B5D0-C7AD612BD472", "Exchange Rate");

		grid.ColumnStyles.AddRange(new ZGridColumnInfo[]
		{
				new ZCheckBoxColumnStyleInfo()
				{
				 CaptionResourceString = Res.GetData("EEB249D5-9BB9-4EA0-8D3A-7FD38E16CC83", "Fixed Rate"),
				 ColumnName = InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				 GroupName = exchangeRatesGroupName,
				 IsMandatory = true,
				 Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74)
				},
				new ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("6B78E4A9-19D6-4985-98AC-6A371E503460", "Exchange Rate"),
					ColumnName = InvoiceCharge.Schema.J7_ExchangeRate,
					Decimals = 6,
					GroupName = exchangeRatesGroupName,
					IsMandatory = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102)
				},
				new ZDateEditColumnStyleInfo()
				{
					CaptionResourceString = Res.GetData("98E5C146-C02E-4217-8ECC-21F7A680ACCE", "Exchange Rate Date"),
					ColumnName = InvoiceLineCharge.Schema.J7_ExchangeRateDate,
					GroupName = exchangeRatesGroupName,
					IsMandatory = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				}
		});
	}
}
