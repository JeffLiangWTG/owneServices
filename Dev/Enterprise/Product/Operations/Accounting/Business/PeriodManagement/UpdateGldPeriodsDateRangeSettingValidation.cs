namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class UpdateGldPeriodsDateRangeSettingValidation : GldDateRangeSettingValidation
	{
		public UpdateGldPeriodsDateRangeSettingValidation(UpdateGldPeriodsDateRangeSetting parent)
			: base(parent)
		{
			Parent = parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CompareStartDateAndEndDate();
		}

		void CompareStartDateAndEndDate()
		{
			if (Parent.StartDate.IsEmpty || Parent.EndDate.IsEmpty)
			{
				return;
			}

			if (Parent.EndDate <= Parent.StartDate)
			{
				var errorMsg = Res.GetString("0CD1AA2C-C516-47B9-A3FB-02C697C2CFEE", "End Date specified must be greater than the 'Start Date'.");
				Parent.StartDateInfo.AddError(errorMsg);
				Parent.EndDateInfo.AddError(errorMsg);
			}

			var startDatePeriodManagement = PeriodCalculator.GetPeriodManagementFromDate(Parent.StartDate);
			var endDatePeriodManagement = PeriodCalculator.GetPeriodManagementFromDate(Parent.EndDate);

			if (startDatePeriodManagement != null && endDatePeriodManagement != null && startDatePeriodManagement.AM_Year != endDatePeriodManagement.AM_Year)
			{
				var warningMsg = Res.GetString("7CD15952-37EA-4224-9443-9270FD53C462", "Start Date and End Date specified do not fall in the same accounting year. It may take a long time for the system to update the General Ledger Data Records.");
				Parent.StartDateInfo.AddWarning(warningMsg);
				Parent.EndDateInfo.AddWarning(warningMsg);
			}
		}

		protected override void CheckStartDate()
		{
			base.CheckStartDate();

			if (!Parent.StartDate.IsEmpty)
			{
				var firstDayForPeriod = PeriodCalculator.GetFirstDayForPeriod(Parent.StartDate);
				if (firstDayForPeriod.Date != Parent.StartDate.Date)
				{
					Parent.StartDateInfo.AddError(Res.GetString("BCA9773B-556C-43DF-8B00-4BA62A071D8C", "Start Date specified must be the 'Start Date' of an accounting period of the login company."));
				}
			}
			else
			{
				Parent.StartDateInfo.AddWarning(Res.GetString("A9743B70-C9D3-4A32-82F9-035D8CC39B2B", "Start Date is empty. It may take a long time for the system to update the General Ledger Data Records."));
			}
		}

		protected override void CheckEndDate()
		{
			base.CheckEndDate();

			if (!Parent.EndDate.IsEmpty)
			{
				var lastDayForPeriod = PeriodCalculator.GetLastDayForPeriod(Parent.EndDate);
				if (lastDayForPeriod.Date != Parent.EndDate.Date)
				{
					Parent.EndDateInfo.AddError(Res.GetString("D69F95EB-FE6A-4B0A-9244-56618B36465D", "End Date specified must be the 'End Date' of an accounting period of the login company."));
				}
			}
			else
			{
				Parent.EndDateInfo.AddWarning(Res.GetString("2A172F76-52EB-4AFC-8A2C-06012FFF517B", "End Date is empty. It may take a long time for the system to update the General Ledger Data Records."));
			}
		}

		UpdateGldPeriodsDateRangeSetting Parent { get; }
	}
}