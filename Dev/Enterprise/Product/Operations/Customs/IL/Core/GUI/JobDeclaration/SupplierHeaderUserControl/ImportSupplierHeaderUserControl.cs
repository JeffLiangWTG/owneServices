using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.GUI
{
	public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, new string[] { JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDisplaySequence,
																								JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate });

				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(false, new string[] { JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
																								JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
																								JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate,
																								JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill,
																								BaseJobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
																								JobComInvoiceHeaderSchema.Constants.JZ_Remarks,
																								BaseJobComInvoiceHeader.Schema.InvoiceLineTotal });
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumns();
			ReorderAndChangeInvoiceHeadersGridVisibility();
		}

		void AddColumns()
		{
			var zDropEditColumnStyleInfo21 = new ZDropEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceHeader.Schema.JZ_InvoiceType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			};

			var invoiceLineTotalColumnStyleInfo = JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.InvoiceLineTotal);
			invoiceLineTotalColumnStyleInfo.IsMandatory = false;

			JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo21);
		}

		void ReorderAndChangeInvoiceHeadersGridVisibility()
		{
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(InvoiceHeadersGridColumnNamesInSortOrder);
			}
		}

		IReadOnlyList<string> InvoiceHeadersGridColumnNamesInSortOrder => new string[]
		{
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDisplaySequence,
			JobComInvoiceHeader.Schema.JZ_InvoiceType,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
			JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
			JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
			JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
			JobComInvoiceHeader.Schema.JZ_Calc_BalanceString
		};
	}
}
