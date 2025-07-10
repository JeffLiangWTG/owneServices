using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;

namespace Enterprise.Accounting.Module
{
	public partial class APIncompleteInvoicesFilterStripControl : TransactionFilterStripControl
	{
		public APIncompleteInvoicesFilterStripControl()
		{
			InitializeComponent();
			RemoveExcessColumnStyles();
		}

		public APIncompleteInvoicesFilterStripControl(IBusinessObjectCollection gridCollection, TransactionFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveExcessColumnStyles();
		}

		void RemoveExcessColumnStyles()
		{
			List<ZString> columnsList = new List<ZString>(new ZString[]
			{
				"EInvoicingStatus", "EInvoicingError", "EInvoicingLastResponseReceivedUtc", "EInvoicingLastSentTimeUtc",
				"EInvoicingBatchNumber", "EInvoicingGovernmentAllocatedNumber", "EInvoicingeHubAllocatedNumber"
			});
			int i = 0;
			while (i < FilteredGrid.ColumnStyles.Count)
			{
				ZGridColumnInfo column = (ZGridColumnInfo)FilteredGrid.ColumnStyles[i];
				if (columnsList.Contains(column.ColumnName))
				{
					FilteredGrid.ColumnStyles.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}
	}
}
