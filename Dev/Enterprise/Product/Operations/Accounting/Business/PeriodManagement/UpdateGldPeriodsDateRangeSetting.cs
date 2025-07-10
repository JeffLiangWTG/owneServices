namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class UpdateGldPeriodsDateRangeSetting : GldDateRangeSetting
	{
		public override GldDateRangeSettingValidation GetNewValidation()
		{
			return new UpdateGldPeriodsDateRangeSettingValidation(this);
		}
	}
}