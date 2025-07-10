using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public partial class DirectPaymentValidation : DirectTransactionHeaderBaseValidation
	{
		public DirectPaymentValidation(DirectPayment parent)
			: base(parent)
		{
		}

		new DirectPayment Parent
		{
			get { return (DirectPayment)base.Parent; }
		}

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			if (!Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, Parent.PaymentMethods);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, PaymentMethods);
			}

			ValidateAH_OSTotalAmount();
		}

		protected override void CheckAH_DrawerBank()
		{
			//Account Number
			base.CheckAH_DrawerBank();

			if (!Parent.AH_DrawerBankInfo.HasErrors() && ShouldCheckDrawerBankIsValid)
			{
				if (!Parent.BankAccount.IsValidAccountNumber(Parent.AH_DrawerBank))
				{
					Parent.AH_DrawerBankInfo.AddError(Parent.BankAccount.GetInvalidAccountNumberErrorMessage());
				}
			}
		}

		bool ShouldCheckDrawerBankIsValid
		{
			get { return (!Parent.AH_DrawerBankInfo.ReadOnly && Parent.BankAccount != null); }
		}

		protected override void CheckAH_DrawerBranch()
		{
			//BSB Number
			base.CheckAH_DrawerBranch();

			if (!Parent.AH_DrawerBranchInfo.HasErrors() && ShouldCheckDrawerBranchIsValid)
			{
				if (!Parent.BankAccount.IsValidBSBNumber(Parent.AH_DrawerBranch))
				{
					Parent.AH_DrawerBranchInfo.AddError(Parent.BankAccount.GetInvalidBSBNumberErrorMessage());
				}
			}
		}

		bool ShouldCheckDrawerBranchIsValid
		{
			get { return (!Parent.AH_DrawerBranchInfo.ReadOnly && Parent.BankAccount != null); }
		}

		protected override void CheckAH_ChequeOrReference()
		{
			if (!((IChequeNumberAutoAllocation)Parent).IsAutoAllocationEnabled)
			{
				base.CheckAH_ChequeOrReference();

				if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.ChequeBook != null && Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque &&
					!Parent.AH_ReceiptTypeInfo.HasErrors())
				{
					string chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(Parent.ChequeBook, Parent.AH_ChequeOrReference);
					if (!string.IsNullOrEmpty(chequeNumberErrorMessage))
					{
						Parent.AH_ChequeOrReferenceInfo.AddError(chequeNumberErrorMessage);
					}
					else
					{
						if (IsChequeNumberInUse)
						{
							Parent.AH_ChequeOrReferenceInfo.AddError(ChequeOrReferenceValidationHelper.GetInUseErrorMessage(Parent.AH_ChequeOrReference, Parent.ChequeBook.BankAccount, Parent.Factory));
						}
						else if (Parent.BankAccount != null)
						{
							ValidationHelper.ValidateChequeDigits(Parent.AH_ChequeOrReferenceInfo, Parent.BankAccount.AB_ChequeNumDigits);
						}
					}
				}
			}
		}

		protected override void CheckChequeBookPK()
		{
			base.CheckChequeBookPK();
			if (Parent.ChequeBook != null)
			{
				ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(Parent.ChequeBook, ((IChequeNumberAutoAllocation)Parent).IsAutoAllocationEnabled);
				if (!errorMessage.IsEmpty)
				{
					Parent.ChequeBookPKInfo.AddError(errorMessage);
				}
				Parent.ChequeBook.AddWarningSamePrinter(Parent.ChequeBookPKInfo);

				if (Parent.ChequeBook.AK_AutoPrintCheque)
				{
					if (Parent.ChequeBook.BankAccount.AB_SO_ChequeTemplate.IsEmpty)
					{
						Parent.ChequeBookPKInfo.AddError(Res.GetString("035795A7-DAE8-42F7-A571-FA59152A08E1", "Auto printing of check is not configured properly."));
					}
					if (!Env.Security.PrintCheque.IsAllowed)
					{
						Parent.ChequeBookPKInfo.AddError(Res.GetString("FCF29EDC-8324-4309-A8B4-2290BEED44B3", "You do not have the permission to print Check. Please Contact System Administrator."));
					}
				}
			}
		}

		protected override void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
			if (Parent.AH_ReceiptType == ReceiptTypes.Cheque && Parent.AH_OSTotalAmount == 0)
			{
				if (Parent.Lines.Count > 0)
				{
					Parent.AH_OSTotalAmountInfo.AddError(Res.GetString("80e60541-e9d5-44d2-8788-b2b963c7816b", "Total cannot be zero. If you want to create this transaction to record a canceled check, you should remove the transaction detail."));
				}
				else
				{
					Parent.AH_OSTotalAmountInfo.AddWarning(Res.GetString("e79f646c-6429-4f46-a898-4243b0219e00", "You are posting a payment with a payment type of 'CHQ' with zero value. This functionality is typically used to record canceled check numbers."));
				}
			}
			else
			{
				base.CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines();
			}
		}

		#region DirectDebit Batch Validation Implementation

		public void ValidateAllowAutoDDR()
		{
			ValidateCalculatedProperty(Parent.AllowAutoDDRInfo);
		}

		protected void CheckAllowAutoDDR()
		{
			if (!Parent.AllowAutoDDR)
			{
				Parent.AllowAutoDDRInfo.AddError(Res.GetString("2eb956ff-7cbb-446d-b616-3b0c04363c2d", "You cannot generate DDR file for non-Auto DDR Payments"));
			}
		}

		public void ValidatePayeeBankBSB()
		{
			ValidateCalculatedProperty(Parent.PayeeBankBSBInfo);
		}

		protected void CheckPayeeBankBSB()
		{
			ZString error = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(Parent);
			if (!error.IsEmpty)
			{
				Parent.PayeeBankBSBInfo.AddError(error);
			}
		}

		public void ValidatePayeeBankAccountNumber()
		{
			ValidateCalculatedProperty(Parent.PayeeBankAccountNumberInfo);
		}

		protected void CheckPayeeBankAccountNumber()
		{
			ZString error = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(Parent);
			if (!error.IsEmpty)
			{
				Parent.PayeeBankAccountNumberInfo.AddError(error);
			}
		}

		#region Lookup

		CodeDescriptionPairList fPaymentMethods;
		protected CodeDescriptionPairList PaymentMethods
		{
			get
			{
				if (fPaymentMethods == null)
				{
					fPaymentMethods = Parent.PaymentMethods;
					fPaymentMethods.AddPair(ReceiptTypes.DirectDebitLine, Res.GetString("5831434a-048b-472d-8511-2b409ab301d9", "Direct Debit Line"));
				}
				return fPaymentMethods;
			}
		}

		#endregion

		#endregion

		#region Implementation

		AccChequeBookAutoAllocationValidation AutoAllocationValidation
		{
			get
			{
				if (fAutoAllocationValidation == null)
				{
					fAutoAllocationValidation = new AccChequeBookAutoAllocationValidation();
				}
				return fAutoAllocationValidation;
			}
		}
		AccChequeBookAutoAllocationValidation fAutoAllocationValidation;

		bool IsChequeNumberInUse
		{
			get
			{
				bool result = false;

				if (Parent.ChequeBook != null)
				{
					if (Parent.ChequeBook.BankAccount != null)
					{
						result = Parent.ChequeBook.BankAccount.HasChequeNumberBeenUsed(Parent.AH_ChequeOrReference, Parent.PK);
					}
				}
				return result;
			}
		}

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

		#endregion
	}
}
