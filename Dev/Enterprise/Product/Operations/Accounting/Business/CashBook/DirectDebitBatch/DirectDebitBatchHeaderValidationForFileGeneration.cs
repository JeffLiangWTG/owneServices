using Enterprise.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	class DirectDebitBatchHeaderValidationForFileGeneration : DirectDebitBatchHeaderValidation
	{
		public DirectDebitBatchHeaderValidationForFileGeneration(DirectDebitBatchHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			ValidateBank();
		}

		void ValidateBank()
		{
			if (Parent.BankAccount != null)
			{
				if (!Parent.BankAccount.AB_AllowAutoDDR)
				{
					Parent.AH_ABInfo.AddError(Res.GetString("111154bf-4140-4b9b-afd5-5c2d023a60aa", "You cannot create DDR file for non-auto DDR Bank. Please check the Bank Master file."));
				}

				if (Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ ||
						 Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.NAB ||
						 Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.CBA ||
						 Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.WBC)
				{
					if (Parent.BankAccount.AB_AccountNum.Length > 9)
					{
						Parent.AH_ABInfo.AddError(Res.GetString("1ef8bee8-5de3-4404-950b-b235cc3ba1a7", "The Bank Account Number setup must be nine characters or less"));
					}
				}
				else if (Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.ASB)
				{
					if (Parent.BankAccount.AB_AccountNum.Length != 9)
					{
						Parent.AH_ABInfo.AddError(Res.GetString("e6eb4196-c16a-4fb0-b2c7-71d1ffca8751", "The Bank Account Number setup must be nine characters in length"));
					}
				}
				else if (Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ)
				{
					if (Parent.BankAccount.AB_AccountNum.Length > 12)
					{
						Parent.AH_ABInfo.AddError(Res.GetString("fb06e407-f697-4c6e-8d11-fa30928f76d6", "The Bank Account Number setup must be less than or equal to 12 characters in length"));
					}
				}

				if (Parent.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.WBC)
				{
					if (Parent.BankAccount.AB_AccountEFTUserID.Length > 6)
					{
						Parent.AH_ABInfo.AddError(Res.GetString("b8a39cbf-1b8a-46e6-a132-d08ecfd9bfe4", "The Bank Account EFT User ID must be less than or equal to 6 characters in length"));
					}
				}
			}
		}
	}
}