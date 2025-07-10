using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public class DepositBatchTransactionLineCollection : TransactionHeaderCollection
	{
		public DepositBatchTransactionLineCollection(BusinessObjectFactory factory, ZGuid bankPK, ZGuid branchPK, DepositBatch parent)
			: this(factory, bankPK, branchPK, parent, ZGuid.Empty)
		{
		}

		public DepositBatchTransactionLineCollection(BusinessObjectFactory factory, ZGuid bankPK, ZGuid branchPK, DepositBatch parent, ZGuid originalReceiptPK)
			: base(factory)
		{
			fBranchPK = branchPK;
			fBankPK = bankPK;
			this.Parent = parent;
			fOriginalReceiptPK = originalReceiptPK;
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		public new DepositBatchTransactionLine this[int i]
		{
			get { return (DepositBatchTransactionLine)Elements[i]; }
		}

		public new DepositBatchTransactionLine AddNew()
		{
			return (DepositBatchTransactionLine)base.AddNew();
		}

		public new DepositBatchTransactionLine AddNew(Type bizObjType)
		{
			return (DepositBatchTransactionLine)base.AddNew(bizObjType);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, Parent.AH_TransactionNum);
			result.AddToFilter(AccTransactionHeaderSchema.AH_AB, fBankPK);
			if (IsNewBatchFetch)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
			}

			if (!fBranchPK.IsEmpty)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_GB, fBranchPK);
			}

			if (!fOriginalReceiptPK.IsEmpty)
			{
				result.AddToFilter(AccTransactionHeaderSchema.PK, fOriginalReceiptPK);
			}

			ZQuery transactionTypeQuery = new ZQuery();
			transactionTypeQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			transactionTypeQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.DirectReceipt);
			result.AddToFilter(transactionTypeQuery, JoinCondition.And);
			result.OrderBy = AccTransactionHeaderSchema.AH_AB.Name;

			return result;
		}

		bool IsNewBatchFetch
		{
			get { return Parent.AH_TransactionNum.IsEmpty; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((DepositBatchTransactionLine)bizOAdded).Parent = this.Parent;
		}

		readonly ZGuid fBankPK;
		readonly ZGuid fBranchPK;
		readonly DepositBatch Parent;
		readonly ZGuid fOriginalReceiptPK;
	}
}
