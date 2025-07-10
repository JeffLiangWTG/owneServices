using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceBatchLineCollection : InvoicingBaseCollection
	{
		readonly InvoiceBatchHeader InvoiceBatchHeader;

		public InvoiceBatchLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public InvoiceBatchLineCollection(BusinessObjectFactory factory, InvoiceBatchHeader invoiceBatchHeader)
			: base(factory)
		{
			this.InvoiceBatchHeader = invoiceBatchHeader;
		}

		public ZQuery GetCombinedQuery(ZQuery alternativeAdditionalFilter)
		{
			alternativeAdditionalFilter.AddToFilter(RelationshipFilter);
			return alternativeAdditionalFilter;
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			if (InvoiceBatchHeader != null && InvoiceBatchHeader.IsInDatabase)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		#region Filter related

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();

			if (InvoiceBatchHeader != null && InvoiceBatchHeader.IsInDatabase)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(TransactionTypeFilter);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_AH_InvoiceStatement, InvoiceBatchHeader.PK);
			}
			else
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(TransactionTypeFilter);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_AH_InvoiceStatement, null);
				filter.AddToFilter(PaymentStatusFilter);
			}
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			return filter;
		}

		ZQuery TransactionTypeFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.AdjustmentNote);
				filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.CreditNote);
				return filter;
			}
		}

		ZQuery PaymentStatusFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(UnpaidFilter);

				ZQuery alternativeFilter = new ZQuery();
				alternativeFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.Equal, 0m);
				alternativeFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_InvoiceAmount, SQLComparisonOperator.Equal, 0m);

				filter.DefaultJoinCondition = JoinCondition.Or;
				filter.AddToFilter(alternativeFilter);

				return filter;
			}
		}

		ZQuery UnpaidFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				string queryText = AccTransactionHeader.AH_LocalTotalSQLFormula + " = " + AccTransactionHeaderSchema.AH_OutstandingAmount.Name;
				filter.AddFilterAndZSQLParameterCollection(queryText, null);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, null);
				return filter;
			}
		}

		#endregion

		#region Utility Method & Properties

		public int SelectedCount
		{
			get
			{
				int total = 0;
				foreach (InvoicingBase line in this)
				{
					if (line.IncludeInTheBatch)
					{
						total++;
					}
				}
				return total;
			}
		}

		public void ClearAllLines()
		{
			RemoveAll();
			InvoiceBatchHeader.SetAmountToZero();
		}

		public void SetIncludeBatchFlags(bool flag)
		{
			foreach (InvoicingBase line in Elements)
			{
				line.IncludeInTheBatch = flag;
			}
		}

		#endregion

		#region Base Override

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (IsBatchHeaderNotInDataBase)
			{
				var invoice = (InvoicingBase)bizO;
				invoice.IncludeInTheBatch = false;
				invoice.UpdateInvoiceBatchTotal -= UpdateSelectedTotal;
			}
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (IsBatchHeaderNotInDataBase)
			{
				var invoice = (InvoicingBase)bizOAdded;
				invoice.UpdateInvoiceBatchTotal += UpdateSelectedTotal;
				invoice.IncludeInTheBatch = true;
			}
		}

		bool IsBatchHeaderNotInDataBase => InvoiceBatchHeader != null && !InvoiceBatchHeader.IsInDatabase;

		#endregion

		void UpdateSelectedTotal(object sender, InvoiceBatchEventArgs e)
		{
			InvoiceBatchHeader.UpdateSelectedTotal(e.Invoice);
		}
	}
}
