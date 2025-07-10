using System.Collections.Generic;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAExportSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public CAExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			this.InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(
					JobComInvoiceHeader.Schema.JZ_OH_Supplier,
					JobComInvoiceHeader.Schema.SupplierName,
					JobComInvoiceHeader.Schema.JZ_OH_Buyer,
					JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate,
					JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
					JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
					JobComInvoiceHeader.Schema.JZ_NetWeight,
					JobComInvoiceHeader.Schema.JZ_NetWeightUQ
					);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAllColumnsVisible(false);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, InvoiceHeadersGridDefaultColumnsInSortOrder);
				JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(InvoiceHeadersGridDefaultColumnsInSortOrder);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, 110);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_IncoTerm, 60);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_InvoiceAmount, 100);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin, 100);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_RW_NKOriginState, 100);
			}
		}

		protected override string ColumnTitleForGSTApplies
		{
			get { return CACustomsSupplierHeaderUserControl.IsCIFComponent; }
		}

		string[] InvoiceHeadersGridDefaultColumnsInSortOrder
		{
			get
			{
				if (invoiceHeadersGridDefaultColumnsInSortOrder == null)
				{
					List<string> columnList = new List<string>();
					columnList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceNumber);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_IncoTerm);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_IncoTermPlace);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceAmount);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
					columnList.Add(JobComInvoiceHeader.Schema.InvoiceLineTotal);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_RW_NKOriginState);
					columnList.Add(JobComInvoiceHeader.Schema.JZ_InvoiceDate);
					invoiceHeadersGridDefaultColumnsInSortOrder = columnList.ToArray();
				}
				return invoiceHeadersGridDefaultColumnsInSortOrder;
			}
		}
		string[] invoiceHeadersGridDefaultColumnsInSortOrder;
	}
}
