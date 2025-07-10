using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract partial class DirectTransactionHeaderBaseValidation : TransactionHeaderWithLinesValidation
	{
		public static string ChequeNumberNumbericErrorMsg
		{
			get { return Res.GetString("6afac2c7-c3ca-4140-92ee-3feca8a5efe2", "Only numbers are allowed in this field."); }
		}

		protected DirectTransactionHeaderBaseValidation(DirectTransactionHeaderBase parent)
			: base(parent)
		{
		}

		new DirectTransactionHeaderBase Parent
		{
			get { return (DirectTransactionHeaderBase)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateChequeBookPK();
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

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			MandatoryValidation.CheckEntered(Parent.AH_ReceiptTypeInfo);
			if (!Parent.AH_ReceiptTypeInfo.HasErrors() && Parent.IsCashAccountType && !Parent.IsCashReceiptType)
			{
				Parent.AH_ReceiptTypeInfo.AddError(GetCashAccountTypeErrorMessage(Parent.AH_ReceiptTypeInfo.HumanReadableName));
			}

			ValidateAH_ChequeOrReference();
		}

		protected override void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
			base.CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines();

			MandatoryValidation.CheckNotNegative(Parent.AH_OSTotalAmountInfo, Res.GetString("6bb2b2ce-3c26-47be-8fe5-acae9fe5eb69", "Total"));
			MandatoryValidation.CheckNotZero(Parent.AH_OSTotalAmountInfo, Res.GetString("6bb2b2ce-3c26-47be-8fe5-acae9fe5eb69", "Total"));
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();

			ValidateAH_OSTotalAmount();
		}

		protected override void CheckAH_ChequeOrReference()
		{
			base.CheckAH_ChequeOrReference();
			MandatoryValidation.CheckEntered(Parent.AH_ChequeOrReferenceInfo, Parent.AH_ChequeOrReferenceLabel_Calc);

			if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, Parent.AH_ChequeOrReference);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.AH_ChequeOrReferenceInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			MandatoryValidation.CheckEntered(Parent.AH_ABInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_ABInfo, Parent.BankAccounts);
		}

		protected override void CheckAH_ChequeDrawer()
		{
			base.CheckAH_ChequeDrawer();

			if (ShouldCheckChequeDrawerEntered)
			{
				MandatoryValidation.CheckEntered(Parent.AH_ChequeDrawerInfo);
			}
		}

		bool ShouldCheckChequeDrawerEntered
		{
			get { return !Parent.AH_ChequeDrawerInfo.ReadOnly; }
		}

		protected override void CheckAH_DrawerBank()
		{
			base.CheckAH_DrawerBank();
			if (ShouldCheckDrawerBankEntered)
			{
				MandatoryValidation.CheckEntered(Parent.AH_DrawerBankInfo);
			}
		}

		bool ShouldCheckDrawerBankEntered
		{
			get { return !Parent.AH_DrawerBankInfo.ReadOnly; }
		}

		protected override void CheckAH_DrawerBranch()
		{
			base.CheckAH_DrawerBranch();
			if (ShouldCheckDrawerBranchEntered)
			{
				MandatoryValidation.CheckEntered(Parent.AH_DrawerBranchInfo);
			}
		}

		bool ShouldCheckDrawerBranchEntered
		{
			get { return !Parent.AH_DrawerBranchInfo.ReadOnly; }
		}

		public void ValidateChequeBookPK()
		{
			ValidateCalculatedProperty(Parent.ChequeBookPKInfo);
		}

		protected virtual void CheckChequeBookPK()
		{
			if (!Parent.ChequeBookPKInfo.ReadOnly && Parent.AH_AB.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.ChequeBookPKInfo);
				TypeValidation.CheckValidGuid(Parent.ChequeBookPKInfo);
			}
		}
	}
}
