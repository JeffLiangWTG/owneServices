using CargoWise.EntityFramework;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class PeriodClosureConfigurationValidation
	{
		public PeriodClosureConfigurationValidation(PeriodClosureConfiguration parent)
		{
			Parent = parent;
		}

		readonly PeriodClosureConfiguration Parent;

		public bool IsExistZero => Parent.SubLedgerInterval == 0 || Parent.GeneralLedgerInterval == 0 || Parent.AdjustmentLedgerInterval == 0;

		public void ValidateInterval()
		{
			Parent.SubLedgerIntervalInfo.ClearAllNotifications();
			Parent.GeneralLedgerIntervalInfo.ClearAllNotifications();
			Parent.AdjustmentLedgerIntervalInfo.ClearAllNotifications();

			bool isValidWhenExistZero = (Parent.SubLedgerInterval == 0 && Parent.GeneralLedgerInterval == 0 && Parent.AdjustmentLedgerInterval == 0)
										|| (Parent.SubLedgerInterval != 0 && Parent.GeneralLedgerInterval == 0 && Parent.AdjustmentLedgerInterval == 0)
										|| (Parent.SubLedgerInterval != 0 && Parent.GeneralLedgerInterval != 0 && Parent.AdjustmentLedgerInterval == 0 && Parent.SubLedgerInterval <= Parent.GeneralLedgerInterval);

			var errorMessageForNoneZeroSubLedgerInterval = Res.GetString("203a2d04-6f8e-4248-a37e-ec69d0a9f8a5", "The 'interval' value of sub ledger type cannot be 0 if the 'interval' value of general ledger type is greater than 0");
			var errorMessageForNoneZeroGeneralLedgerInterval = Res.GetString("0803c4ca-3b81-4c37-b01a-69b01edabd21", "The 'interval' value of general ledger type cannot be 0 if the 'interval' value of adjustment ledger type is greater than 0");

			var errorMessageForGeneralLedgerIntervalSize = Res.GetString("6f03d785-d8b6-4c7c-9fc7-84a892bd2a76", "The 'interval' value for general ledger type must be the same or greater than the sub ledger type");
			var errorMessageForAdjustmentLedgerIntervalSize = Res.GetString("142f77c5-517b-4b1b-9d81-3f0985dba75b", "The 'interval' value for adjustment ledger type must be the same or greater than the general ledger type");

			if (Parent.SubLedgerInterval == 0 && Parent.GeneralLedgerInterval != 0)
			{
				Parent.SubLedgerIntervalInfo.AddError(errorMessageForNoneZeroSubLedgerInterval);
			}
			else if (Parent.GeneralLedgerInterval == 0 && Parent.AdjustmentLedgerInterval != 0)
			{
				Parent.GeneralLedgerIntervalInfo.AddError(errorMessageForNoneZeroGeneralLedgerInterval);
			}
			else if (!isValidWhenExistZero && Parent.SubLedgerInterval > Parent.GeneralLedgerInterval)
			{
				Parent.GeneralLedgerIntervalInfo.AddError(errorMessageForGeneralLedgerIntervalSize);
			}
			else if (!isValidWhenExistZero && Parent.GeneralLedgerInterval > Parent.AdjustmentLedgerInterval)
			{
				Parent.AdjustmentLedgerIntervalInfo.AddError(errorMessageForAdjustmentLedgerIntervalSize);
			}
		}

		public void ValidateSubLedgerInterval()
		{
			ValidateInterval();
		}

		public void ValidateGeneralLedgerInterval()
		{
			ValidateInterval();
		}

		public void ValidateAdjustmentLedgerInterval()
		{
			ValidateInterval();
		}

		public void ValidateIntervalType()
		{
			Parent.IntervalTypeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.IntervalTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IntervalTypeInfo, Parent.IntervalTypes);
		}
	}
}
