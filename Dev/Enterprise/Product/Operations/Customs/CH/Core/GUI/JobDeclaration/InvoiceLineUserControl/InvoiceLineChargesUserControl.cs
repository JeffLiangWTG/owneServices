using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.GUI;

public partial class InvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
{
	public InvoiceLineChargesUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	public void InitializeGridLayout()
	{
		using (ChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			ImportSupplierHeaderUserControl.AddIsStatisticalValueApplicableColumn(ChargesGrid);
			ImportSupplierHeaderUserControl.AddExchangeRatesColumns(ChargesGrid);
			ChargesGrid.ReOrderColumns(
				[
					BaseInvoiceLineCharge.Schema.J7_ChargeType,
					BaseInvoiceLineCharge.Schema.ChargeCodeDescription,
					BaseInvoiceLineCharge.Schema.J7_Amount,
					BaseInvoiceLineCharge.Schema.J7_RX_NKCurrency,
					BaseInvoiceLineCharge.Schema.J7_IsDutiable,
					BaseInvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
					BaseInvoiceLineCharge.Schema.J7_IsGSTApplicable,
					BaseInvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
					BaseInvoiceLineCharge.Schema.J7_Percentage,
					BaseInvoiceLineCharge.Schema.IsJ7_ExchangeRateUserEnterable,
					BaseInvoiceLineCharge.Schema.J7_ExchangeRate,
					BaseInvoiceLineCharge.Schema.J7_ExchangeRateDate
				]);
		}
	}
}
