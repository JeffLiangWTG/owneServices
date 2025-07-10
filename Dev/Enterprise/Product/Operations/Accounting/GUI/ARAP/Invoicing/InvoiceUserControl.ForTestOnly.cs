#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoiceUserControl
	{
		internal CollapsibleTableLayoutPanel InvoiceCollapsibleTableLayoutPanel_ForTestOnly
		{
			get { return InvoiceCollapsibleTableLayoutPanel; }
			set { InvoiceCollapsibleTableLayoutPanel = value; }
		}

		public ZTabPage TaxSummaryTabPage_ForTestOnly
		{
			get { return TaxSummaryTabPage; }
			set { TaxSummaryTabPage = value; }
		}

		public ZTabPage TaxTransactionSummaryTabPage_ForTestOnly
		{
			get { return TaxTransactionSummaryTabPage; }
			set { TaxTransactionSummaryTabPage = value; }
		}

		public ZButton RemoveTaxTransactionsButton_ForTestOnly
		{
			get { return RemoveTaxTransactionsButton; }
			set { RemoveTaxTransactionsButton = value; }
		}

		public ZDropEdit ExtendDropEdit_ForTestOnly => ExtendDropEdit;
	}
}

#endif
