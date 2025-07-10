using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionOrderLine : AutoAccCollectionOrderLine
	{
		public AccCollectionOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[RelatedBusinessObject("Transaction")]
		public override ZGuid AOL_AH
		{
			get { return base.AOL_AH; }
			set { base.AOL_AH = value; }
		}

		public AccTransactionHeader Transaction
		{
			get
			{
				return Factory.Load<TransactionHeader>(AOL_AH);
			}
		}

		public ZString TransactionType
		{
			get
			{
				return Transaction != null ? Transaction.AH_TransactionType : ZString.Empty;
			}
		}

		public ZString TransactionNumber
		{
			get
			{
				return Transaction != null ? Transaction.AH_TransactionNum : ZString.Empty;
			}
		}

		public ZString JobInvoicingNumber
		{
			get
			{
				return Transaction != null ? Transaction.JobNumber : ZString.Empty;
			}
		}

		public ZDateTime PostDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_PostDate : ZDateTime.Empty;
			}
		}

		public ZDateTime InvoiceDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_InvoiceDate : ZDateTime.Empty;
			}
		}

		public ZDateTime DueDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_DueDate : ZDateTime.Empty;
			}
		}

		public ZString Description
		{
			get
			{
				return Transaction != null ? Transaction.AH_Desc : ZString.Empty;
			}
		}

		public ZDateTime FullyPaidDate
		{
			get
			{
				return Transaction != null ? Transaction.AH_FullyPaidDate : ZDateTime.Empty;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalInvoiceAmount
		{
			get
			{
				return Transaction != null ? ((TransactionHeader)Transaction).AH_LocalTotalAmount : ZDecimal.Zero;
			}
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OSInvoiceAmount
		{
			get
			{
				return Transaction != null ? ((TransactionHeader)Transaction).AH_OSTotalAmount : ZDecimal.Zero;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalOutstandingAmount
		{
			get
			{
				return Transaction != null ? ((TransactionHeader)Transaction).AH_OutstandingAmount : ZDecimal.Zero;
			}
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OSOutstandingAmount
		{
			get
			{
				return Transaction != null ? ((TransactionHeader)Transaction).AH_Calc_OSOutstandingAmount : ZDecimal.Zero;
			}
		}

		public ZString OSCurrency
		{
			get
			{
				return Transaction != null ? ((TransactionHeader)Transaction).AH_RX_NKTransactionCurrency : ZString.Empty;
			}
		}

		public ZString LocalCurrency
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public ZString CollectionCurrency
		{
			get
			{
				//return batch/order's currency
				return CollectionOrder != null ? CollectionOrder.ACO_RX_NKCurrency : ZString.Empty;
			}
		}

		public int LocalDecimals => Transaction?.Company.GetLocalDecimals() ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		public int OSDecimals => Transaction?.TransactionCurrency.Decimals ?? LocalDecimals;

		public int CollectionAmountDecimals => (Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CollectionCurrency))?.Decimals ?? OSDecimals;

		[DecimalPlaces(nameof(CollectionAmountDecimals))]
		public ZDecimal CollectionAmount
		{
			get
			{
				if (Transaction != null && Transaction is TransactionHeader)
				{
					// if batch/order uses local currency, then return local outstanding amount.
					if (CollectionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						return ((TransactionHeader)Transaction).AH_OutstandingAmount;
					}
					else  // if there's only 1 foreign currency then return OS outstanding amount.
					{
						return ((TransactionHeader)Transaction).AH_OSOutstandingAmountWithoutMultiplier;
					}
				}
				else
				{
					return ZDecimal.Zero;
				}
			}
		}

		[RelatedBusinessObject("CollectionOrder")]
		public override ZGuid AOL_ACO
		{
			get { return base.AOL_ACO; }
			set { base.AOL_ACO = value; }
		}

		public AccCollectionOrder CollectionOrder
		{
			get { return Factory.Load<AccCollectionOrder>(AOL_ACO); }
		}

		public ZBool IsMatchedWithReceipt
		{
			get
			{
				return CollectionAmount == 0 && !FullyPaidDate.IsEmpty;
			}
		}

		[ReadOnlyMember(nameof(IncludeInOrder_ReadOnly))]
		public ZBool IncludeInOrder
		{
			get
			{
				return fIncludeInOrder;
			}
			set
			{
				bool hasChanges = fIncludeInOrder != value;
				fIncludeInOrder = value;
				IncludeInOrderInfo.RefreshBinding();
				if (hasChanges && CollectionOrder != null)
				{
					if (value)
					{
						CollectionOrder.ACO_Amount += CollectionAmount;
					}
					else
					{
						CollectionOrder.ACO_Amount -= CollectionAmount;
					}
				}
			}
		}

		ZBool fIncludeInOrder = ZBool.False;

		public ZPropertyInfo IncludeInOrderInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInOrder)); }
		}

		public bool IncludeInOrder_ReadOnly
		{
			get { return IsCancelled || IsMatchedWithReceipt; }
		}

		public void SetIncludeInOrderWithoutRecalculateOrderAmount(ZBool isIncluded)
		{
			fIncludeInOrder = isIncluded;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				CollectionOrder.ACO_CollectionDate = ZDateTime.Today.Date;
				CollectionOrder.ACO_Amount = 50m;
				CollectionOrder.CollectionBatch.ACB_TotalAmount = 50m;
			}
		}
#endif
	}
}

