using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment
{
	public class OpeningPaymentValidation : TransactionHeaderValidation
	{
		public OpeningPaymentValidation(OpeningPayment parent)
			: base(parent)
		{
		}

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
		}

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			MandatoryValidation.CheckEntered(Parent.AH_ReceiptTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, Parent.PaymentMethods);
			if (!Parent.AH_ReceiptTypeInfo.HasErrors() && !Parent.IsCashReceiptType && Parent.IsCashAccountType)
			{
				Parent.AH_ReceiptTypeInfo.AddError(GetCashAccountTypeErrorMessage(Parent.AH_ReceiptTypeInfo.HumanReadableName));
			}
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			MandatoryValidation.CheckEntered(Parent.AH_ABInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_ABInfo, Parent.Lookups.BankAccounts);
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			MandatoryValidation.CheckEntered(Parent.AH_OSExTaxAmountInfo);
			if (Parent.AH_OSExTaxAmount < 0)
			{
				Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("e7152f47-f32a-4103-aedc-245a07478ad1", "Amount cannot be negative"));
			}
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_ChequeOrReference()
		{
			base.CheckAH_ChequeOrReference();
			MandatoryValidation.CheckEntered(Parent.AH_ChequeOrReferenceInfo);
			if (!Parent.AH_ChequeOrReferenceInfo.HasErrors() && Parent.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, Parent.AH_ChequeOrReference);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.AH_ChequeOrReferenceInfo.AddError(errorMessage);
				}
			}
		}
	}
}
