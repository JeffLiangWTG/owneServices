using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchLineCollection : TransactionHeaderCollection
	{
		public readonly DirectDebitBatchHeader DDRHeader;
		ZString BatchNo;
		AccBankAccount bank;

		public DirectDebitBatchLineCollection(BusinessObjectFactory factory, DirectDebitBatchHeader dDRHeader)
			: base(factory, null, dDRHeader.Company.PK != GlbCompany.CurrentCompany.PK)
		{
			this.DDRHeader = dDRHeader;
			InitializeBatchNo();
		}

		public DirectDebitBatchLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IDirectDebitBatchTransaction this[int index]
		{
			get { return (IDirectDebitBatchTransaction)Elements[index]; }
		}

		public void LoadNewLines(AccBankAccount bank)
		{
			this.bank = bank;
			ClearTotalAmount();

			ZQuery query = new ZQuery(NewLineFilter);
			if (DDRHeader != null && !DDRHeader.RelatedTransactionPK.IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.PK, DDRHeader.RelatedTransactionPK);
			}

			RemoveAll();
			AddRange(Factory.Load(TypeOfElements, GetCompleteLoadFilter(query)));
			((IBindingListView)this).ApplySort(((IBindingListView)this).SortDescriptions);

			SetIncludeBatchFlags(true);
			UpdateSelectedTotal(0m, 0m);
		}

		public override void Load()
		{
			InitializeBatchNo();
			if (IsDDRHeaderPosted)
			{
				ClearTotalAmount();
				base.Load();
				SetIncludeBatchFlags(true, suspendHasChanges: true);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TransactionHeader);
		}

		public void UpdateSelectedTotal(ZDecimal amountToUpdate, ZDecimal localAmountToUpdate)
		{
			fSelectedTotal += amountToUpdate;
			fLocalSelectedTotal += localAmountToUpdate;
			if (DDRHeader != null)
			{
				DDRHeader.AH_OSExTaxAmount = fSelectedTotal;
				DDRHeader.AH_LocalExTaxAmount = fLocalSelectedTotal;
				DDRHeader.AH_OSExTaxAmountInfo.RefreshBinding();
			}
		}

		public void ClearTotalAmount()
		{
			SetIncludeBatchFlags(false, suspendHasChanges: true);
			fSelectedTotal = 0m;
			fLocalSelectedTotal = 0m;
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		#region Filter Related

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();

			if (IsDDRHeaderPosted)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, BatchNo);
				filter.AddToFilter(TransactionTypeFilter);
			}

			return filter;
		}

		bool IsDDRHeaderPosted
		{
			get { return !BatchNo.IsEmpty; }
		}

		void InitializeBatchNo()
		{
			BatchNo = DDRHeader.AH_TransactionNum;
		}

		protected ZQuery NewLineFilter
		{
			get
			{
				ZQuery filter = new ZQuery();

				if (bank != null)
				{
					filter.IsNoResultQuery = false;
					filter.AddToFilter(AccTransactionHeaderSchema.AH_AB, bank.PK);
					var receiptType = DDRHeader?.AH_ReceiptType ?? ZString.Empty;
					receiptType = receiptType == ReceiptTypes.NonRolledUpBatch ? ReceiptTypes.DirectDebit : receiptType;
					filter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, receiptType.IsEmpty ? ReceiptTypes.DirectDebit : receiptType);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, "");
					filter.AddToFilter(TransactionTypeFilter);
				}
				else
				{
					filter.IsNoResultQuery = true;
				}

				return filter;
			}
		}

		ZQuery TransactionTypeFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
				filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.DirectPayment);
				return filter;
			}
		}

		#endregion

		void SetReceiptType(ZString receiptType)
		{
			if (!IsInDatabaseIncludingChildren)
			{
				foreach (TransactionHeader lines in Elements)
				{
					lines.AH_ReceiptType = receiptType;
				}
			}
		}

		public void SetIncludeBatchFlags(bool flag, bool suspendHasChanges = false)
		{
			foreach (IDirectDebitBatchTransaction line in Elements)
			{
				var bizo = line as BusinessObject;
				IDisposable hasChangesSuspenderForLines = null;
				IDisposable hasChangesSuspenderForHeader = null;
				if (bizo != null && suspendHasChanges)
				{
					hasChangesSuspenderForHeader = DDRHeader.SuspendSettingHasChanges();
					hasChangesSuspenderForLines = bizo.SuspendSettingHasChanges();
				}
				try
				{
					line.IncludeInTheBatch = flag;
				}
				finally
				{
					if (hasChangesSuspenderForLines != null)
					{
						hasChangesSuspenderForLines.Dispose();
					}
					if (hasChangesSuspenderForHeader != null)
					{
						hasChangesSuspenderForHeader.Dispose();
					}
				}
			}
		}

		ZDecimal fSelectedTotal;
		ZDecimal fLocalSelectedTotal;
	}
}
