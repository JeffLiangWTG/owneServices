using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI;

public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();

		InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
	}

	protected override void AddColumnsOnChargeGrids()
	{
		base.AddColumnsOnChargeGrids();

		using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			AddIsStatisticalValueApplicableColumn(InvoiceChargesGrid);
			AddExchangeRatesColumns(InvoiceChargesGrid);
		}

		using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			AddIsStatisticalValueApplicableColumn(BaseGroupChargesGrid);
			AddExchangeRatesColumns(BaseGroupChargesGrid);
		}
	}

	protected override string[] InvoiceChargesGridColumnOrder => new[]
	{
			InvoiceCharge.Schema.J7_ChargeType,
			InvoiceCharge.Schema.J7_Amount,
			InvoiceCharge.Schema.J7_RX_NKCurrency,
			InvoiceCharge.Schema.J7_IsDutiable,
			InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
			InvoiceCharge.Schema.J7_IsGSTApplicable,
			InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
			InvoiceCharge.Schema.J7_IsIncludedInITOT,
			InvoiceCharge.Schema.J7_DistributeBy,
			InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
			InvoiceCharge.Schema.J7_ExchangeRate,
			InvoiceCharge.Schema.J7_ExchangeRateDate,
		};

	protected override string[] BaseGroupChargesGridColumnOrder => new[]
	{
			InvoiceCharge.Schema.J7_ChargeType,
			InvoiceCharge.Schema.J7_Amount,
			InvoiceCharge.Schema.J7_RX_NKCurrency,
			InvoiceCharge.Schema.J7_IsDutiable,
			InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
			InvoiceCharge.Schema.J7_IsGSTApplicable,
			InvoiceCharge.Schema.J7_Percentage,
			InvoiceCharge.Schema.J7_DistributeBy,
			InvoiceCharge.Schema.J7_FullOrPartialApportionment,
			BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
			InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
			InvoiceCharge.Schema.J7_ExchangeRate,
			InvoiceCharge.Schema.J7_ExchangeRateDate,
		};
}
