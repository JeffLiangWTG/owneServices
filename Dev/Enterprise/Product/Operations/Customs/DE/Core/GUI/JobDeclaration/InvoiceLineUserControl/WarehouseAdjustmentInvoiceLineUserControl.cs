using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class WarehouseAdjustmentInvoiceLineUserControl : ImportInvoiceLineUserControl
	{
		public WarehouseAdjustmentInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var unwantedColunms = new string[]
				{
					AutoJobComInvoiceLine.Schema.JI_PartNo,
					JobComInvoiceLine.Schema.JI_FormattedTariff,
					JobComInvoiceLine.Schema.JI_Description,
					JobComInvoiceLine.Schema.JI_InvoiceQuantity,
					JobComInvoiceLine.Schema.JI_InvoiceUQ,
					JobComInvoiceLine.Schema.JI_CountryOfOrigin,
					JobComInvoiceLine.Schema.JI_PrimaryPreference,
					JobComInvoiceLine.Schema.JI_CustomsQuantity,
					JobComInvoiceLine.Schema.JI_CustomsUnitQty,
					JobComInvoiceLine.Schema.JI_LinePrice,
					JobComInvoiceLine.Schema.JI_SupplementaryCode1,
					JobComInvoiceLine.Schema.JI_SupplementaryCode2,
					JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					JobComInvoiceLine.Schema.JI_BondedWhsQuantity,
					JobComInvoiceLine.Schema.JI_BondedWhsUnitQty,
					nameof(JobComInvoiceLine.JI_NetPrice),
					nameof(JobComInvoiceLine.JI_RX_NKNetPriceCurr)
				};

				foreach (var unwantedColumn in unwantedColunms)
				{
					CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(CustomsInvoiceLinesBoundGrid.GetColumnStyle(unwantedColumn));
				}
			}
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new WarehouseAdjustmentInvoiceLineDetailsLayout();

		protected override IPanelLayoutProvider GetAdditionalInfoPanelLayout() => new WarehouseAdjustmentAdditionalInfoLayout();
	}
}
