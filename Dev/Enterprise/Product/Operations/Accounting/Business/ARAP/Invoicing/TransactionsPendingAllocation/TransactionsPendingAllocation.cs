using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionsPendingAllocation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransactionsPendingAllocation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		bool AreTotalsDirty = true;

		public void TotalsDirty()
		{
			AreTotalsDirty = true;
		}

		ZDecimal fBatchLocalTotal;
		public ZDecimal BatchLocalTotal
		{
			get
			{
				if (AreTotalsDirty)
				{
					RefreshTotals();
					AreTotalsDirty = false;
				}
				return fBatchLocalTotal;
			}
		}

		public ZPropertyInfo BatchLocalTotalInfo
		{
			get { return GetZPropertyInfo(nameof(BatchLocalTotal)); }
		}

		ZDecimal fBatchLocalTaxTotal;
		public ZDecimal BatchLocalTaxTotal
		{
			get
			{
				if (AreTotalsDirty)
				{
					RefreshTotals();
					AreTotalsDirty = false;
					BatchLocalTaxTotalInfo.RefreshBinding();
				}
				return fBatchLocalTaxTotal;
			}
		}

		public ZPropertyInfo BatchLocalTaxTotalInfo
		{
			get { return GetZPropertyInfo(nameof(BatchLocalTaxTotal)); }
		}

		bool IsRefreshingTotals;

		void RefreshTotals()
		{
			if (IsRefreshingTotals) //try to avoid infinite Loop
			{
				return;
			}

			try
			{
				IsRefreshingTotals = true;

				fBatchLocalTaxTotal = 0m;
				fBatchLocalTotal = 0m;
				foreach (TransactionPendingAllocation transaction in Transactions)
				{
					fBatchLocalTaxTotal += transaction.AH_LocalTaxAmount;
					fBatchLocalTotal += transaction.AH_LocalTotalAmount;
				}
				BatchLocalTaxTotalInfo.RefreshBinding();
				BatchLocalTotalInfo.RefreshBinding();
			}
			finally
			{
				IsRefreshingTotals = false;
			}
		}

		[List("Currencies")]
		public ZString Currency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				return new RefCurrencyCollection(Factory);
			}
		}

		TransactionPendingAllocationCollection fTransactionsPendingAllocation;
		public TransactionPendingAllocationCollection Transactions
		{
			get
			{
				if (fTransactionsPendingAllocation == null)
				{
					fTransactionsPendingAllocation = new TransactionPendingAllocationCollection(this, Factory);
					RegisterEditableChildObject(fTransactionsPendingAllocation);
				}
				return fTransactionsPendingAllocation;
			}
		}
	}
}