using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public abstract partial class BankTransferRowValidation : TransactionHeaderValidation
	{
		public BankTransferRowValidation(BankTransferRow parent)
			: base(parent)
		{
		}

		static string GreaterThanZeroMessage
		{
			get { return Res.GetString("ff965a40-ae93-4ebc-95f2-8fdce9dc426c", "Amount must be greater than 0."); }
		}
		static string AmountPositiveMessage
		{
			get { return Res.GetString("f13fc806-67a4-43af-995a-bd31af11a93e", "Amount must be a positive value."); }
		}
		static string ExchangeRatesPositiveMessage
		{
			get { return Res.GetString("9e629e27-006c-4f97-a617-97e511422832", "Exchange rates must be a positive value."); }
		}
		static string EnterValidBankAccountMessage
		{
			get { return Res.GetString("a19b472f-6a60-4a4a-826e-fd0e7275185f", "Please enter a valid bank account."); }
		}
		static string BankAccountsMustBeDifferenthMessage
		{
			get { return Res.GetString("45be1fe5-da27-4a45-b757-62e8d6465007", "From / To bank accounts must be different."); }
		}

		new BankTransferRow Parent
		{
			get { return (BankTransferRow)base.Parent; }
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			MandatoryValidation.CheckEntered(Parent.AH_ABInfo);

			if (!Parent.AH_ABInfo.HasErrors())
			{
				if (!Parent.AH_AB.IsValid || Parent.BankAccount == null)
				{
					Parent.AH_ABInfo.AddError(EnterValidBankAccountMessage);
				}
				else if (Parent.BankAccount.AB_GB.IsValid && Parent.BankAccount.AB_GB != GlbBranch.CurrentBranch.PK)
				{
					Parent.AH_ABInfo.AddWarning(Res.GetString("97af6135-cf9a-4e5e-9398-d92bd8d1acc8", "Selected Bank account belongs to ({0}) Branch.", Parent.BankAccount.Branch.GB_Code));
				}
				else if (Parent.BankTransferParent != null && Parent.BankTransferParent.BankTransferFromPK == Parent.BankTransferParent.BankTransferToPK)
				{
					Parent.AH_ABInfo.AddError(BankAccountsMustBeDifferenthMessage);
				}
			}
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			if (Parent.AH_OSExTaxAmount == 0m)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(GreaterThanZeroMessage);
			}
			else if (Parent.AH_OSExTaxAmount < 0m)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(AmountPositiveMessage);
			}
		}

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();
			if (Parent.AH_ExchangeRate <= 0m)
			{
				Parent.AH_ExchangeRateInfo.AddError(ExchangeRatesPositiveMessage);
			}
		}

		protected override void CheckAH_LocalExTaxAmount()
		{
			base.CheckAH_LocalExTaxAmount();
			if (Parent.AH_LocalExTaxAmount == 0m)
			{
				Parent.AH_LocalExTaxAmountInfo.AddError(GreaterThanZeroMessage);
			}
			else if (Parent.AH_LocalExTaxAmount < 0m)
			{
				Parent.AH_LocalExTaxAmountInfo.AddError(AmountPositiveMessage);
			}
		}
	}
}
