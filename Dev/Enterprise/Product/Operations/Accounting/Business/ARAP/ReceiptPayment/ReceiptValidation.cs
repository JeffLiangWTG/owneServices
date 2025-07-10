using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ReceiptValidation : ReceiptPaymentBaseValidation
	{
		public ReceiptValidation(Receipt parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region Overrides

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			MandatoryValidation.CheckEntered(Parent.AH_ReceiptTypeInfo, Res.GetString("3b91065e-92a5-4d90-bcfe-858582b45f6a", "Receipt Type"));
			ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, Parent.ReceiptMethods);

			if (!Parent.AH_ReceiptTypeInfo.HasErrors())
			{
				if (Parent.IsCashAccountType && !Parent.IsCashReceiptType)
				{
					Parent.AH_ReceiptTypeInfo.AddError(TransactionHeaderValidation.GetCashAccountTypeErrorMessage(Parent.AH_ReceiptTypeInfo.HumanReadableName));
				}
				else
				{
					CheckAH_ReceiptTypeSecurity();
				}
			}
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			if (!Parent.IsPostWithMatching)
			{
				if (Parent.AH_OSExTaxAmount <= 0)
				{
					Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("1993D9AE-73B0-44e9-9606-A74AD3C54036", "Overseas amount must be greater than zero"));
				}
			}

			else
			{
				if (Parent.AH_OSExTaxAmount < 0)
				{
					Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("64C4FBCB-06F8-4347-B12D-AA842DDAE5B0", "Overseas amount must be greater than or equal to zero"));
				}
			}
		}

		protected override void CheckAH_LocalExTaxAmount()
		{
			base.CheckAH_LocalExTaxAmount();
			if (!Parent.IsPostWithMatching && Parent.AH_LocalExTaxAmount == 0)
			{
				Parent.AH_LocalExTaxAmountInfo.AddError(Res.GetString("ed53d0c6-eaf7-40ae-aa4c-e5785c0dea85", "Must be not 0"));
			}
		}

		void CheckAH_ReceiptTypeSecurity()
		{
			bool isAllowed = true;
			switch (Parent.AH_ReceiptType)
			{
				case ReceiptTypes.Cheque:
					isAllowed = (Parent is ARReceipt) ? Env.Security.NewReceivablesReceiptCheque.IsAllowed : Env.Security.NewPayablesReceiptCheque.IsAllowed;
					break;

				case ReceiptTypes.Cash:
					isAllowed = (Parent is ARReceipt) ? Env.Security.NewReceivablesReceiptCash.IsAllowed : Env.Security.NewPayablesReceiptCash.IsAllowed;
					break;

				case ReceiptTypes.CreditCard:
					isAllowed = (Parent is ARReceipt) ? Env.Security.NewReceivablesReceiptCreditCard.IsAllowed : Env.Security.NewPayablesReceiptCreditCard.IsAllowed;
					break;

				case ReceiptTypes.DirectCredit:
					isAllowed = (Parent is ARReceipt) ? Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed : Env.Security.NewPayablesReceiptDirectCredit.IsAllowed;
					break;
			}

			if (!isAllowed)
			{
				Parent.AH_ReceiptTypeInfo.AddError(Res.GetString("1a093076-adb7-4b2c-bb5c-186af1ebea98", "You do not have appropriate security rights to select this receipt type."));
			}
		}

		protected override void CheckAH_ChequeDrawer()
		{
			base.CheckAH_ChequeDrawer();
			if (Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				MandatoryValidation.CheckEntered(Parent.AH_ChequeDrawerInfo);
			}
		}

		protected override void CheckAH_DrawerBranch()
		{
			base.CheckAH_DrawerBranch();
			if (Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				MandatoryValidation.CheckEntered(Parent.AH_DrawerBranchInfo);
			}
		}

		protected override void CheckAH_DrawerBank()
		{
			base.CheckAH_DrawerBank();
			if (Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				MandatoryValidation.CheckEntered(Parent.AH_DrawerBankInfo);
			}
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			if (Parent.BankAccount != null && !Parent.BankAccount.AB_IsActive)
			{
				Parent.AH_ABInfo.AddError(Res.GetString("2E6FF4CA-C3B0-4427-8D58-75A845522A1E", "This Bank Account is inactive - it may not be used."));
			}
		}

		protected new Receipt Parent;

		#endregion
	}
}
