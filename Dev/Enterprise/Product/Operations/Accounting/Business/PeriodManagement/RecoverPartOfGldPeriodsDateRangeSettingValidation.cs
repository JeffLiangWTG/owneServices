using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class RecoverPartOfGldPeriodsDateRangeSettingValidation : GldDateRangeSettingValidation
	{
		public RecoverPartOfGldPeriodsDateRangeSettingValidation(RecoverPartOfGldPeriodsDateRangeSetting parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected override void CheckStartDate()
		{
			base.CheckStartDate();
			MandatoryValidation.CheckEntered(Parent.StartDateInfo, Res.GetString("16F41CFF-64C5-4D0D-B8C1-5F4A9EBCE92F", "Start Date"));

			if (Parent.StartDateInfo.HasErrors())
			{
				return;
			}

			if (Parent.StartDate < AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value)
			{
				Parent.StartDateInfo.AddError(Res.GetString("9DC60FDD-3E56-4895-BCD0-8D8104B628C6", "Start Date cannot be set to before General Ledger Data Creation Date."));
			}
			else if (PeriodCalculator.GetPeriodManagementFromDate(Parent.StartDate) == null)
			{
				Parent.StartDateInfo.AddError(Res.GetString("7C7A46C2-467A-4B72-84E3-CB63B9068922", "Start Date must be in a Financial Period."));
			}
		}

		protected override void CheckEndDate()
		{
			base.CheckEndDate();
			MandatoryValidation.CheckEntered(Parent.EndDateInfo, Res.GetString("DAF96036-A5F2-4D80-9DD1-D6BA27983B7F", "End Date"));

			if (Parent.EndDateInfo.HasErrors())
			{
				return;
			}

			if (Parent.EndDate < Parent.StartDate)
			{
				Parent.EndDateInfo.AddError(Res.GetString("3969E14F-DA34-455E-8A16-56FF7B3988A6", "End Date cannot be earlier than Start Date."));
			}
			else if (!ValidateStartDateAndEndDateInSamePeriod())
			{
				Parent.EndDateInfo.AddError(Res.GetString("D31E626B-54FC-405A-85E6-098FB2FF343A", "End Date must lie in same Accounting Period as entered Start Date."));
			}
		}

		bool ValidateStartDateAndEndDateInSamePeriod()
		{
			if (Parent.StartDateInfo.HasErrors() || Parent.EndDateInfo.HasErrors())
			{
				return true;
			}

			var startDatePeriodManagement = PeriodCalculator.GetPeriodManagementFromDate(Parent.StartDate);
			var endDatePeriodManagement = PeriodCalculator.GetPeriodManagementFromDate(Parent.EndDate);
			if (startDatePeriodManagement == null || endDatePeriodManagement == null
				|| startDatePeriodManagement.AM_Period != endDatePeriodManagement.AM_Period)
			{
				return false;
			}

			return true;
		}

		public RecoverPartOfGldPeriodsDateRangeSetting Parent { get; }
	}
}