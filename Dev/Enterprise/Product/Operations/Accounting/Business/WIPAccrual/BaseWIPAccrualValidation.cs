using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	/// <summary>
	/// Summary description for BaseWIPAccrualValidation.
	/// </summary>
	public class BaseWIPAccrualValidation : TransactionLineValidation
	{
		public BaseWIPAccrualValidation(BaseWIPAccrual parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region CheckAL_JH
		protected override void CheckAL_JH()
		{
			base.CheckAL_JH();
			MandatoryValidation.CheckEntered(Parent.AL_JHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AL_JHInfo, Parent.JobCollection);
		}
		#endregion

		#region CheckAL_AC
		protected override void CheckAL_AC()
		{
			base.CheckAL_AC();
			MandatoryValidation.CheckEntered(Parent.AL_ACInfo);
		}
		#endregion

		#region CheckAL_RX
		protected override void CheckAL_RX_NKTransactionCurrency()
		{
			base.CheckAL_RX_NKTransactionCurrency();
			MandatoryValidation.CheckEntered(Parent.AL_RX_NKTransactionCurrencyInfo);
		}
		#endregion

		#region CheckAL_PostDate
		protected override void CheckAL_PostDate()
		{
			base.CheckAL_PostDate();
			//Don't validate PostDate if an existing transaction, to prevent errors on old transactions
			//TODO: Need a more generic solution.
			if (!Parent.IsReversing && !Parent.IsInDatabase)
			{
				if (Parent.AL_PostDate.IsValid)
				{
					if (Parent.AL_PostDate.Date > ZDateTime.Today.Date)
					{
						Parent.AL_PostDateInfo.AddError(PostDateMustBeAtOrBeforeTodaysDate);
					}
				}
			}
		}
		#endregion

		public sealed override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			ValidateAllCore();
		}

		protected virtual void ValidateAllCore()
		{
			base.ValidateAll();
		}

		protected new BaseWIPAccrual Parent;
	}
}

