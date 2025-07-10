using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract class DirectTransactionLineBaseValidation : DependentTransactionLineValidation
	{
		public DirectTransactionLineBaseValidation(DirectTransactionLineBase parent)
			: base(parent)
		{
		}

		new DirectTransactionLineBase Parent
		{
			get { return (DirectTransactionLineBase)base.Parent; }
		}

		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
		}

		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();

			MandatoryValidation.CheckEntered(Parent.AL_AGInfo);

			if (Parent.GLHeader != null)
			{
				if ((Parent.GLHeader.AG_AccountType != AccountTypeComboBoxConstants.BalanceSheetAccount)
					&& (Parent.GLHeader.AG_AccountType != AccountTypeComboBoxConstants.ProfitAndLossAccount))
				{
					Parent.AL_AGInfo.AddError(InvalidGLAccountError);
				}

				if (!Parent.GLHeader.AG_IsActive)
				{
					Parent.AL_AGInfo.AddError(InactiveGLAccountError);
				}

				if (Parent.GLHeader.AG_ControlAccount &&
					(Parent.TransactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment ||
					Parent.TransactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectReceipt))
				{
					Parent.AL_AGInfo.AddError(Res.GetString("b0e0f0b0-3270-4e18-8c1d-6ad57275e74c", "You cannot post to control accounts from this screen"));
				}
			}
		}

		protected override void CheckAL_AT()
		{
			base.CheckAL_AT();

			if (Parent.AL_AT.IsEmpty && GlbCompany.CurrentCompany.GC_IsGSTRegistered && !Parent.AL_ATInfo.ReadOnly && !Parent.IsCommentCharge)
			{
				MandatoryValidation.CheckEntered(Parent.AL_ATInfo);
				ListValidation.ErrorIfInvalidPK(Parent.AL_ATInfo, Parent.Lookups.TaxRates);
			}

			if (Parent.TaxRate != null && Parent.TaxRate.IsReverseCharge)
			{
				Parent.AL_ATInfo.AddError(Res.GetString("52c9aa80-d973-4b5e-aa16-b398fa2eae40", "Reverse tax rates cannot be used in cashbook transactions"));
			}
			if (Parent.TaxRate != null && Parent.TaxRate.IsSuspendedCharge)
			{
				Parent.AL_ATInfo.AddError(Res.GetString("edc1c8fb-adf5-4625-9c27-18511dae3bb9", "Suspended tax rates cannot be used in cashbook transactions"));
			}
			if (Parent.TaxRate != null && Parent.TaxRate.IsVATWithholding)
			{
				Parent.AL_ATInfo.AddError(Res.GetString("2ea7e76d-1447-4bd3-b3a4-fcbfaa12ceda", "VAT Withholding tax rates cannot be used in cashbook transactions"));
			}
		}

		protected override void CheckAL_OSExTaxAmount()
		{
			MandatoryValidation.CheckNotZero(Parent.AL_OSExTaxAmountInfo);
			TypeValidation.CheckValidDecimal(Parent.AL_OSExTaxAmountInfo, 19, 4);
		}

		protected override bool ShouldValidateNoTaxMessage => true;
	}
}
