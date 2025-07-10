#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls
{
	public partial class CollectionCallsTransactionsPrintingControl
	{
		public List<ZString> TypesThatCanBeViewed_ForTestOnly => TypesThatCanBeViewed;

		public Business.OrgCollectionCalls.CollectionNotesTransactionsFilter CollectionNotesTransactionsFilter_ForTestOnly => CollectionNotesTransactionsFilter;

		public ZArchitecture.ZGrid InvoicesGrid_ForTestOnly
		{
			get { return InvoicesGrid; }
			set { InvoicesGrid = value; }
		}

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;

		public ZBool CheckSelectedTransactionsOnInvoices_ForTestOnly(ZString checkpointName, ZString message, ZString caption)
		{
			return CheckSelectedTransactionsOnInvoices(checkpointName, message, caption);
		}

		public void PrintClassAInvoices_ForTestOnly(object sender, EventArgs e)
		{
			PrintClassAInvoices(sender, e);
		}

		public void PrintInvoices_ForTestOnly(object sender, EventArgs e)
		{
			PrintInvoices(sender, e);
		}

		public void UpdateClassAInvoices_ForTestOnly(object sender, EventArgs e)
		{
			UpdateClassAInvoices(sender, e);
		}
	}
}

#endif
