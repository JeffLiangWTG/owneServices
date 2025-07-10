using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ARReceiptCollection : BusinessObjectCollection<ARReceipt>
	{
		public ARReceiptCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ARReceiptCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ARReceiptCollection(BusinessObjectFactory factory, BusinessObject parentBizObj)
			: base(factory)
		{
			Parent = parentBizObj;
		}

		#region Events

		public delegate void OnAmountOnChildChangedHandler();
		public event OnAmountOnChildChangedHandler OnAmountOnChildChanged;

		public void RaiseOnAmountOnChildChanged()
		{
			if (OnAmountOnChildChanged != null)
			{
				OnAmountOnChildChanged();
			}
		}

		public delegate void OnReceiptTypeOnChildChangedHandler();
		public event OnReceiptTypeOnChildChangedHandler OnReceiptTypeOnChildChanged;

		public void RaiseOnReceiptTypeOnChildChanged()
		{
			if (OnReceiptTypeOnChildChanged != null)
			{
				OnReceiptTypeOnChildChanged();
			}
		}

		#endregion

		#region Public Members

		public ZDecimal ForeignTotalAmount
		{
			get
			{
				ZDecimal result = 0M;
				foreach (ARReceipt receipt in this)
				{
					result += receipt.AH_OSExTaxAmount;
				}
				return result;
			}
		}

		public ZDecimal LocalTotalAmount
		{
			get
			{
				ZDecimal result = 0M;
				foreach (ARReceipt receipt in this)
				{
					result += receipt.AH_LocalExTaxAmount;
				}
				return result;
			}
		}

		public void SetReceiptHeaderDataIsReadOnly()
		{
			ReceiptHeaderDataIsReadOnly = ZBool.True;
		}
		ZBool ReceiptHeaderDataIsReadOnly;

		public void SetIncludeInDepositBatchForAllReceipts(ZBool value)
		{
			foreach (ARReceipt receipt in this)
			{
				if (receipt.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque || receipt.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash || receipt.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard)
				{
					receipt.IncludeInDepositBatch = value;
				}
				receipt.SetDefaultIncludeInDepositBatch(value);
			}
			DefaultIncludeInDepositBatch = value;
		}
		ZBool DefaultIncludeInDepositBatch;

		#endregion

		#region Overrides

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			ARReceipt newReceipt = ((ARReceipt)child);
			newReceipt.SetDefaultIncludeInDepositBatch(DefaultIncludeInDepositBatch);
			newReceipt.SuspendSettingDefaultBankForOrganisation(ZBool.True);

			if (ParentBatchPoster != null)
			{
				SetDefaultsFromParentBatchPoster(newReceipt);
			}

			if (ReceiptHeaderDataIsReadOnly)
			{
				newReceipt.IsPopulatedFromARReceipt = true;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			RaiseOnReceiptTypeOnChildChanged();
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		readonly BusinessObject Parent;

		ARReceiptBatchPoster ParentBatchPoster
		{
			get
			{
				return Parent as ARReceiptBatchPoster;
			}
		}

		void SetDefaultsFromParentBatchPoster(ARReceipt newReceipt)
		{
			newReceipt.AH_InvoiceDate = ParentBatchPoster.InvoiceDate;
			newReceipt.AH_PostDate = ParentBatchPoster.PostDate;
			newReceipt.AH_ReceiptType = ParentBatchPoster.ReceiptType;
			newReceipt.AH_RX_NKTransactionCurrency = ParentBatchPoster.RX_NK;
			newReceipt.AH_AB = ParentBatchPoster.BankAccountPK;
			newReceipt.AH_Desc = ParentBatchPoster.Description;
			newReceipt.AH_ExchangeRate = ParentBatchPoster.SellExRate;
		}

		#endregion
	}
}
