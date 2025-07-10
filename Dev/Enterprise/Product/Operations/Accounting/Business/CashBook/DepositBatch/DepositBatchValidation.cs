using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchValidation : TransactionHeaderValidation
	{
		public DepositBatchValidation(DepositBatch parent)
			: base(parent)
		{
		}

		new DepositBatch Parent
		{
			get { return (DepositBatch)base.Parent; }
		}

		protected override bool ShouldValidateBranchDepartmentCombination
		{
			get
			{
				return false;
			}
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

		protected override void CheckAH_PostDateNotInPast()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.AH_PostDate.Date < ZDateTime.Today)
				{
					if (!Parent.AllowBackPosting)
					{
						Parent.AH_PostDateInfo.AddError(Res.GetString("A30773AF-A010-4431-9EF7-62FE81AFF455", "The deposit post date cannot be in the past"));
					}
					else
					{
						Parent.AH_PostDateInfo.AddWarning(PreviousPostDateWarning);
					}
				}
			}
		}
	}
}