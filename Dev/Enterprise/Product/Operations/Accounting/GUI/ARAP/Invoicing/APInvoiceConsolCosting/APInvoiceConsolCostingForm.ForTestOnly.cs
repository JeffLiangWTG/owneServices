#if DEBUG

using System;
using System.ComponentModel;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class APInvoiceConsolCostingForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public void OnClosed_ForTestOnly(EventArgs e)
		{
			OnClosed(e);
		}

		public void OnClosing_ForTestOnly(CancelEventArgs e)
		{
			OnClosing(e);
		}

		public ZArchitecture.GUI.ZButton BulkConsolCostImportButton_ForTestOnly
		{
			get { return BulkConsolCostImportButton; }
			set { BulkConsolCostImportButton = value; }
		}

		public ZArchitecture.GUI.ZButton ApportionButton_ForTestOnly
		{
			get { return ApportionButton; }
			set { ApportionButton = value; }
		}

		public ZArchitecture.GUI.ZButton CloseButton_ForTestOnly
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}

		public Business.ARAP.Invoicing.APInvoiceConsolCosting ApportionmentList_ForTestOnly => ApportionmentList;

		public ZArchitecture.ZGrid ApportionmentChargesGrid_ForTestOnly
		{
			get { return ApportionmentChargesGrid; }
			set { ApportionmentChargesGrid = value; }
		}

		public ZArchitecture.ZGrid ApportionmentsGrid_ForTestOnly
		{
			get { return ApportionmentsGrid; }
			set { ApportionmentsGrid = value; }
		}

		public ZArchitecture.ZGrid ConsolSummaryGrid_ForTestOnly
		{
			get { return ConsolSummaryGrid; }
			set { ConsolSummaryGrid = value; }
		}
	}
}

#endif
