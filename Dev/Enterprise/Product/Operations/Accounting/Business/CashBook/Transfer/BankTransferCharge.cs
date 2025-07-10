using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferCharge : DirectPayment.DirectPayment
	{
		public const byte TransactionCount = 3;
		public const byte TransactionCountWhenReversing = 6;

		public BankTransferCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_ReceiptType = ReceiptTypes.EFT;
			AH_TransactionCount = TransactionCount;

			if (Lines.Count == 0)
			{
				Lines.AddNew(typeof(BankTransferChargeLine));
			}

			ExchangeRate.IsCurrencyRequired = false;
			ExchangeRate.IsRateRequired = false;
		}

		public BankTransfer BankTransferParent
		{
			get { return fParent; }
			set { fParent = value; }
		}
		BankTransfer fParent;

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			if (IsInDatabase || !EnableFinanceCharge)
			{
				return new BankTransferChargeEmptyValidation(this);
			}
			else
			{
				return new BankTransferChargeValidation(this);
			}
		}

		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory && EnableFinanceCharge;
			}
		}

		public bool EnableFinanceCharge
		{
			get { return fEnableFinanceCharge; }
			set
			{
				fEnableFinanceCharge = value;
				ExchangeRate.IsCurrencyRequired = value;
				ExchangeRate.IsRateRequired = value;
				if (value)
				{
					if (BankTransferParent != null)
					{
						AH_AB = BankTransferParent.BankTransferFromPK;
						ExchangeRate.Currency = BankTransferParent.SellCurrency;
						ExchangeRate.Rate = BankTransferParent.SellExchangeRate;
					}
				}
				Validation.ValidateAll();
				Lines[0].Validation.ValidateAll();
			}
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		public override ZDecimal AH_ExchangeRate
		{
			get { return base.AH_ExchangeRate; }
			set
			{
				base.AH_ExchangeRate = value;
				if (BankTransferParent != null)
				{
					BankTransferParent.RefreshBinding();
				}
			}
		}

		bool fEnableFinanceCharge;

		protected override ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				return ZBool.False;
			}
		}
	}
}
