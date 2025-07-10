using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class BaseWIPAccrualReverseValidation : TransactionLineEmptyValidation
	{
		public BaseWIPAccrualReverseValidation(BaseWIPAccrual parent)
			: base(parent)
		{
		}

		BaseWIPAccrual BaseWIPAccrual
		{
			get
			{
				return (BaseWIPAccrual)base.Parent;
			}
		}

		#region Validation Providers

		PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = GetPeriodValidationProvider();
				}
				return fPeriodValidation;
			}
		}

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Parent.Factory);
		}

		PeriodValidationProvider fPeriodValidation;

		#endregion

		protected override void CheckAL_ReverseDate()
		{
			base.CheckAL_ReverseDate();
			if (!((BaseWIPAccrual)Parent).CanReverseWhenRelatedJobStatusIsJFC)
			{
				Parent.AL_ReverseDateInfo.AddError(Res.GetString("2DE032B7-A1ED-44E3-970F-313CF2C1F02D", "This transaction cannot be reversed as the related job has Ready For Financial Closure status."));
			}
			if (BaseWIPAccrual.AL_ReverseDate.IsValid && BaseWIPAccrual.AL_ReverseDateInfo.OriginalValue.IsValid)
			{
				Parent.AL_ReverseDateInfo.AddError(Res.GetString("dcdf40e2-d9a5-499c-874d-acfb3a0882d2", "This transaction has already been reversed."));
			}
			MandatoryValidation.CheckEntered(BaseWIPAccrual.AL_ReverseDateInfo);
			PeriodValidation.CheckDateFallsIntoValidPeriod(BaseWIPAccrual.AL_ReverseDateInfo);
			if (BaseWIPAccrual.AL_ReverseDate.IsValid && BaseWIPAccrual.AL_PostDate.IsValid)
			{
				if (BaseWIPAccrual.AL_PostDate.Date > ZDateTime.Now.Date)
				{
					if (BaseWIPAccrual.AL_ReverseDate > BaseWIPAccrual.AL_PostDate)
					{
						BaseWIPAccrual.AL_ReverseDateInfo.AddError(ReverseDateMustBeAtOrBeforePostDateWhenFuturePostingError);
					}
				}
				else if (BaseWIPAccrual.AL_PostDate.Date <= ZDateTime.Now.Date)
				{
					if (BaseWIPAccrual.AL_ReverseDate.Date > ZDateTime.Now.Date && BaseWIPAccrual.AL_ReverseDate != BaseWIPAccrual.AL_ReverseDate_FutureSystemCalculatedValue)
					{
						var message = ReverseDateMustBeAtOrBeforeTodaysDateWithCurrentOrBackPostingError;
						if (BaseWIPAccrual.AL_ReverseDate_FutureSystemCalculatedValue != DateTime.MinValue)
						{
							message = GetReverseDateMustBeAtOrBeforeTodaysDateOrOnRevenueRecognitionDatePostingError(BaseWIPAccrual.AL_ReverseDate_FutureSystemCalculatedValue.ToDateTime());
						}
						BaseWIPAccrual.AL_ReverseDateInfo.AddError(message);
					}
				}
			}
		}

		ZString ReverseDateMustBeAtOrBeforePostDateWhenFuturePostingError
		{
			get { return Res.GetString("dfb397b2-9456-4d07-a4e4-16483a3308ed", "This transaction has been posted into the future. You can only reverse this transaction up to its post date."); }
		}

		ZString ReverseDateMustBeAtOrBeforeTodaysDateWithCurrentOrBackPostingError
		{
			get { return Res.GetString("d89b0371-e809-446c-8a5f-c08195af0a25", "You can only reverse this transaction up to today's date"); }
		}

		ZString GetReverseDateMustBeAtOrBeforeTodaysDateOrOnRevenueRecognitionDatePostingError(ZDateTime revenueRecognitionDate)
		{
			return Res.GetString("ab78c5a5-e239-4180-b805-3be9dd13fd6e", "You can only reverse this transaction up to today's date or use Revenue Recognition date - {0}", revenueRecognitionDate.ToShortDateString());
		}
	}
}

