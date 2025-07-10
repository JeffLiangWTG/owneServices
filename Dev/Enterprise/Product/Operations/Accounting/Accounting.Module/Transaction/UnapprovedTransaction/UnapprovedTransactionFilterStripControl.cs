using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedTransactionFilterStripControl : TransactionFilterStripControl
	{
		public UnapprovedTransactionFilterStripControl()
		{
			InitializeComponent();
			RemoveExcessColumnStyles();
			SetDefaultDisplayColumnsOrder();
		}

		public UnapprovedTransactionFilterStripControl(IBusinessObjectCollection gridCollection, TransactionFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveExcessColumnStyles();
		}

		void SetDefaultDisplayColumnsOrder()
		{
			FindAndInsertColumn(0, TransactionHeader.Schema.AH_IsCancelled);
			FindAndInsertColumn(1, TransactionHeader.Schema.AH_TransactionType);
			FindAndInsertColumn(2, TransactionHeader.Schema.AH_TransactionNum);
			FindAndInsertColumn(3, TransactionHeader.Schema.AH_ConsolidatedInvoiceRef);
			FindAndInsertColumn(4, TransactionHeader.Schema.AH_Desc);
			FindAndInsertColumn(5, TransactionHeader.Schema.AH_TransactionCategory);
			FindAndInsertColumn(6, TransactionHeader.Schema.AH_OH);
			FindAndInsertColumn(7, "IntercompanyOrgProxy");
			FindAndInsertColumn(8, TransactionHeader.Schema.AH_RX_NKTransactionCurrency);
			FindAndInsertColumn(9, TransactionHeader.Schema.AH_OSTotal);
			FindAndInsertColumn(10, TransactionHeader.Schema.AH_ExchangeRate);
			FindAndInsertColumn(11, TransactionHeader.Schema.AH_LocalTotal);
			FindAndInsertColumn(12, TransactionHeader.Schema.AH_OSTax);
			FindAndInsertColumn(13, TransactionHeader.Schema.AH_PostDate);
			FindAndInsertColumn(14, TransactionHeader.Schema.AH_InvoiceDate);
			FindAndInsertColumn(15, TransactionHeader.Schema.AH_DueDate);
			FindAndInsertColumn(16, TransactionHeader.Schema.AH_GB);
			FindAndInsertColumn(17, TransactionHeader.Schema.AH_GE);
			bool columnAdded = FindAndInsertColumn(18, "IsSelfBillingInvoice");

			FilteredGrid.SetAllColumnsVisible(true);
			if (columnAdded)
			{
				FilteredGrid.SetColumnVisible(false, "IsSelfBillingInvoice");
			}
			FindAndInsertColumn(19, TransactionHeader.Schema.AH_ChequeOrReference);

			columnAdded = FindAndInsertColumn(20, TransactionHeader.Schema.ReceivingOperator);
			if (columnAdded)
			{
				FilteredGrid.SetColumnVisible(false, TransactionHeader.Schema.ReceivingOperator);
			}

			columnAdded = FindAndInsertColumn(21, TransactionHeader.Schema.ReceivingBranch);
			if (columnAdded)
			{
				FilteredGrid.SetColumnVisible(false, TransactionHeader.Schema.ReceivingBranch);
			}

			columnAdded = FindAndInsertColumn(22, TransactionHeader.Schema.ReceivingDepartment);
			if (columnAdded)
			{
				FilteredGrid.SetColumnVisible(false, TransactionHeader.Schema.ReceivingDepartment);
			}
		}

		bool FindAndInsertColumn(int position, string name)
		{
			bool result = false;
			for (int i = 0; i < FilteredGrid.ColumnStyles.Count; i++)
			{
				if (((ZGridColumnInfo)FilteredGrid.ColumnStyles[i]).ColumnName == name)
				{
					FilteredGrid.ColumnStyles.Insert(position, FilteredGrid.ColumnStyles[i]);
					result = true;
					break;
				}
			}
			return result;
		}

		void RemoveExcessColumnStyles()
		{
			List<ZString> columnsList = new List<ZString>(new ZString[]
			{
				AccTransactionHeaderSchema.AH_FullyPaidDate.Name,
				TransactionHeader.Schema.DepositBatchNumber,
				TransactionHeader.Schema.DirectDebitNumber,
				AccTransactionHeaderSchema.AH_InvoicePrinted.Name,
				TransactionHeader.Schema.AH_TransactionReference,
				TransactionHeader.Schema.OSOutstandingAmountMatching,
				TransactionHeader.Schema.AH_Ledger,
				TransactionHeader.Schema.AH_PostToGL,
				"JobNumber", "EInvoicingStatus", "EInvoicingError", "EInvoicingLastResponseReceivedUtc", "EInvoicingLastSentTimeUtc",
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

		protected override void Bind()
		{
			base.Bind();
			SetDefaultDisplayColumnsOrder();
		}
	}
}
