namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class RecoverPartOfGldPeriodsDateRangeSetting : GldDateRangeSetting
	{
		public override GldDateRangeSettingValidation GetNewValidation()
		{
			return new RecoverPartOfGldPeriodsDateRangeSettingValidation(this);
		}
	}
}