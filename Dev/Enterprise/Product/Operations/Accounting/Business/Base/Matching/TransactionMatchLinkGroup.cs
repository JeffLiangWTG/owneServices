
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLinkGroup : TransactionMatchLinkCollection, ISupportCriticalValidation, IFactoryProvider
	{
		public TransactionMatchLinkGroup(TransactionMatchLinkCollection collection)
			: base(collection.Factory)
		{
			this.AddRange(collection);
		}

		public TransactionMatchLinkGroup(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionMatchLinkGroup(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public bool ContainsInvoice(APInvoice invoice)
		{
			foreach (AccTransactionMatchLink matchLink in this)
			{
				if (matchLink.AP_AH == invoice.PK)
				{
					return true;
				}
			}
			return false;
		}

		public ZDecimal GetBalance()
		{
			ZDecimal balance = 0;
			foreach (AccTransactionMatchLink matchLink in this)
			{
				balance += matchLink.AP_Amount;
			}

			return balance;
		}

		public void SetMatchGroupNumberAndMatchDate(ZString groupNum, ZDateTime matchDate)
		{
			foreach (AccTransactionMatchLink matchLink in this)
			{
				matchLink.AP_MatchGroupNum = groupNum;
				matchLink.AP_MatchDate = matchDate;
			}
		}

		#region ISupportCriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new TransactionMatchLinkGroupCriticalValidation(this); }
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region DEBUG Methods
#if DEBUG

		/// <summary>
		/// DEBUG USE ONLY - Find a match link by passing in its parent transaction header and amount
		/// </summary>

		public TransactionMatchLink FindMatchLinkByTransactionHeaderAndAmount(TransactionHeader transaction, decimal amount)
		{
			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.Equal, transaction.PK);
			filter.AddToFilter(AccTransactionMatchLinkSchema.AP_Amount, SQLComparisonOperator.Equal, amount);
			return (TransactionMatchLink)Find(filter)[0];
		}

#endif
		#endregion
	}
}
