using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ReceiptPaymentBaseValidation : TransactionHeaderValidation
	{
		public ReceiptPaymentBaseValidation(ReceiptPaymentBase parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			ListValidation.ErrorIfInvalidPK(Parent.AH_ABInfo);
			MandatoryValidation.CheckEntered(Parent.AH_ABInfo, Res.GetString("4357c0b2-c542-42d0-a99b-c5adc87b821b", "Bank"));

			if (BankCurrencyShouldMatchTransactionCurrency && Parent.BankAccount.AB_RX_NKAccountCurrency != Parent.AH_RX_NKTransactionCurrency)
			{
				Parent.AH_ABInfo.AddError(Res.GetString("EA2BBAC5-847E-43CC-8CCB-B34A902729C3", "Bank account currency does not match the {0} currency.", Parent.HumanReadableName));
			}
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			ValidateAH_AB();
		}

		bool BankCurrencyShouldMatchTransactionCurrency
		{
			get
			{
				return Parent.BankAccount != null && Parent.BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			if (Parent.AH_Desc.IsEmpty)
			{
				Parent.AH_DescInfo.AddError(Res.GetString("095cd933-196b-43ee-8a37-72ea385b0be8", "You must enter a description"));
			}
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			MandatoryValidation.CheckEntered(Parent.AH_OSExTaxAmountInfo, Res.GetString("df3bf6a6-49f2-4b03-aa1a-f352ecc25df1", "Amount Received"));
			if (Parent.AH_OSExTaxAmount < 0)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("b129eac0-d2fb-4bd0-9f6f-33d06db8a673", "Overseas amount must be greater than 0"));
			}
		}

		protected override void CheckAH_ChequeOrReference()
		{
			base.CheckAH_ChequeOrReference();

			MandatoryValidation.CheckEntered(Parent.AH_ChequeOrReferenceInfo, (Parent.AH_ReceiptType == ReceiptTypes.Cheque ? Res.GetString("4aac4932-087b-4fde-a028-0132fa9d65b6", "Check Number") : Res.GetString("12848afb-5998-488a-82ea-263408e6ddb3", "Reference Number")));

			if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, Parent.AH_ChequeOrReference);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.AH_ChequeOrReferenceInfo.AddError(errorMessage);
				}
				else if (Parent.BankAccount != null && Parent.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment)
				{
					ValidationHelper.ValidateChequeDigits(Parent.AH_ChequeOrReferenceInfo, Parent.BankAccount.AB_ChequeNumDigits);
				}
			}
		}

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
		}

		protected override void CheckAH_PostDateNotInFuture()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.AH_PostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						Parent.AH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						Parent.AH_PostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		protected new ReceiptPaymentBase Parent;

		AccValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccValidationHelper();
				}
				return fValidationHelper;
			}
		}
		AccValidationHelper fValidationHelper;
	}
}
