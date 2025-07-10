using System.Collections.Generic;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>
		{
			JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
			JobComInvoiceHeader.Schema.JZ_OH_Supplier,
			JobComInvoiceHeader.Schema.JZ_OH_Buyer,
			JobComInvoiceHeader.Schema.JZ_IncoTerm,
			JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
			JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
			JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
			JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
			JobComInvoiceHeader.Schema.InvoiceLineTotal,
			JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
			JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
			JobComInvoiceHeader.Schema.JZ_Weight,
			JobComInvoiceHeader.Schema.JZ_WeightUQ,
			JobComInvoiceHeader.Schema.JZ_NetWeight,
			JobComInvoiceHeader.Schema.JZ_NetWeightUQ,
			JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
		};
	}
}
