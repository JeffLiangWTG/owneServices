using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
{
	public ExportSupplierHeaderUserControl()
	{
		InitializeComponent();
		InvoiceTabControl.ReorderTabPages(ExportTabPagesInOrder);

		InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
	}

	protected ZTabPage[] ExportTabPagesInOrder => new ZTabPage[]
	{
			ComInvoiceDetailsTabPage,
			PreviousDocumentsTabPage,
			SupportingDocumentsTabPage,
			TransportDocumentsTabPage,
			CustomFieldsTabPage
	};

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		TransportDocumentsTabPage.TabVisible = JobDeclaration?.IsExport ?? false;
		SpecialMentionsTabPage.TabVisible = false;
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

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
}
